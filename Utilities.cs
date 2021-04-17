using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GitAddedModifiedFilesBackup
{
    public class Utilities
    {
        public static string GetFolderPath(string description = "") {

            FolderBrowserDialog folderDlg = new FolderBrowserDialog();

            if(!string.IsNullOrWhiteSpace(description))
                folderDlg.Description = description;
            
            folderDlg.ShowNewFolderButton = true;

            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                Environment.SpecialFolder root = folderDlg.RootFolder;
            }
            else
            {
                Environment.Exit(0);
            }

            return folderDlg.SelectedPath;
        }
    }
}
