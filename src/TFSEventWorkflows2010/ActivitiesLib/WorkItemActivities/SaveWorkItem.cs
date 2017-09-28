using System;
using System.Text;
using System.Activities;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using artiso.TFSEventWorkflows.LoggingLib;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{

    public sealed class SaveWorkItem : CodeActivity
    {
        /// <summary>
        /// Gets or sets the work item.
        /// </summary>
        /// <value>The work item.</value>
        [RequiredArgument]
        public InArgument<WorkItem> WorkItem { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            WorkItem workItem = context.GetValue(this.WorkItem);

            try
            {
                workItem.Save();
                LogExtensions.LogInfo(this, string.Format("Activity SaveWorkItem: Workitem {0} has been saved.", workItem.Id));
            }
            catch (Exception ex)
            {
                throw new Exception("Activity SaveWorkItem:", ex);
            }
        }
    }
}
