
namespace artiso.TFSEventWorkflows.TFSActivitiesLib.WorkspaceActivities
{
    using System.IO;

    using artiso.TFSEventWorkflows.LoggingLib;

    using System;
    using System.Activities;

    public sealed class DeleteFile : CodeActivity
    {
        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>The file path.</value>
        [RequiredArgument]
        public InArgument<string> FilePath { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            var filePath = context.GetValue(this.FilePath);
            try
            {
                File.Delete(filePath);
                LogExtensions.LogInfo(this, string.Format("File {0} deleted", filePath));
            }
            catch (Exception ex)
            {
                throw new Exception("Activity DeleteFile:", ex);
            }
        }
    }
}
