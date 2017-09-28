using System;
using System.Activities;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using artiso.TFSEventWorkflows.LoggingLib;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{

    public sealed class ChangeWorkItemField : CodeActivity
    {
        /// <summary>
        /// Gets or sets the work item.
        /// </summary>
        /// <value>The work item.</value>
        [RequiredArgument]
        public InArgument<WorkItem> WorkItem { get; set; }

        /// <summary>
        /// Gets or sets the name of the field reference.
        /// </summary>
        /// <value>The name of the field reference.</value>
        [RequiredArgument]
        public InArgument<String> FieldReferenceName { get; set; }

        /// <summary>
        /// Gets or sets the new value.
        /// </summary>
        /// <value>The new value.</value>
        public InArgument<Object> NewValue { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {            
            var workItem = context.GetValue(this.WorkItem);
            var fieldReferenceName = context.GetValue(this.FieldReferenceName);
            var newValue = context.GetValue(this.NewValue);

            try
            {
                if (!workItem.Fields.Contains(fieldReferenceName))
                {
                    throw new Exception();
                }
                workItem.Fields[fieldReferenceName].Value = newValue;
                LogExtensions.LogInfo(this, string.Format("Activity ChangeWorkItemField: Field {0} on workitem {1} changed to {2}", fieldReferenceName, workItem.Id, newValue));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Activity ChangeWorkItemField: Error assigning value to field {0} on workitem {1}", fieldReferenceName, workItem.Id), ex);
            }
        }
    }
}
