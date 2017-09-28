using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Activities;
using artiso.TFSEventWorkflows.LoggingLib;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{
    public sealed class GetChildWorkItems : CodeActivity
    {
        /// <summary>
        /// Gets or sets the work item.
        /// </summary>
        /// <value>The work item.</value>
        [RequiredArgument]
        public InArgument<WorkItem> WorkItem { get; set; }

        /// <summary>
        /// Gets or sets the work item fields list.
        /// </summary>
        /// <value>The work item fields list.</value>
        public InArgument<List<string>> WorkItemFieldsList { get; set; }

        /// <summary>
        /// Gets or sets the child work items.
        /// </summary>
        /// <value>The child work items.</value>
        [RequiredArgument]
        public OutArgument<List<WorkItem>> ChildWorkItems { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                var inWorkItem = context.GetValue<WorkItem>(WorkItem);
                List<string> fieldsList = context.GetValue<List<string>>(WorkItemFieldsList);
                context.SetValue(ChildWorkItems, GetChildWorkitems(inWorkItem, fieldsList));
                LogExtensions.LogInfo(this, string.Format("Activity GetChildWorkItems: All child workitems of workitem {0} collected.", inWorkItem.Id));
            }
            catch (Exception ex)
            {
                throw new Exception("Activity GetChildWorkItems:", ex);
            }
        }

        /// <summary>
        /// Gets the child workitems.
        /// </summary>
        /// <param name="parentWorkitem">The parent workitem.</param>
        /// <param name="fieldsList">The fields list.</param>
        /// <returns></returns>
        public List<WorkItem> GetChildWorkitems(WorkItem parentWorkitem, List<string> fieldsList)
        {
            List<int> childIds = new List<int>();
            string parentChildLinkTypeName = "System.LinkTypes.Hierarchy";
            string parentChildLinkType = string.Empty;
            foreach (WorkItemLinkType linkType in parentWorkitem.Store.WorkItemLinkTypes)
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

            string queryString = string.Format("SELECT * FROM WorkItemLinks WHERE [Source].[System.Id]='{0}' AND [System.Links.LinkType]='{1}'", parentWorkitem.Id, parentChildLinkType);
            var query = new Query(parentWorkitem.Store, queryString);
            var links = query.RunLinkQuery();
            
            if (links.Length == 0)
            {
                return new List<WorkItem>();
            }
            foreach (var item in links.Where(x => x.TargetId != parentWorkitem.Id))
            {
                childIds.Add(item.TargetId);
            }
            string childIdString = string.Join(", ", childIds);
            string fields = string.Empty;
            if (fieldsList == null || fieldsList.Count == 0)
            {
                fields = "*";
            }
            else
            {
                fields = string.Join(", ", fieldsList);
            }

            string workItemQuery = string.Format("SELECT {0} FROM Workitems WHERE [System.ID] in ({1})", fields, childIdString);
            WorkItemCollection childWorkItems = parentWorkitem.Store.Query(workItemQuery);

            return childWorkItems.Cast<WorkItem>().ToList();
        }
    }
}
