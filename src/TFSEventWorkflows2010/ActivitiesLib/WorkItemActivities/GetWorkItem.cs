using System;
using System.Activities;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using artiso.TFSEventWorkflows.LoggingLib;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{

    public sealed class GetWorkItem : CodeActivity
    {
        /// <summary>
        /// Gets or sets the TFS collection URL.
        /// </summary>
        /// <value>The TFS collection URL.</value>
        [RequiredArgument]
        public InArgument<string> TFSCollectionUrl { get; set; }

        /// <summary>
        /// Gets or sets the work item ID.
        /// </summary>
        /// <value>The work item ID.</value>
        [RequiredArgument]
        public InArgument<int> WorkItemID { get; set; }

        /// <summary>
        /// Gets or sets the work item.
        /// </summary>
        /// <value>The work item.</value>
        [RequiredArgument]
        public OutArgument<WorkItem> WorkItem { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {

            string serverUri = context.GetValue(this.TFSCollectionUrl);
            int workItemID = context.GetValue(this.WorkItemID);
            try
            {
                var collection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(serverUri));
                var workItemStore = collection.GetService<WorkItemStore>();
                context.SetValue(this.WorkItem, workItemStore.GetWorkItem(workItemID));
                LogExtensions.LogInfo(this, string.Format("Activity GetWorkItem: Workitem {0} loaded.", workItemID));
            }
            catch (Exception ex)
            {
                throw new Exception("Activity GetWorkItem:", ex);
            }
        }
    }
}
