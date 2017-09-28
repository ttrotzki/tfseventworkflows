
namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{
    using System;
    using System.Activities;
    using System.Diagnostics;

    using artiso.TFSEventWorkflows.LoggingLib;

    public sealed class ExecuteProgrammWithArguments : CodeActivity
    {
        /// <summary>
        /// Gets or sets the programm to execute.
        /// </summary>
        /// <value>The programm to execute.</value>
        [RequiredArgument]
        public InArgument<string> ProgrammToExecute { get; set; }

        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        /// <value>The arguments.</value>
        [RequiredArgument]
        public InArgument<string> Arguments { get; set; }

        /// <summary>
        /// Gets or sets the wait time in ms.
        /// </summary>
        /// <value>The wait time in ms.</value>
        [RequiredArgument]
        public InArgument<int> WaitTimeInMs { get; set; }

        /// <summary>
        /// Gets or sets if the window is hidden
        /// </summary>
        /// <value>The hide window.</value>
        [RequiredArgument]
        public InArgument<bool> HideWindow { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            var programmToExecute = context.GetValue(this.ProgrammToExecute);
            var argument = context.GetValue(this.Arguments);
            var waitTimeInMs = context.GetValue(this.WaitTimeInMs);
            var hideWindow = context.GetValue(this.HideWindow);

            try
            {
                ExecuteCmd(programmToExecute, argument, waitTimeInMs, hideWindow);
                LogExtensions.LogInfo(this, string.Format("Programm {0} executed with arguments: {1}", programmToExecute, argument));
            }
            catch (Exception ex)
            {
                throw new Exception("Activity ExecuteProgrammWithArguments:", ex);
            }
        }

        private static void ExecuteCmd(string programmToExecute, string argument, int waitTimeInMs, bool hideWindow)
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo();

            startInfo.WindowStyle = hideWindow ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal;
            startInfo.FileName = programmToExecute;
            startInfo.Arguments = argument;
            process.StartInfo = startInfo;
            process.StartInfo.UseShellExecute = false;
            process.Start();

            process.WaitForExit(waitTimeInMs);
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
    }
}
