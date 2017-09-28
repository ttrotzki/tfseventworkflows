namespace artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin
{ 
  #region *** using namepaces ***
  using System;
  using System.Activities;
  using System.Activities.Tracking;
  using System.Activities.XamlIntegration;
  using System.Collections.Generic;
  using System.Configuration;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Xml;
  using System.Xml.Serialization;

  // TFS server object model
  using Microsoft.TeamFoundation.Framework.Common;
  using Microsoft.TeamFoundation.Framework.Server;
  using Microsoft.TeamFoundation.VersionControl.Server;
  using Microsoft.TeamFoundation.WorkItemTracking.Server;
  #if UsingMicrosoftTeamFoundationServerCore
  using Microsoft.TeamFoundation.Server.Core;
  #endif
  #if UsingMicrosoftTeamFoundationFrameworkServerAlm
  using Microsoft.TeamFoundation.Framework.Server.Alm;
  #endif
  #if UsingMicrosoftVisualStudioServicesIdentityEvents
  using Microsoft.VisualStudio.Services.Identity.Events;
  #endif
  #if UsingILocationService
  using Microsoft.VisualStudio.Services.Location.Server;
  #endif

  using artiso.TFSEventWorkflows.LoggingLib;

  using log4net.Config;

  // TFS client object model - explicit using to avoid conflicts with the server object model
  using TfsTeamProjectCollection = Microsoft.TeamFoundation.Client.TfsTeamProjectCollection;
  using ITeamFoundationRegistry = Microsoft.TeamFoundation.Framework.Client.ITeamFoundationRegistry;
  using VersionControlServer = Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer;
  using WorkingFolder = Microsoft.TeamFoundation.VersionControl.Client.WorkingFolder;
  using VersionSpec = Microsoft.TeamFoundation.VersionControl.Client.VersionSpec;
  using DeletedState = Microsoft.TeamFoundation.VersionControl.Client.DeletedState;
  using ItemType = Microsoft.TeamFoundation.VersionControl.Client.ItemType;
  using WorkingFolderType = Microsoft.TeamFoundation.VersionControl.Client.WorkingFolderType;
  using RecursionType = Microsoft.TeamFoundation.VersionControl.Client.RecursionType;
  using Workspace = Microsoft.TeamFoundation.VersionControl.Client.Workspace;
  using WorkspaceNotFoundException = Microsoft.TeamFoundation.VersionControl.Client.WorkspaceNotFoundException;
  using CreateWorkspaceParameters = Microsoft.TeamFoundation.VersionControl.Client.CreateWorkspaceParameters;
  using WorkspaceLocation = Microsoft.TeamFoundation.VersionControl.Common.WorkspaceLocation;
  using WorkspaceOptions = Microsoft.TeamFoundation.VersionControl.Common.WorkspaceOptions;
  using GetStatus = Microsoft.TeamFoundation.VersionControl.Client.GetStatus;
  using TfsConfigurationServer = Microsoft.TeamFoundation.Client.TfsConfigurationServer;

  #if UsingIVssRequestContext
  using TeamFoundationRequestContext = Microsoft.TeamFoundation.Framework.Server.IVssRequestContext;
  #else
  using TeamFoundationRequestContext = Microsoft.TeamFoundation.Framework.Server.TeamFoundationRequestContext;
  #endif

    #endregion

    internal class WorkflowRunner
  {
    #region *** Constants and Fields ***

    private static FileInfo executionPath;

    private static List<TFSEvent> _tfsEvents = null;
    private List<TFSEvent> TfsEvents(TeamFoundationRequestContext requestContext)
        {
      lock (PluginConfig.Config)
      {
        if (_tfsEvents == null)
        {
          this.LogInfo(string.Format("Starting Update Cache"));

          List<TFSEvent> _tfsEventsFile = GetTFSEventConfigFromFile();
          List<TFSEvent> _tfsEventsTfs = GetTFSEventConfigFromTfs(requestContext);

          List<TFSEvent> __tfsEvents = new List<TFSEvent>();
          __tfsEvents.AddRange(_tfsEventsFile);
          __tfsEvents.AddRange(_tfsEventsTfs);
          _tfsEvents = __tfsEvents;

          _tfsEvents.ForEach((TFSEvent tfsEvent) =>
          {
            LogEvent(tfsEvent.Disabled ? "Disabled" : "Watching", tfsEvent);
          });

          // restart job agent if needed
          TriggerReloadJobAgentPlugin();

          this.LogInfo(string.Format("Leave Update Cache"));
        }
      }
      return _tfsEvents;
    }

    private Assembly configurationDefiningAssembly;

    private static int CacheChangeSetId = 0;

    #endregion

    #region *** Constructors and Destructors ***

    public WorkflowRunner()
    {
      this.LogInfo(string.Format("WorkflowRunner.WorkflowRunner()"));
    }

    #endregion

    #region *** Public Properties ***

    /// <summary>
    /// Gets the execution path of the assembly.
    /// </summary>
    /// <value>The execution path.</value>
    public static FileInfo ExecutionPath
    {
      get
      {
        if (executionPath == null)
        {
          executionPath = new FileInfo(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
        }
        return executionPath;
      }
    }

    #endregion

    #region *** event processing ***

    private bool InJobAgent = false;

    /// <summary>
    /// Processes the event.
    /// </summary>
    /// <param name="requestContext">The request context.</param>
    /// <param name="notificationType">Type of the notification.</param>
    /// <param name="notificationEventArgs">The notification event args.</param>
    /// <param name="InJobAgent">true: context JobAgent - false: context WebService</param>
    /// <param name="IsAsncEvent">true: called from JobAgent.Run - false: called from ISubcriber.ProcessEvent</param>
    /// <returns></returns>
    public EventNotificationStatus ProcessEvent(
      TeamFoundationRequestContext requestContext, NotificationType notificationType, object notificationEventArgs,
      bool InJobAgent, bool IsAsyncEvent)
    {
      // web service: we handle only sync events
      // job agent: we handle only async evente (ISubscriber is also called in job agent context
      // we only handle notifications, not decision points
      // we only handle events related to a collection (attention: HostReadyEvent on apphost TEAM FOUNDATION with long running handler results in problems for build vNext (Definition templates - Loading...)
      bool CanBeHandled = (InJobAgent == IsAsyncEvent) && (notificationType == NotificationType.Notification) && (requestContext.ServiceHost.HostType == TeamFoundationHostType.ProjectCollection);
          
      // dump info
      Type typeNotification = notificationEventArgs.GetType();
      this.LogInfo(string.Format("Detecting event on [{8} ({9})] [{1} - {0} - jobagent:{5} - async:{6}{7}] defined in [{2}] - [{3}] defined in [{4}]", 
        notificationType, typeNotification.Name, System.IO.Path.GetFileName(typeNotification.Assembly.Location),
        typeNotification.FullName, typeNotification.Assembly.Location,
        InJobAgent, IsAsyncEvent, CanBeHandled ? "" : " ** handling not supported by TfsEventWorkflows **",
        requestContext.ServiceHost.Name,
        requestContext.ServiceHost.HostType
        ));

      // exit if not supported
      if (!CanBeHandled)
      {
        return EventNotificationStatus.ActionPermitted;
      }

      this.InJobAgent = InJobAgent;

      string strCollection = requestContext.ServiceHost.Name;
      string strProject = null;

      if (typeNotification == typeof(TFSEventAsync))
      {
        // that's a TfsEventWorkflows internal notification send from Web Service to Job Agent only
        System.Diagnostics.Debug.Assert(InJobAgent);
            
        // deserialize the job parameters tp avoid long term reloading of this values
        var notification = notificationEventArgs as TFSEventAsync;
        notification.tfsEvent.EventType = notification.notificationEventArgs.GetType();
        notification.tfsEvent.Async = false;
        _tfsEvents = new List<TFSEvent>();
        _tfsEvents.Add(notification.tfsEvent);
        CachedWorkflowFolders = notification.CachedWorkflowFolders;
        typeNotification = notification.notificationEventArgs.GetType();
        notificationEventArgs = notification.notificationEventArgs;
      }

      if (typeNotification == typeof(Microsoft.TeamFoundation.WorkItemTracking.Server.WorkItemChangedEvent))
      {
        // workitem - is assigned to one project
        var notification = notificationEventArgs as Microsoft.TeamFoundation.WorkItemTracking.Server.WorkItemChangedEvent;
        strProject = notification.PortfolioProject;
      }
      else if (typeNotification == typeof(Microsoft.TeamFoundation.VersionControl.Server.CheckinNotification))
      {
        // checkin - may contain items releated to different projects - don't set project
        var notification = notificationEventArgs as Microsoft.TeamFoundation.VersionControl.Server.CheckinNotification;

        // check whether the change set contains any cached TfsEventWorkflows files
        bool UpdateNeeded = true;
        IEnumerable<string> cache = null;
        if (CachedWorkflowFolders != null)
        {
          UpdateNeeded = false;
          cache = CachedWorkflowFolders.Where((CachedWorkflowFolder cachedWorkflowFolder) =>
            {
              return cachedWorkflowFolder.PathServerColletion == strCollection;
            }).Select((CachedWorkflowFolder cachedWorkflowFolder) =>
              {
                return cachedWorkflowFolder.PathServer;
              });

          UpdateNeeded = notification.SubmittedItems.Any((string SubmittedItem) =>
          {
            return cache.Any((string PathServer) => { return SubmittedItem.StartsWith(PathServer); });
          });
        }

        this.LogInfo(string.Format("cached server paths"));
        foreach (var cachedWorkflowFolder in CachedWorkflowFolders)
          this.LogInfo(string.Format("  cached server path - {0}", cachedWorkflowFolder.PathServer));

        this.LogInfo(string.Format("cached server paths matching checkin collection"));
          foreach (var pathServer in cache)
            this.LogInfo(string.Format("  cached server path - {0}", pathServer));

        if (UpdateNeeded)
        {
          foreach (var SubmittedItem in notification.SubmittedItems)
            this.LogInfo(string.Format("checkin in workspace detected - {0}", SubmittedItem));

          // Update Cache
          TriggerReloadPlugin();
        }
      }
      else if (typeNotification == typeof(Microsoft.TeamFoundation.Build.Server.BuildQualityChangedNotificationEvent))
      {
        // build - is assigned to one project
        var notification = notificationEventArgs as Microsoft.TeamFoundation.Build.Server.BuildQualityChangedNotificationEvent;
        strProject = notification.Build.TeamProject;
      }
      else if (typeNotification == typeof(Microsoft.TeamFoundation.Build.Server.BuildCompletionNotificationEvent))
      {
        // build - is assigned to one project
        var notification = notificationEventArgs as Microsoft.TeamFoundation.Build.Server.BuildCompletionNotificationEvent;
        strProject = notification.Build.TeamProject;
      }
      else if (typeNotification == typeof(Microsoft.TeamFoundation.Build.Server.BuildDefinitionChangedEvent))
      {
        // build - is assigned to one project
        var notification = notificationEventArgs as Microsoft.TeamFoundation.Build.Server.BuildDefinitionChangedEvent;
        strProject = notification.TeamProject;
      }
      else if (typeNotification == typeof(Microsoft.TeamFoundation.TestManagement.Server.TestSuiteChangedNotification))
      {
        // testsuite - is assigned to one project
        var notification = notificationEventArgs as Microsoft.TeamFoundation.TestManagement.Server.TestSuiteChangedNotification;
        strProject = notification.ProjectName;
      }

      List<TFSEvent> TfsEvents = this.TfsEvents(requestContext);

      var TfsEventsMatching = TfsEvents.Where((TFSEvent tfsEvent) =>
      {
        return 
          (!tfsEvent.Disabled && notificationEventArgs.GetType() == tfsEvent.EventType) &&
          (string.IsNullOrEmpty(tfsEvent.Collection) || tfsEvent.Collection == strCollection) &&
          (string.IsNullOrEmpty(tfsEvent.Project) || string.IsNullOrEmpty(strProject) || tfsEvent.Project == strProject);
      });
      if(TfsEventsMatching.Count() != 0)
        this.LogInfo(string.Format("Handling event on [{0}|{1}] - {2} workflows match", strCollection, string.IsNullOrEmpty(strProject) ? "<not provided>" : strProject, TfsEventsMatching.Count())); 

      foreach (TFSEvent tfsEvent in TfsEventsMatching)
      {
        if (!tfsEvent.Async)
        {
          try
          {
            // to enable assemlies probing in cached files got from version control
            AppDomain.CurrentDomain.AssemblyResolve += BinariesResolveEventHandler;

            // get the name of the workflow file
            var workflow = this.GetWorkflow(tfsEvent.WorkflowFileName);

            // initialize the workflow arguments
            // TODO: provide more comfort - check the arguments defined in the workflow and provide the information requested by the workflow -> makes it easier for the workflow
            var workflowParameters = new Dictionary<string, object>
            {
              { "TFSEvent", notificationEventArgs },
              { "TeamFoundationRequestContext", requestContext }
            };

            // invoke the workflow
            LogEvent("Starting", tfsEvent, strCollection, strProject);

            if(!tfsEvent.Trace)
            {
              WorkflowInvoker.Invoke(workflow, workflowParameters);
            }
            else
            {
              WorkflowInvoker workflowInvoker = new WorkflowInvoker(workflow);
              customTrackingParticipant.CurrentWorkflow = workflow;
              workflowInvoker.Extensions.Add(customTrackingParticipant);
              workflowInvoker.Invoke(workflowParameters);
              customTrackingParticipant.CurrentWorkflow = null;
            }
          }
          catch (Exception e)
          {
            this.LogError(string.Format("Workflow error:"), e);
          }
          finally
          {
            // to disable assemlies probing in cached files got from version control
            AppDomain.CurrentDomain.AssemblyResolve -= BinariesResolveEventHandler;

            LogEvent("Leaving", tfsEvent, strCollection, strProject);
          }
        }
        else
        {
          try
          {
            // provide the job some parameters to avoid long term reloading of this values
            TFSEventAsync async = new TFSEventAsync()
            {
              notificationEventArgs = notificationEventArgs,
              tfsEvent = tfsEvent,
              CachedWorkflowFolders = CachedWorkflowFolders,
            };

            XmlNode xmlData = SerializeXml(async);

            // handle the notification by queueing the information we need for a job
            LogEvent("Queuing", tfsEvent, strCollection, strProject);
            var jobService = requestContext.GetService<TeamFoundationJobService>();
            var jobGuid = jobService.QueueOneTimeJob(
                requestContext,
                "TFSEventWorkflow Job",
                "artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin.WorkflowRunnerJob",
                xmlData,
                false);
            int result = jobService.QueueJobsNow(requestContext, new List<Guid>() { jobGuid }, true);
          }
          catch (Exception e)
          {
            this.LogError(string.Format("Queuing error:"), e);
          }
        }
      }

      if (TfsEventsMatching.Count() != 0)
        this.LogInfo("");
      return EventNotificationStatus.ActionPermitted;
    }

    private static void TriggerReloadPlugin()
    {
      // setting _tfsEvents = null only updates the workspace and gets the files from version control
      // assemblies already loaded are not reloaded by this and can't be updated 
      // because they they are locked by web services or job agent
      // _tfsEvents = null;

      // create content for a batch file to restart the job agent
      string strTfsJobAgentServiceName = "TfsJobAgent";
      string strBatchFile = PluginConfig.Config.ExecutionPath.DirectoryName + @"\restartjobagent.bat";
      string strNewLine = Environment.NewLine;
      string strCommandsInBatch = string.Format(
        "net stop \"{0}\"{2}" + // stop job agent
        "net start \"{0}\"{2}" + // start job agent
        "( del /q /f \"%~f0\" >nul 2>&1 & exit /b 0  ){2}", // delete batch file
        strTfsJobAgentServiceName,
        strBatchFile,
        strNewLine);

      // touch file in plugin dir -> this triggers reload of plugin in the web service
      System.IO.File.WriteAllText(strBatchFile, strCommandsInBatch);
    }

    private static void TriggerReloadJobAgentPlugin()
    {
      string strBatchFile = PluginConfig.Config.ExecutionPath.DirectoryName + @"\restartjobagent.bat";
      if (File.Exists(strBatchFile))
      {
        // start the batch to restart the job agent
        string strCommandArgsRunBatch = string.Format("/c \"{0}\"", strBatchFile);
        System.Diagnostics.Process.Start("cmd.exe", strCommandArgsRunBatch);
      }
    } 

    public Type[] GetTypesFromTfsAssemblies()
    {
      Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
      List<Type> listTypes = new List<Type>();
      foreach (Assembly loadedAssembly in loadedAssemblies)
      {
        if (loadedAssembly.FullName.StartsWith("Microsoft.TeamFoundation") || loadedAssembly.FullName.StartsWith("Microsoft.VisualStudio"))
        {
          Type[] typesInAssembly = new Type[0];
          try { typesInAssembly = loadedAssembly.GetExportedTypes(); } // GetExportedTypes is better than GetTypes - you can use only public Types in workflows
          catch (ReflectionTypeLoadException rtle) { typesInAssembly = rtle.Types.Where(x => x != null).ToArray(); } // thrown by GetTypes only
          catch (Exception) {}

          listTypes.AddRange(typesInAssembly);
        }
      }

      return listTypes.ToArray();
    }

    /// <summary>
    /// Subscribeds the types.
    /// </summary>
    /// <returns></returns>
    public Type[] SubscribedTypes()
    {
      Type[] typesToSubscribe = GetTypesFromTfsAssemblies();

      Type[] typesToIgnore = new Type[]
      {
        #if UsingMicrosoftVisualStudioServicesIdentityEvents
        typeof(BeforeReadIdentitiesOnService),
        typeof(AfterReadIdentitiesOnService),
        typeof(TfsmqDequeueEvent),
        #endif
      };
      typesToSubscribe = typesToSubscribe.Except(typesToIgnore).ToArray();

      return typesToSubscribe;
    }

    #endregion

    #region *** helpers ***

    private void LogEvent(string action, TFSEvent tfsEvent, string strCollection = "", string strProject = "")
    {
      if(string.IsNullOrWhiteSpace(strCollection))
        strCollection = string.IsNullOrWhiteSpace(tfsEvent.Collection) ? "*" : tfsEvent.Collection;
      if(string.IsNullOrWhiteSpace(strProject))
        strProject = string.IsNullOrWhiteSpace(tfsEvent.Project) ? "*" : tfsEvent.Project;

      this.LogInfo(string.Format("{0} event [{1}] on [{6}|{7}] workflow file [{2}] defined in config [{3}] - workflow file [{4}] defined in config [{5}])",
        action,
        tfsEvent.Name,
        System.IO.Path.GetFileName(tfsEvent.WorkflowFileName), 
        System.IO.Path.GetFileName(tfsEvent.ConfigFileName),
        tfsEvent.WorkflowFileName, 
        tfsEvent.ConfigFileName,
        strCollection,
        strProject
        ));
    }

    private XmlNode SerializeXml(TFSEventAsync notificationEventArgs)
    {
      if (null == notificationEventArgs)
      {
        return null;
      }

      Type typeNotificationEventArgs = notificationEventArgs.GetType();
      var xmlSerializer = new XmlSerializer(typeNotificationEventArgs, new Type[] { (notificationEventArgs as TFSEventAsync).notificationEventArgs.GetType() });
      var memoryStream = new MemoryStream();
      xmlSerializer.Serialize(memoryStream, notificationEventArgs);

      memoryStream.Position = 0;
      var xmlDocument = new XmlDocument();
      xmlDocument.Load(memoryStream);
      memoryStream.Close();

      XmlNode xmlNode = xmlDocument.ChildNodes[1];
      XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("assemblyQualifiedName");
      xmlAttribute.Value = typeNotificationEventArgs.AssemblyQualifiedName + "|" + notificationEventArgs.notificationEventArgs.GetType().AssemblyQualifiedName;
      xmlNode.Attributes.Append(xmlAttribute);

      return xmlNode;
    }

    #endregion

    #region *** helpers loading configuation from tfs

    private static List<CachedWorkflowFolder> CachedWorkflowFolders = null;

    /// <summary>
    /// Gets the TFS event config.
    /// </summary>
    private List<TFSEvent> GetTFSEventConfigFromTfs(TeamFoundationRequestContext requestContextCollection)
    {
      List<TFSEvent> _tfsEvents = new List<TFSEvent>();

      try
      {
        // services on the collection
        TeamFoundationRegistryService srvRegistryCollection = requestContextCollection.GetService<TeamFoundationRegistryService>();
        #if UsingILocationService
        ILocationService srvLocation = requestContextCollection.GetService<ILocationService>();
        #else
        TeamFoundationLocationService srvLocation = requestContextCollection.GetService<TeamFoundationLocationService>();
        #endif

        // services on the configuration
        #if UsingOrganizationServiceHost
        TeamFoundationRequestContext requestContextConfiguration = requestContextCollection.ServiceHost.OrganizationServiceHost.CreateServicingContext();
        #else
        TeamFoundationRequestContext requestContextConfiguration = requestContextCollection.ServiceHost.ApplicationServiceHost.CreateServicingContext();
        #endif
        TeamFoundationRegistryService srvRegistryConfiguration = requestContextConfiguration.GetService<TeamFoundationRegistryService>();

        // setup a registry notification on collection specific reg keys to refresh cache on registry change
        srvRegistryCollection.RegisterNotification(requestContextCollection, new RegistrySettingsChangedCallback(RegistryChanged_Collection), new string[] 
        { 
          // TODO: verify how to do this for ALL collection, not only for the collection corresponding to the current request
          TfsRegistryPath.WorkflowsCollectionPath,
        });

        // setup a registry notification on configuration specific reg keys to refresh cache on registry change
        srvRegistryConfiguration.RegisterNotification(requestContextConfiguration, new RegistrySettingsChangedCallback(RegistryChanged_Configuration), new string[] 
        { 
          TfsRegistryPath.WorkflowActivitiesGlobalCollection,
          TfsRegistryPath.WorkflowActivitiesGlobalPath,
          TfsRegistryPath.WorkflowsGlobalCollection,
          TfsRegistryPath.WorkflowsGlobalPath,
          TfsRegistryPath.WorkflowsProjectReleativePath,
          TfsRegistryPath.WorkflowsConfigFileNameWebService,
        });

        // unregister notification for change in registry key for sync with other app tiers to avoid recursion
        srvRegistryConfiguration.UnregisterNotification(requestContextConfiguration, RegistryChanged_ConfigurationCacheChangeSet);

        // patch the server uri to get exactly this app tier
#if UsingILocationService
        Uri uriRequestContextCollection = new Uri(srvLocation.GetLocationData(requestContextCollection, Guid.Empty).GetServerAccessMapping(requestContextCollection).AccessPoint);
#else
        Uri uriRequestContextCollection = new Uri(srvLocation.GetServerAccessMapping(requestContextCollection).AccessPoint);
#endif
        UriBuilder uriBuilderRequestContextCollection = new UriBuilder(uriRequestContextCollection.Scheme, System.Environment.MachineName, uriRequestContextCollection.Port, uriRequestContextCollection.PathAndQuery);
        uriRequestContextCollection = uriBuilderRequestContextCollection.Uri;

        // get access to tfs and collection
        Uri uriTfsServer = new Uri(uriRequestContextCollection.AbsoluteUri);
        Uri uriTfsCollection = new Uri(uriRequestContextCollection.AbsoluteUri + "/" + requestContextCollection.ServiceHost.Name);

        // read the configuration registry values
        RegistryValues regValues = ReadTfsEventWorkflowSettings(requestContextConfiguration, srvRegistryConfiguration);
        this.LogInfo(string.Format("{0}: {1}", "WorkflowActivitiesGlobalCollection", regValues.WorkflowActivitiesGlobalCollection));
        this.LogInfo(string.Format("{0}: {1}", "WorkflowActivitiesGlobalPath", regValues.WorkflowActivitiesGlobalPath));
        this.LogInfo(string.Format("{0}: {1}", "WorkflowsGlobalCollection", regValues.WorkflowsGlobalCollection));
        this.LogInfo(string.Format("{0}: {1}", "WorkflowsGlobalPath", regValues.WorkflowsGlobalPath));
        this.LogInfo(string.Format("{0}: {1}", "WorkflowsProjectRelativePath", regValues.WorkflowsProjectRelativePath));
        this.LogInfo(string.Format("{0}: {1}", "WorkflowsConfigFileName", regValues.WorkflowsConfigFileName));

        // local base path for version control workspace mapping
        string pathLocalWorkflowData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\TfsEventWorkflows";

        string strWorkspaceName = "TfsEventWorkflows-On-" + System.Environment.MachineName;
        string strWorkspaceOwner = string.Format(@"{0}\{1}", Environment.UserDomainName, Environment.UserName);
        string strWorkspaceComputer = Environment.MachineName;

        List<CachedWorkflowFolder> workflowFolders = new List<CachedWorkflowFolder>();

        VersionSpec vcVersionSpec = VersionSpec.Latest;
        DeletedState vcDeleteState = DeletedState.NonDeleted;
        ItemType vcItemType = ItemType.Folder;
        WorkingFolderType vcWorkFolderType = WorkingFolderType.Map;
        RecursionType vcRecursionType = RecursionType.Full;

        int idChangeSetMax = 0;

        TeamFoundationCatalogService srvCatalogConfiguration = requestContextConfiguration.GetService<TeamFoundationCatalogService>();

        // get all app tiers
        CatalogNode nodeRootInfrastructure = srvCatalogConfiguration.QueryRootNode(requestContextConfiguration, CatalogTree.Infrastructure);
        var nodesMachine = nodeRootInfrastructure.QueryChildren(requestContextConfiguration,
          new Guid[] { CatalogResourceTypes.Machine }, true, CatalogQueryOptions.None);

        foreach (var nodeMachine in nodesMachine)
        {
          this.LogInfo(string.Format("machine: {0}", nodeMachine.Resource.DisplayName));
          foreach(var prop in  nodeMachine.Resource.Properties)
          {
            this.LogInfo(string.Format("  {0}: {1}", prop.Key, prop.Value));
          }
        }

        // TODO: the next block is a server sided implementaion try for all the version control specific stuff
        // TODO: change logic to access version control only for the current collection (maybe in HostReadyEvent)
        if(false)
        {
          // get all collections
          CatalogNode nodeRootOrganizational = srvCatalogConfiguration.QueryRootNode(requestContextConfiguration, CatalogTree.Organizational);
          var nodesCollection = nodeRootOrganizational.QueryChildren(requestContextConfiguration,
            new Guid[] { CatalogResourceTypes.ProjectCollection }, true, CatalogQueryOptions.None);

          foreach (var nodeCollection in nodesCollection)
          {
            string strCollection = nodeCollection.Resource.DisplayName;
            Guid quidCollection = new Guid(nodeCollection.Resource.Properties["InstanceId"]);

            this.LogInfo(string.Format("*** [{0} ({1})]", strCollection, quidCollection));

            //server sided an active request can only be assigned to one collection -> the following will fail for all other collection then the current
            if(strCollection == requestContextCollection.ServiceHost.Name)
            {
              TeamFoundationHostManagementService service = requestContextConfiguration.GetService<TeamFoundationHostManagementService>();
              TeamFoundationRequestContext rqCollection = service.BeginRequest(requestContextConfiguration, quidCollection, RequestContextType.UserContext, true, true);

              TeamFoundationVersionControlService vcs = rqCollection.GetService<TeamFoundationVersionControlService>();
              int changesetmax = vcs.GetLatestChangeset(rqCollection);
              this.LogInfo(string.Format("*** [{0} ({1})] maxChangeSet: {2}", rqCollection.ServiceHost.Name, rqCollection.ServiceHost.HostType, changesetmax));
              var ws = vcs.QueryWorkspace(rqCollection, strWorkspaceName, strWorkspaceOwner);
              if(null != ws)
              foreach(var f in ws.Folders)
                this.LogInfo(string.Format("*** {0} -> {1}", f.ServerItem, f.LocalItem));
            }
          }
        }

        TfsConfigurationServer tfsConfigurationServer = new TfsConfigurationServer(uriTfsServer);
        var collectionNodes = tfsConfigurationServer.CatalogNode.QueryChildren(
              new[] { CatalogResourceTypes.ProjectCollection }, false, CatalogQueryOptions.None);
        foreach (var collectionNode in collectionNodes)
        {
          // use client object model to access other collections
          string strCollection = collectionNode.Resource.DisplayName;
          Guid quidCollection = new Guid(collectionNode.Resource.Properties["InstanceId"]);
          TfsTeamProjectCollection tfsTeamProjectCollection = tfsConfigurationServer.GetTeamProjectCollection(quidCollection);
          ITeamFoundationRegistry tfsRegistry = tfsTeamProjectCollection.GetService<ITeamFoundationRegistry>();
          VersionControlServer tfsVersionControl = tfsTeamProjectCollection.GetService<VersionControlServer>();

          List<WorkingFolder> newWorkingFolders = new List<WorkingFolder>();

          // map workflow activities
          bool bWorkflowActivitiesGlobalConfigured = (!string.IsNullOrEmpty(regValues.WorkflowActivitiesGlobalCollection)) && !(string.IsNullOrEmpty(regValues.WorkflowActivitiesGlobalPath));
          if (bWorkflowActivitiesGlobalConfigured && (strCollection == regValues.WorkflowActivitiesGlobalCollection))
          {
            string pathServer = string.Format("$/{0}", regValues.WorkflowActivitiesGlobalPath);
            string pathLocal = string.Format(@"{0}\{1}\{2}", pathLocalWorkflowData, strCollection, regValues.WorkflowActivitiesGlobalPath.Replace(@"/", @"\"));

            if (tfsVersionControl.ServerItemExists(pathServer, vcVersionSpec, vcDeleteState, vcItemType))
            {
              newWorkingFolders.Add(new WorkingFolder(pathServer, pathLocal, vcWorkFolderType, vcRecursionType));
              workflowFolders.Add(new CachedWorkflowFolder(
                CachedWorkflowFolderType.WorkflowActivitiesGlobal, null, null, pathServer, pathLocal, strCollection));
            }
          }
          // map global workflows
          bool bWorkflowsGlobalCollectionConfigured = (!string.IsNullOrEmpty(regValues.WorkflowsGlobalCollection)) && !(string.IsNullOrEmpty(regValues.WorkflowsGlobalPath));
          if (bWorkflowsGlobalCollectionConfigured && (strCollection == regValues.WorkflowsGlobalCollection))
          {
            string pathServer = string.Format("$/{0}", regValues.WorkflowsGlobalPath);
            string pathLocal = string.Format(@"{0}\{1}\{2}", pathLocalWorkflowData, strCollection, regValues.WorkflowsGlobalPath.Replace(@"/", @"\"));

            if (tfsVersionControl.ServerItemExists(pathServer, vcVersionSpec, vcDeleteState, vcItemType))
            {
              newWorkingFolders.Add(new WorkingFolder(pathServer, pathLocal, vcWorkFolderType, vcRecursionType));
              workflowFolders.Add(new CachedWorkflowFolder(
                CachedWorkflowFolderType.WorkflowsGlobal, null, null, pathServer, pathLocal, strCollection));
            }
          }

          // map collection workflows
          string WorkflowsCollectionPath = tfsRegistry.GetValue(TfsRegistryPath.WorkflowsCollectionPath, "").Trim(new char[] { '/' });
          this.LogInfo(string.Format("collection: {0} - {1}: {2}", strCollection, "WorkflowsCollectionPath", WorkflowsCollectionPath));
          if (!string.IsNullOrEmpty(WorkflowsCollectionPath))
          {
            string pathServer = string.Format("$/{0}", WorkflowsCollectionPath);
            string pathLocal = string.Format(@"{0}\{1}\{2}", pathLocalWorkflowData, strCollection, WorkflowsCollectionPath.Replace(@"/", @"\"));

            if (tfsVersionControl.ServerItemExists(pathServer, vcVersionSpec, vcDeleteState, vcItemType))
            {
              newWorkingFolders.Add(new WorkingFolder(pathServer, pathLocal, vcWorkFolderType, vcRecursionType));
              workflowFolders.Add(new CachedWorkflowFolder(
                CachedWorkflowFolderType.WorkflowsCollection, strCollection, null, pathServer, pathLocal, strCollection));
            }
          }

          // List the team projects in the collection
          bool bWorkflowsProjectRelativePathConfigured = (!string.IsNullOrEmpty(regValues.WorkflowsProjectRelativePath));
          if (bWorkflowsProjectRelativePathConfigured)
          {
            // get a catalog of team projects for the collection
            var projectNodes = collectionNode.QueryChildren(
                new[] { CatalogResourceTypes.TeamProject }, false, CatalogQueryOptions.None);

            foreach (var projectNode in projectNodes)
            {
              string strProject = projectNode.Resource.DisplayName;
              string pathServer = string.Format("$/{0}/{1}", strProject, regValues.WorkflowsProjectRelativePath);
              string pathLocal = string.Format(@"{0}\{1}\{2}\{3}", pathLocalWorkflowData, strCollection, strProject, regValues.WorkflowsProjectRelativePath.Replace(@"/", @"\"));

              if (tfsVersionControl.ServerItemExists(pathServer, vcVersionSpec, vcDeleteState, vcItemType))
              {
                // map project workflows
                newWorkingFolders.Add(new WorkingFolder(pathServer, pathLocal, vcWorkFolderType, vcRecursionType));
                workflowFolders.Add(new CachedWorkflowFolder(
                  CachedWorkflowFolderType.WorkflowsProject, strCollection, strProject, pathServer, pathLocal, strCollection));
              }
            }
          }

          // create/adjust workspace
          Workspace workspace = null;
          try
          {
            // get the workspace
            workspace = tfsVersionControl.GetWorkspace(strWorkspaceName, strWorkspaceOwner);
            // find changes
            var NewNotInOld = newWorkingFolders.Where(x1 => !workspace.Folders.Any(x2 => x2.ServerItem == x1.ServerItem));
            var OldNotInNew = workspace.Folders.Where(x1 => !newWorkingFolders.Any(x2 => x2.ServerItem == x1.ServerItem));
            // adjust folder mapping
            foreach (var map in OldNotInNew) workspace.DeleteMapping(map);
            foreach (var map in NewNotInOld) workspace.Map(map.ServerItem, map.LocalItem);
          }
          catch (WorkspaceNotFoundException)
          {
            // define workspace paraemters
            CreateWorkspaceParameters cwp = new CreateWorkspaceParameters(strWorkspaceName);
            cwp.Comment = "this workspace was created by TfsEventWorkflows. Please don't modify";
            cwp.Computer = strWorkspaceComputer;
            cwp.Folders = newWorkingFolders.ToArray();
            cwp.Location = WorkspaceLocation.Server;
            cwp.OwnerName = strWorkspaceOwner;
            cwp.WorkspaceName = strWorkspaceName;
            cwp.WorkspaceOptions = WorkspaceOptions.SetFileTimeToCheckin;
            // create new workspace
            workspace = tfsVersionControl.CreateWorkspace(cwp);
          }

          if (newWorkingFolders.Count > 0)
          {
            // TODO: verify this get - there is a problem if one of the custom activity assemblies is loaded and so blocked
            // get latest version
            var srvItems = newWorkingFolders.ConvertAll<string>(x => x.ServerItem).ToArray();
            GetStatus status = workspace.Get(srvItems, Microsoft.TeamFoundation.VersionControl.Client.VersionSpec.Latest, Microsoft.TeamFoundation.VersionControl.Client.RecursionType.Full, Microsoft.TeamFoundation.VersionControl.Client.GetOptions.Overwrite);

            newWorkingFolders.ForEach(folder =>
            {
              var hist = tfsVersionControl.QueryHistory(folder.ServerItem, RecursionType.Full, 1);
              if (hist.Count() > 0)
              {
                int idChangeset = hist.First().ChangesetId;
                idChangeSetMax = Math.Max(idChangeSetMax, idChangeset);
              }
            });
          }
        }

        // TODO: verify ChangeSet Caching and reloading on changes !!!!!!!!!!!!!!!!
        // PROBLEM 1: in the implementation above we look for the highest changeset over all collections, but each collaction has its own change set
        //            below we store only ONE change set in the configuration registry for all collections
        //            possible causes -> not all changes trigger a reload on another app tier -> logical problem
        // PROBLEM 2: multithreading - can a reload be triggered if more than one WorkflowerRunner instance exist in this process
        //            possible causes -> the plugin reloads itself, performance problem

        // write idChangeSetMax to registry ==> this triggers a notification on all other app tiers
        CacheChangeSetId = idChangeSetMax;
        if (CacheChangeSetId > ReadTfsEventWorkflowCacheChangeSet(requestContextConfiguration, srvRegistryConfiguration))
          WriteTfsEventWorkflowCacheChangeSet(requestContextConfiguration, srvRegistryConfiguration, CacheChangeSetId);

        // register notification for change in registry key for sync with other app tiers
        srvRegistryConfiguration.RegisterNotification(requestContextCollection, new RegistrySettingsChangedCallback(RegistryChanged_ConfigurationCacheChangeSet), new string[] 
        { 
          TfsRegistryPath.CacheChangeSet,
        });

        // read all config files
        foreach (CachedWorkflowFolder workflowFolder in workflowFolders)
        {
          switch (workflowFolder.FolderType)
          {
            default:
              break;
            case CachedWorkflowFolderType.WorkflowActivitiesGlobal:
              break;
            case CachedWorkflowFolderType.WorkflowsGlobal:
            case CachedWorkflowFolderType.WorkflowsCollection:
            case CachedWorkflowFolderType.WorkflowsProject:
              string strConfigFile = System.IO.Path.Combine(workflowFolder.PathLocal, regValues.WorkflowsConfigFileName);
              if (System.IO.File.Exists(strConfigFile))
              {
                List<TFSEvent> __tfsEvents = GetTFSEventConfigFromFile(strConfigFile);
                __tfsEvents.ForEach(x =>
                {
                  x.WorkflowFileName = System.IO.Path.Combine(workflowFolder.PathLocal, System.IO.Path.GetFileName(x.WorkflowFileName));

                  // overwrite project/collection to ensure collection/project specific workflow are not executed for other projects
                  if(workflowFolder.FolderType == CachedWorkflowFolderType.WorkflowsProject)
                  {
                    x.Collection = workflowFolder.Collection;
                    x.Project = workflowFolder.Project;
                  }
                  else if (workflowFolder.FolderType == CachedWorkflowFolderType.WorkflowsCollection)
                  {
                    x.Collection = workflowFolder.Collection;
                  }
                });
                _tfsEvents.AddRange(__tfsEvents);
              }
              break;
          }
        }

        CachedWorkflowFolders = workflowFolders;

        requestContextConfiguration.Dispose();
      }
      catch (Exception ex)
      {
        this.LogError(string.Format("Loading error:"), ex);
      }

      this.LogInfo(string.Format("cached server paths 1"));
      foreach (var cachedWorkflowFolder in CachedWorkflowFolders)
        this.LogInfo(string.Format("  cached server path - {0}", cachedWorkflowFolder.PathServer));


      return _tfsEvents;
    }

#endregion

#region *** helpers for loading configuration from file

    /// <summary>
    /// Gets the TFS event config.
    /// </summary>
    private List<TFSEvent> GetTFSEventConfigFromFile()
    {
      // load the config
      string strPluginFile = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
      string strConfigFile = string.Format("{0}.config", strPluginFile);
      return GetTFSEventConfigFromFile(strConfigFile, strPluginFile);
    }

    /// <summary>
    /// Gets the TFS event config.
    /// </summary>
    private List<TFSEvent> GetTFSEventConfigFromFile(string strConfigFile)
    {
      // load the config
      string strPluginFile = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
      return GetTFSEventConfigFromFile(strConfigFile, strPluginFile);
    }

    /// <summary>
    /// Gets the TFS event config.
    /// </summary>
    private List<TFSEvent> GetTFSEventConfigFromFile(string strConfigFile, string strPluginFile)
    {
      List<TFSEvent> _tfsEvents = new List<TFSEvent>();

      Configuration config = ConfigurationManager.OpenExeConfiguration(strPluginFile);

      TFSEventConfig tfsEventConfig;
      try
      {
        tfsEventConfig = (TFSEventConfig)config.GetSection("tfsEventConfig");
      }
      catch (ConfigurationErrorsException)
      {
        // if the config can not be loaded try to load it with reflection
        tfsEventConfig = GetCustomConfig<TFSEventConfig>(strPluginFile, strConfigFile, "tfsEventConfig");
      }

      tfsEventConfig = GetCustomConfig<TFSEventConfig>(strPluginFile, strConfigFile, "tfsEventConfig");
                
      _tfsEvents = (from TFSEventElement tfsEventCollection in tfsEventConfig.TFSEvents
                        select
                            new TFSEvent
                                {
                                    Collection = tfsEventCollection.Collection,
                                    Project = tfsEventCollection.Project,
                                    FullTypeName = tfsEventCollection.FullTypeName,
                                    EventAssemblyName = tfsEventCollection.EventAssemblyName,
                                    WorkflowFileName = tfsEventCollection.WorkflowFileName,
                                    ConfigFileName = strConfigFile,
                                    Disabled = tfsEventCollection.Disabled,
                                    Async = tfsEventCollection.Async,
                                    Trace = tfsEventCollection.Trace,
                                    EventType = null,
                                    Name = tfsEventCollection.Name
                                }).ToList();

      // set the full type
      foreach (TFSEvent tfsEvent in _tfsEvents)
      {
          Assembly externalAssembly = Assembly.Load(tfsEvent.EventAssemblyName);
          tfsEvent.EventType = externalAssembly.GetType(tfsEvent.FullTypeName);

          if (!Path.IsPathRooted(tfsEvent.WorkflowFileName))
            tfsEvent.WorkflowFileName = ExecutionPath.Directory.FullName + "\\" + tfsEvent.WorkflowFileName;
      }
          
      return _tfsEvents;
    }

#endregion

#region *** config and assmebly loading helpers ***

    /// <summary>
    /// Gets the custom config.
    /// </summary>
    /// <typeparam name="TConfig">The type of the config.</typeparam>
    /// <param name="configDefiningAssemblyPath">The config defining assembly path.</param>
    /// <param name="configFilePath">The config file path.</param>
    /// <param name="sectionName">Name of the section.</param>
    /// <returns></returns>
    private TConfig GetCustomConfig<TConfig>(string configDefiningAssemblyPath, string configFilePath, string sectionName) where TConfig : ConfigurationSection
    {
      TConfig returnConfig = null;
      try
      {
        AppDomain.CurrentDomain.AssemblyResolve += this.ConfigResolveEventHandler;
        configurationDefiningAssembly = Assembly.LoadFrom(configDefiningAssemblyPath);
        var exeFileMap = new ExeConfigurationFileMap();
        exeFileMap.ExeConfigFilename = configFilePath;
        var customConfig = ConfigurationManager.OpenMappedExeConfiguration(exeFileMap, ConfigurationUserLevel.None);
        returnConfig = customConfig.GetSection(sectionName) as TConfig;
        AppDomain.CurrentDomain.AssemblyResolve -= ConfigResolveEventHandler;
      }
      catch (Exception ex)
      {
        this.LogError(string.Format("GetCustomConfig failed for section '{0}' in .config '{1}'", sectionName, configFilePath), ex);
      }

      return returnConfig;
    }

    private Assembly ConfigResolveEventHandler(object sender, ResolveEventArgs args)
    {
        return configurationDefiningAssembly;
    }

    private Assembly BinariesResolveEventHandler(object sender, ResolveEventArgs args)
    {
      CachedWorkflowFolder cachedWorkFolder = CachedWorkflowFolders.Find(x => x.FolderType == CachedWorkflowFolderType.WorkflowActivitiesGlobal);
      if(null == cachedWorkFolder) return null;

      string strWorkflowActivitiesGlobal = cachedWorkFolder.PathLocal;
      if(string.IsNullOrEmpty(strWorkflowActivitiesGlobal)) return null;
          
      // get all assemblies from WorkflowActivitiesGlobal (reflection only)
      Assembly[] cachedAssemblies = System.IO.Directory.EnumerateFiles(strWorkflowActivitiesGlobal).Select(x => 
      {
        try { return Assembly.ReflectionOnlyLoadFrom(x); }
        catch (Exception) { return null; }
      }).Where(x => x!=null).ToArray();

      // load the requested assembly
      Assembly cachedAssembly = cachedAssemblies.FirstOrDefault(x => { return x.GetName().Name == args.Name; });
      if (null != cachedAssembly)
      {
        this.LogInfo(string.Format("Loading Assembly [{0}]", cachedAssembly.Location));
        return Assembly.LoadFrom(cachedAssembly.Location);
      }

      return null;
    }

    /// <summary>
    /// Gets the workflow.
    /// </summary>
    /// <param name="workflowFileName">The workflow file.</param>
    /// <returns></returns>
    private Activity GetWorkflow(string workflowFileName)
    {
        // check whether file can be found as given in config
        if (!File.Exists(workflowFileName))
        {
            workflowFileName = ExecutionPath.Directory.FullName + "\\" + workflowFileName;
        }

        if (!File.Exists(workflowFileName))
        {
            throw new IOException(string.Format("Workflow file not found: {0}", workflowFileName));
        }

        Activity workflow = null;
        if (workflowFileName.EndsWith(".xaml"))
        {
            // for dynamic load from xaml
            workflow = ActivityXamlServices.Load(workflowFileName);
            //CheckWorkflowArguments(workflow as DynamicActivity);
            var d = workflow as DynamicActivity;
        }
        else if (workflowFileName.EndsWith(".dll"))
        {
            // for dynamic load from dll
            Assembly assm = Assembly.LoadFrom(workflowFileName);
            Type workflowType = null;
            foreach (Type typeInAssembly in assm.GetTypes())
            {
                if (typeInAssembly.BaseType == typeof(Activity))
                {
                    //CheckWorkflowArguments(typeInAssembly);
                    workflowType = typeInAssembly;
                    break;
                }
            }
            workflow = (Activity)assm.CreateInstance(workflowType.ToString());
        }

        return workflow;
    }

#endregion

#region *** tfs registry helpers ***

    private struct RegistryValues
    {
      // workflow activities are stored in a central folder in the version control and distributed to each application tier
      public string WorkflowActivitiesGlobalCollection;
      public string WorkflowActivitiesGlobalPath;

      // global workflows are stored in a central folder in the version control and distributed to each application tier
      public string WorkflowsGlobalCollection;
      public string WorkflowsGlobalPath;

      // collection specific workflows are stored in a central folder in the version control and distributed to each application tier
      // public string WorkflowsCollectionPath; // stored in collection registry, not in configuration registry

      // collection and project specific workflows are stored in a central folder in the version control and distributed to each application tier
      public string WorkflowsProjectRelativePath;

      // name of the confif file - only workflows configured in this file are triggered
      public string WorkflowsConfigFileName;
    }

    // TFS registry paths
    class TfsRegistryPath
    {
      public const string Root = "/Configuration/Application/TfsEventWorkflows";
      public const string CacheChangeSet = Root + "/CacheChangeSet";
      public const string WorkflowActivitiesGlobalCollection = Root + "/WorkflowActivities/Global/Collection";
      public const string WorkflowActivitiesGlobalPath = Root + "/WorkflowActivities/Global/Path";
      public const string WorkflowsGlobalCollection = Root + "/Workflows/Global/Collection";
      public const string WorkflowsGlobalPath = Root + "/Workflows/Global/Path";
      public const string WorkflowsCollectionPath = Root + "/Workflows/Collection/Path";
      public const string WorkflowsProjectReleativePath = Root + "/Workflows/Project/PathRelative";
      public const string WorkflowsConfigFileNameWebService = Root + "/Workflows/ConfigFileName/WebService";
      public const string WorkflowsConfigFileNameJobAgent = Root + "/Workflows/ConfigFileName/JobAgent";
    }

    // read all configuation values
    private RegistryValues ReadTfsEventWorkflowSettings(TeamFoundationRequestContext requestContext, TeamFoundationRegistryService srvRegistry)
    {
      RegistryValues regValues = new RegistryValues()
      {
        WorkflowActivitiesGlobalCollection = ReadTfsEventWorkflowSetting(requestContext, srvRegistry, TfsRegistryPath.WorkflowActivitiesGlobalCollection),
        WorkflowActivitiesGlobalPath = ReadTfsEventWorkflowSetting(requestContext, srvRegistry, TfsRegistryPath.WorkflowActivitiesGlobalPath),

        WorkflowsGlobalCollection = ReadTfsEventWorkflowSetting(requestContext, srvRegistry, TfsRegistryPath.WorkflowsGlobalCollection),
        WorkflowsGlobalPath = ReadTfsEventWorkflowSetting(requestContext, srvRegistry, TfsRegistryPath.WorkflowsGlobalPath),
        WorkflowsProjectRelativePath = ReadTfsEventWorkflowSetting(requestContext, srvRegistry, TfsRegistryPath.WorkflowsProjectReleativePath),
        WorkflowsConfigFileName = ReadTfsEventWorkflowSetting(requestContext, srvRegistry, InJobAgent ? TfsRegistryPath.WorkflowsConfigFileNameJobAgent : TfsRegistryPath.WorkflowsConfigFileNameWebService),
      };

      return regValues;
    }

    private string ReadTfsEventWorkflowSetting(TeamFoundationRequestContext requestContext, TeamFoundationRegistryService srvRegistry, string strKey)
    {
      string strValue = srvRegistry.GetValue(requestContext, strKey, "");
      if(string.IsNullOrEmpty(strValue))
        return "";

      return strValue.Trim(new char[] { '/' });
    }

    // read cache change set id
    private int ReadTfsEventWorkflowCacheChangeSet(TeamFoundationRequestContext requestContext, TeamFoundationRegistryService srvRegistry)
    {
      return srvRegistry.GetValue<int>(requestContext, TfsRegistryPath.CacheChangeSet, -1);
    }

    // write cache change set id
    private void WriteTfsEventWorkflowCacheChangeSet(TeamFoundationRequestContext requestContext, TeamFoundationRegistryService srvRegistry, int idChangeSet)
    {
      srvRegistry.SetValue(requestContext, TfsRegistryPath.CacheChangeSet, idChangeSet);
    }

    private void RegistryChanged_Collection(TeamFoundationRequestContext requestContext, RegistryEntryCollection entries)
    {
      foreach (var entry in entries)
        this.LogInfo(string.Format("registry changed (collection) - {0} [{1}]", entry.Value, entry.Path));

      // someone has changed the collection registry under the TfsEventWorkflows hive
      TriggerReloadPlugin();
    }

    private void RegistryChanged_Configuration(TeamFoundationRequestContext requestContext, RegistryEntryCollection entries)
    {
      foreach (var entry in entries)
        this.LogInfo(string.Format("registry changed (configuration) - {0} [{1}]", entry.Value, entry.Path));

      // someone has changed the configuation registry under the TfsEventWorkflows hive
      TriggerReloadPlugin();
    }

    private void RegistryChanged_ConfigurationCacheChangeSet(TeamFoundationRequestContext requestContext, RegistryEntryCollection entries)
    {
      foreach (var entry in entries)
        this.LogInfo(string.Format("registry changed (configuration) - {0} [{1}]", entry.Value, entry.Path));

      // another application tier has detected a change of the cached files on the version control server
      TriggerReloadPlugin();
    }

#endregion

#region workflow tracking

    // A custom tracking participant that emits TrackingRecord objects to the console
    private class TfsEventWorkflowsTrackingParticipant : TrackingParticipant
    {
      // CurrentWorkflow - must be set before executing the workflow
      public Activity CurrentWorkflow = null;

      public TfsEventWorkflowsTrackingParticipant()
      {
      }

      protected override void Track(TrackingRecord record, TimeSpan timeout)
      {
        // log kind of tracking record
        // this.LogInfo(string.Format("  {0} ({1})", record.GetType().Name, record.RecordNumber));

        // workflow tracking
        WorkflowInstanceRecord workflowInstanceRecord = record as WorkflowInstanceRecord;
        if (workflowInstanceRecord != null)
        {
          this.LogInfo(string.Format("  > {0} - {1} ({2})", "workflow", workflowInstanceRecord.State, workflowInstanceRecord.InstanceId));
        } // end of workflow tracking

        // activity tracking
        ActivityStateRecord activityStateRecord = record as ActivityStateRecord;
        if (activityStateRecord != null)
        {
          // get arguments and variables
          IDictionary<String, object> arguments = activityStateRecord.Arguments;
          IDictionary<String, object> variables = activityStateRecord.Variables;

          // get activity and log general inforamtion
          Activity act = WorkflowInspectionServices.Resolve(CurrentWorkflow, activityStateRecord.Activity.Id);
          bool IsInDirection = (activityStateRecord.State == "Executing");
          this.LogInfo(string.Format("    {0} {1} ({2})", IsInDirection ? ">" : "<", activityStateRecord.Activity.Name, activityStateRecord.State));

          Type typeAcivity = act.GetType();
          if(typeAcivity == typeof(DynamicActivity))
          {
            // DynamicActivity store information about the arguments in Properties
            DynamicActivity da = act as DynamicActivity;
            foreach (var prop in da.Properties)
            {
              Argument arg = prop.Value as Argument;
              if (arg != null)
              {
                string stringType = arg.ArgumentType.Name;
                string stringProperty = prop.Name;
                object objectValue = arguments.ContainsKey(stringProperty) ? arguments[stringProperty] : null;
                string stringValue = (objectValue == null) ? "null" : objectValue.ToString().Replace("\r\n", " - ");

                // dump in respect to the direction
                if (IsInDirection && arg.Direction == ArgumentDirection.In || arg.Direction == ArgumentDirection.InOut)
                  this.LogInfo(string.Format("    ->{0} {1}: {2}", stringType, stringProperty, stringValue));
                if (!IsInDirection && arg.Direction == ArgumentDirection.Out || arg.Direction == ArgumentDirection.InOut)
                  this.LogInfo(string.Format("    <-{0} {1}: {2}", stringType, stringProperty, stringValue));
              }
            }
          }
          else
          {
            // all other activities use reflection to get the arguments
            foreach(var prop in typeAcivity.GetProperties())
            {
              Argument arg = prop.GetValue(act) as Argument;
              if(arg != null)
              {
                string stringType = arg.ArgumentType.Name;
                string stringProperty = prop.Name;
                object objectValue = arguments.ContainsKey(stringProperty) ? arguments[stringProperty] : null;
                string stringValue = (objectValue == null) ? "null" : objectValue.ToString().Replace("\r\n", " - ");

                // dump in respect to the direction
                if (IsInDirection && arg.Direction == ArgumentDirection.In || arg.Direction == ArgumentDirection.InOut)
                  this.LogInfo(string.Format("    ->{0} {1}: {2}", stringType, stringProperty, stringValue));
                if (!IsInDirection && arg.Direction == ArgumentDirection.Out || arg.Direction == ArgumentDirection.InOut)
                  this.LogInfo(string.Format("    <-{0} {1}: {2}", stringType, stringProperty, stringValue));
              }
            }
          }

          // dump variables defined for this activity
          if (variables.Count > 0)
          {
            foreach (KeyValuePair<string, object> variable in variables)
            {
              this.LogInfo(string.Format("      {0}: {1}", variable.Key, variable.Value));
            }
          }
        } // end of activity tracking
      }
    }

    private TfsEventWorkflowsTrackingParticipant _customTrackingParticipant = null;
    private TfsEventWorkflowsTrackingParticipant customTrackingParticipant
    {
      get
      {
        if(null == _customTrackingParticipant)
        {
          const String all = "*";
          _customTrackingParticipant = new TfsEventWorkflowsTrackingParticipant()
          {
            // Create a tracking profile to subscribe for tracking records
            // In this sample the profile subscribes for CustomTrackingRecords,
            // workflow instance records and activity state records
            TrackingProfile = new TrackingProfile()
            {
              Name = "CustomTrackingProfile",
              Queries =
                    {
                        new CustomTrackingQuery()
                        {
                         Name = all,
                         ActivityName = all
                        },
                        new WorkflowInstanceQuery()
                        {
                            // Limit workflow instance tracking records for started and completed workflow states
                            States = { WorkflowInstanceStates.Started, WorkflowInstanceStates.Completed },
                        },
                        new ActivityStateQuery()
                        {
                            // Subscribe for track records from all activities for all states
                            ActivityName = all,
                            States = { all },

                            // Extract workflow variables and arguments as a part of the activity tracking record
                            // VariableName = "*" allows for extraction of all variables in the scope
                            // of the activity
                            Variables =
                            {
                                { all }
                            },

                            Arguments =
                            {
                              { all }
                            }
                        }
                    }
            }
          };
        }
        return _customTrackingParticipant;
      }
    }

#endregion


  }
}
