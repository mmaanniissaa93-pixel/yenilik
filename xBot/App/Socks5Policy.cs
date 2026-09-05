using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace xBot.App
{
    /// <summary>
    /// Pure decision and packet format engine for SOCKS5 (RFC 1928 & RFC 1929).
    /// Free of network I/O dependencies for complete unit testability.
    /// </summary>
    public static class Socks5Policy
    {
        public const byte SOCKS_VERSION = 0x05;
        public const byte METHOD_NO_AUTH = 0x00;
        public const byte METHOD_USER_PASS = 0x02;
        public const byte METHOD_NO_ACCEPTABLE = 0xFF;

        public const byte CMD_CONNECT = 0x01;
        public const byte ATYP_IPV4 = 0x01;
        public const byte ATYP_DOMAIN = 0x03;

        public const byte REP_SUCCESS = 0x00;
        public const byte REP_GENERAL_FAILURE = 0x01;
        public const byte REP_NOT_ALLOWED = 0x02;
        public const byte REP_NETWORK_UNREACHABLE = 0x03;
        public const byte REP_HOST_UNREACHABLE = 0x04;
        public const byte REP_CONNECTION_REFUSED = 0x05;
        public const byte REP_TTL_EXPIRED = 0x06;
        public const byte REP_COMMAND_NOT_SUPPORTED = 0x07;
        public const byte REP_ADDRESS_TYPE_NOT_SUPPORTED = 0x08;

        /// <summary>
        /// Validates SOCKS5 configuration parameters.
        /// </summary>
        public static bool IsValidConfig(string proxyHost, ushort proxyPort)
        {
            if (string.IsNullOrWhiteSpace(proxyHost))
                return false;
            return proxyPort > 0;
        }

        /// <summary>
        /// Builds RFC 1928 method selection greeting message.
        /// </summary>
        public static byte[] BuildGreeting(bool hasAuth)
        {
            if (hasAuth)
            {
                // Version 5, 2 methods: 0x00 (No Auth), 0x02 (Username/Password)
                return new byte[] { SOCKS_VERSION, 0x02, METHOD_NO_AUTH, METHOD_USER_PASS };
            }
            // Version 5, 1 method: 0x00 (No Auth)
            return new byte[] { SOCKS_VERSION, 0x01, METHOD_NO_AUTH };
        }

        /// <summary>
        /// Parses the server's method selection response. Returns chosen method or 0xFF if invalid.
        /// </summary>
        public static byte ParseGreetingResponse(byte[] response, int length)
        {
            if (response == null || length < 2)
                return METHOD_NO_ACCEPTABLE;

            if (response[0] != SOCKS_VERSION)
                return METHOD_NO_ACCEPTABLE;

            return response[1];
        }

        /// <summary>
        /// Builds RFC 1929 username and password authentication request.
        /// Format: [0x01, ulen, ...usernameBytes..., plen, ...passwordBytes...]
        /// </summary>
        public static byte[] BuildAuthRequest(string username, string password)
        {
            byte[] userBytes = Encoding.ASCII.GetBytes(username ?? string.Empty);
            byte[] passBytes = Encoding.ASCII.GetBytes(password ?? string.Empty);

            if (userBytes.Length > 255) Array.Resize(ref userBytes, 255);
            if (passBytes.Length > 255) Array.Resize(ref passBytes, 255);

            byte[] buffer = new byte[3 + userBytes.Length + passBytes.Length];
            buffer[0] = 0x01; // Subnegotiation version 1
            buffer[1] = (byte)userBytes.Length;
            Buffer.BlockCopy(userBytes, 0, buffer, 2, userBytes.Length);

            int passOffset = 2 + userBytes.Length;
            buffer[passOffset] = (byte)passBytes.Length;
            Buffer.BlockCopy(passBytes, 0, buffer, passOffset + 1, passBytes.Length);

            return buffer;
        }

        /// <summary>
        /// Parses RFC 1929 username/password authentication response.
        /// </summary>
        public static bool ParseAuthResponse(byte[] response, int length)
        {
            if (response == null || length < 2)
                return false;

            // Subnegotiation version 1, status 0x00 means success
            return response[0] == 0x01 && response[1] == 0x00;
        }

        /// <summary>
        /// Builds RFC 1928 SOCKS5 CONNECT request packet for a target host and port.
        /// </summary>
        public static byte[] BuildConnectRequest(string targetHost, ushort targetPort)
        {
            if (string.IsNullOrWhiteSpace(targetHost))
                throw new ArgumentException("Target host cannot be empty.", nameof(targetHost));

            byte[] addressBytes;
            byte atyp;

            if (IPAddress.TryParse(targetHost, out IPAddress ip) && ip.AddressFamily == AddressFamily.InterNetwork)
            {
                atyp = ATYP_IPV4;
                addressBytes = ip.GetAddressBytes();
            }
            else
            {
                atyp = ATYP_DOMAIN;
                byte[] domainBytes = Encoding.ASCII.GetBytes(targetHost);
                if (domainBytes.Length > 255)
                    Array.Resize(ref domainBytes, 255);

                addressBytes = new byte[1 + domainBytes.Length];
                addressBytes[0] = (byte)domainBytes.Length;
                Buffer.BlockCopy(domainBytes, 0, addressBytes, 1, domainBytes.Length);
            }

            // Packet format: [VER, CMD, RSV, ATYP, ...DST.ADDR..., DST.PORT (2 bytes, big-endian)]
            byte[] packet = new byte[4 + addressBytes.Length + 2];
            packet[0] = SOCKS_VERSION;
            packet[1] = CMD_CONNECT;
            packet[2] = 0x00; // Reserved
            packet[3] = atyp;

            Buffer.BlockCopy(addressBytes, 0, packet, 4, addressBytes.Length);

            int portOffset = 4 + addressBytes.Length;
            packet[portOffset] = (byte)((targetPort >> 8) & 0xFF);
            packet[portOffset + 1] = (byte)(targetPort & 0xFF);

            return packet;
        }

        /// <summary>
        /// Parses RFC 1928 SOCKS5 CONNECT response.
        /// </summary>
        public static bool ParseConnectResponse(byte[] response, int length, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (response == null || length < 4)
            {
                errorMessage = "Invalid SOCKS5 response length.";
                return false;
            }

            if (response[0] != SOCKS_VERSION)
            {
                errorMessage = $"Unsupported SOCKS version: {response[0]}";
                return false;
            }

            byte rep = response[1];
            switch (rep)
            {
                case REP_SUCCESS:
                    return true;
                case REP_GENERAL_FAILURE:
                    errorMessage = "SOCKS5: General SOCKS server failure";
                    return false;
                case REP_NOT_ALLOWED:
                    errorMessage = "SOCKS5: Connection not allowed by ruleset";
                    return false;
                case REP_NETWORK_UNREACHABLE:
                    errorMessage = "SOCKS5: Network unreachable";
                    return false;
                case REP_HOST_UNREACHABLE:
                    errorMessage = "SOCKS5: Host unreachable";
                    return false;
                case REP_CONNECTION_REFUSED:
                    errorMessage = "SOCKS5: Connection refused";
                    return false;
                case REP_TTL_EXPIRED:
                    errorMessage = "SOCKS5: TTL expired";
                    return false;
                case REP_COMMAND_NOT_SUPPORTED:
                    errorMessage = "SOCKS5: Command not supported";
                    return false;
                case REP_ADDRESS_TYPE_NOT_SUPPORTED:
                    errorMessage = "SOCKS5: Address type not supported";
                    return false;
                default:
                    errorMessage = $"SOCKS5: Unknown reply code ({rep})";
                    return false;
            }
        }
    }
}
