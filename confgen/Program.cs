using System;
using System.IO;

namespace Confgen
{
    class Program
    {
        static void Main(string[] args) {
            try {
                var help = new Help(System.Console.Out);

                if (help.AreHelpArguments(args)) {
                    help.PrintHelp();
                } else {
                    var masterFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*" + Console.MasterConfigExtension);

                    Console console = new Console();

                    if (masterFiles.Length > 0) {
                        foreach (string masterConfigFileName in masterFiles) {
                            console.BuildConfigFilesFromMaster(masterConfigFileName);
                        }
                    } else {
                        System.Console.WriteLine(String.Format("could not find any files with extension `{0}'", Console.MasterConfigExtension));
                    }
                }
            }
            catch (ConfgenException e)
            {
                System.Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                Environment.Exit(1);
            }
        }
    }
}
