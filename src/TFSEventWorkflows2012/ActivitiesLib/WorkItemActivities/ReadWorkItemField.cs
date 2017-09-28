using System;
using System.Activities;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using artiso.TFSEventWorkflows.LoggingLib;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{

    public sealed class ReadWorkItemField : CodeActivity
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
        public InArgument<string> FieldReferenceName { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [RequiredArgument]
        public OutArgument<object> Value { get; set; }

        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {            
            var workItem = context.GetValue(this.WorkItem);
            var fieldReferenceName = context.GetValue(this.FieldReferenceName);
 
            try
            {
                if (!workItem.Fields.Contains(fieldReferenceName))
                {
                    throw new Exception("The given field does not exist on the workitem");
                }
                context.SetValue(this.Value, workItem.Fields[fieldReferenceName].Value);
                LogExtensions.LogInfo(this, string.Format("Activity ReadWorkItemField: Read value for field {0} on workitem {1} successful.", fieldReferenceName, workItem.Id));
            }
            catch (Exception ex)
            {
                throw new Exception("Activity ReadWorkItemField:", ex);
            }
        }
    }
}
