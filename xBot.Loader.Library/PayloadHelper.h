#pragma once

#include <iostream>
#include <fstream>

using namespace std;

template <typename T>
bool PayloadRead(istream& stream, T& dst)
{
	return static_cast<bool>(stream.read(reinterpret_cast<char*>(&dst), sizeof(T)));
}

bool PayloadReadString(istream& stream, string& dst, int maxLength = 4096)
{
	int nLength;
	if (!PayloadRead(stream, nLength) || nLength < 0 || nLength > maxLength)
	{
		dst.clear();
		return false;
	}

	if (nLength > 0) 
	{
		dst.resize(nLength);
		if (!stream.read(&dst[0], nLength))
		{
			dst.clear();
			return false;
		}
	}
	else
		dst = "";
	return true;
}

template <typename T>
void PayloadWrite(ofstream& stream, T& src)
{
	stream.write(reinterpret_cast<char*>(&src), sizeof(T));
}

void PayloadWriteString(ofstream& stream, string& src)
{
	int nLength = src.size();
	PayloadWrite(stream, nLength);

	stream.write(src.c_str(), nLength);
}
