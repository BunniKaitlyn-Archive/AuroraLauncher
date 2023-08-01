using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace AuroraLauncher
{
    class Program
    {
        private const string CLIENT_EXECUTABLE = "FortniteClient-Win64-Shipping.exe";

        // TODO: Figure out how to generate FLToken's without hardcoding them.
        private const string BE_TOKEN = "f7b9gah4h5380d10f721dd6a";
        private const string EAC_TOKEN = "10ga222d803bh65851660E3d";

        private static Process _clientProcess;
        private static byte _clientAnticheat; // 0 = None, 1 = BattlEye, 2 = EasyAntiCheat

        private static Win32.HandlerRoutine _handlerRoutine;

        static void Main(string[] args)
        {
            string formattedArgs = string.Join(" ", args);

            // Check if -CONSOLE exists in args (regardless of case) to enable/allocate console.
            if (formattedArgs.ToUpper().Contains("-CONSOLE"))
            {
                formattedArgs = Regex.Replace(formattedArgs, "-CONSOLE", string.Empty, RegexOptions.IgnoreCase);

                Win32.AllocConsole();
            }

            // Check if -FORCEBE exists in args (regardless of case) to force BattlEye.
            if (formattedArgs.ToUpper().Contains("-FORCEBE"))
            {
                formattedArgs = Regex.Replace(formattedArgs, "-FORCEBE", string.Empty, RegexOptions.IgnoreCase);

                _clientAnticheat = 1;
            }

            // Check if -FORCEEAC exists in args (regardless of case) to force EasyAntiCheat.
            if (formattedArgs.ToUpper().Contains("-FORCEEAC"))
            {
                formattedArgs = Regex.Replace(formattedArgs, "-FORCEEAC", string.Empty, RegexOptions.IgnoreCase);

                _clientAnticheat = 2;
            }

            // Check if the client exists in the current work path, if it doesn't, just exit.
            if (!File.Exists(CLIENT_EXECUTABLE))
            {
                Win32.AllocConsole();

                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine($"{CLIENT_EXECUTABLE}, not found.");
                Console.ReadKey();

                Console.ForegroundColor = ConsoleColor.Gray;

                return;
            }

            // Print message.
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("AuroraLauncher by Cyuubi");
            Console.ForegroundColor = ConsoleColor.Gray;

            // Initialize client process with start info.
            _clientProcess = new Process
            {
                StartInfo =
                {
                    FileName = CLIENT_EXECUTABLE,
                    Arguments = formattedArgs,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = false
                }
            };

            if (_clientAnticheat == 0) // None
                _clientProcess.StartInfo.Arguments += $" -noeac -nobe -fltoken=none";
            else if (_clientAnticheat == 1) // BattlEye
                _clientProcess.StartInfo.Arguments += $" -noeac -fromfl=be -fltoken={BE_TOKEN}";
            else if (_clientAnticheat == 2) // EasyAntiCheat
                _clientProcess.StartInfo.Arguments += $" -nobe -fromfl=eac -fltoken={EAC_TOKEN}";

            SwapLauncher(); // Swap launcher.

            _clientProcess.Start(); // Start client process.

            // Set console handler routine.
            _handlerRoutine = new Win32.HandlerRoutine(HandlerRoutineCallback);

            Win32.SetConsoleCtrlHandler(_handlerRoutine, true);

            // Setup an AsyncStreamReader for standard output.
            AsyncStreamReader asyncOutputReader = new AsyncStreamReader(_clientProcess.StandardOutput);

            asyncOutputReader.DataReceived += delegate (object sender, string data)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(data);
                Console.ForegroundColor = ConsoleColor.Gray;
            };

            asyncOutputReader.Start(); // Start our AsyncStreamReader

            _clientProcess.WaitForExit(); // We'll wait for the client process to exit, otherwise our launcher will just close instantly.

            SwapLauncher(); // Swap launcher, again.
        }

        private static void SwapLauncher()
        {
            // Swap to original launcher.
            if (File.Exists("FortniteLauncher.exe.original"))
            {
                File.Move("FortniteLauncher.exe", "FortniteLauncher.exe.custom");
                File.Move("FortniteLauncher.exe.original", "FortniteLauncher.exe");
            }

            // Swap to custom launcher.
            if (File.Exists("FortniteLauncher.exe.custom"))
            {
                File.Move("FortniteLauncher.exe", "FortniteLauncher.exe.original");
                File.Move("FortniteLauncher.exe.custom", "FortniteLauncher.exe");
            }
        }

        private static bool HandlerRoutineCallback(int dwCtrlType)
        {
            switch (dwCtrlType)
            {
                case 2:
                    if (!_clientProcess.HasExited)
                        _clientProcess.Kill();
                    break;
            }

            return false;
        }
    }
}
