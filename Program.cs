using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GitRepositoryWIPFilesBackup
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            BackupModifiedGitFiles backupModifiedGitFiles;

            if (args == null || args.Length < 1 ||string.IsNullOrWhiteSpace(args[0]))
            {
                backupModifiedGitFiles = new BackupModifiedGitFiles();
                backupModifiedGitFiles.IsScheduledTask = true;
            }
            else
            {
                backupModifiedGitFiles = new BackupModifiedGitFiles(args[0]);
            }

            backupModifiedGitFiles.StartBackup();
        }
    }
}
