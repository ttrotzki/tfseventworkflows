using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{

    public class GetFromSourceControl : CodeActivity
    {
        /// <summary>
        /// Gets or sets the workspace.
        /// </summary>
        /// <value>The workspace.</value>
        [RequiredArgument]
        public InArgument<Workspace> Workspace { get; set; }

        /// <summary>
        /// Gets or sets the server path.
        /// </summary>
        /// <value>The server path.</value>
        [RequiredArgument]
        public InArgument<string> ServerPath { get; set; }


        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                string serverPath = context.GetValue(this.ServerPath);
                Workspace workspace = context.GetValue(this.Workspace);
                workspace.Get(new string[] { serverPath }, VersionSpec.Latest, RecursionType.Full, GetOptions.GetAll);
            }
            catch (Exception ex)
            {
                throw new Exception("Activity GetFromSourceControl:", ex);
            }
        }
    }
}
