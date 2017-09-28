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

    public class WorkFlowRunnerServerPlugin : ISubscriber
    {
        #region Constants and Fields

        private readonly WorkflowRunner workflowRunner;

        #endregion

        #region Constructors and Destructors

        public WorkFlowRunnerServerPlugin()
        {
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

            // we only handle notifications
            if (notificationType != NotificationType.Notification)
            {
                return EventNotificationStatus.ActionPermitted;
            }

            string strConfigFile = this.workflowRunner.ExecutionPath.FullName + ".config";
            XmlConfigurator.Configure(new Uri(strConfigFile));

            if (notificationEventArgs.GetType() == typeof(WorkItemChangedEvent))
            {
                // run workflow asynchronously in a TFS job

                var workItemChangedEvent = (WorkItemChangedEvent)notificationEventArgs;

                var xmlData = WorkItemChangedEventSerializer.SerializeXml(workItemChangedEvent);

                this.LogInfo(string.Format("Queuing Job for WorkitemChangedEvent"));

                // Handle the notification by queueing the information we need for a job
                var jobService = requestContext.GetService<TeamFoundationJobService>();
                jobService.QueueOneTimeJob(
                    requestContext,
                    "TFSEventWorkflow Job",
                    "artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin.WorkflowRunnerJob",
                    xmlData,
                    false);
            }
            else
            {
                // run workflow synchronously in the Server PLugin itself
                return this.workflowRunner.ProcessEvent(requestContext, notificationEventArgs);
            }

            return EventNotificationStatus.ActionPermitted;
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