using System;
using System.IO;

namespace AntiCheat
{
    class Program
    {
        static void Main(string[] args)
        {
            Int32 targetPID = 0;
            string targetExe = null;
            string channelName = null;

            // Process command line arguments or print instructions and retrieve argument value
            ProcessArgs(args, out targetPID, out targetExe);

            if (targetPID <= 0 && string.IsNullOrEmpty(targetExe))
                return;

            // Create the IPC server using the FileMonitorIPC.ServiceInterface class as a singleton
            EasyHook.RemoteHooking.IpcCreateServer<FuncoesAC.ServerInterface>(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton);

            // Get the full path to the assembly we want to inject into the target process
            string injectionLibrary = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "AntiCheatDLL.dll");

            try
            {
                // Injecting into existing process by Id
                if (targetPID > 0)
                {
                    Console.WriteLine("Tentando injetar no processo: {0}", targetPID);

                    // inject into existing process
                    EasyHook.RemoteHooking.Inject(
                        targetPID,          // ID of process to inject into
                        injectionLibrary,   // 32-bit library to inject (if target is 32-bit)
                        injectionLibrary,   // 64-bit library to inject (if target is 64-bit)
                        channelName         // the parameters to pass into injected library
                                            // ...
                    );
                }
                // Create a new process and then inject into it
                else if (!string.IsNullOrEmpty(targetExe))
                {
                    Console.WriteLine("Tentando criar e injetar no processo {0}", targetExe);
                    // start and inject into a new process
                    EasyHook.RemoteHooking.CreateAndInject(
                        targetExe,          // executable to run
                        "",                 // command line arguments for target
                        0,                  // additional process creation flags to pass to CreateProcess
                        EasyHook.InjectionOptions.DoNotRequireStrongName, // allow injectionLibrary to be unsigned
                        injectionLibrary,   // 32-bit library to inject (if target is 32-bit)
                        injectionLibrary,   // 64-bit library to inject (if target is 64-bit)
                        out targetPID,      // retrieve the newly created process ID
                        channelName         // the parameters to pass into injected library
                                            // ...
                    );
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Houve um erro ao injetar no processo alvo:");
                Console.ResetColor();
                Console.WriteLine(e.ToString());
            }

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("");
            Console.WriteLine("<Pressione qualquer tecla para sair>");
            Console.WriteLine("");
            Console.ResetColor();
            Console.ReadKey();
        }

        static void ProcessArgs(string[] args, out int targetPID, out string targetExe)
        {
            targetPID = 0;
            targetExe = null;

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(@"                               
 █████╗ ███╗   ██╗████████╗██╗       ██████╗██╗  ██╗███████╗ █████╗ ████████╗
██╔══██╗████╗  ██║╚══██╔══╝██║      ██╔════╝██║  ██║██╔════╝██╔══██╗╚══██╔══╝
███████║██╔██╗ ██║   ██║   ██║█████╗██║     ███████║█████╗  ███████║   ██║   
██╔══██║██║╚██╗██║   ██║   ██║╚════╝██║     ██╔══██║██╔══╝  ██╔══██║   ██║   
██║  ██║██║ ╚████║   ██║   ██║      ╚██████╗██║  ██║███████╗██║  ██║   ██║   
╚═╝  ╚═╝╚═╝  ╚═══╝   ╚═╝   ╚═╝       ╚═════╝╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝   ╚═╝   
                                                                    
");

            Console.WriteLine(@"======================================*****======================================");
            Console.WriteLine("Monitor de trapaça");
            Console.WriteLine();
            Console.ResetColor();

            // Load any parameters
            while ((args.Length != 1) || !Int32.TryParse(args[0], out targetPID) || !File.Exists(args[0]))
            {
                if (targetPID > 0)
                {
                    break;
                }
                if (args.Length != 1 || !File.Exists(args[0]))
                {
                    Console.WriteLine("Modo de uso: Informe o ProcessID (PID)");
                    Console.WriteLine("        Ex.: 1234");
                    Console.WriteLine("             para monitorar um processo existente com PID 1234");
                    Console.WriteLine();
                    Console.WriteLine("Digite o PID do processo do jogo");
                    Console.Write("> ");

                    args = new string[] { Console.ReadLine() };

                    Console.WriteLine();

                    if (String.IsNullOrEmpty(args[0])) return;
                }
                else
                {
                    targetExe = args[0];
                    break;
                }
            }
        }
    }
}
