using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GitAddedModifiedFilesBackup
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            BackupAddedModifiedGitFiles backupModifiedGitFiles;

            if (args == null || args.Length < 1 ||string.IsNullOrWhiteSpace(args[0]))
            {
                backupModifiedGitFiles = new BackupAddedModifiedGitFiles();
            }
            else
            {
                backupModifiedGitFiles = new BackupAddedModifiedGitFiles(args[0]);
                backupModifiedGitFiles.IsScheduledTask = true;
            }

            backupModifiedGitFiles.StartBackup();
        }
    }
}
