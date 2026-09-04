#include <Windows.h>
#include <stdio.h>
#include <string>
#include <vector>
#include <iostream>
#include <sstream>
#include <fstream>
#include <iphlpapi.h>
#include "detours.h"
#include "PayloadHelper.h"

#pragma comment(lib, "IPHLPAPI.lib")
#pragma comment(lib, "ws2_32.lib")

using namespace std;

HMODULE g_Instance = NULL;
HANDLE g_Thread = NULL;

BYTE g_IsDebug;
string g_RedirectIP = "127.0.0.1";
WORD g_RedirectPort = 1500;
bool g_Activated = false;

vector<string> g_RealGatewayAddresses;
WORD g_RealGatewayPort = 15779;

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
	if (name && namelen >= 16)
	{
		sockaddr_in* inaddr = (sockaddr_in*)name;
		if (inaddr->sin_port == ntohs(g_RealGatewayPort))
		{
			inaddr->sin_port = 0;
		}
	}
	return Real_bind(s, name, namelen);
}

extern "C" DWORD(WINAPI* Real_GetAdaptersInfo)(PIP_ADAPTER_INFO pAdapterInfo, PULONG pOutBufLen) = GetAdaptersInfo;
DWORD WINAPI User_GetAdaptersInfo(PIP_ADAPTER_INFO pAdapterInfo, PULONG pOutBufLen)
{
	DWORD dwResult = Real_GetAdaptersInfo(pAdapterInfo, pOutBufLen);
	if (dwResult == ERROR_SUCCESS && pAdapterInfo != NULL)
	{
		srand((unsigned int)(__rdtsc() & 0xFFFFFFFF));

		PIP_ADAPTER_INFO pAdapter = pAdapterInfo;
		while (pAdapter)
		{
			UINT maxLen = min((UINT)pAdapter->AddressLength, (UINT)MAX_ADAPTER_ADDRESS_LENGTH);
			for (UINT i = 1; i < maxLen; i++)
			{
				pAdapter->Address[i] = (BYTE)(rand() % 256);
			}
			pAdapter = pAdapter->Next;
		}
	}
	return dwResult;
}

extern "C" int (WINAPI* Real_connect)(SOCKET, const struct sockaddr*, int) = connect;
int WINAPI Detour_connect(SOCKET s, const struct sockaddr* name, int len)
{
	auto* editing = reinterpret_cast<sockaddr_in*>(const_cast<sockaddr*>(name));

	DBG_PRINT("[Detour_connect] ip: %s, port: %d \n", inet_ntoa(editing->sin_addr), htons(editing->sin_port));

	for (auto& gatewayAddress : g_RealGatewayAddresses)
	{
		struct hostent* remoteHost = gethostbyname(gatewayAddress.c_str());
		if (remoteHost == NULL || remoteHost->h_addr_list[0] == NULL) continue;

		bool ipMatches = false;
		for (int k = 0; remoteHost->h_addr_list[k] != NULL; k++)
		{
			if (editing->sin_addr.s_addr == *(u_long*)remoteHost->h_addr_list[k])
			{
				ipMatches = true;
				break;
			}
		}

		if (ipMatches && htons(editing->sin_port) == g_RealGatewayPort)
		{
			editing->sin_addr.S_un.S_addr = inet_addr(g_RedirectIP.c_str());
			editing->sin_port = htons(g_RedirectPort);

			DBG_PRINT("[connect] redirected gateway %s:%d\n", g_RedirectIP.c_str(), g_RedirectPort);
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
	// DetourAttach(&(PVOID&)Real_GetAdaptersInfo,  User_GetAdaptersInfo); // Disabled: randomizing MAC breaks HWID on private servers with anti-cheat
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
	// DetourDetach(&(PVOID&)Real_GetAdaptersInfo,  User_GetAdaptersInfo);
	DetourDetach(&(PVOID&)Real_CreateSemaphoreA, User_CreateSemaphoreA);
	DetourDetach(&(PVOID&)Real_CreateSemaphoreW, User_CreateSemaphoreW);
	DetourDetach(&(PVOID&)Real_connect,          Detour_connect);

	DetourTransactionCommit();
}

void LoadConfig()
{
	char* tempFolder = NULL;
	_dupenv_s(&tempFolder, NULL, "TMP");
	if (!tempFolder) return;

	stringstream payloadPath;
	payloadPath << tempFolder << "\\xBot_" << GetCurrentProcessId() << ".tmp";

	// FIX: tempFolder is allocated by _dupenv_s and must be freed with free().
	// Previously it was never freed, leaking heap memory on every LoadConfig call.
	free(tempFolder);
	tempFolder = NULL;

	for (int i = 0; i < 30; i++) {
		ifstream stream(payloadPath.str(), ifstream::binary);
		if (stream.is_open()) {
			g_Activated = true;
			PayloadRead(stream, g_IsDebug);
			PayloadReadString(stream, g_RedirectIP);
			PayloadRead(stream, g_RedirectPort);

			DWORD nRealGatewayAddressCount = 0;
			PayloadRead(stream, nRealGatewayAddressCount);
			for (size_t j = 0; j < nRealGatewayAddressCount; j++)
			{
				string address;
				PayloadReadString(stream, address);
				g_RealGatewayAddresses.push_back(address);
			}
			PayloadRead(stream, g_RealGatewayPort);

			stream.close();
			DeleteFileA(payloadPath.str().c_str());
			return;
		}
		Sleep(100);
	}
}

DWORD WINAPI Initialize(LPVOID lpParam) {
	LoadConfig();

	if (!g_Activated) {
		return 0;
	}

	if (g_IsDebug) {
		AllocConsole();
		freopen_s((FILE**)stdout, "CONOUT$", "w", stdout);
	}

	Install();
	return 0;
}

extern "C" _declspec(dllexport) BOOL APIENTRY DllMain(HMODULE hModule, DWORD ulReason, LPVOID lpReserved)
{
	if (ulReason == DLL_PROCESS_ATTACH) {
		CloseHandle(CreateThread(NULL, 0, Initialize, NULL, 0, NULL));
	}
	return TRUE;
}
