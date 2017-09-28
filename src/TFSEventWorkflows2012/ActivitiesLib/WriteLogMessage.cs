using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using artiso.TFSEventWorkflows.LoggingLib;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{
    public sealed class WriteLogMessage : CodeActivity
    {
        [RequiredArgument]
        public InArgument<bool> IsError { get; set; }

        [RequiredArgument]
        public InArgument<string> LogMessage { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            bool isError = context.GetValue(this.IsError);
            string logMessage = context.GetValue(this.LogMessage);
            WriteMessage(logMessage, isError);
        }

        public static void WriteMessage(string logMessage, bool isError)
        {
            if (isError)
            {
                LogExtensions.LogError(typeof(WriteLogMessage), logMessage, null);
            }
            else
            {
                LogExtensions.LogInfo(typeof(WriteLogMessage), logMessage);
            }            
        }
    }
}
