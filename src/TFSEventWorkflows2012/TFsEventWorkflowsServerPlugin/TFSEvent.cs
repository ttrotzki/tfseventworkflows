
namespace artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin
{
    using System;
    using System.Collections.Generic;

    public enum CachedWorkflowFolderType
    {
      WorkflowActivitiesGlobal,
      WorkflowsGlobal,
      WorkflowsCollection,
      WorkflowsProject
    }

    public class CachedWorkflowFolder
    {
      public CachedWorkflowFolder() { }

      public CachedWorkflowFolder(CachedWorkflowFolderType FolderType, string Collection, string Project, string PathServer, string PathLocal, string PathServerCollection)
      {
        this.FolderType = FolderType;
        this.Collection = Collection;
        this.Project = Project;
        this.PathServer = PathServer;
        this.PathLocal = PathLocal;
        this.PathServerColletion = PathServerCollection;
      }

      public CachedWorkflowFolderType FolderType;
      public string Collection; // null for (all)
      public string Project;    // null for (all)
      public string PathServer; // null for local only
      public string PathLocal;
      public string PathServerColletion;
    }

    public class TFSEventAsync
    {
      public TFSEvent tfsEvent { get; set; }
      public Object notificationEventArgs { get; set; }
      public List<CachedWorkflowFolder> CachedWorkflowFolders { get; set; }
    }

    public class TFSEvent
    {
        /// <summary>
        /// Gets or sets the collection.
        /// </summary>
        /// <value>The collection.</value>
        public string Collection { get; set; }

        /// <summary>
        /// Gets or sets the project.
        /// </summary>
        /// <value>The project.</value>
        public string Project { get; set; }
        
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
        /// Gets or sets the name of the workflow file.
        /// </summary>
        /// <value>The name of the workflow file.</value>
        public string ConfigFileName { get; set; }

        /// <summary>
        /// Gets or sets the disabled flag.
        /// </summary>
        /// <value>The value of the disabled flag.</value>
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets the async flag.
        /// </summary>
        /// <value>The value of the async flag.</value>
        public bool Async { get; set; }

        /// <summary>
        /// Gets or sets the trace flag.
        /// </summary>
        /// <value>The value of the trace flag.</value>
        public bool Trace { get; set; }

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        /// <value>The type of the event.</value>
        [System.Xml.Serialization.XmlIgnore()]
        public Type EventType { get; set; }
    }
}
