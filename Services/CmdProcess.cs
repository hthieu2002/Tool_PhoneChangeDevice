using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Services
{
    public class CmdProcess
    {
        public static string ExecuteCommand(string argument, int timeout = 0)
        {
            using (var process = new Process())
            {
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.StartInfo.Arguments = argument;
                if (timeout == 0)
                {
                    process.Start();
                    StringBuilder result = new StringBuilder();
                    try
                    {
                        while (!process.HasExited)
                        {
                            result.Append(process.StandardOutput.ReadToEnd());
                        }
                        process.WaitForExit();
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
                else
                {
                    using (var outputWaitHandle = new AutoResetEvent(false))
                    {
                        using (var errorWaitHandle = new AutoResetEvent(false))
                        {
                            return HandleOutput(process, outputWaitHandle, errorWaitHandle, timeout, false);
                        }
                    }
                }
            }
        }
        public static string ExecuteCommandRootEx(string argument, int timeoutMs = 0)
        {
            string adbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "adb.exe");

            var psi = new ProcessStartInfo
            {
                FileName = adbPath,
                Arguments = argument,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            var outputSb = new StringBuilder();
            var errorSb = new StringBuilder();

            using (var process = new Process { StartInfo = psi, EnableRaisingEvents = true })
            using (var outputWait = new AutoResetEvent(false))
            using (var errorWait = new AutoResetEvent(false))
            {
                process.OutputDataReceived += (s, e) =>
                {
                    if (e.Data == null) outputWait.Set(); else outputSb.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data == null) errorWait.Set(); else errorSb.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                bool exited = timeoutMs > 0
                    ? process.WaitForExit(timeoutMs)
                    : (process.WaitForExit(int.MaxValue), true).Item2; // effectively infinite

                // Đảm bảo đọc hết buffer
                outputWait.WaitOne(TimeSpan.FromSeconds(2));
                errorWait.WaitOne(TimeSpan.FromSeconds(2));

                if (!exited)
                {
                    try { process.Kill(true); } catch { /* ignore */ }
                    throw new TimeoutException($"adb timeout: {argument}");
                }

                // Gộp stderr để bạn nhìn rõ lỗi (Permission denied, v.v.)
                if (errorSb.Length > 0)
                {
                    outputSb.AppendLine(errorSb.ToString());
                }
                return outputSb.ToString();
            }
        }

        public static string ExecuteCommandRoot(string argument, int timeout = 0)
        {
            string adbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "adb.exe");

            using (var process = new Process())
            {
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.FileName = adbPath;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.StartInfo.Arguments = argument;

                if (timeout == 0)
                {
                    process.Start();
                    StringBuilder result = new StringBuilder();
                    try
                    {
                        while (!process.HasExited)
                        {
                            result.Append(process.StandardOutput.ReadToEnd());
                        }
                        process.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

#if DEBUG
                    Console.WriteLine("{0}. Result: {1}", argument, result.ToString());
#endif
                    return result.ToString();
                }
                else
                {
                    using (var outputWaitHandle = new AutoResetEvent(false))
                    {
                        using (var errorWaitHandle = new AutoResetEvent(false))
                        {
                            return HandleOutput(process, outputWaitHandle, errorWaitHandle, timeout, false);
                        }
                    }
                }
            }
        }

        public static byte[] ExecuteCommandByteReturn(string argument, int timeout = 0)
        {
            using (var process = new Process())
            {
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.StartInfo.Arguments = argument;
                byte[] buffer = new byte[4000000];
                int length = 0;
                if (timeout == 0)
                {
                    process.Start();
                    try
                    {
                        while (!process.HasExited)
                        {

                            int lengthread = process.StandardOutput.BaseStream.Read(buffer, length, 1000000);
                            length = length + lengthread;
                        }

                        process.WaitForExit();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return null;
                    }
                    //watch.Stop();

                    //System.Console.WriteLine("Command {0} takes {1}ms", argument, watch.ElapsedMilliseconds);
#if DEBUG
                    // Console.WriteLine("{0}. Result: {1}", argument, result.ToString());
#endif
                    return buffer;
                }
                else
                {
                    using (var outputWaitHandle = new AutoResetEvent(false))
                    {
                        using (var errorWaitHandle = new AutoResetEvent(false))
                        {
                            return null;
                        }
                    }
                }
            }
        }
        private static string HandleOutput(Process p, AutoResetEvent outputWaitHandle, AutoResetEvent errorWaitHandle, int timeout, bool forceRegular)
        {
            var output = new StringBuilder();
            var error = new StringBuilder();
            try
            {
                p.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        try
                        {
                            outputWaitHandle.Set();
                        }
                        catch
                        {
                            //ignored
                        }
                    }
                    else
                    {
                        try
                        {
                            output.AppendLine(e.Data);
                        }
                        catch
                        {
                            //ignored
                        }
                    }
                };

                p.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        try
                        {
                            errorWaitHandle.Set();
                        }
                        catch
                        {
                            //ignored
                        }
                    }
                    else
                    {
                        try
                        {
                            error.AppendLine(e.Data);
                        }
                        catch
                        {
                            //ignored
                        }
                    }
                };

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                if (p.WaitForExit(timeout) && outputWaitHandle.WaitOne(timeout) && errorWaitHandle.WaitOne(timeout))
                {
                    string strReturn;

                    if (error.ToString().Trim().Length.Equals(0) || forceRegular)
                    {
                        strReturn = output.ToString().Trim();
                    }
                    else
                    {
                        strReturn = error.ToString().Trim();
                    }

                    return strReturn;
                }
                // Timed out.
                return "PROCESS TIMEOUT";
            }
            catch (Exception ex)
            {
                //return "PROCESS TIMEOUT";
                return ex.Message;
            }
        }
    }
}
