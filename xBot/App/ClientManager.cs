using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace xBot.App
{
    public static class ClientManager
    {
        #region Constants & Win32 API
        public const uint CREATE_SUSPENDED = 0x00000004;
        public const uint MEM_COMMIT = 0x00001000;
        public const uint MEM_RESERVE = 0x00002000;
        public const uint MEM_RELEASE = 0x00008000;
        public const uint PAGE_READWRITE = 0x04;

        public const uint SW_HIDE = 0x00;
        public const uint SW_SHOW = 0x05;

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [DllImport("user32.dll")]
        public static extern int ShowWindow(IntPtr hwnd, uint nCmdShow);

        [DllImport("user32.dll")]
        public static extern int SetWindowText(IntPtr hWnd, string text);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation
        );

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateMutex(IntPtr lpMutexAttributes, bool bInitialOwner, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            uint nSize,
            out IntPtr lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandleW(string lpModuleName);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            IntPtr lpThreadId
        );

        [DllImport("kernel32.dll")]
        public static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            uint flAllocationType,
            uint flProtect
        );

        [DllImport("kernel32.dll")]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtectEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            UIntPtr dwSize,
            uint flNewProtect,
            out uint lpflOldProtect
        );

        [DllImport("kernel32.dll")]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);
        #endregion

        private static IntPtr _launcherMutex = IntPtr.Zero;
        private static IntPtr _readyMutex = IntPtr.Zero;

        private static Process _process;
        public static Process CurrentProcess => _process;
        public static bool IsRunning => _process != null && !_process.HasExited;

        private static bool SafeWriteMemory(IntPtr hProcess, IntPtr address, byte[] data)
        {
            uint oldProtect = 0;
            bool vp = VirtualProtectEx(hProcess, address, (UIntPtr)data.Length, 0x40 /* PAGE_EXECUTE_READWRITE */, out oldProtect);
            IntPtr bytesWritten;
            bool wpm = WriteProcessMemory(hProcess, address, data, (uint)data.Length, out bytesWritten);
            if (vp)
            {
                uint dummy;
                VirtualProtectEx(hProcess, address, (UIntPtr)data.Length, oldProtect, out dummy);
            }
            return wpm && bytesWritten.ToInt64() == data.Length;
        }

        private static void LogInfo(string msg)
        {
            try
            {
                Window w = Window.Get;
                if (w != null)
                {
                    w.LogProcess(msg);
                    w.Log("[Loader] " + msg);
                }
            }
            catch { }
        }

        private static void LogWarn(string msg)
        {
            try
            {
                Window w = Window.Get;
                if (w != null)
                {
                    w.LogProcess(msg, Window.ProcessState.Warning);
                    w.Log("[Loader Warning] " + msg);
                }
            }
            catch { }
        }

        private static void LogError(string msg)
        {
            try
            {
                Window w = Window.Get;
                if (w != null)
                {
                    w.LogProcess(msg, Window.ProcessState.Error);
                    w.Log("[Loader Error] " + msg);
                }
            }
            catch { }
        }

        public static void WriteAscii(this BinaryWriter writer, string value)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(value ?? string.Empty);
            writer.Write(buffer.Length);
            writer.Write(buffer);
        }

        /// <summary>
        /// Start Silkroad game client with Detours injection and optional XIGNCODE patching.
        /// </summary>
        public static Process Start(
            string clientPath,
            byte locale,
            int divisionIndex,
            int gatewayIndex,
            ushort redirectPort,
            List<string> gatewayHosts,
            ushort gatewayPort,
            string redirectIp = "127.0.0.1",
            bool isDebug = false)
        {
            if (!File.Exists(clientPath))
            {
                LogError($"Silkroad executable not found: {clientPath}");
                return null;
            }

            string libraryDllName = "Client.Library.dll";
            string assemblyDir = Path.GetDirectoryName(typeof(ClientManager).Assembly.Location) ?? AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(assemblyDir, libraryDllName);
            if (!File.Exists(fullPath))
            {
                fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, libraryDllName);
            }
            if (!File.Exists(fullPath))
            {
                fullPath = Path.Combine(Directory.GetCurrentDirectory(), libraryDllName);
            }

            if (!File.Exists(fullPath))
            {
                LogError($"Client library not found: {fullPath}");
                return null;
            }

            byte[] buffer = Encoding.Unicode.GetBytes(fullPath + "\0");
            uint pathLen = (uint)buffer.Length;

            string silkroadDirectory = Path.GetDirectoryName(clientPath);
            string args = $"/{locale} {divisionIndex} {gatewayIndex} 0";
            string commandLine = $"\"{clientPath}\" {args}";

            string originalPathEnv = Environment.GetEnvironmentVariable("PATH");
            string appBaseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            if (!string.IsNullOrEmpty(originalPathEnv) && !originalPathEnv.Contains(appBaseDir))
            {
                Environment.SetEnvironmentVariable("PATH", appBaseDir + ";" + originalPathEnv);
            }

            STARTUPINFO si = new STARTUPINFO();
            si.cb = (uint)Marshal.SizeOf(typeof(STARTUPINFO));
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

            try
            {
                if (_launcherMutex == IntPtr.Zero)
                    _launcherMutex = CreateMutex(IntPtr.Zero, false, "Silkroad Online Launcher");
                if (_readyMutex == IntPtr.Zero)
                    _readyMutex = CreateMutex(IntPtr.Zero, false, "Ready");

                LogInfo("Creating game client process (suspended)...");
                if (!CreateProcess(
                    null,
                    commandLine,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    CREATE_SUSPENDED,
                    IntPtr.Zero,
                    silkroadDirectory,
                    ref si,
                    out pi))
                {
                    LogError("Failed to create game client process.");
                    return null;
                }

                try
                {
                    PrepareTempConfigFile(pi.dwProcessId, redirectIp, redirectPort, gatewayHosts, gatewayPort, isDebug, false);
                    bool mainThreadResumedForPatch = false;

                    Process sroProcess = Process.GetProcessById((int)pi.dwProcessId);

                    if (RequiresXigncodePatch(locale, clientPath))
                    {
                        LogInfo("Checking and applying XIGNCODE patch...");
                        if (!ApplyXigncodePatch(sroProcess, pi))
                        {
                            LogError("XIGNCODE patch failed, terminating client.");
                            CleanupProcess(pi);
                            return null;
                        }

                        uint resumeResult = ResumeThread(pi.hThread);
                        if (resumeResult == uint.MaxValue)
                        {
                            LogWarn("Failed to resume main thread after XIGNCODE patch; continuing with suspended state.");
                        }
                        else
                        {
                            mainThreadResumedForPatch = true;
                        }
                    }

                    _process = sroProcess;

                    LogInfo("Injecting client library (Client.Library.dll)...");
                    if (!InjectClientLibrary(pi, buffer, pathLen))
                    {
                        LogError("DLL injection failed, terminating client.");
                        CleanupProcess(pi);
                        return null;
                    }

                    if (!mainThreadResumedForPatch)
                    {
                        ResumeThread(pi.hThread);
                    }

                    _process.Refresh();
                    if (_process.HasExited)
                    {
                        LogError($"Process exited immediately after start (exit code: 0x{_process.ExitCode:X})");
                        return null;
                    }

                    _process.EnableRaisingEvents = true;
                    _process.Exited += (s, e) =>
                    {
                        try
                        {
                            LogWarn($"Client process exited! Exit code: 0x{_process.ExitCode:X} ({_process.ExitCode})");
                        }
                        catch
                        {
                            LogWarn("Client process exited!");
                        }
                    };

                    LogInfo("Client started and hooked successfully.");
                    return _process;
                }
                catch (Exception ex)
                {
                    LogError($"Failed to initialize client: {ex.Message}");
                    CleanupProcess(pi);
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in ClientManager.Start: {ex.Message}");
                return null;
            }
        }

