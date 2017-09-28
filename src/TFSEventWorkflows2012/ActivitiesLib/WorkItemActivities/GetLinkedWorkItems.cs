using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using artiso.TFSEventWorkflows.LoggingLib;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib.WorkItemActivities
{

    public sealed class GetLinkedWorkItems : CodeActivity
    {
        /// <summary>
        /// Gets or sets the work item.
        /// </summary>
        /// <value>The work item.</value>
        [RequiredArgument]
        public InArgument<WorkItem> WorkItem { get; set; }

        [RequiredArgument]
        public InArgument<string> LinkTypeReferenceName { get; set; }

        public InArgument<string> WorkItemTypeName { get; set; }

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
        public OutArgument<List<WorkItem>> LinkedWorkItems { get; set; }

        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                var inWorkItem = context.GetValue<WorkItem>(this.WorkItem);
                var workItemTypeName = context.GetValue<string>(this.WorkItemTypeName);
                var linkTypeReferenceName = context.GetValue<string>(this.LinkTypeReferenceName);

                List<string> fieldsList = context.GetValue<List<string>>(WorkItemFieldsList);
                context.SetValue(LinkedWorkItems, GetLinkedWorkitems(inWorkItem, fieldsList, linkTypeReferenceName, workItemTypeName));
                LogExtensions.LogInfo(this, string.Format("Activity GetLinkedWorkItems: All linked workitems of workitem {0} collected.", inWorkItem.Id));
            }
            catch (Exception ex)
            {
                throw new Exception("Activity GetLinkedWorkItems:", ex);
            }
        }

        /// <summary>
        /// Gets the linked workitems.
        /// </summary>
        /// <param name="sourceWorkitem">The workitem from where to check the links.</param>
        /// <param name="fieldsList">The fields list.</param>
        /// <param name="linkTypeReferenceName"></param>
        /// <returns></returns>
        public List<WorkItem> GetLinkedWorkitems(WorkItem sourceWorkitem, List<string> fieldsList, string linkTypeReferenceName, string workItemTypeName)
        {
            List<int> childIds = new List<int>();
            string targetLinkType = string.Empty;
            foreach (WorkItemLinkType linkType in sourceWorkitem.Store.WorkItemLinkTypes)
            {
                if (linkTypeReferenceName.Equals(linkType.ReferenceName))
                {
                    targetLinkType = linkType.ForwardEnd.ImmutableName;
                    break;
                }
            }
            if (string.IsNullOrEmpty(targetLinkType))
            {
                return null;
            }

            string queryString = string.Format("SELECT * FROM WorkItemLinks WHERE [Source].[System.Id]='{0}' AND [System.Links.LinkType]='{1}'", sourceWorkitem.Id, targetLinkType);
            var query = new Query(sourceWorkitem.Store, queryString);
            var links = query.RunLinkQuery();

            if (links.Length == 0)
            {
                return new List<WorkItem>();
            }
            foreach (var item in links.Where(x => x.TargetId != sourceWorkitem.Id))
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
            if (!String.IsNullOrEmpty(workItemTypeName))
            {
                workItemQuery += String.Format(" AND [System.WorkItemType] = '{0}'", workItemTypeName);
            }
            WorkItemCollection childWorkItems = sourceWorkitem.Store.Query(workItemQuery);

            return childWorkItems.Cast<WorkItem>().ToList();
        }

    }
}
