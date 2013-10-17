using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace InnerDb.Tests
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            string nUnitPath = System.Configuration.ConfigurationManager.AppSettings["NUnitPath"];
            if (string.IsNullOrEmpty(nUnitPath)) {
                Console.WriteLine("The key 'NUnitPath' is missing from App.config. Tell it where nunit-console.exe is (including the EXE name).");
            } else {
                Console.WriteLine(string.Format("Running NUnit ({0}) ...\n", nUnitPath));
                
                Process p = new Process();
                p.StartInfo = new ProcessStartInfo(nUnitPath, Assembly.GetExecutingAssembly().Location)
                {
                    UseShellExecute = false
                };
                p.Start();
                p.WaitForExit();
            }
            Console.WriteLine("\nPress any key to end.");
            Console.ReadKey();
        }
    }
}
