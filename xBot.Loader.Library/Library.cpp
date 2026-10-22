#include <Windows.h>
#include <stdio.h>
#include <string>
#include <vector>
#include <iostream>
#include <sstream>
#include <fstream>
#include "detours.h"
#include "PayloadHelper.h"

#pragma comment(lib, "ws2_32.lib")

using namespace std;

HMODULE g_Instance = NULL;
HANDLE g_Thread = NULL;

BYTE g_IsDebug;
string g_RedirectIP = "127.0.0.1";
WORD g_RedirectPort = 1500;
bool g_Activated = false;

vector<string> g_RealGatewayAddresses;
vector<u_long> g_ResolvedGatewayIPs;
WORD g_RealGatewayPort = 15779;
WORD g_RealGatewayPortNet = 0;
u_long g_RedirectIPAddr = 0;
WORD g_RedirectPortNet = 0;

#define DBG_PRINT(...) do { if (g_IsDebug) { printf(__VA_ARGS__); } } while(0)
#define DBG_WPRINT(...) do { if (g_IsDebug) { wprintf(__VA_ARGS__); } } while(0)

std::vector<std::string> TokenizeString(const std::string& str, const std::string& delim)
{
	// http://www.gamedev.net/community/forums/topic.asp?topic_id=381544#TokenizeString
	using namespace std;
	vector<string> tokens;
	size_t p0 = 0, p1 = string::npos;
	while (p0 != string::npos)
	{
		p1 = str.find_first_of(delim, p0);
		if (p1 != p0)
		{
			string token = str.substr(p0, p1 - p0);
			tokens.push_back(token);
		}
		p0 = str.find_first_not_of(delim, p1);
	}
	return tokens;
}

extern "C" HANDLE(WINAPI* Real_CreateSemaphoreW)(
	LPSECURITY_ATTRIBUTES lpSemaphoreAttributes,
	LONG lInitialCount,
	LONG lMaximumCount,
	LPCWSTR lpName) = CreateSemaphoreW;

HANDLE WINAPI User_CreateSemaphoreW(
	LPSECURITY_ATTRIBUTES lpSemaphoreAttributes,
	LONG lInitialCount,
	LONG lMaximumCount,
	LPCWSTR lpName)
{
	if (lpName && wcsstr(lpName, L"Silkroad Client") != nullptr)
	{
		wchar_t newName[128] = { 0 };

		_snwprintf_s(
			newName,
			_countof(newName),
			_TRUNCATE,
			L"%s_%llu",
			lpName,
			(unsigned long long)(__rdtsc() & 0xFFFFFFFF)
		);

		DBG_WPRINT(L"%hs %ls", __FUNCTION__, newName);

		return Real_CreateSemaphoreW(
			lpSemaphoreAttributes,
			lInitialCount,
			lMaximumCount,
			newName
		);
	}

	return Real_CreateSemaphoreW(
		lpSemaphoreAttributes,
		lInitialCount,
		lMaximumCount,
		lpName
	);
}

extern "C" HANDLE(WINAPI* Real_CreateSemaphoreA)(
	LPSECURITY_ATTRIBUTES lpSemaphoreAttributes,
	LONG lInitialCount,
	LONG lMaximumCount,
	LPCSTR lpName) = CreateSemaphoreA;

HANDLE WINAPI User_CreateSemaphoreA(
	LPSECURITY_ATTRIBUTES lpSemaphoreAttributes,
	LONG lInitialCount,
	LONG lMaximumCount,
	LPCSTR lpName)
{
	if (lpName && strstr(lpName, "Silkroad Client") != nullptr)
	{
		char newName[128] = { 0 };

		_snprintf_s(
			newName,
			_countof(newName),
			_TRUNCATE,
			"%s_%llu",
			lpName,
			(unsigned long long)(__rdtsc() & 0xFFFFFFFF)
		);

		DBG_PRINT("%s %s", __FUNCTION__, newName);

		return Real_CreateSemaphoreA(
			lpSemaphoreAttributes,
			lInitialCount,
			lMaximumCount,
			newName
		);
	}

	return Real_CreateSemaphoreA(
		lpSemaphoreAttributes,
		lInitialCount,
		lMaximumCount,
		lpName
	);
}

extern "C" HANDLE(WINAPI* Real_CreateMutexA)(LPSECURITY_ATTRIBUTES lpMutexAttributes, BOOL bInitialOwner, LPCSTR lpName) = CreateMutexA;
HANDLE WINAPI User_CreateMutexA(LPSECURITY_ATTRIBUTES lpMutexAttributes, BOOL bInitialOwner, LPCSTR lpName)
{

	if (lpName && strstr(lpName, "Silkroad Client") != nullptr)
	{
		char newName[128] = { 0 };

		_snprintf_s(
			newName,
			_countof(newName),
			_TRUNCATE,
			"%s_%llu",
			lpName,
			(unsigned long long)(__rdtsc() & 0xFFFFFFFF)
		);
		DBG_PRINT("%s %s", __FUNCTION__, newName);

		return Real_CreateMutexA(lpMutexAttributes, bInitialOwner, newName);
	}
	return Real_CreateMutexA(lpMutexAttributes, bInitialOwner, lpName);
}

