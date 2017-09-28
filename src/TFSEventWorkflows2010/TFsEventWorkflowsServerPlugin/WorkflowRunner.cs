namespace artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin
{
    using System;
    using System.Activities;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.TeamFoundation.Framework.Server;

    using artiso.TFSEventWorkflows.LoggingLib;

    using log4net.Config;

    internal class WorkflowRunner
    {
        #region Constants and Fields

        private FileInfo executionPath;

        private List<TFSEvent> tfsEvents;

        #endregion

        #region Constructors and Destructors

        public WorkflowRunner()
        {
            this.GetTFSEventConfig();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the execution path of the assembly.
        /// </summary>
        /// <value>The execution path.</value>
        public FileInfo ExecutionPath
        {
            get
            {
                if (this.executionPath == null)
                {
                    this.executionPath = new FileInfo(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                }
                return this.executionPath;
            }
        }

        #endregion

        #region Public Methods and Operators

        public EventNotificationStatus ProcessEvent(TeamFoundationRequestContext requestContext, object notificationEventArgs)
        {
            foreach (TFSEvent tfsEvent in this.tfsEvents)
            {
                if (notificationEventArgs.GetType() == tfsEvent.EventType)
                {
                    try
                    {
                        // get the name of the workflow file
                        var workflow = this.GetWorkflow(tfsEvent.WorkflowFileName);

                        // initialize the workflow arguments
                        var workflowParameters = new Dictionary<string, object>
                            {
                                { "TFSEvent", notificationEventArgs },
                                { "TeamFoundationRequestContext", requestContext }
                            };

                        // invoke the workflow
                        this.LogInfo(
                            string.Format(
                                "Starting event {0} with workflow file {1}", tfsEvent.Name, tfsEvent.WorkflowFileName));
                        WorkflowInvoker.Invoke(workflow, workflowParameters);
                        this.LogInfo(
                            string.Format(
                                "Leave event {0} with workflow file {1}", tfsEvent.Name, tfsEvent.WorkflowFileName));
                    }
                    catch (Exception e)
                    {
                        this.LogError(string.Format("Workflow error:"), e);
                    }
                }
            }

            return EventNotificationStatus.ActionPermitted;
        }

        /// <summary>
        /// Subscribeds the types.
        /// </summary>
        /// <returns></returns>
        public Type[] SubscribedTypes()
        {
            return this.tfsEvents.Select(x => x.EventType).ToArray();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the TFS event config.
        /// </summary>
        private void GetTFSEventConfig()
        {
            if (this.tfsEvents == null)
            {
                this.tfsEvents = new List<TFSEvent>();

                // load the config
                string strPluginFile = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
                Configuration config = ConfigurationManager.OpenExeConfiguration(strPluginFile);
                var tfsEventConfig = (TFSEventConfig)config.GetSection("tfsEventConfig");
                this.tfsEvents = (from TFSEventElement tfsEventCollection in tfsEventConfig.TFSEvents
                                  select
                                      new TFSEvent
                                          {
                                              FullTypeName = tfsEventCollection.FullTypeName,
                                              EventAssemblyName = tfsEventCollection.EventAssemblyName,
                                              WorkflowFileName = tfsEventCollection.WorkflowFileName,
                                              EventType = null,
                                              Name = tfsEventCollection.Name
                                          }).ToList();

                // set the full type
                foreach (TFSEvent tfsEvent in this.tfsEvents)
                {
                    Assembly externalAssembly = Assembly.Load(tfsEvent.EventAssemblyName);
                    tfsEvent.EventType = externalAssembly.GetType(tfsEvent.FullTypeName);
                }
            }
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
                workflowFileName = this.ExecutionPath.Directory.FullName + "\\" + workflowFileName;
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
    }
}