using System;
using System.Reflection;
using System.Text;

namespace Services
{
    [ObfuscationAttribute(Exclude = false)]
    public sealed class CmdProcessSingleton : IDisposable
    {
        private CmdProcessSingleton()
        {
            process = new System.Diagnostics.Process();
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
        }

        private static System.Diagnostics.Process process = null;

        private static CmdProcessSingleton instance = null;
        public static CmdProcessSingleton Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new CmdProcessSingleton();
                }
                return instance;
            }
        }

        public string ExecuteCommand(string argument)
        {
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            process.StartInfo.Arguments = argument;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.Start();
            StringBuilder result = new StringBuilder();
            try
            {
                while (!process.HasExited)
                {
                    result.Append(process.StandardOutput.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            //watch.Stop();

            //System.Console.WriteLine("Command {0} takes {1}ms", argument, watch.ElapsedMilliseconds);
#if DEBUG
            Console.WriteLine("{0}. Result: {1}", argument, result.ToString());
#endif
            return result.ToString();
        }


        public void Dispose()
        {
            process.Dispose();
        }
    }
}
