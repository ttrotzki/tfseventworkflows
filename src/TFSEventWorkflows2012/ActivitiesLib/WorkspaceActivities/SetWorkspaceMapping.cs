using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib.Workspace_Activities
{
    /// <summary>
    /// Sets the workspace mapping
    /// </summary>
    public sealed class SetWorkspaceMapping : CodeActivity
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
        /// Gets or sets the local path.
        /// </summary>
        /// <value>The local path.</value>
        [RequiredArgument]
        public InArgument<string> LocalPath { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                Workspace workspace = context.GetValue(this.Workspace);
                string serverPath = context.GetValue(this.ServerPath);
                string localPath = context.GetValue(this.LocalPath);
                workspace.Map(serverPath, localPath);
            }
            catch (Exception ex)
            {
                throw new Exception("Activity SetWorkspaceMapping:", ex);
            }
        }
    }
}
