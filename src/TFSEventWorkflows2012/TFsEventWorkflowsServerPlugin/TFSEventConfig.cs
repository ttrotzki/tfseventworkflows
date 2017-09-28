
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
        /// Gets or sets the collection.
        /// if not provided explicit or implicit (all) is used
        /// </summary>
        /// <value>The collection.</value>
        [ConfigurationProperty("collection", IsRequired = false)]
        public string Collection
        {
          get
          {
            return (String)this["collection"];
          }
          set
          {
            this["collection"] = value;
          }
        }

        /// <summary>
        /// Gets or sets the project.
        /// if not provided explicit or implicit (all) is used
        /// </summary>
        /// <value>The project.</value>
        [ConfigurationProperty("project", IsRequired = false)]
        public string Project
        {
          get
          {
            return (String)this["project"];
          }
          set
          {
            this["project"] = value;
          }
        }

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
        /// Disables this event
        /// </summary>
        /// <value>Disabled or Enabled</value>
        [ConfigurationProperty("disabled", IsRequired = false, DefaultValue = false)]
        public bool Disabled
        {
            get
            {
              return (bool)this["disabled"];
            }
            set
            {
              this["disabled"] = value;
            }
        }

        /// <summary>
        /// async flag
        /// </summary>
        /// <value>True for running the event in the job agent</value>
        [ConfigurationProperty("async", IsRequired = false, DefaultValue = false)]
        public bool Async
        {
          get
          {
            return (bool)this["async"];
          }
          set
          {
            this["async"] = value;
          }
        }

        /// <summary>
        /// trace flag
        /// </summary>
        /// <value>True for running the event in the job agent</value>
        [ConfigurationProperty("trace", IsRequired = false, DefaultValue = false)]
        public bool Trace
        {
          get
          {
            return (bool)this["trace"];
          }
          set
          {
            this["trace"] = value;
          }
        }

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        /// <value>The type of the event.</value>
        public Type EventType { get; set; }
    }
}
