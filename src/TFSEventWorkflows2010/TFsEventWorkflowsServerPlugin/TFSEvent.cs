
namespace artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin
{
    using System;

    public class TFSEvent
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        /// <value>The name of the event.</value>
        public string FullTypeName { get; set; }

        /// <summary>
        /// Gets or sets the name of the event assembly.
        /// </summary>
        /// <value>The name of the event assembly.</value>
        public string EventAssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow file.
        /// </summary>
        /// <value>The name of the workflow file.</value>
        public string WorkflowFileName { get; set; }


        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        /// <value>The type of the event.</value>
        public Type EventType { get; set; }
    }
}