extern "C" int (WINAPI* Real_bind)(SOCKET s, const struct sockaddr* name, int namelen) = bind;
int WINAPI User_bind(SOCKET s, const struct sockaddr* name, int namelen)
{
	if (name != nullptr && namelen >= static_cast<int>(sizeof(sockaddr_in)) && name->sa_family == AF_INET)
	{
		const sockaddr_in* inaddr = reinterpret_cast<const sockaddr_in*>(name);
		if (inaddr->sin_port == htons(g_RealGatewayPort))
		{
			sockaddr_in localAddr = *inaddr;
			localAddr.sin_port = 0;
			return Real_bind(s, reinterpret_cast<const sockaddr*>(&localAddr), namelen);
		}
	}
	return Real_bind(s, name, namelen);
}

extern "C" int (WINAPI* Real_connect)(SOCKET, const struct sockaddr*, int) = connect;
int WINAPI Detour_connect(SOCKET s, const struct sockaddr* name, int len)
{
	if (name != nullptr && len >= static_cast<int>(sizeof(sockaddr_in)) && name->sa_family == AF_INET)
	{
		const sockaddr_in* inaddr = reinterpret_cast<const sockaddr_in*>(name);

		DBG_PRINT("[Detour_connect] ip: %s, port: %d \n", inet_ntoa(inaddr->sin_addr), htons(inaddr->sin_port));

		if (inaddr->sin_port == g_RealGatewayPortNet)
		{
			bool ipMatches = g_ResolvedGatewayIPs.empty();
			for (u_long gwIP : g_ResolvedGatewayIPs)
			{
				if (inaddr->sin_addr.s_addr == gwIP)
				{
					ipMatches = true;
					break;
				}
			}

			if (ipMatches)
			{
				sockaddr_in redirected = *inaddr;
				redirected.sin_addr.s_addr = g_RedirectIPAddr;
				redirected.sin_port = g_RedirectPortNet;

				DBG_PRINT("[connect] redirected gateway %s:%d\n", g_RedirectIP.c_str(), g_RedirectPort);
				return Real_connect(s, reinterpret_cast<const sockaddr*>(&redirected), len);
			}
		}
	}

	// Regular connect
	return Real_connect(s, name, len);
}

void Install()
{
	CreateMutexA(0, FALSE, "Silkroad Online Launcher");
	CreateMutexA(0, FALSE, "Ready");

	DetourTransactionBegin();

	//Multiclient
	DetourAttach(&(PVOID&)Real_CreateMutexA,     User_CreateMutexA);
	DetourAttach(&(PVOID&)Real_bind,             User_bind);
	DetourAttach(&(PVOID&)Real_CreateSemaphoreA, User_CreateSemaphoreA);
	DetourAttach(&(PVOID&)Real_CreateSemaphoreW, User_CreateSemaphoreW);
	DetourAttach(&(PVOID&)Real_connect,          Detour_connect);

	DetourTransactionCommit();
}

void Uninstall()
{
	DetourTransactionBegin();

	DetourDetach(&(PVOID&)Real_CreateMutexA,     User_CreateMutexA);
	DetourDetach(&(PVOID&)Real_bind,             User_bind);
	DetourDetach(&(PVOID&)Real_CreateSemaphoreA, User_CreateSemaphoreA);
	DetourDetach(&(PVOID&)Real_CreateSemaphoreW, User_CreateSemaphoreW);
	DetourDetach(&(PVOID&)Real_connect,          Detour_connect);

	DetourTransactionCommit();
}

