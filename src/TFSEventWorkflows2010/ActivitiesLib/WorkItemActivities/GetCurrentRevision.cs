using System;
using System.Activities;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using artiso.TFSEventWorkflows.LoggingLib;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{
    public sealed class GetCurrentRevision : CodeActivity
    {
        /// <summary>
        /// Gets or sets the work item.
        /// </summary>
        /// <value>The work item.</value>
        [RequiredArgument]
        public InArgument<WorkItem> WorkItem { get; set; }

        /// <summary>
        /// Gets or sets the current revision.
        /// </summary>
        /// <value>The current revision.</value>
        [RequiredArgument]
        public OutArgument<WorkItem> CurrentRevision { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                var inWorkItem = context.GetValue<WorkItem>(WorkItem);
                context.SetValue(this.CurrentRevision, this.GetCurrentWIRevision(inWorkItem));
                LogExtensions.LogInfo(this, string.Format("Activity GetCurrentRevision: Latest revision of workitem {0} returned.", inWorkItem.Id));
            }
            catch (Exception ex)
            {
                throw new Exception("Activity GetCurrentRevision:", ex);
            }
        }

        /// <summary>
        /// Gets the current WI revision.
        /// </summary>
        /// <param name="workItem">The work item.</param>
        /// <returns></returns>
        private WorkItem GetCurrentWIRevision(WorkItem workItem)
        {
            WorkItem currentRevision = workItem.Store.GetWorkItem(workItem.Id, workItem.Revisions.Count-1);

            return currentRevision;
        }

    }
}
