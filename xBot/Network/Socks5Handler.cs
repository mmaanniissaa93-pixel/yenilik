using System;
using System.IO;
using System.Net.Sockets;
using xBot.App;

namespace xBot.Network
{
    /// <summary>
    /// Connects a TCP socket to a target server via a SOCKS5 proxy (RFC 1928 / RFC 1929).
    /// </summary>
    public static class Socks5Handler
    {
        /// <summary>
        /// Performs connection, handshake, authentication, and tunneling to the target host and port.
        /// </summary>
        public static void Connect(
            Socket socket,
            string proxyHost,
            ushort proxyPort,
            string targetHost,
            ushort targetPort,
            string username = null,
            string password = null,
            int timeoutMs = 15000)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            if (!Socks5Policy.IsValidConfig(proxyHost, proxyPort))
                throw new ArgumentException($"Invalid SOCKS5 proxy address: {proxyHost}:{proxyPort}");

            socket.ReceiveTimeout = timeoutMs;
            socket.SendTimeout = timeoutMs;

            // 1. Connect TCP to proxy
            socket.Connect(proxyHost, proxyPort);

            // 2. Greeting (Method negotiation)
            bool hasAuth = !string.IsNullOrEmpty(username);
            byte[] greeting = Socks5Policy.BuildGreeting(hasAuth);
            socket.Send(greeting);

            byte[] greetResp = new byte[2];
            ReceiveExact(socket, greetResp, 2);

            byte method = Socks5Policy.ParseGreetingResponse(greetResp, 2);
            if (method == Socks5Policy.METHOD_NO_ACCEPTABLE)
            {
                throw new InvalidOperationException("SOCKS5 proxy authentication method rejected (0xFF).");
            }

            // 3. Username / Password subnegotiation (RFC 1929)
            if (method == Socks5Policy.METHOD_USER_PASS)
            {
                byte[] authReq = Socks5Policy.BuildAuthRequest(username, password);
                socket.Send(authReq);

                byte[] authResp = new byte[2];
                ReceiveExact(socket, authResp, 2);

                if (!Socks5Policy.ParseAuthResponse(authResp, 2))
                {
                    throw new InvalidOperationException("SOCKS5 proxy authentication failed.");
                }
            }

            // 4. SOCKS5 CONNECT command (RFC 1928)
            byte[] connectReq = Socks5Policy.BuildConnectRequest(targetHost, targetPort);
            socket.Send(connectReq);

            byte[] respHeader = new byte[4];
            ReceiveExact(socket, respHeader, 4);

            if (!Socks5Policy.ParseConnectResponse(respHeader, 4, out string errMsg))
            {
                throw new InvalidOperationException(errMsg);
            }

            // 5. Consume bound address & port from response
            byte atyp = respHeader[3];
            if (atyp == Socks5Policy.ATYP_IPV4)
            {
                byte[] bnd = new byte[4 + 2]; // 4 bytes IPv4 + 2 bytes port
                ReceiveExact(socket, bnd, bnd.Length);
            }
            else if (atyp == Socks5Policy.ATYP_DOMAIN)
            {
                byte[] lenBuf = new byte[1];
                ReceiveExact(socket, lenBuf, 1);
                byte domainLen = lenBuf[0];
                byte[] bnd = new byte[domainLen + 2]; // domain bytes + 2 bytes port
                ReceiveExact(socket, bnd, bnd.Length);
            }
            else // IPv6 (0x04)
            {
                byte[] bnd = new byte[16 + 2]; // 16 bytes IPv6 + 2 bytes port
                ReceiveExact(socket, bnd, bnd.Length);
            }
        }

        private static void ReceiveExact(Socket socket, byte[] buffer, int count)
        {
            int offset = 0;
            while (offset < count)
            {
                int received = socket.Receive(buffer, offset, count - offset, SocketFlags.None);
                if (received <= 0)
                    throw new EndOfStreamException("SOCKS5 proxy server closed the connection unexpectedly.");
                offset += received;
            }
        }
    }
}
