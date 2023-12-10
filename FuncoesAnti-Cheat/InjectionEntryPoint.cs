
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace FuncoesAC
{
    public class InjectionEntryPoint: EasyHook.IEntryPoint
    {
        ServerInterface _server = null;
        Queue<string> _messageQueue = new Queue<string>();


        //DLLs Conhecidas
        private static readonly List<string> _whitelist = new List<string>
    {
        "zlib1.dll",
        "wtsapi32.dll",
        "ws2_32.dll",
        "wrap_oal.dll",
        "wow64win.dll",
        "wow64cpu.dll",
        "wow64con.dll",
        "wow64base.dll",
        "wow64.dll",
        "wldp.dll",
        "WinTypes.dll",
        "wintrust.dll",
        "winsta.dll",
        "winmmbase.dll",
        "winmm.dll",
        "Windows.UI.dll",
        "windows.storage.dll",
        "win32u.dll",
        "wdmaud.drv",
        "version.dll",
        "uxtheme.dll",
        "user32.dll",
        "KernelBase.dll",
        "kernel32.dll",
        "umpdc.dll",
        "ucrtbase.dll",
        "TextInputFramework.dll",
        "shlwapi.dll",
        "shell32.dll",
        "SHCore.dll",
        "setupapi.dll",
        "sechost.dll",
        "SDL_image.dll",
        "SDL.dll",
        "rpcrt4.dll",
        "ResourcePolicyClient.dll",
        "RESAMPLEDMO.DLL",
        "profapi.dll",
        "powrprof.dll",
        "opengl32.dll",
        "OpenAL32.dll",
        "oleaut32.dll",
        "ole32.dll",
        "ogg.dll",
        "nvspcap.dll",
        "nvoglv32.dll",
        "ntmarta.dll",
        "ntdll.dll",
        "ntdll.dll",
        "msvcrt.dll",
        "msvcp_win.dll",
        "msdmo.dll",
        "msctf.dll",
        "mscms.dll",
        "msasn1.dll",
        "msacm32.drv",
        "msacm32.dll",
        "MMDevAPI.dll",
        "midimap.dll",
        "libvorbisfile.dll",
        "libvorbis.dll",
        "libpng12-0.dll",
        "libintl3.dll",
        "libiconv2.dll",
        "ksuser.dll",
        "kernel.appcore.dll",
        "jpeg.dll",
        "IPHLPAPI.DLL",
        "imm32.dll",
        "glu32.dll",
        "gdi32full.dll",
        "gdi32.dll",
        "DXCore.dll",
        "dwmapi.dll",
        "dsound.dll",
        "drvstore.dll",
        "devobj.dll",
        "dbghelp.dll",
        "cryptnet.dll",
        "cryptbase.dll",
        "crypt32.dll",
        "CoreUIComponents.dll",
        "CoreMessaging.dll",
        "combase.dll",
        "clbcatq.dll",
        "cfgmgr32.dll",
        "bcryptprimitives.dll",
        "bcrypt.dll",
        "avrt.dll",
        "AudioSes.dll",
        "advapi32.dll",
        "DLL-Permitida.dll"
    };

        //estabeleço a comunicação do ipc
        public InjectionEntryPoint(EasyHook.RemoteHooking.IContext context, string channelName)
        {
         
            _server = EasyHook.RemoteHooking.IpcConnectClient<ServerInterface>(channelName);
            _server.Ping();
        }

        //lista de nomes de processos suspeitos
        private static readonly List<string> ForbiddenProcesses = new List<string>
        {
            "cheatengine",
            "cheat",
            "aim",
            "esp",
            "wall",
            "hacking",
            "form",
            "hack",
            "h4ck",
        };
        //verificar os processos rodando
        public static String IsCheatProcessRunning()
        {
            //validar o nome dos processos rodando no computador
            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes)
            {
                foreach (string forbiddenProcess in ForbiddenProcesses)
                {
                    if (process.ProcessName.ToLower().Contains(forbiddenProcess))
                    {
                        return process.ProcessName.ToLower(); // Um processo de trapaça foi encontrado
                    }
                }
            }

            return null; // Nenhum processo de trapaça foi encontrado
        }

        public void Run(EasyHook.RemoteHooking.IContext context, string channelName)
        {
            // Aviso que foi feita a conexão IPC
            _server.IsInstalled(EasyHook.RemoteHooking.GetCurrentProcessId());

            // Instalo os hooks
            var loadLibraryAHook = EasyHook.LocalHook.Create(
                EasyHook.LocalHook.GetProcAddress("kernel32.dll", "LoadLibraryA"),
                new LoadLibrary_Delegate(LoadLibraryA_Hook),
                this);
            var loadLibraryWHook = EasyHook.LocalHook.Create(
                EasyHook.LocalHook.GetProcAddress("kernel32.dll", "LoadLibraryW"),
                new LoadLibrary_Delegate(LoadLibraryW_Hook),
                this);

            // Ativa o hook em todas as threads, exceto na atual
            loadLibraryAHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            loadLibraryWHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });

            _server.ReportMessage("Hooks para LoadLibrary instalados no PID: " + EasyHook.RemoteHooking.GetCurrentProcessId(), false);

            // Abre o processo se utilizar o CreateandInject
            EasyHook.RemoteHooking.WakeUpProcess();

            try
            {
                //loop ate que se feche a aplicação ou encerre a comunicação
                while (true)
                {
                    System.Threading.Thread.Sleep(500);

                    string[] queued = null;
                    lock (_messageQueue)
                    {
                        queued = _messageQueue.ToArray();
                        _messageQueue.Clear();
                    }

                    if (queued != null && queued.Length > 0)
                    {
                        _server.ReportMessages(queued);
                    }
                    else
                    {
                        _server.Ping();
                    }

                    if (!string.IsNullOrEmpty(IsCheatProcessRunning()))
                    {
                        _server.ReportMessage("Processo de trapaça encontrado\nProcesso: " + IsCheatProcessRunning() + ".\n Encerrando o jogo...", true);

                        foreach (var p in Process.GetProcessesByName("ac_client"))
                        {
                            p.Kill();
                            p.WaitForExit();
                        }
                    }
                    Thread.Sleep(1000); // 1 segundo
                }
            }
            catch
            {
            }

            // Retiro os hooks
            loadLibraryAHook.Dispose();
            loadLibraryWHook.Dispose();
            EasyHook.LocalHook.Release();
        }

        //Defino como será o hook
        #region LoadLibrary Hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate IntPtr LoadLibrary_Delegate(string lpFileName);

        //chamada da função original na DLL
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr LoadLibraryA(string lpFileName);

        //função de Hook
        IntPtr LoadLibraryA_Hook(string lpFileName)
        {

            _server.ReportMessage("Tentativa de injeção de DLL!", false);
            _server.ReportMessage("Verificando se a DLL esta na lista de permitidas...", false);
            foreach (string item in _whitelist)
            {
                //verifico se a DLL que esta sendo injetada esta na whitelist
                if (lpFileName.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _server.ReportMessage("Nova DLL injetada: ", false);
                    _server.ReportMessage(lpFileName.ToString(), false);
                    IntPtr result = LoadLibraryA(lpFileName);
                    return result;
                }
                else
                {
                }

            }
            _server.ReportMessage(@"********************************************************", true);
            _server.ReportMessage("", true);
            _server.ReportMessage("Bloqueada tentativa de uso da LoadLibraryA para injeção de DLL desconhecida:", true);
            _server.ReportMessage(lpFileName.ToString(), true);
            _server.ReportMessage("", true);
            _server.ReportMessage(@"********************************************************", true);
            return IntPtr.Zero;
        } 


        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr LoadLibraryW(string lpFileName);


        IntPtr LoadLibraryW_Hook(string lpFileName)
        {

            _server.ReportMessage("Tentativa de injeção de DLL!", false);
            _server.ReportMessage("Verificando se a DLL esta na lista de permitidas...", false);
            foreach (string item in _whitelist)
            {
                
                if (lpFileName.ToLower().IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    _server.ReportMessage("Nova DLL injetada: ", false);
                    _server.ReportMessage(lpFileName.ToString(), false);
                    IntPtr result = LoadLibraryW(lpFileName);
                    return result;
                }
                else
                {
                }
            }
            _server.ReportMessage(@"********************************************************", true);
            _server.ReportMessage("", true);
            _server.ReportMessage("Bloqueada tentativa de uso da LoadLibraryW para injeção de DLL desconhecida:", true);
            _server.ReportMessage(lpFileName, true);
            _server.ReportMessage("", true);
            _server.ReportMessage(@"********************************************************", true);
            return IntPtr.Zero;
        }
        #endregion
    }
}
