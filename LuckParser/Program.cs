﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LuckParser
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(StandardHandle nStdHandle);
        [DllImport("kernel32.dll")]
        private static extern FileType GetFileType(IntPtr handle);
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private const int _attachParentProcess = -1;

        private enum StandardHandle : int
        {
            Input =-10,
            Output = -11,
            Error = -12
        }

        private enum FileType : uint
        {
            Unknown = 0x0000,
            Disk = 0x0001,
            Char = 0x0002,
            Pipe = 0x0003
        }

        private static bool IsRedirected(IntPtr handle)
        {
            FileType fileType = GetFileType(handle);

            return (fileType == FileType.Disk) || (fileType == FileType.Pipe);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            Application.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
            if (args.Length > 0)
            {
                /*
                 * We need to do this, because the console output is lazy initialized
                 * and if we are redirecting to a file or pipe we want to make sure Console.out points to the correct handle
                 * and doesnt init with the console ignoring existing stdout
                 */
                if (IsRedirected(GetStdHandle(StandardHandle.Output)))
                {
                    var dummy = Console.Out;
                }

                if (!AttachConsole(_attachParentProcess))
                {
                    AllocConsole();
                }

                int parserArgOffset = 0; 
                if (args[0] == "-c" && args.Length > 2)
                {
                    // Do not access settings before this, else this will not work
                    AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", args[1]);

                    parserArgOffset += 2;
                }
                else if (args[0] == "-c")
                {
                    Console.WriteLine("More arguments required for option -c:");
                    Console.WriteLine("GuildWars2EliteInsights.exe -c [config path] [logs]");
                }

                string[] parserArgs = new string[args.Length-parserArgOffset];
                for (int i = parserArgOffset; i < args.Length; i++)
                {
                    parserArgs[i - parserArgOffset] = args[i];
                }
                // Use the application through console 
                new ConsoleProgram(parserArgs);
                return 0;
            }

            SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            return 0;
        }
    }
}
