using System;
using System.Activities;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.Client;
using artiso.TFSEventWorkflows.LoggingLib;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{
    public sealed class CreateNewWorkItem : CodeActivity
    {
        /// <summary>
        /// Gets or sets the name of the team project.
        /// </summary>
        /// <value>The name of the team project.</value>
        [RequiredArgument]
        public InArgument<String> TeamProjectName { get; set; }

        /// <summary>
        /// Gets or sets the TFS collection URL.
        /// </summary>
        /// <value>The TFS collection URL.</value>
        [RequiredArgument]
        public InArgument<String> TFSCollectionUrl { get; set; }

        /// <summary>
        /// Gets or sets the type of the work item.
        /// </summary>
        /// <value>The type of the work item.</value>
        [RequiredArgument]
        public InArgument<String> WorkItemType { get; set; }

        /// <summary>
        /// Gets or sets the new work item.
        /// </summary>
        /// <value>The new work item.</value>
        [RequiredArgument]
        public OutArgument<WorkItem> NewWorkItem { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                var inServerUri = context.GetValue<string>(this.TFSCollectionUrl);
                var inTeamProjectName = context.GetValue<string>(this.TeamProjectName);
                var inWorkItemType = context.GetValue<string>(this.WorkItemType);
                context.SetValue(this.NewWorkItem, this.CreateNewEmptyWorkitem(inWorkItemType, inServerUri, inTeamProjectName));
            }
            catch (Exception ex)
            {
                throw new Exception("Activity CreateNewWorkItem:", ex);
            }
        }

        /// <summary>
        /// Creates the new empty workitem.
        /// </summary>
        /// <param name="inWorkItemType">Type of the work item.</param>
        /// <param name="inServerUri">The server URI.</param>
        /// <param name="inTeamProjectName">Name of the team project.</param>
        /// <returns>The new work item</returns>
        private WorkItem CreateNewEmptyWorkitem(string inWorkItemType, string inServerUri, string inTeamProjectName)
        {
            var tfs = new TfsTeamProjectCollection(new Uri(inServerUri));
            var workItemStore = tfs.GetService<WorkItemStore>();
            var project = workItemStore.Projects[inTeamProjectName];
            WorkItem newEmptyWorkItem = null;

            if (project.WorkItemTypes.Contains(inWorkItemType))
            {
                newEmptyWorkItem = new WorkItem(project.WorkItemTypes[inWorkItemType]);
                LogExtensions.LogInfo(this, string.Format("Activity CreateNewWorkItem: New workitem of type {0} was created. On server {1} in the project: {2}", inWorkItemType, inServerUri, inTeamProjectName));
            }

            return newEmptyWorkItem;
        }
    }
}
