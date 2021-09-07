using GitBackupUncommitedFiles;
using System;
using System.Collections.Generic;
using System.IO;
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

        public static ProgressWindow progressWindow = new ProgressWindow();

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
                    CurrentBranch = CmdRunCommands.RunCommands(new List<string> { GitRepositoryPath.Substring(0,2), $@"cd {GitRepositoryPath}", @"git branch --show-current" });

                    progressWindow.Hide();

                    string message = $"Discovered {GitAffectedFilesList.Count} added/modified file(s) proceed?";
                    string title = "Confirmation";
                    MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                    progressWindow.lblProgress.Text = "Waiting for backup confirmation....";
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
                        if (!IsScheduledTask)
                        {
                            progressWindow.lblProgress.Text = "Backing up uncommitted files....";
                            progressWindow.Show();
                        }
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
            
            if (!IsScheduledTask)
            {
                Application.EnableVisualStyles();
                progressWindow.lblProgress.Text = "Scanning for uncommitted files....";
                progressWindow.Show();
                progressWindow.BringToFront();
            }

            progressWindow.lblProgress.Text = "Waiting for repository path....";
            Console.WriteLine(GitRepositoryPath);
            progressWindow.lblProgress.Text = "Scanning for uncommitted files....";
            // Get added modified files list
            List<string> getAffectedFilesStrings = new List<string> {
                CmdRunCommands.RunCommands(new List<string> { GitRepositoryPath.Substring(0, 2), $@"cd {GitRepositoryPath}", @"git diff --cached --name-only --diff-filter=A" }),
                CmdRunCommands.RunCommands(new List<string> { GitRepositoryPath.Substring(0, 2), $@"cd {GitRepositoryPath}", @"git diff --cached --name-only --diff-filter=M" }),
                CmdRunCommands.RunCommands(new List<string> { GitRepositoryPath.Substring(0, 2), $@"cd {GitRepositoryPath}", @"git ls-files -m --others --exclude-standard" })
            };

            for (int i = 0; i < getAffectedFilesStrings.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(getAffectedFilesStrings[i]) && i < 2)
                {
                    GitAffectedFilesList.AddRange(getAffectedFilesStrings[i].Split('\n').ToList());
                }

                if(GitAffectedFilesList.Count == 0 && i > 1)
                {
                    GitAffectedFilesList.AddRange(getAffectedFilesStrings[i].Split('\n').ToList());
                }
            }
        }

        static void BackupAnotherGitRepository(bool success = false)
        {
            progressWindow.Hide();
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

            string backupName = "";
            progressWindow.lblProgress.Text = "Waiting for backup name....";

            if (Utilities.InputBox("Backup folder name", "What are you backing up?", ref backupName) == DialogResult.OK)
            {
                
            }
            else
            {
                Environment.Exit(0);
            }

            progressWindow.lblProgress.Text = "Backing up uncommitted files....";

            foreach (var gitModifiedFilePath in GitAffectedFilesList)
            {
                List<string> elements = gitModifiedFilePath.Split('\\').ToList();
                elements.RemoveAt(elements.Count - 1);
                gitModifiedFilesAbsolutePath.Add(string.Join("\\", elements).Replace(GitRepositoryName, $"Backups\\{(IsScheduledTask ? "{Scheduled\\GitRepositoryName}\\" : "")}{DateTime.Now.ToString("s").Replace(":", ".")}-[{backupName}][{GitRepositoryName}][{CurrentBranch.Replace("/", "-")}][backup]") + "\\");
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
