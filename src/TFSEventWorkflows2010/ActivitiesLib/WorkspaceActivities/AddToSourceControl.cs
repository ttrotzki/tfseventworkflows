using System;
using System.IO;
using System.Linq;
using System.Activities;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib.Workspace_Activities
{

    public sealed class AddToSourceControl : CodeActivity
    {
        /// <summary>
        /// Gets or sets the workspace.
        /// </summary>
        /// <value>The workspace.</value>
        [RequiredArgument]
        public InArgument<Workspace> Workspace { get; set; }

        /// <summary>
        /// Gets or sets the directory to upload.
        /// </summary>
        /// <value>The directory to upload.</value>
        [RequiredArgument]
        public InArgument<string> DirectoryToUpload { get; set; }

        /// <summary>
        /// Gets or sets the check in comment.
        /// </summary>
        /// <value>The check in comment.</value>
        public InArgument<string> CheckInComment { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                string directoryToUpload = context.GetValue(this.DirectoryToUpload);
                string checkInComment = context.GetValue(this.CheckInComment);
                Workspace workspace = context.GetValue(this.Workspace);
                string localWorkspacePath = workspace.Folders[0].LocalItem;

                workspace.Get();
                workspace.PendEdit(localWorkspacePath, RecursionType.Full);
                this.CopyAll(directoryToUpload, localWorkspacePath);
                workspace.PendAdd(localWorkspacePath, true);
                PendingChange[] pendingChanges = workspace.GetPendingChanges();

                if (pendingChanges.Count() > 0)
                {
                    workspace.CheckIn(pendingChanges, checkInComment);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Activity AddToSourceControl:", ex);
            }
        }

        /// <summary>
        /// Copies all files and folders.
        /// </summary>
        /// <param name="sourceFolder">The source folder.</param>
        /// <param name="targetFolder">The target folder.</param>
        private void CopyAll(string sourceFolder, string targetFolder)
        {
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(targetFolder, name);
                File.Copy(file, dest, true);
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(targetFolder, name);
                this.CopyAll(folder, dest);
            }
        }
    }
}
