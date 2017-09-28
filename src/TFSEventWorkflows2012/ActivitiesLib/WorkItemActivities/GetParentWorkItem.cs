using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Activities;
using artiso.TFSEventWorkflows.LoggingLib;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{
    public sealed class GetParentWorkItem : CodeActivity
    {
        /// <summary>
        /// Gets or sets the work item.
        /// </summary>
        /// <value>The work item.</value>
        [RequiredArgument]
        public InArgument<WorkItem> WorkItem { get; set; }

        /// <summary>
        /// Gets or sets the parent work item.
        /// </summary>
        /// <value>The parent work item.</value>
        [RequiredArgument]
        public OutArgument<WorkItem> ParentWorkItem { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                var inWorkItem = context.GetValue<WorkItem>(WorkItem);
                WorkItem parentWorkItem = GetParentWorkitem(inWorkItem);
                context.SetValue(ParentWorkItem, parentWorkItem);
                if (parentWorkItem != null)
                {
                    LogExtensions.LogInfo(this, string.Format("Activity GetParentWorkItem: Parent {0} found for workitem {1}", parentWorkItem.Id, inWorkItem.Id));
                }
                else
                {
                    LogExtensions.LogInfo(this, string.Format("Activity GetParentWorkItem: No parent found for workitem {0}", inWorkItem.Id));
                }
                
            }
            catch (Exception ex)
            {
                throw new Exception("Activity GetParentWorkItem:", ex);
            }
        }


        /// <summary>
        /// Gets the parent workitem.
        /// </summary>
        /// <param name="workitem">The workitem.</param>
        /// <returns></returns>
        private WorkItem GetParentWorkitem(WorkItem workitem)
        {
            string parentChildLinkTypeName = "System.LinkTypes.Hierarchy";
            string parentChildLinkType = string.Empty;

            if (workitem.Links.Count == 0)
            {
                return null;
            }

            foreach (WorkItemLinkType linkType in workitem.Store.WorkItemLinkTypes)
            {
                if (parentChildLinkTypeName.Equals(linkType.ReferenceName))
                {
                    parentChildLinkType = linkType.ForwardEnd.ImmutableName;
                    break;
                }
            }

            if (string.IsNullOrEmpty(parentChildLinkType))
            {
                return null;
            }            

            String QueryString = string.Format("SELECT * FROM WorkItemLinks WHERE [Target].[System.Id]='{0}' AND [System.Links.LinkType]='{1}'", workitem.Id, parentChildLinkType);
            var query = new Query(workitem.Store, QueryString);
            var links = query.RunLinkQuery();

            if (links.Any(i => i.TargetId != workitem.Id))
            {
                var parentId = links.Where(x => x.TargetId != workitem.Id).Select(x => x.TargetId).Single();
                return workitem.Store.GetWorkItem(parentId);
            }
            else
            {
                return null;
            }
        }
    }
}