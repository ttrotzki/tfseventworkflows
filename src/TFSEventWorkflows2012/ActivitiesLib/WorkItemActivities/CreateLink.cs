using System;
using System.Activities;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using artiso.TFSEventWorkflows.LoggingLib;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{
    public sealed class CreateLink : CodeActivity
    {
        /// <summary>
        /// Gets or sets the parent work item.
        /// </summary>
        /// <value>The parent work item.</value>
        [RequiredArgument]
        public InArgument<WorkItem> ParentWorkItem { get; set; }

        /// <summary>
        /// Gets or sets the child work item.
        /// </summary>
        /// <value>The child work item.</value>
        [RequiredArgument]
        public InArgument<WorkItem> ChildWorkItem { get; set; }

        /// <summary>
        /// Gets or sets the type of the link.
        /// </summary>
        /// <value>The type of the link.</value>
        [RequiredArgument]
        public InArgument<string> LinkType { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                var inLinkType = context.GetValue<string>(LinkType);
                var inParentWorkItem = context.GetValue<WorkItem>(ParentWorkItem);
                var inChildWorkItem = context.GetValue<WorkItem>(ChildWorkItem);

                if (inParentWorkItem == null)
                {
                    throw new Exception("ParentWorkItem is null.");
                }
                if (inChildWorkItem == null)
                {
                    throw new Exception("ChildWorkItem is null.");
                }
                if (string.IsNullOrEmpty(inLinkType))
                {
                    throw new Exception("LinkType is empty.");
                }
                WorkItemLinkType outLinkType;

                if (inParentWorkItem.Project.Store.WorkItemLinkTypes.TryGetByName(inLinkType, out outLinkType))
                {
                    WorkItemLink newLink = new WorkItemLink(outLinkType.ForwardEnd, inParentWorkItem.Id, (inChildWorkItem.Id > 0) ? inChildWorkItem.Id : inChildWorkItem.TemporaryId);
                    inParentWorkItem.WorkItemLinks.Add(newLink);
                    inParentWorkItem.Save();
                    LogExtensions.LogInfo(this, string.Format("Activity CreateLink: The parent workitem {0} is now linked with the child workitem {1} with a link of the type: {2}", inParentWorkItem.Id, inChildWorkItem.Id, inLinkType));
                }
                else
                {
                    LogExtensions.LogInfo(this, string.Format("Activity CreateLink: {0} is not a valid link type.", inLinkType));
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Activity CreateLink:", ex);
            }
        }
    }
}
