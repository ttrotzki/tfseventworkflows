using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib.Workspace_Activities
{

    /// <summary>
    /// Deletes the given workspace
    /// </summary>
    public sealed class DeleteWorkspace : CodeActivity
    {
        /// <summary>
        /// Gets or sets the workspace.
        /// </summary>
        /// <value>The workspace.</value>
        [RequiredArgument]
        public InArgument<Workspace> Workspace { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                Workspace workspace = context.GetValue(this.Workspace);
                workspace.Delete();
            }
            catch (Exception ex)
            {
                throw new Exception("Activity DeleteWorkspace:", ex);
            }
        }
    }
}