private static void PrepareTempConfigFile(
            uint processId,
            string redirectIp,
            ushort redirectPort,
            List<string> gatewayHosts,
            ushort gatewayPort,
            bool isDebug,
            bool randomizeMac = false)
        {
            try
            {
                string tmpConfigFile = Path.Combine(Path.GetTempPath(), $"xBot_{processId}.tmp");
                using (FileStream fs = new FileStream(tmpConfigFile, FileMode.Create, FileAccess.Write))
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    writer.Write(isDebug); // 1 byte
                    writer.WriteAscii(redirectIp); // int32 length + ASCII bytes
                    writer.Write(redirectPort); // 2 bytes WORD

                    int count = gatewayHosts != null ? gatewayHosts.Count : 0;
                    writer.Write(count); // int32

                    if (gatewayHosts != null)
                    {
                        foreach (string host in gatewayHosts)
                        {
                            writer.WriteAscii(host);
                        }
                    }

                    writer.Write(gatewayPort); // 2 bytes WORD
                    writer.Write(randomizeMac); // 1 byte - MAC randomization flag
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to prepare temp config file: {ex.Message}");
            }
        }

        private static bool InjectClientLibrary(PROCESS_INFORMATION pi, byte[] buffer, uint pathLen)
        {
            IntPtr handle = pi.hProcess;
            if (handle == IntPtr.Zero)
            {
                LogError("Process handle is invalid.");
                return false;
            }

            try
            {
                IntPtr kernelHandle = GetModuleHandleW("kernel32.dll");
                if (kernelHandle == IntPtr.Zero)
                {
                    LogError("Failed to get kernel32.dll handle.");
                    return false;
                }

                IntPtr loadLibAddr = GetProcAddress(kernelHandle, "LoadLibraryW");
                if (loadLibAddr == IntPtr.Zero)
                {
                    LogError("Failed to get LoadLibraryW address.");
                    return false;
                }

                IntPtr remotePath = VirtualAllocEx(
                    handle,
                    IntPtr.Zero,
                    pathLen,
                    MEM_COMMIT | MEM_RESERVE,
                    PAGE_READWRITE
                );

                if (remotePath == IntPtr.Zero)
                {
                    LogError("Failed to allocate memory in client process.");
                    return false;
                }

                try
                {
                    IntPtr bytesWritten;
                    if (!WriteProcessMemory(handle, remotePath, buffer, pathLen, out bytesWritten))
                    {
                        LogError("Failed to write DLL path to remote process.");
                        return false;
                    }

                    IntPtr remoteThread = CreateRemoteThread(
                        handle,
                        IntPtr.Zero,
                        0,
                        loadLibAddr,
                        remotePath,
                        0,
                        IntPtr.Zero
                    );

                    if (remoteThread == IntPtr.Zero)
                    {
                        LogError("Failed to create remote thread for LoadLibraryW.");
                        return false;
                    }

                    try
                    {
                        uint waitResult = WaitForSingleObject(remoteThread, 10000);
                        if (waitResult != 0)
                        {
                            LogError("LoadLibraryW timed out after 10 seconds.");
                            return false;
                        }

                        uint exitCode;
                        if (!GetExitCodeThread(remoteThread, out exitCode))
                        {
                            LogError("Failed to get remote thread exit code.");
                            return false;
                        }

                        if (exitCode == 0 || exitCode >= 0xC0000000)
                        {
                            LogError($"LoadLibraryW failed (exit code: 0x{exitCode:X}).");
                            return false;
                        }

                        LogInfo($"Client library injected successfully (module: 0x{exitCode:X}).");
                        return true;
                    }
                    finally
                    {
                        CloseHandle(remoteThread);
                    }
                }
                finally
                {
                    VirtualFreeEx(handle, remotePath, 0, MEM_RELEASE);
                }
            }
            catch (Exception ex)
            {
                LogError($"DLL injection exception: {ex.Message}");
                return false;
            }
        }

        private static bool RequiresXigncodePatch(byte locale, string clientPath)
        {
            if (locale == 8 || locale == 22 || locale == 56 || locale == 63)
                return true;

            if (!string.IsNullOrEmpty(clientPath))
            {
                string lower = clientPath.ToLowerInvariant();
                if (lower.Contains("turkey") || lower.Contains("silkroadtr") || lower.Contains("trsro") || lower.Contains("vtc") || lower.Contains("taiwan"))
                    return true;
            }
            return false;
        }

        private static Dictionary<string, string> LoadSignatures()
        {
            Dictionary<string, string> sigs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            string[] candidatePaths = new string[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client-signatures.cfg"),
                Path.Combine(Directory.GetCurrentDirectory(), "client-signatures.cfg")
            };

            foreach (string path in candidatePaths)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(path);
                        foreach (string line in lines)
                        {
                            string trimmed = line.Trim();
                            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#") || trimmed.StartsWith("//"))
                                continue;

                            string[] parts = trimmed.Split(new char[] { '=' }, 2);
                            if (parts.Length == 2)
                            {
                                string name = parts[0].Trim();
                                string sig = parts[1].Replace("\"", "").Trim();
                                if (!string.IsNullOrEmpty(sig))
                                    sigs[name] = sig;
                            }
                        }
                        if (sigs.Count > 0)
                            return sigs;
                    }
                    catch { }
                }
            }

            // Embedded fallbacks
            sigs["Turkey"] = "6A 00 68 78 18 43 01 68 8C 18 43 01";
            sigs["VTC_Game"] = "6A 00 68 F8 91 3F 01 68 0C 92 3F 01";
            sigs["Taiwan"] = "6A 00 68 30 58 43 01 68 44 58 43 01";

            return sigs;
        }

        private static bool ApplyXigncodePatch(Process process, PROCESS_INFORMATION pi)
        {
            try
            {
                Dictionary<string, string> signatures = LoadSignatures();
                if (signatures.Count == 0)
                {
                    LogError("No client signatures available for XIGNCODE patch.");
                    return false;
                }

                ResumeThread(pi.hThread);
                DateTime deadline = DateTime.UtcNow.AddSeconds(10);
                process.Refresh();
                while (process.MainModule == null && DateTime.UtcNow < deadline)
                {
                    Thread.Sleep(50);
                    process.Refresh();
                }
                SuspendThread(pi.hThread);

                if (process.MainModule == null)
                {
                    LogError("Process main module did not load within 10 seconds.");
                    return false;
                }

                byte[] moduleMemory = new byte[process.MainModule.ModuleMemorySize];
                IntPtr bytesRead;
                if (!ReadProcessMemory(
                    process.Handle,
                    process.MainModule.BaseAddress,
                    moduleMemory,
                    process.MainModule.ModuleMemorySize,
                    out bytesRead))
                {
                    LogError("Failed to read client memory for XIGNCODE patch.");
                    return false;
                }

                int baseAddress = process.MainModule.BaseAddress.ToInt32();
                IntPtr patchAddress = IntPtr.Zero;

                // Try signatures in order
                string[] priorityKeys = new string[] { "Turkey", "VTC_Game", "Taiwan" };
                foreach (string key in priorityKeys)
                {
                    if (signatures.TryGetValue(key, out string patternStr))
                    {
                        patchAddress = FindPattern(patternStr, moduleMemory, baseAddress);
                        if (patchAddress != IntPtr.Zero)
                        {
                            LogInfo($"Found matching XIGNCODE signature ({key}) at 0x{patchAddress.ToInt64():X}");
                            break;
                        }
                    }
                }

                if (patchAddress == IntPtr.Zero)
                {
                    LogWarn("XIGNCODE signature not found in module memory (client may not use XIGNCODE or uses unknown signature).");
                    return true;
                }

                byte[] patchJmp = new byte[] { 0xEB };
                byte[] patchNop5 = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 };

                // Magic offsetler: imza tabanından patch noktaları (client-signatures.cfg ile birlikte sürümlenir)
                const long OffJmp1 = -0x6F;
                const long OffJmp2 = 0x13;
                const long OffNop = 0x0C;
                const long OffJmp3 = 0x95;
                bool ok1 = SafeWriteMemory(pi.hProcess, (IntPtr)(patchAddress.ToInt64() + OffJmp1), patchJmp);
                bool ok2 = SafeWriteMemory(pi.hProcess, (IntPtr)(patchAddress.ToInt64() + OffJmp2), patchJmp);
                bool ok3 = SafeWriteMemory(pi.hProcess, (IntPtr)(patchAddress.ToInt64() + OffNop), patchNop5);
                bool ok4 = SafeWriteMemory(pi.hProcess, (IntPtr)(patchAddress.ToInt64() + OffJmp3), patchJmp);

                if (!ok1 || !ok2 || !ok3 || !ok4)
                {
                    LogError("Failed writing XIGNCODE memory patches.");
                    return false;
                }

                LogInfo("XIGNCODE memory patch applied successfully.");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"XIGNCODE patching exception: {ex.Message}");
                return false;
            }
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, byte?[]> s_patternCache = new System.Collections.Concurrent.ConcurrentDictionary<string, byte?[]>();
        private static IntPtr FindPattern(string stringPattern, byte[] buffer, int baseAddress)
        {
            try
            {
                byte?[] pattern = s_patternCache.GetOrAdd(stringPattern, key => {
                    string[] toks = key.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    byte?[] arr = new byte?[toks.Length];
                    for (int t = 0; t < toks.Length; t++)
                        arr[t] = toks[t] == "??" ? (byte?)null : byte.Parse(toks[t], NumberStyles.AllowHexSpecifier);
                    return arr;
                });

                int patternLength = pattern.Length;
                int searchLength = buffer.Length - patternLength;

                for (int i = 0; i < searchLength; i++)
                {
                    bool found = true;
                    for (int j = 0; j < patternLength; j++)
                    {
                        if (pattern[j].HasValue && buffer[i + j] != pattern[j].Value)
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                    {
                        return (IntPtr)(baseAddress + i);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Pattern search exception: {ex.Message}");
            }

            return IntPtr.Zero;
        }

        public static void Kill()
        {
            Process proc = _process;
            if (proc == null) return;

            try
            {
                if (!proc.HasExited)
                {
                    proc.Kill();
                    proc.WaitForExit(5000);
                }
            }
            catch { }
            finally
            {
                try { proc.Dispose(); } catch { }
                if (ReferenceEquals(_process, proc))
                    _process = null;
            }
        }

        public static void SetTitle(string title)
        {
            if (_process != null && _process.MainWindowHandle != IntPtr.Zero)
                SetWindowText(_process.MainWindowHandle, title);
        }

        public static bool IsClientHidden { get; private set; } = false;

        public static void HideClient()
        {
            SetVisible(false);
            IsClientHidden = true;
        }

        public static void ShowClient()
        {
            SetVisible(true);
            IsClientHidden = false;
        }

        public static void SetVisible(bool visible)
        {
            if (_process != null && _process.MainWindowHandle != IntPtr.Zero)
                ShowWindow(_process.MainWindowHandle, visible ? SW_SHOW : SW_HIDE);
            IsClientHidden = !visible;
        }

        private static void CleanupProcess(PROCESS_INFORMATION pi)
        {
            try
            {
                if (pi.hThread != IntPtr.Zero)
                    CloseHandle(pi.hThread);

                if (pi.hProcess != IntPtr.Zero)
                {
                    try
                    {
                        Process proc = Process.GetProcessById((int)pi.dwProcessId);
                        proc?.Kill();
                    }
                    catch { }

                    CloseHandle(pi.hProcess);
                }
            }
            catch { }
        }
    }
}
