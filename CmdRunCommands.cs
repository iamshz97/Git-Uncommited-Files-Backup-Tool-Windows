using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitRepositoryWIPFilesBackup
{
    public class CmdRunCommands
    {
        public static string RunCommands(List<string> cmds, string workingDirectory = @"C:\Windows\System32\")
        {
            var cmdOut = string.Empty;

            var process = new Process();
            var psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.WorkingDirectory = workingDirectory;
            process.StartInfo = psi;
            process.Start();

            List<string> WhiteListStrings = new List<string> { "Microsoft", "cd", ">" };

            process.OutputDataReceived += (x, y) =>
            {
                if (!string.IsNullOrWhiteSpace(y.Data) && (WhiteListStrings.Any(y.Data.Contains) || cmds.Any(y.Data.Contains)))
                {

                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(y.Data) && y.Data != "\n")
                    {
                        cmdOut += !string.IsNullOrWhiteSpace(cmdOut) ? "\n" + y.Data : y.Data;
                    }

                }
            };
            //process.ErrorDataReceived += (x, y) => cmdOut += "\n" + y.Data;
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            using (StreamWriter sw = process.StandardInput)
            {
                foreach (var cmd in cmds)
                {
                    sw.WriteLine(cmd);
                }
            }
            process.WaitForExit();

            return cmdOut;
        }
    }
}
