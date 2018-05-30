﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LuckParser
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            if (args.Length > 0)
            {
                AttachConsole(ATTACH_PARENT_PROCESS);
                foreach (String arg in args)
                {
                    //Console.WriteLine(arg);
                }
                // Use the application through console 
                Form1 myConsoleParser = new Form1(args);
                
                return 0;
            }
            Application.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            return 0;
        }
    }
}
