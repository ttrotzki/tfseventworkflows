
namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{
    using System;
    using System.Activities;
    using System.Linq;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.Framework.Server;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Server;

    /// <summary>
    /// Gets the saved workitem and TFS data from WorkItemChangedEvent
    /// </summary>
    public class GetWorkItemChangedEventData : CodeActivity
    {
        /// <summary>
        /// Gets or sets the work item changed event.
        /// </summary>
        /// <value>The work item changed event.</value>
        [RequiredArgument]
        public InArgument<WorkItemChangedEvent> WorkItemChangedEvent { get; set; }

        /// <summary>
        /// Gets or sets the team foundation request context.
        /// </summary>
        /// <value>The team foundation request context.</value>
        [RequiredArgument]
        public InArgument<TeamFoundationRequestContext> TeamFoundationRequestContext { get; set; }

        /// <summary>
        /// Gets or sets the work item.
        /// </summary>
        /// <value>The work item.</value>
        public OutArgument<WorkItem> WorkItem { get; set; }

        /// <summary>
        /// Gets or sets the TFS collection URL.
        /// </summary>
        /// <value>The TFS collection URL.</value>
        public OutArgument<string> TFSCollectionUrl { get; set; }

        /// <summary>
        /// Gets a List of all fields changed on the current work item
        /// </summary>
        public OutArgument<ChangedFieldsType> ChangedFields { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            var requestContext = context.GetValue(this.TeamFoundationRequestContext);
            var workItemEvent = context.GetValue(this.WorkItemChangedEvent);
            int workItemId = workItemEvent.CoreFields.IntegerFields.First(k => k.Name == "ID").NewValue;
            string strVirtualDirectory = requestContext.ServiceHost.VirtualDirectory;
            if (requestContext.ServiceName == "Team Foundation JobAgent")
              strVirtualDirectory = "/tfs" + strVirtualDirectory;
            string serverUri = new Uri(workItemEvent.DisplayUrl).GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped) + strVirtualDirectory;
            WorkItemStore workItemStore = GetWorkitemStore(serverUri);
            var workItem = workItemStore.GetWorkItem(workItemId);

            context.SetValue(this.ChangedFields, workItemEvent.ChangedFields);
            context.SetValue(this.WorkItem, workItem);
            context.SetValue(this.TFSCollectionUrl, serverUri);
        }

        /// <summary>
        /// Gets the workitem store.
        /// </summary>
        /// <param name="serverUri">The server URI.</param>
        /// <returns></returns>
        private WorkItemStore GetWorkitemStore(string serverUri)
        {
            var collection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(serverUri));
            var wis = collection.GetService<WorkItemStore>();
            return wis;
        }
    }
}
