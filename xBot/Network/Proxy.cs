using SecurityAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using xBot.App;
using xBot.App.Theme;
using xBot.Game;

namespace xBot.Network
{
	public class Proxy
	{
		/// <summary>
		/// Gateway connection.
		/// </summary>
		public Gateway Gateway { get; private set; }
		/// <summary>
		/// Agent connection.
		/// </summary>
		public Agent Agent { get; private set; }
		/// <summary>
		/// Gets the current Silkroad process connected to the proxy.
		/// </summary>
		public Process SRO_Client {
			get {
				if (sro_client != null)
				{
					sro_client.Refresh();
					if (sro_client.HasExited)
						sro_client = null;
				}
				return sro_client;
			}
		}
		private Process sro_client;
		public bool ClientlessMode { get { return SRO_Client == null; } }
		/// <summary>
		/// Check the current client mode.
		/// </summary>
		public bool LoginClientlessMode { get; }

		private Thread ThreadProxyReconnection;
		private int CurrentAttemptReconnections;
		private Thread PingHandler;
		public bool isRunning { get; private set; }
		private int lastPortIndexSelected;
		private int lastHostIndexSelected;
		private List<ushort> GatewayPorts { get; }
		private List<string> GatewayHosts { get; }
		public Proxy(bool LoginClientlessMode, List<string> Hosts,List<ushort> Ports)
		{
			this.LoginClientlessMode = LoginClientlessMode;
			sro_client = null;
			GatewayHosts = Hosts;
			RandomHost = false;
			GatewayPorts = Ports;
			lastPortIndexSelected = lastHostIndexSelected = -1;
		}
		private Random rand;
		/// <summary>
		/// Gets o sets the host selection to random or sequence.
		/// </summary>
		public bool RandomHost {
			get {
				return (rand != null);
			}
			set {
				if (value && rand == null)
				{
					rand = new Random();
				}
				else if(!value && rand != null)
				{
					rand = null;
				}
			}
		}
		/// <summary>
		/// Start the connection.
		/// </summary>
		public void Start()
		{
			isRunning = true;
			Window w = Window.Get;
			w.Login_btnStart.InvokeIfRequired(()=> {
				w.Login_btnStart.Text = "STOP";
				w.Login_btnStart.Enabled = true;
			});

			Thread gwThread = (new Thread(ThreadGateway));
			gwThread.Priority = ThreadPriority.AboveNormal;
			gwThread.Start();
		}
		private string SelectHost()
		{
			if (RandomHost)
				lastHostIndexSelected = rand.Next(GatewayHosts.Count);
			else
				lastHostIndexSelected++;
			if (lastHostIndexSelected == GatewayHosts.Count)
				lastHostIndexSelected = 0;
			return GatewayHosts[lastHostIndexSelected];
		}
		private ushort SelectPort()
		{
			lastPortIndexSelected++;
			if (lastPortIndexSelected == GatewayPorts.Count)
				lastPortIndexSelected = 0;
			return GatewayPorts[lastPortIndexSelected];
		}
        /// <summary>
        /// Find an available port to bind
        /// </summary>
        public static int GetAvailablePort(AddressFamily addressFamily = AddressFamily.InterNetwork, SocketType socketType = SocketType.Stream, ProtocolType protocolType = ProtocolType.Tcp)
        {
            using (var socket = new Socket(addressFamily, socketType, protocolType))
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, 0));
                return ((IPEndPoint)socket.LocalEndPoint).Port;
            }
        }
        private void ThreadGateway()
		{
			Gateway = new Gateway(this.SelectHost(), this.SelectPort());

			Window w = Window.Get;
			Socket SocketBinded = BindGatewaySocket("127.0.0.1");
			if (!LoginClientlessMode)
			{
				Gateway.Local.Socket = SocketBinded;
				try
				{
					// Loader setup
					w.LogProcess("Executing ClientManager (Detours)...");
					sro_client = ClientManager.Start(
						DataManager.ClientPath,
						DataManager.Locale,
						0,
						lastHostIndexSelected,
						(ushort)((IPEndPoint)Gateway.Local.Socket.LocalEndPoint).Port,
						GatewayHosts,
						Gateway.Port
					);
					if (sro_client == null)
					{
						Stop();
						return;
					}
					//else
					//	sro_client.PriorityClass = ProcessPriorityClass.AboveNormal;

					int dummy = 0;
					w.Log("Waiting for client connection [" + Gateway.Local.Socket.LocalEndPoint.ToString() + "]");
					w.LogProcess("Waiting client connection...", Window.ProcessState.Warning);
					// Wait 2min. Infinity attempts
					ProxyReconnection(120, ref dummy, int.MaxValue);
					Gateway.Local.Socket = Gateway.Local.Socket.Accept();
					ProxyReconnectionStop();
					w.LogProcess("Connected");

					// Save client process
					sro_client.EnableRaisingEvents = true;
					sro_client.Exited += new EventHandler(this.Client_Closed);
				}
				catch (Exception ex)
				{
					w.LogProcess(ex.ToString());
					return;
				}
			}
			try
			{
				Gateway.Remote.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				w.Log("Connecting to Gateway server [" + Gateway.Host + ":" + Gateway.Port + "]");
				w.LogProcess("Waiting server connection...");

				ProxyReconnection(30, ref CurrentAttemptReconnections, 10); // Wait 30 seconds, max. 10 attempts
				if (Socks5Config.GetEffectiveProxy(out string pHost, out ushort pPort, out string pUser, out string pPass))
				{
					w.Log($"[SOCKS5] Routing via {pHost}:{pPort} to Gateway [{Gateway.Host}:{Gateway.Port}]");
					Socks5Handler.Connect(Gateway.Remote.Socket, pHost, pPort, Gateway.Host, Gateway.Port, pUser, pPass);
				}
				else
				{
					Gateway.Remote.Socket.Connect(Gateway.Host, Gateway.Port);
				}
				ProxyReconnectionStop();
				CurrentAttemptReconnections = 0;
				w.Log("Connected");
				w.LogProcess("Connected");
			}
			catch { return; }
			try
			{
				// Handle it easily by iterating
				List<Context> gws = new List<Context>();
				gws.Add(Gateway.Remote);
				if (!ClientlessMode)
				{
					gws.Add(Gateway.Local);
				}
				if (PingHandler != null && PingHandler.IsAlive)
				{
					try { PingHandler.Interrupt(); } catch { }
				}
				PingHandler = new Thread(ThreadPing);
				PingHandler.IsBackground = true;
				PingHandler.Start();
				// Running process
				while (isRunning)
				{
					bool didWork = false;
					// Analyzer bayraklarını döngü başına bir kez oku (paket başına Invoke yok)
					bool gwShowServer = false, gwOnlyShow = false;
					System.Collections.Generic.HashSet<string> gwFilter = null;
					try
					{
						WinAPI.InvokeIfRequired(w.Settings_cbxShowPacketServer, () => {
							gwShowServer = w.Settings_cbxShowPacketServer.Checked;
							gwOnlyShow = w.Settings_rbnPacketOnlyShow.Checked;
							if (gwShowServer && w.Settings_lstvOpcodes.Items.Count > 0)
							{
								gwFilter = new System.Collections.Generic.HashSet<string>();
								foreach (System.Windows.Forms.ListViewItem it in w.Settings_lstvOpcodes.Items)
									gwFilter.Add(it.Text);
							}
						});
					}
					catch { }
					// Network input event processing
					foreach (Context context in gws)
					{
						if (context.Socket.Poll(0, SelectMode.SelectRead))
						{
							int count = 0;
							try
							{
								count = context.Socket.Receive(context.Buffer.Buffer);
							}
							catch (Exception ex)
							{
								string src = (context == Gateway.Remote) ? "Gateway.Remote (Server)" : "Gateway.Local (Client)";
								throw new Exception($"[{src}] {ex.Message}", ex);
							}
							if (count == 0)
							{
								string src = (context == Gateway.Remote) ? "Gateway.Remote (Server)" : "Gateway.Local (Client)";
								throw new Exception($"[{src}] Connection closed gracefully (0 bytes).");
							}
							context.Security.Recv(context.Buffer.Buffer, 0, count);
						}
					}
					// Logic event processing
					foreach (Context context in gws)
					{
						List<Packet> packets = context.Security.TransferIncoming();
						if (packets != null)
						{
							foreach (Packet packet in packets)
							{
								// Show all incoming packets on analizer
								if (context == Gateway.Remote && gwShowServer)
								{
									bool opcodeFound = gwFilter != null && gwFilter.Contains(packet.Opcode.ToString());
									if (opcodeFound && gwOnlyShow
										|| !opcodeFound && !gwOnlyShow)
									{
										byte[] logBytes = packet.GetBytes();
										w.LogPacket(string.Format("[G][{0}][{1:X4}][{2} bytes]{3}{4}{6}{5}", "S->C", packet.Opcode, logBytes.Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Utility.HexDump(logBytes), Environment.NewLine));
									}
								}
								didWork = true;
								// Switch from gateway to agent process
								if (packet.Opcode == Gateway.Opcode.SERVER_LOGIN_RESPONSE || packet.Opcode == Gateway.Opcode.SERVER_LOGIN_RESPONSE_CUSTOM)
								{
									byte result = packet.ReadByte();
									if (result == 1)
									{
										// Stop ping while switch
										try { if (PingHandler != null) PingHandler.Interrupt(); } catch { }

										uint loginID = packet.ReadUInt();
										string remoteAgentHost = packet.ReadAscii();
										ushort remoteAgentPort = packet.ReadUShort();
										byte[] extraBytes = null;
										if (packet.RemainingRead() > 0)
										{
											extraBytes = packet.ReadByteArray(packet.RemainingRead());
										}

										Agent = new Agent(loginID, remoteAgentHost, remoteAgentPort);

										// Bind socket available
										string agentHost = ((IPEndPoint)SocketBinded.LocalEndPoint).Address.ToString();
										int agentPort = GetAvailablePort();
										
										Thread agThread = new Thread(() => {
											ThreadAgent(agentHost, agentPort);
										});
										agThread.Priority = ThreadPriority.AboveNormal;
										agThread.Start();
										
										// Proxy packet (bot listening)
										PacketBuilder.Client.CreateAgentLogin(result, Agent.id, agentHost, (ushort)agentPort, packet.Opcode, extraBytes, packet.Encrypted);
									}
									else if (result == 2)
									{
										byte error = packet.ReadByte();
										switch (error)
										{
											case 1:
												{
													uint maxAttempts = packet.ReadUInt();
													uint attempts = packet.ReadUInt();
													w.Log("Password entry has failed (" + attempts + " / " + maxAttempts + " attempts)");
													w.LogProcess("Password failed", Window.ProcessState.Warning);
													break;
												}
											case 2:
												byte blockType = packet.ReadByte();
												if (blockType == 1)
												{
													string blockedReason = packet.ReadAscii();
													ushort endYear = packet.ReadUShort();
													ushort endMonth = packet.ReadUShort();
													ushort endDay = packet.ReadUShort();
													ushort endHour = packet.ReadUShort();
													ushort endMinute = packet.ReadUShort();
													ushort endSecond = packet.ReadUShort();
													w.Log("Account banned till [" + endDay + "/" + endMonth + "/" + endYear + " " + endHour + "/" + endMinute + "/" + endSecond + "]. Reason: " + blockedReason);
													w.LogProcess("Account banned", Window.ProcessState.Error);
												}
												break;
											case 3:
												w.Log("This user is already connected. Please try again in 5 minutes");
												break;
											default:
												w.Log("Login error [" + error + "]");
												break;
										}
										// Client bugfix reset
										Bot.Get.LoggedFromBot = false;
										w.Login_btnStart.InvokeIfRequired(() => {
											w.Login_btnStart.Enabled = true;
										});

										context.RelaySecurity.Send(packet);
									}
									else
									{
										w.Log("Login response with unknown result [" + result + "]");
										context.RelaySecurity.Send(packet);
									}
								}
								else if (!Gateway.PacketHandler(context, packet)
									&& !Gateway.IgnoreOpcode(packet.Opcode, context))
								{
									// Send normally through proxy
									context.RelaySecurity.Send(packet);
								}
							}
						}
					}
					// Network output event processing
					foreach (Context context in gws)
					{
						if (context.Socket.Poll(0, SelectMode.SelectWrite))
						{
							List<KeyValuePair<TransferBuffer, Packet>> buffers = context.Security.TransferOutgoing();
							if (buffers != null)
							{
								foreach (KeyValuePair<TransferBuffer, Packet> kvp in buffers)
								{
									TransferBuffer buffer = kvp.Key;
									Packet packet = kvp.Value;

									byte[] packet_bytes = packet.GetBytes();
									didWork = true;
									// Show outcoming packets on analizer
									if (context == Gateway.Remote && gwShowServer)
									{
										bool opcodeFound = gwFilter != null && gwFilter.Contains(packet.Opcode.ToString());
										if (opcodeFound && gwOnlyShow
											|| !opcodeFound && !gwOnlyShow)
										{
											w.LogPacket(string.Format("[G][{0}][{1:X4}][{2} bytes]{3}{4}{6}{5}", "C->S", packet.Opcode, packet_bytes.Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Utility.HexDump(packet_bytes), Environment.NewLine));
										}
									}

									while (true)
									{
										int count = 0;
										try
										{
											count = context.Socket.Send(buffer.Buffer, buffer.Offset, buffer.Size, SocketFlags.None);
										}
										catch (Exception ex)
										{
											string src = (context == Gateway.Remote) ? "Gateway.Remote (Server)" : "Gateway.Local (Client)";
											throw new Exception($"[{src} Send Error] {ex.Message}", ex);
										}
										buffer.Offset += count;
										if (buffer.Offset == buffer.Size)
										{
											break;
										}
									}
								}
							}
						}
					}
					if (didWork)
						Thread.Sleep(0);
					else
						Thread.Sleep(1); // Idle: prevent 100% CPU usage
				}
			}
			catch (Exception ex)
			{
				CloseGateway();
				if (Agent != null)
				{
					// Normal transition: Client closed Gateway connection to connect to Agent server
					w.Log("[Gateway] Closed (switched to Agent server)");
				}
				else
				{
					w.LogPacket("[G] Error: " + ex.ToString());
					w.Log("[Gateway Error] " + ex.Message);
					Stop();
				}
			}
		}
		public void CloseClient()
		{
			if (SRO_Client != null)
			{
				Window.Get?.Log("[Proxy] Terminating client process (PID: " + sro_client.Id + ")...");
				sro_client.Kill();
				sro_client = null;
			}
		}
		private void Client_Closed(object sender, EventArgs e)
		{
			try
			{
				if (InfoManager.inGame)
				{
					if (LoginStrategyManager.StayConnected)
					{
						Window.Get?.Log("[Failover] Client exited unexpectedly! Bot switched to Clientless mode and preserved session.");
					}
					else
					{
						Window.Get?.Log("Switched to clientless mode");
					}
				}
			}
			catch { /* Window closed probably.. */ }
		}
		private void ThreadAgent(string Host, int Port)
		{
			Window w = Window.Get;
			// Connect AgentClient to Proxy
			if (!ClientlessMode)
			{
				Agent.Local.Socket = BindSocket(Host, Port);
				try
				{
					int dummy = 0;
					w.LogProcess("Waiting client connection...", Window.ProcessState.Warning);
					ProxyReconnection(10, ref dummy, int.MaxValue);
					Agent.Local.Socket = Agent.Local.Socket.Accept();
					ProxyReconnectionStop();
					w.LogProcess("Connected");
				}
				catch { return; }
			}
			Agent.Remote.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try
			{
				int dummy = 0;
				w.Log("Connecting to Agent server [" + Agent.Host + ":" + Agent.Port + "]");
				w.LogProcess("Waiting server connection...");
				ProxyReconnection(10, ref dummy, int.MaxValue);
				if (Socks5Config.GetEffectiveProxy(out string pHost, out ushort pPort, out string pUser, out string pPass))
				{
					w.Log($"[SOCKS5] Routing via {pHost}:{pPort} to Agent [{Agent.Host}:{Agent.Port}]");
					Socks5Handler.Connect(Agent.Remote.Socket, pHost, pPort, Agent.Host, Agent.Port, pUser, pPass);
				}
				else
				{
					Agent.Remote.Socket.Connect(Agent.Host, Agent.Port);
				}
				ProxyReconnectionStop();
				w.Log("Connected");
				w.LogProcess("Connected");
			}
			catch
			{
				w.Log("Failed to connect to the server");
				w.LogProcess();
				return;
			}
			try
			{
				// Handle it easily by iterating
				List<Context> ags = new List<Context>();
				ags.Add(Agent.Remote);
				if (!ClientlessMode)
				{
					ags.Add(Agent.Local);
				}
				if (PingHandler != null && PingHandler.IsAlive)
				{
					try { PingHandler.Interrupt(); } catch { }
				}
				PingHandler = new Thread(ThreadPing);
				PingHandler.IsBackground = true;
				PingHandler.Start();
				while (isRunning)
				{
					bool didWork = false;
					bool agShowServer = false, agShowClient = false, agOnlyShow = false;
					System.Collections.Generic.HashSet<string> agFilter = null;
					bool traceOn = xBot.App.Theme.ModernLogger.EnablePacketTrace;
					try
					{
						WinAPI.InvokeIfRequired(w.Settings_cbxShowPacketServer, () => {
							agShowServer = w.Settings_cbxShowPacketServer.Checked;
							agShowClient = w.Settings_cbxShowPacketClient.Checked;
							agOnlyShow = w.Settings_rbnPacketOnlyShow.Checked;
							if ((agShowServer || agShowClient) && w.Settings_lstvOpcodes.Items.Count > 0)
							{
								agFilter = new System.Collections.Generic.HashSet<string>();
								foreach (System.Windows.Forms.ListViewItem it in w.Settings_lstvOpcodes.Items)
									agFilter.Add(it.Text);
							}
						});
					}
					catch { }
					// Network input event processing
					foreach (Context context in ags)
					{
						if (context.Socket.Poll(0, SelectMode.SelectRead))
						{
							try
							{
								int count = context.Socket.Receive(context.Buffer.Buffer);
								if (count == 0)
								{
									DumpDisconnectDiagnostic(w, "Sunucu soketi kapattı (Receive count == 0)");
									throw new Exception("The remote connection has been lost.");
								}
								context.Security.Recv(context.Buffer.Buffer, 0, count);
							}
							catch(Exception ex) {
								if (context == Agent.Local)
								{
									// Try to continue without client
									ags.Remove(context);
									break;
								}
								else
								{
									DumpDisconnectDiagnostic(w, $"Recv Hatası: {ex.Message}");
									w.Log($"[Proxy -> Server Recv Error] {ex.Message}");
									throw ex;
								}
							}
						}
					}
					// Logic event processing
					foreach (Context context in ags)
					{
						List<Packet> packets = context.Security.TransferIncoming();
						if (packets != null)
						{
							foreach (Packet packet in packets)
							{
								didWork = true;
								if (traceOn)
								{
									byte[] bytes = packet.GetBytes();
									string hex = Utility.HexDump(bytes).Replace("\r", "").Replace("\n", " ").Trim();
									if (hex.Length > 80) hex = hex.Substring(0, 80) + "...";
									ModernLogger.TracePacket(context == Agent.Remote ? "Server->Client" : "Client->Server", packet.Opcode, bytes.Length, hex);
								}

								// Show all incoming packets on analizer
								if (context == Agent.Remote && agShowServer)
								{
									bool opcodeFound = agFilter != null && agFilter.Contains(packet.Opcode.ToString());
									if (opcodeFound && agOnlyShow
										|| !opcodeFound && !agOnlyShow)
									{
										byte[] logBytes = packet.GetBytes();
										w.LogPacket(string.Format("[A][{0}][{1:X4}][{2} bytes]{3}{4}{6}{5}", "S->C", packet.Opcode, logBytes.Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Utility.HexDump(logBytes), Environment.NewLine));
									}
								}

								if (!Agent.PacketHandler(context, packet) && !Agent.IgnoreOpcode(packet.Opcode, context))
								{
									// Send normally through proxy
									context.RelaySecurity.Send(packet);
								}
							}
						}
					}
					// Network output event processing
					foreach (Context context in ags)
					{
						if (context.Socket.Poll(0, SelectMode.SelectWrite))
						{
							List<KeyValuePair<TransferBuffer, Packet>> buffers = context.Security.TransferOutgoing();
							if (buffers != null)
							{
								foreach (KeyValuePair<TransferBuffer, Packet> kvp in buffers)
								{
									TransferBuffer buffer = kvp.Key;
									Packet packet = kvp.Value;

									didWork = true;
									if (traceOn && context == Agent.Remote)
									{
										ModernLogger.TracePacket("Proxy->Server", packet.Opcode, buffer.Size);
									}
									// Show outcoming packets on analizer
									if (context == Agent.Remote && agShowClient)
									{
										bool opcodeFound = agFilter != null && agFilter.Contains(packet.Opcode.ToString());
										if (opcodeFound && agOnlyShow
											|| !opcodeFound && !agOnlyShow)
										{
											w.LogPacket(string.Format("[A][{0}][{1:X4}][{2} bytes]{3}{4}{6}{5}", "C->S", packet.Opcode, packet.GetBytes().Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Utility.HexDump(packet.GetBytes()), Environment.NewLine));
										}
									}
									
									while (true)
									{
										int count;
										try
										{
											count = context.Socket.Send(buffer.Buffer, buffer.Offset, buffer.Size, SocketFlags.None);
										}
										catch (Exception ex)
										{
											if (context == Agent.Local)
											{
												// Try to continue without send to client
												break;
											}
											else
											{
												DumpDisconnectDiagnostic(w, $"Send Hatası: Opcode 0x{packet.Opcode:X4} - {ex.Message}");
												w.Log($"[Proxy -> Server Send Error] Opcode: 0x{packet.Opcode:X4}, Error: {ex.Message}");
												throw ex;
											}
										}
										buffer.Offset += count;
										if (buffer.Offset == buffer.Size)
											break;
									}
								}
							}
						}
					}
					if (didWork)
						Thread.Sleep(0);
					else
						Thread.Sleep(1); // Idle: prevent 100% CPU usage (Agent)
				}
			}
			catch (Exception ex)
			{
				w.LogPacket("[A] Error: " + ex.Message + Environment.NewLine);
				w.Log("[Agent Error] " + ex.Message);
				Stop();
			}
		}
		private void DumpDisconnectDiagnostic(Window w, string reason)
		{
			try
			{
				var recent = ModernLogger.GetRecentPackets();
				ModernLogger.LogToFile($"=== DISCONNECT DIAGNOSTIC [{reason}] ===");
				w?.Log("========== DISCONNECT DIAGNOSTIC ==========", LogLevel.Warning);
				w?.Log($"[Sunucu Bağlantıyı Kesti] Neden: {reason}", LogLevel.Warning);
				w?.Log("Kopma anından hemen önceki son paketler:", LogLevel.Warning);
				int start = Math.Max(0, recent.Count - 15);
				for (int i = start; i < recent.Count; i++)
				{
					var t = recent[i];
					string line = $"  #{i - start + 1} [{t.Timestamp:HH:mm:ss.fff}] [{t.Direction}] 0x{t.Opcode:X4} ({t.Length}B) {t.Summary}";
					ModernLogger.LogToFile(line);
					w?.Log(line, LogLevel.Info);
				}
				w?.Log("===========================================", LogLevel.Warning);
			}
			catch { }
		}
		private Socket BindGatewaySocket(string ip)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                s.Bind(new IPEndPoint(IPAddress.Parse(ip), 0));
                s.Listen(1);
                return s;
            }
            catch (SocketException)
            {
                /* ignore and continue */
            }
            return null;
        }
		private Socket BindSocket(string ip, int port)
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			s.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
			s.Listen(1);
			return s;
		}
		/// <summary>
		/// Wait the time specified and try to reconnect the proxy if is necessary.
		/// </summary>
		/// <param name="Seconds">Maximum time for waiting</param>
		/// <param name="CurrentAttempts">Current connection counter</param>
		/// <param name="MaxAttempts">Max connections to stop</param>
		private void ProxyReconnection(int Seconds,ref int CurrentAttempts,int MaxAttempts)
		{
			int refCurrentAttempts = CurrentAttempts;
			int refMaxAttempts = MaxAttempts;
			ThreadProxyReconnection = (new Thread((ThreadStart)delegate {
				try
				{
					while (true)
					{
						if (Seconds == 0)
						{
							if(refCurrentAttempts < refMaxAttempts)
							{
								Reset();
								Start();
								refCurrentAttempts++;
							}
							return;
						}
						Thread.Sleep(1000);
						Seconds--;
					}
				}
				catch (ThreadInterruptedException) { return; }
				catch { return; }
			}));
			ThreadProxyReconnection.Start();
		}
		private void ProxyReconnectionStop()
		{
			if (ThreadProxyReconnection != null)
			{
				try { ThreadProxyReconnection.Interrupt(); } catch { }
				ThreadProxyReconnection = null;
			}
		}
		private void ThreadPing()
		{
				while (isRunning)
				{
				try { Thread.Sleep(6666); }
				catch (ThreadInterruptedException) { return; }
				catch { return; }
				// Keep connection alive
				if (Agent != null)
				{
					try
					{
						Packet p = new Packet(Agent.Opcode.GLOBAL_PING);
						Agent.InjectToServer(p);
					}
					catch { /*Connection closed*/}
				}
				else if (Gateway != null)
				{
					try
					{
						Packet p = new Packet(Gateway.Opcode.GLOBAL_PING);
						Gateway.InjectToServer(p);
					}
					catch { /*Connection closed*/ }
				}
			}
		}
		private void CloseGateway()
		{
			if (PingHandler != null && PingHandler.IsAlive && Agent == null)
			{
				try { PingHandler.Interrupt(); } catch { }
			}
			if (Gateway != null)
			{
				if (Gateway.Local.Socket != null)
				{
					try
					{
						Gateway.Local.Socket.Close();
					}
					catch { }
				}
				if (Gateway.Remote.Socket != null)
				{
					try
					{
						Gateway.Remote.Socket.Close();
					}
					catch { }
				}
			}
		}
		private void CloseAgent()
		{
			if (Agent != null)
			{
				if (Agent.Local.Socket != null)
				{
					try
					{
						Agent.Local.Socket.Close();
					}
					catch { }
				}
				if (Agent.Remote.Socket != null)
				{
					try
					{
						Agent.Remote.Socket.Close();
					}
					catch { }
				}
			}
		}
		private void Reset()
		{
			isRunning = false;
			if (PingHandler != null)
				try { PingHandler.Interrupt(); } catch { }
			CloseClient();
			CloseGateway();
			CloseAgent();
		}
		public void Stop()
		{
			ProxyReconnectionStop();
			Reset();
			Window w = Window.Get;
			// Reset locket controls
			w.Login_cmbxSilkroad.InvokeIfRequired(() => {
				w.Login_cmbxSilkroad.Enabled = true;
			});
			w.Login_btnStart.InvokeIfRequired(() => {
				w.Login_btnStart.Text = "START";
				w.Login_btnStart.Enabled = true;
			});
			w.Login_btnLauncher.InvokeIfRequired(() => {
				w.Login_btnLauncher.Enabled = true;
			});
			w.Login_gbxCharacters.InvokeIfRequired(() => {
				w.Login_gbxCharacters.Visible = false;
			});
			w.Login_gbxServers.InvokeIfRequired(() => {
				w.Login_gbxServers.Visible = true;
			});

			if (InfoManager.inGame)
				InfoManager.OnDisconnected();
			DataManager.DisconnectDatabase();
			w.Log("Disconnected");
			w.LogProcess("Disconnected");
			// Relogin — SADECE açık onay varsa: login ekranındaki Relogin tiki
			// veya ayarlardaki oto-yeniden-bağlanma. "Otomatik Giriş" tiki tek
			// başına client açmaz (kullanıcı START'a basmadan işlem yok).
			if (w.Login_cbxRelogin.Checked || LoginStrategyManager.AutoRelogin)
			{
				System.Timers.Timer Relogin = new System.Timers.Timer(1000);
				Relogin.AutoReset = false;
				Relogin.Elapsed += ReloginOnDisconnect;
				ReloginCountdown = Math.Max(15, LoginStrategyManager.WaitAfterDCMinutes * 60);
				Relogin.Start();
				w.LogProcess("Relogin at " + ReloginCountdown + " seconds...");
			}
		}

		private int ReloginCountdown;
		private void ReloginOnDisconnect(object sender, System.Timers.ElapsedEventArgs e){
			System.Timers.Timer timer = (System.Timers.Timer)sender;
			try
			{
				Window w = Window.Get;
				if ((w.Login_cbxRelogin.Checked || LoginStrategyManager.AutoRelogin) && !Bot.Get.Proxy.isRunning)
				{
					// Her tick tam 1 saniye: modulo kayması yok
					ReloginCountdown--;
					if(ReloginCountdown <= 0)
					{
						w.LogProcess("Relogin...");
						w.InvokeIfRequired(() => {
							w.Control_Click(w.Login_btnStart, null);
						});
						timer.Dispose();
					}
					else
					{
						w.LogProcess("Relogin at " + ReloginCountdown + " seconds...");
						timer.Start();
					}
				}
				else
				{
					w.LogProcess("Automatic relogin canceled!");
					timer.Dispose();
				}
			}
			catch (Exception ex)
			{
				Window.Get?.Log("[Relogin] " + ex.Message);
				try { timer.Dispose(); } catch { }
			}
		}
		/// <summary>
		/// Send packet to the server if exists connection (Gateway/Agent).
		/// </summary>
		/// <param name="p">Packet to inject</param>
		public void InjectToServer(Packet packet)
		{
			if (Agent != null && Agent.Remote != null && Agent.Remote.Socket.Connected)
			{
				Agent.InjectToServer(packet);
			}
			else if (Gateway != null && Gateway.Remote != null && Gateway.Remote.Socket.Connected)
			{
				Gateway.InjectToServer(packet);
			}
		}
		/// <summary>
		/// Send packet to the client if exists connection (Gateway/Agent).
		/// </summary>
		/// <param name="p">Packet to inject</param>
		public void InjectToClient(Packet packet)
		{
			if (Agent != null && Agent.Local != null && Agent.Local.Socket.Connected)
			{
				Agent.InjectToClient(packet);
			}
			else if (Gateway != null && Gateway.Local != null && Gateway.Local.Socket.Connected)
			{
				Gateway.InjectToClient(packet);
			}
		}
	}

	/// <summary>
	/// Manages global SOCKS5 proxy configuration and resolution with account overrides.
	/// </summary>
	public static class Socks5Config
	{
		public static bool Enabled { get; set; } = false;
		public static string Host { get; set; } = string.Empty;
		public static ushort Port { get; set; } = 1080;
		public static string Username { get; set; } = string.Empty;
		public static string Password { get; set; } = string.Empty;

		/// <summary>
		/// Resolves the effective proxy settings, prioritizing selected account's proxy if configured.
		/// </summary>
		public static bool GetEffectiveProxy(out string host, out ushort port, out string username, out string password)
		{
			SavedAccount activeAcc = AccountManager.GetAccount(AccountManager.SelectedAccountUsername);
			if (activeAcc != null && activeAcc.UseProxy && !string.IsNullOrWhiteSpace(activeAcc.ProxyHost))
			{
				host = activeAcc.ProxyHost.Trim();
				port = activeAcc.ProxyPort > 0 ? activeAcc.ProxyPort : (ushort)1080;
				username = activeAcc.ProxyUsername ?? string.Empty;
				password = activeAcc.ProxyPassword ?? string.Empty;
				return true;
			}
			if (Enabled && !string.IsNullOrWhiteSpace(Host))
			{
				host = Host.Trim();
				port = Port > 0 ? Port : (ushort)1080;
				username = Username ?? string.Empty;
				password = Password ?? string.Empty;
				return true;
			}
			host = string.Empty;
			port = 0;
			username = string.Empty;
			password = string.Empty;
			return false;
		}

		public static Newtonsoft.Json.Linq.JObject ToJson()
		{
			var json = new Newtonsoft.Json.Linq.JObject();
			json["Enabled"] = Enabled;
			json["Host"] = Host;
			json["Port"] = Port;
			json["Username"] = Username;
			json["Password"] = Password;
			return json;
		}

		public static void FromJson(Newtonsoft.Json.Linq.JObject json)
		{
			if (json == null) return;
			if (json.ContainsKey("Enabled")) Enabled = (bool)json["Enabled"];
			if (json.ContainsKey("Host")) Host = (string)json["Host"] ?? string.Empty;
			if (json.ContainsKey("Port")) Port = (ushort)json["Port"];
			if (json.ContainsKey("Username")) Username = (string)json["Username"] ?? string.Empty;
			if (json.ContainsKey("Password")) Password = (string)json["Password"] ?? string.Empty;
		}
	}
}