﻿using Microsoft.Win32.SafeHandles;
using PortableRegistrator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace PortableRegistrator
{
    // https://social.msdn.microsoft.com/Forums/vstudio/en-US/415e37da-21ed-4f3f-acb2-98aef77b5c4a/i-want-to-show-console-output-in-my-cmd-prompt-in-c-winform-application?forum=csharpgeneral
    class CLI
    {
        enum Options { UNKNOWN, Help, Configuration }

        static Options Option = Options.UNKNOWN;
        static string OptionStr = null;
        static string ParameterName = null;
        static Dictionary<string, string> Parameters = null;

        internal static void Run(string[] args)
        {
            GUIConsoleWriter.RegisterGUIConsoleWriter();
            Console.WriteLine();

            // HACK: Delete first line of CMD output
            // this could be made a free-standing static method for multiple calls
            ClearCurrentConsoleLine(2);

            GetOptionsAndParameters(args);
            ProcessOptions();

            //Console.Write(blankline, 0, 80); // this will line-wrap at 80 so use Write()
            //Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
            SendKeys.SendWait("{ENTER}");
            Application.Exit();
        }

        internal static void GetOptionsAndParameters(string[] args)
        {
            OptionStr = args[1].ToLower();

            if (OptionStr == "?" || OptionStr == "h" || OptionStr == "/h" || OptionStr == "-h" ||
                OptionStr == "--help" || OptionStr == "/?" || OptionStr == "-?" || OptionStr == "--?")
                Option = Options.Help;
            else if (OptionStr == "c" || OptionStr == "-c" || OptionStr == "/c" || OptionStr == "--config")
                Option = Options.Configuration;

            if (args.Length == 3)
            {
                ParameterName = args[2].ToString();
            }
            else if (args.Length > 3)
            {
                string type = null;
                string value = null;

                for (int i = 2; i < args.Length; i += 2)
                {
                    try
                    {
                        type = args[i].ToString();
                        value = args[i + 1].ToString();

                        Parameters.Add(type, value);
                    }
                    catch (Exception ex)
                    {
                        //WriteLine(ex.Message);

                        //WriteLine();
                    }
                }
            }
        }
        internal static void ProcessOptions()
        {
            Console.WriteLine("ProcessOptions >> " + Option.ToString());
            ClearCurrentConsoleLine(1);

            switch (Option)
            {
                case Options.UNKNOWN:
                    PrintUnknown();
                    break;
                case Options.Help:
                    PrintHelp();
                    break;
                case Options.Configuration:
                    GetConfigration();
                    break;
            }
        }

        public static void ClearCurrentConsoleLine(int linesBack = 0)
        {
            try
            {
                Console.SetCursorPosition(0, Console.CursorTop - linesBack);
                int currentLineCursor = Console.CursorTop;
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, currentLineCursor);
            }
            catch (Exception)
            {
            }

        }

        private static void PrintUnknown()
        {
            Console.WriteLine();
            ClearCurrentConsoleLine();
            Console.WriteLine("UKNOWN OPTIONS!" + Environment.NewLine + "Use PortableRegistrator.exe /? ");
        }

        internal static void PrintHelp()
        {
            Console.WriteLine(@" ___              _         _    _       ___            _        _                _             ");
            Console.WriteLine(@"| . \ ___  _ _  _| |_  ___ | |_ | | ___ | . \ ___  ___ <_> ___ _| |_  _ _  ___  _| |_  ___  _ _ ");
            Console.WriteLine(@"|  _// . \| '_>  | |  <_> || . \| |/ ._>|   // ._>/ . || |<_-<  | |  | '_><_> |  | |  / . \| '_>");
            Console.WriteLine(@"|_|  \___/|_|    |_|  <___||___/|_|\___.|_\_\\___.\_. ||_|/__/  |_|  |_|  <___|  |_|  \___/|_|  ");
            Console.WriteLine(@"                                                  <___'                                         ");
            Console.WriteLine(@"================================================================================================");
            Console.WriteLine(@"                                 CLI - PARAMETER - OPTIONS                                      ");
            Console.WriteLine(@"================================================================================================");
            Console.WriteLine();
            Console.WriteLine(@"  Usage: PortableRegistrator [OPTION] [<AppType-NAME>]");
            Console.WriteLine("         PortableRegistrator c \"Generic Web-Browser\"");
            Console.WriteLine();
            Console.WriteLine(@"  ?, -?, /?, --help                      Show parameter options");
            Console.WriteLine(@"  c, -c, /c, --config <AppType-NAME>     Display AppType items from the configuration file,");
            Console.WriteLine(@"                                         use a string to search for containing AppType names");
            Console.WriteLine(@"");
            Console.WriteLine(@"================================================================================================");
        }
        internal static void GetConfigration()
        {
            var config = Configuration.Load();
            var items = config.AppTypes.Where(a => a.Name.ToLower().Contains(ParameterName.ToLower())).ToList();

            if (items.Count == 0)
            {
                Console.WriteLine();
                ClearCurrentConsoleLine();
                Console.WriteLine("No items found!");
                return;
            }

            foreach (var item in items)
            {
                var xml = Helper.XMLSerializer.Serialize(item);
                using (StringReader reader = new StringReader(xml))
                {
                    Console.WriteLine();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Console.WriteLine(line);
                    }
                }
            }
        }
    }

}
