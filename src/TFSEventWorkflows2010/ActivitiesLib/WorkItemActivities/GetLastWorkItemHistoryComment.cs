
namespace Kuka_WorkItemSynchronizer.SynchronizeWorkItemsActivitiesLib
{
    using System;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;
    using System.Activities;

    /// <summary>
    /// Returns the last workitem history comment.
    /// </summary>
    public sealed class GetLastWorkItemHistoryComment : CodeActivity
    {
        /// <summary>
        /// Gets or sets the work item.
        /// </summary>
        /// <value>The work item.</value>
        [RequiredArgument]
        public InArgument<WorkItem> WorkItem { get; set; }

        /// <summary>
        /// Gets or sets the history comment.
        /// </summary>
        /// <value>The history comment.</value>
        [RequiredArgument]
        public OutArgument<string> HistoryComment { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            WorkItem workItem = context.GetValue(this.WorkItem);

            try
            {
                context.SetValue(this.HistoryComment, this.GetLastHistoryComment(workItem));
            }
            catch (Exception ex)
            {
                throw new Exception("Activity CheckWorkItemHistoryComment:", ex);
            }
        }

        /// <summary>
        /// Gets the last history comment.
        /// </summary>
        /// <param name="workItem">The work item.</param>
        /// <returns></returns>
        private string GetLastHistoryComment(WorkItem workItem)
        {
            string historyComment = string.Empty;
            FieldCollection fields = null;
            if (workItem.Revisions.Count > 0)
            {
                fields = workItem.Revisions[workItem.Revisions.Count - 1].Fields;
            }
            else
            {
                return historyComment;
            }

            foreach (Field f in fields)
            {
                if (f.ReferenceName.Equals("System.History"))
                {
                    historyComment= f.Value.ToString();
                    break;
                }
            }
            return historyComment;
        }
    }
}
