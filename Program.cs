using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GitUncommitedFilesBackup
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            GitBackupUncommitedFiles backupModifiedGitFiles;

            if (args == null || args.Length < 1 ||string.IsNullOrWhiteSpace(args[0]))
            {
                backupModifiedGitFiles = new GitBackupUncommitedFiles();
            }
            else
            {
                backupModifiedGitFiles = new GitBackupUncommitedFiles(args[0]);
                backupModifiedGitFiles.IsScheduledTask = true;
            }

            backupModifiedGitFiles.StartBackup();
        }
    }
}
