
namespace artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin
{
    using System;
    using System.Configuration;

    public class TFSEventConfig : ConfigurationSection
    {
        [ConfigurationProperty("tfsEvents")]
        public TFSEventCollection TFSEvents
        {
            get { return (TFSEventCollection)this["tfsEvents"]; }
        }
    }

    public class TFSEventCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new TFSEventElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TFSEventElement)element).Name;
        }
    }

    /// <summary>
    /// TFS Event Configuration
    /// </summary>
    public class TFSEventElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (String)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        /// <value>The name of the event.</value>
        [ConfigurationProperty("fullTypeName", IsRequired = true)]
        public string FullTypeName
        {
            get
            {
                return (String)this["fullTypeName"];
            }
            set
            {
                this["fullTypeName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the event assembly.
        /// </summary>
        /// <value>The name of the event assembly.</value>
        [ConfigurationProperty("eventAssemblyName", IsRequired = true)]
        public string EventAssemblyName
        {
            get
            {
                return (String)this["eventAssemblyName"];
            }
            set
            {
                this["eventAssemblyName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the workflow file.
        /// </summary>
        /// <value>The name of the workflow file.</value>
        [ConfigurationProperty("workflowFileName", IsRequired = true)]
        public string WorkflowFileName
        {
            get
            {
                return (String)this["workflowFileName"];
            }
            set
            {
                this["workflowFileName"] = value;
            }
        }


        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        /// <value>The type of the event.</value>
        public Type EventType { get; set; }
    }
}
