using log4net.Config;

[assembly: XmlConfigurator(Watch = true)]

namespace artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    using Microsoft.TeamFoundation.Common;
    using Microsoft.TeamFoundation.Framework.Server;
    using Microsoft.TeamFoundation.WorkItemTracking.Server;

    using artiso.TFSEventWorkflows.LoggingLib;

    #if UsingIVssRequestContext
    using TeamFoundationRequestContext = Microsoft.TeamFoundation.Framework.Server.IVssRequestContext;
    #else
    using TeamFoundationRequestContext = Microsoft.TeamFoundation.Framework.Server.TeamFoundationRequestContext;
    #endif

  public class WorkFlowRunnerServerPlugin : ISubscriber
    {
      /*
       * ISubscriber - Web Services
       * (S) - startup - called under the following conditions:
       *     - timeout of app pool (std: 20min)
       *     - change in the plugin directory
       * (E) - execution of event - called under the following conditions:
       *     - event was triggered
       *     
       * call order
       * - ctor            (S)
       * - SubScribedTypes (S)
       * - Name            (S) (E)
       * - ProcessEvent    (S) (E)
       * 
      */
        #region Constants and Fields

        private readonly PluginConfig pluginConfig;
        private readonly WorkflowRunner workflowRunner;

        #endregion

        #region Constructors and Destructors

        public WorkFlowRunnerServerPlugin()
        {
            this.pluginConfig = PluginConfig.Config;
            this.workflowRunner = new WorkflowRunner();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return "TFSEventWorkflow";
            }
        }

        /// <summary>
        /// Gets the priority.
        /// </summary>
        /// <value>The priority.</value>
        public SubscriberPriority Priority
        {
            get
            {
                return SubscriberPriority.Normal;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Processes the event.
        /// </summary>
        /// <param name="requestContext">The request context.</param>
        /// <param name="notificationType">Type of the notification.</param>
        /// <param name="notificationEventArgs">The notification event args.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="statusMessage">The status message.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public EventNotificationStatus ProcessEvent(
            TeamFoundationRequestContext requestContext,
            NotificationType notificationType,
            object notificationEventArgs,
            out int statusCode,
            out string statusMessage,
            out ExceptionPropertyCollection properties)
        {
            statusCode = 0;
            statusMessage = "TFSEventWorkflow executed successfully";
            properties = null;

            
            // run workflow synchronously in the Server Plugin itself
            return this.workflowRunner.ProcessEvent(requestContext, notificationType, notificationEventArgs, PluginConfig.Config.InJobAgent, false);
        }

        /// <summary>
        /// Subscribeds the types.
        /// </summary>
        /// <returns></returns>
        public Type[] SubscribedTypes()
        {
            return this.workflowRunner.SubscribedTypes();
        }

#endregion
    }
}