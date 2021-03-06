﻿using System;
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
            if (isError)
            {
                LogExtensions.LogError(this, logMessage, null);
            }
            else
            {
                LogExtensions.LogInfo(this, logMessage);
            }
        }
    }
}
