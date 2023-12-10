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

            // Processa os argumentos da janela de comando ou escreve as instruções
            ProcessArgs(args, out targetPID, out targetExe);

            if (targetPID <= 0 && string.IsNullOrEmpty(targetExe))
                return;

            EasyHook.RemoteHooking.IpcCreateServer<FuncoesAC.ServerInterface>(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton);

            //Informo o caminho completo da DLL que quero injetar no processo
            string injectionLibrary = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "AntiCheatDLL.dll");

            try
            {
                // Injetando no processo a partir do PID
                if (targetPID > 0)
                {
                    Console.WriteLine("Tentando injetar no processo: {0}", targetPID);

                    EasyHook.RemoteHooking.Inject(
                        targetPID,          // PID
                        injectionLibrary,   // Caminho para arquivo de 32 bits
                        injectionLibrary,   // Caminho para arquivo de 64 bits
                        channelName         // mais parametros passados para a injeção

                    // ...
                    );
                }
                // Cria um novo processo e injeto nele
                else if (!string.IsNullOrEmpty(targetExe))
                {
                    Console.WriteLine("Tentando criar e injetar no processo {0}", targetExe);
                    EasyHook.RemoteHooking.CreateAndInject(
                        targetExe,          // caminho para o executavel
                        "",                 // argumento para executar o processo na linha de comando
                        0,                  // flags adicionais para a criação do processo
                        EasyHook.InjectionOptions.DoNotRequireStrongName, // Permitir a injeção
                        injectionLibrary,   // Caminho para arquivo de 32 bits
                        injectionLibrary,   // Caminho para arquivo de 64 bits
                        out targetPID,      // Pega o PID do processo recem-criado
                        channelName         // mais parametros passados para a injeção
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

            // Carrega todos os parametros passados
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