bool ParseAndApplyPayload(const char* buffer, size_t bufferLength)
{
	if (buffer == nullptr || bufferLength < 14)
		return false;

	string payloadBytes(buffer, bufferLength);
	istringstream stream(payloadBytes, ios::in | ios::binary);

	DWORD magic = 0;
	WORD version = 0;
	DWORD payloadLength = 0;
	DWORD expectedChecksum = 0;
	bool headerValid = PayloadRead(stream, magic)
		&& PayloadRead(stream, version)
		&& PayloadRead(stream, payloadLength)
		&& PayloadRead(stream, expectedChecksum)
		&& magic == 0x544F4258
		&& version == 1
		&& payloadLength > 0
		&& payloadLength <= 65536
		&& (14 + payloadLength <= bufferLength);

	if (!headerValid)
		return false;

	vector<char> payload(payloadLength);
	if (!stream.read(payload.data(), payloadLength))
		return false;

	DWORD checksum = 2166136261u;
	for (size_t j = 0; j < payload.size(); j++)
	{
		checksum ^= static_cast<unsigned char>(payload[j]);
		checksum *= 16777619u;
	}
	if (checksum != expectedChecksum)
		return false;

	string innerBytes(payload.begin(), payload.end());
	istringstream payloadStream(innerBytes, ios::in | ios::binary);
	BYTE isDebug = 0;
	string redirectIP;
	WORD redirectPort = 0;
	DWORD gatewayAddressCount = 0;
	vector<string> gatewayAddresses;
	WORD gatewayPort = 0;
	bool legacyMac = false;

	bool valid = PayloadRead(payloadStream, isDebug)
		&& PayloadReadString(payloadStream, redirectIP, 255)
		&& PayloadRead(payloadStream, redirectPort)
		&& PayloadRead(payloadStream, gatewayAddressCount)
		&& gatewayAddressCount <= 64;

	for (size_t j = 0; valid && j < gatewayAddressCount; j++)
	{
		string address;
		valid = PayloadReadString(payloadStream, address, 255) && !address.empty();
		if (valid)
			gatewayAddresses.push_back(address);
	}
	valid = valid
		&& PayloadRead(payloadStream, gatewayPort)
		&& PayloadRead(payloadStream, legacyMac)
		&& !redirectIP.empty()
		&& redirectPort != 0
		&& gatewayPort != 0;

	if (!valid)
		return false;

	vector<u_long> resolvedIPs;
	for (const auto& addrStr : gatewayAddresses)
	{
		u_long ip = inet_addr(addrStr.c_str());
		if (ip != INADDR_NONE)
		{
			resolvedIPs.push_back(ip);
		}
		else
		{
			struct hostent* remoteHost = gethostbyname(addrStr.c_str());
			if (remoteHost != nullptr && remoteHost->h_addr_list != nullptr)
			{
				for (int k = 0; remoteHost->h_addr_list[k] != nullptr; k++)
				{
					resolvedIPs.push_back(*(u_long*)remoteHost->h_addr_list[k]);
				}
			}
		}
	}

	g_IsDebug = isDebug;
	g_RedirectIP = redirectIP;
	g_RedirectPort = redirectPort;
	g_RealGatewayAddresses.swap(gatewayAddresses);
	g_ResolvedGatewayIPs.swap(resolvedIPs);
	g_RealGatewayPort = gatewayPort;
	g_RealGatewayPortNet = htons(gatewayPort);
	g_RedirectIPAddr = inet_addr(g_RedirectIP.c_str());
	g_RedirectPortNet = htons(g_RedirectPort);
	g_Activated = true;
	return true;
}

extern "C" __declspec(dllexport) DWORD WINAPI InitializeWithMemory(LPVOID lpParam)
{
	if (g_Activated) {
		return 1;
	}

	WSADATA wsaData;
	WSAStartup(MAKEWORD(2, 2), &wsaData);

	if (lpParam == nullptr) {
		return 0;
	}

	const BYTE* raw = reinterpret_cast<const BYTE*>(lpParam);
	DWORD magic = 0;
	WORD version = 0;
	DWORD payloadLength = 0;
	memcpy(&magic, raw, 4);
	memcpy(&version, raw + 4, 2);
	memcpy(&payloadLength, raw + 6, 4);

	if (magic != 0x544F4258 || version != 1 || payloadLength == 0 || payloadLength > 65536) {
		return 0;
	}

	size_t totalSize = 14 + payloadLength;
	if (!ParseAndApplyPayload(reinterpret_cast<const char*>(raw), totalSize)) {
		return 0;
	}

	if (g_IsDebug) {
		AllocConsole();
		freopen_s((FILE**)stdout, "CONOUT$", "w", stdout);
	}

	Install();
	return 1;
}
#pragma comment(linker, "/EXPORT:InitializeWithMemory=_InitializeWithMemory@4")

void LoadConfigLegacy()
{
	char* tempFolder = NULL;
	_dupenv_s(&tempFolder, NULL, "TMP");
	if (!tempFolder) return;

	stringstream payloadPath;
	payloadPath << tempFolder << "\\xBot_" << GetCurrentProcessId() << ".tmp";
	free(tempFolder);

	ifstream stream(payloadPath.str(), ifstream::binary | ifstream::ate);
	if (stream.is_open()) {
		streamsize size = stream.tellg();
		stream.seekg(0, ios::beg);
		if (size >= 14 && size <= 65536 + 14) {
			vector<char> buf(static_cast<size_t>(size));
			if (stream.read(buf.data(), size)) {
				stream.close();
				DeleteFileA(payloadPath.str().c_str());
				if (ParseAndApplyPayload(buf.data(), buf.size())) {
					if (g_IsDebug) {
						AllocConsole();
						freopen_s((FILE**)stdout, "CONOUT$", "w", stdout);
					}
					Install();
				}
				return;
			}
		}
		stream.close();
		DeleteFileA(payloadPath.str().c_str());
	}
}

extern "C" _declspec(dllexport) BOOL APIENTRY DllMain(HMODULE hModule, DWORD ulReason, LPVOID lpReserved)
{
	if (ulReason == DLL_PROCESS_ATTACH) {
		DisableThreadLibraryCalls(hModule);
		g_Instance = hModule;
	}
	return TRUE;
}
