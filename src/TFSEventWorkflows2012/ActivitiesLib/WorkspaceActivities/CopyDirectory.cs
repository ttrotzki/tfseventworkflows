using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Activities;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib.Workspace_Activities
{
    using artiso.TFSEventWorkflows.LoggingLib;

    /// <summary>
    /// Copy directory
    /// </summary>
    public sealed class CopyDirectory : CodeActivity
    {
        private int filesCopied;

        /// <summary>
        /// Gets or sets the source folder.
        /// </summary>
        /// <value>The source folder.</value>
        [RequiredArgument]
        public InArgument<string> SourceFolder { get; set; }

        /// <summary>
        /// Gets or sets the target folder.
        /// </summary>
        /// <value>The target folder.</value>
        [RequiredArgument]
        public InArgument<string> TargetFolder { get; set; }

        /// <summary>
        /// Gets or sets the override exisiting files.
        /// </summary>
        /// <value>The override exisiting files.</value>
        [RequiredArgument]
        public InArgument<bool> OverrideExisitingFiles { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            string sourceFolder = context.GetValue(this.SourceFolder);
            string targetFolder = context.GetValue(this.TargetFolder);
            bool overrideExisitingFiles = context.GetValue(this.OverrideExisitingFiles);

            try
            {
                this.filesCopied = 0;
                this.CopyAll(sourceFolder, targetFolder, overrideExisitingFiles);
                LogExtensions.LogInfo(this, string.Format("{0} files copied from {1} to {2}", this.filesCopied, sourceFolder, targetFolder));
            }
            catch (Exception ex)
            {
                throw new Exception("Activity CopyDirectory:", ex);
            }
            
        }

        /// <summary>
        /// Copies all files and folders.
        /// </summary>
        /// <param name="sourceFolder">The source folder.</param>
        /// <param name="targetFolder">The target folder.</param>
        /// <param name="overrideExisitingFiles">if set to <c>true</c> [override exisiting files].</param>
        private void CopyAll(string sourceFolder, string targetFolder, bool overrideExisitingFiles)
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
                
                if (overrideExisitingFiles)
                {
                    File.Copy(file, dest, true);
                }
                else
                {
                    if (!File.Exists(dest))
                    {
                        File.Copy(file, dest, false);
                        this.filesCopied++;
                    }
                }
            }

            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(targetFolder, name);
                this.CopyAll(folder, dest, overrideExisitingFiles);
            }
        }
    }
}
