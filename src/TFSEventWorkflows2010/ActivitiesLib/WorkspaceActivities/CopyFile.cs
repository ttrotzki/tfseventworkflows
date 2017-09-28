using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Activities;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib.Workspace_Activities
{

    /// <summary>
    /// Copy file
    /// </summary>
    public sealed class CopyFile : CodeActivity
    {
        /// <summary>
        /// Gets or sets the source path.
        /// </summary>
        /// <value>The source path.</value>
        [RequiredArgument]
        public InArgument<string> SourcePath { get; set; }

        /// <summary>
        /// Gets or sets the target path.
        /// </summary>
        /// <value>The target path.</value>
        [RequiredArgument]
        public InArgument<string> TargetPath { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                string sourcePath = context.GetValue(this.SourcePath);
                var sourceFile = new FileInfo(sourcePath);
                string targetPath = context.GetValue(this.TargetPath);
                string targetFile = Path.Combine(targetPath, sourceFile.Name);

                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                File.Copy(sourcePath, targetFile, true);
            }
            catch (Exception ex)
            {
                throw new Exception("Activity CopyFile:", ex);
            }
        }
    }
}
