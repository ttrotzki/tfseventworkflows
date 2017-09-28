using System;
using System.Collections.Generic;
using System.Linq;
using System.Activities;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using artiso.TFSEventWorkflows.LoggingLib;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{
    public sealed class GetValidationOfWorkItem : CodeActivity
    {
        /// <summary>
        /// Gets or sets the work item.
        /// </summary>
        /// <value>The work item.</value>
        [RequiredArgument]
        public InArgument<WorkItem> WorkItem { get; set; }

        /// <summary>
        /// Gets or sets the collection of not valid fields.
        /// </summary>
        /// <value>The collection of not valid fields.</value>
        [RequiredArgument]
        public OutArgument<List<Field>> CollectionOfNotValidFields { get; set; }


        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                var inWorkItem = context.GetValue<WorkItem>(WorkItem);
                context.SetValue(CollectionOfNotValidFields, GetValidationOfWorkitem(inWorkItem));
                LogExtensions.LogInfo(this, string.Format("Activity GetValidationOfWorkItem: Workitem {0} validated.", inWorkItem.Id));
            }
            catch (Exception ex)
            {
                throw new Exception("Activity GetValidationOfWorkItem:", ex);
            }
        }

        /// <summary>
        /// Gets the validation of workitem.
        /// </summary>
        /// <param name="inWorkItem">The in work item.</param>
        /// <returns></returns>
        private List<Field> GetValidationOfWorkitem(WorkItem inWorkItem)
        {
            var getVailidationOfWorkItem = new List<Field>();
            getVailidationOfWorkItem.AddRange(from invalidfield in inWorkItem.Validate().ToArray() select invalidfield as Field);
            return getVailidationOfWorkItem;
        }
    }
}
