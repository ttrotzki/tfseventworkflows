
namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{
    using System;
    using System.Activities;
    using System.IO;

    using artiso.TFSEventWorkflows.LoggingLib;

    public sealed class ReadTextFile : CodeActivity
    {
        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>The file path.</value>
        [RequiredArgument]
        public InArgument<string> FilePath { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        [RequiredArgument]
        public OutArgument<string> Text { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            var filePath = context.GetValue(this.FilePath);
            try
            {
                var text = File.ReadAllText(filePath);
                context.SetValue(this.Text, text);
                LogExtensions.LogInfo(this, string.Format("Read the content of file {0}", filePath));
            }
            catch (Exception ex)
            {
                throw new Exception("Activity ReadTextFile:", ex);
            }
        }
    }
}
