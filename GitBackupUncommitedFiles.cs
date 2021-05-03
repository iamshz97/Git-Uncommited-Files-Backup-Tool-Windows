using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GitUncommitedFilesBackup
{
    public class GitBackupUncommitedFiles
    {
        public List<string> GitAffectedFilesList = new List<string>();

        public bool IsScheduledTask { get; set; } = false;

        public string GitRepositoryName { get; set; }

        public string _GitRepositoryPath { get; set; }

        public string CurrentBranch { get; set; }

        public string CopiedDirectory { get; set; }

        public string GitRepositoryPath 
        {

            get
            {
                if (string.IsNullOrWhiteSpace(_GitRepositoryPath))
                {
                    GitRepositoryPath = Utilities.GetFolderPath("Select a valid Git Repository");
                }
                return _GitRepositoryPath;
            }
            set 
            {
                _GitRepositoryPath = value;
                GitRepositoryName = _GitRepositoryPath.Split('\\').Last();
            }

        }

        public GitBackupUncommitedFiles()
        {
            
        }

        public GitBackupUncommitedFiles(string gitRepositoryPath)
        {
            GitRepositoryPath = gitRepositoryPath;
        }

        public void StartBackup() 
        {

            try
            {
                GetAffectedFiles();

                if (GitAffectedFilesList.Count >= 1)
                {
                    CurrentBranch = CmdRunCommands.RunCommands(new List<string> { $@"cd {GitRepositoryPath}", @"git branch --show-current" });

                    string message = $"Discovered {GitAffectedFilesList.Count} added/modified file(s) proceed?";
                    string title = "Confirmation";
                    MessageBoxButtons buttons = MessageBoxButtons.YesNo;

                    DialogResult result2;
                    if (!IsScheduledTask)
                    {
                        result2 = MessageBox.Show(message, title, buttons, MessageBoxIcon.Question);
                    }
                    else
                    {
                        result2 = DialogResult.Yes;
                    }

                    if (result2 == DialogResult.Yes)
                    {
                        BackupFiles();
                    }
                    else
                    {
                        Environment.Exit(0);
                    }
                }
                else
                {
                    if (!IsScheduledTask)
                        BackupAnotherGitRepository(false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"An error has occured", MessageBoxButtons.OK ,MessageBoxIcon.Error);
            }

            if (!IsScheduledTask)
                BackupAnotherGitRepository(true);
        }

        public void GetAffectedFiles() 
        {
            GitAffectedFilesList.Clear();

            // Get added modified files list
            List<string> getAffectedFilesStrings = new List<string> {
                CmdRunCommands.RunCommands(new List<string> { $@"cd {GitRepositoryPath}", @"git diff --cached --name-only --diff-filter=A" }),
                CmdRunCommands.RunCommands(new List<string> { $@"cd {GitRepositoryPath}", @"git diff --cached --name-only --diff-filter=M" })
            };

            foreach (var getAffectedFilesString in getAffectedFilesStrings)
            {
                if (!string.IsNullOrWhiteSpace(getAffectedFilesString))
                {
                    GitAffectedFilesList.AddRange(getAffectedFilesString.Split('\n').ToList());
                }

            }
        }

        static void BackupAnotherGitRepository(bool success = false)
        {

            string message = success ? "Do you want to backup another git repository?" : "Seems like the previous chosen folder did not have any modified files.\n" +
                "Do you wish to backup another Git Repository?";
            string title = "Backup Another Git Repository";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, title, buttons, MessageBoxIcon.Question, new MessageBoxDefaultButton(), MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.Yes)
            {
                // Starts a new instance of the program itself
                System.Diagnostics.Process.Start(Application.ExecutablePath);

                // Closes the current process
                Environment.Exit(0);
            }
            else
            {
                Environment.Exit(0);
            }
        }

        public void BackupFiles() 
        {

            GitAffectedFilesList = GitAffectedFilesList.Select(s => $@"{GitRepositoryPath}\{s.Replace("/", "\\")}").ToList();

            List<string> gitModifiedFilesAbsolutePath = new List<string>();

            foreach (var gitModifiedFilePath in GitAffectedFilesList)
            {
                List<string> elements = gitModifiedFilePath.Split('\\').ToList();
                elements.RemoveAt(elements.Count - 1);
                gitModifiedFilesAbsolutePath.Add(string.Join("\\", elements).Replace(GitRepositoryName, $"Backups\\{(IsScheduledTask ? "{Scheduled\\GitRepositoryName}\\" : "")}{GitRepositoryName}.{CurrentBranch.Replace("/", "-")}.backup.{DateTime.Now:dddd.dd.MMMM.yyyy.HH.mm.ss}") + "\\");
            }

            List<string> copyCommands = new List<string>();
            if (GitAffectedFilesList.Count == gitModifiedFilesAbsolutePath.Count)
            {
                for (int i = 0; i < GitAffectedFilesList.Count; i++)
                {
                    copyCommands.Add($"xcopy \"{GitAffectedFilesList[i]}\" \"{gitModifiedFilesAbsolutePath[i]}\" ");
                }

                CmdRunCommands.RunCommands(copyCommands);
            }


            if (!IsScheduledTask)
            {
                string backupPath = GitRepositoryPath.Replace(GitRepositoryPath.Split('\\').Last(), "Backups");
                CmdRunCommands.RunCommands(new List<string> { $"explorer.exe \"{backupPath}\"" });
            }
        }
    }
}
