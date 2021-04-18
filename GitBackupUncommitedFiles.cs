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
        public List<string> GitModifiedFilesList = new List<string>();

        public bool IsScheduledTask { get; set; } = false;

        public string GitRepositoryName { get; set; }

        public string _GitRepositoryPath { get; set; }

        public string CurrentBranch { get; set; }

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
                GetModifiedFiles();

                if (GitModifiedFilesList.Count >= 1)
                {
                    CurrentBranch = CmdRunCommands.RunCommands(new List<string> { $@"cd {GitRepositoryPath}", @"git branch --show-current" });

                    string message = $"Discovered {GitModifiedFilesList.Count} modified file(s) proceed?";
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

        public void GetModifiedFiles() 
        {

            string gitModifiedFilesPathsString = CmdRunCommands.RunCommands(new List<string> { $@"cd {GitRepositoryPath}", @"git ls-files -m --others --exclude-standard" });

            if(!string.IsNullOrWhiteSpace(gitModifiedFilesPathsString))
            {
                GitModifiedFilesList.Clear();
                GitModifiedFilesList = gitModifiedFilesPathsString.Split('\n').ToList(); ;
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

            GitModifiedFilesList = GitModifiedFilesList.Select(s => $@"{GitRepositoryPath}\{s.Replace("/", "\\")}").ToList();

            List<string> gitModifiedFilesAbsolutePath = new List<string>();

            foreach (var gitModifiedFilePath in GitModifiedFilesList)
            {
                List<string> elements = gitModifiedFilePath.Split('\\').ToList();
                elements.RemoveAt(elements.Count - 1);
                gitModifiedFilesAbsolutePath.Add(string.Join("\\", elements).Replace(GitRepositoryName, $"backup\\{(IsScheduledTask ? "{GitRepositoryName}\\" : "")}{GitRepositoryName}.{CurrentBranch}.backup.{DateTime.Now:dddd.dd.MMMM.yyyy.HH.mm.ss}") + "\\");
            }

            List<string> copyCommands = new List<string>();
            if (GitModifiedFilesList.Count == gitModifiedFilesAbsolutePath.Count)
            {
                for (int i = 0; i < GitModifiedFilesList.Count; i++)
                {
                    copyCommands.Add($"xcopy \"{GitModifiedFilesList[i]}\" \"{gitModifiedFilesAbsolutePath[i]}\" ");
                }

                CmdRunCommands.RunCommands(copyCommands);
            }

        }
    }
}
