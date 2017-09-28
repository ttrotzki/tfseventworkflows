namespace artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    using artiso.TFSEventWorkflows.LoggingLib;

    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Repository.Hierarchy;

    using Microsoft.TeamFoundation.Build.Server;
    using Microsoft.TeamFoundation.Framework.Server;
    using Microsoft.TeamFoundation.WorkItemTracking.Server;

    #if UsingIVssRequestContext
    using TeamFoundationRequestContext = Microsoft.TeamFoundation.Framework.Server.IVssRequestContext;
    #else
    using TeamFoundationRequestContext = Microsoft.TeamFoundation.Framework.Server.TeamFoundationRequestContext;
    #endif

    public class WorkflowRunnerJob : ITeamFoundationJobExtension
    {
      /*
       * ITeamFoundationJobExtension - Job Agrnt
       * (E) - execution of job - called under the following conditions:
       *     - event was queued by the Web Service
       *     
       * call order
       * - ctor            (E)
       * - Run             (E)
       * 
      */
      #region Static Fields

      private static readonly object JobLock = new object();

        #endregion

        #region Fields

        private readonly PluginConfig pluginConfig;
        private readonly WorkflowRunner workflowRunner;

        #endregion

        #region Constructors and Destructors

        public WorkflowRunnerJob()
        {
            this.pluginConfig = PluginConfig.Config;
            this.workflowRunner = new WorkflowRunner();
        }

        #endregion

        #region Public Methods and Operators

        public TeamFoundationJobExecutionResult Run(
            TeamFoundationRequestContext requestContext,
            TeamFoundationJobDefinition jobDefinition,
            DateTime queueTime,
            out string resultMessage)
        {
            resultMessage = string.Empty;
            try
            {
                this.LogInfo(string.Format("Enter Job"));

                lock (JobLock)
                {
                    XmlNode xmlData = jobDefinition.Data;
                    object notificationEventArgs = DeserializeXml(xmlData);
                    this.workflowRunner.ProcessEvent(requestContext, NotificationType.Notification, notificationEventArgs, true, true);
                }
            }
            catch (RequestCanceledException e)
            {
                this.LogError(string.Format("Workflow cancel: "), e);
                return TeamFoundationJobExecutionResult.Stopped;
            }
            catch (Exception e)
            {
                this.LogError(string.Format("Workflow error: "), e);
                resultMessage = e.ToString();
                return TeamFoundationJobExecutionResult.Failed;
            }

            this.LogInfo(string.Format("Leave Job"));

            return TeamFoundationJobExecutionResult.Succeeded;
        }

        #endregion

        #region Methods

        private static object DeserializeXml(XmlNode xmlData)
        {
          string strAssemblyQualifiedName = xmlData.Attributes.GetNamedItem("assemblyQualifiedName").Value;
          string [] arrAssemblyQualifiedNames = strAssemblyQualifiedName.Split(new char[] { '|' });
          Type typeNotificationEventArgs = System.Type.GetType(arrAssemblyQualifiedNames[0]);
          Type[] typesAddional = new Type[arrAssemblyQualifiedNames.Length - 1];
          for (int index = 1; index < arrAssemblyQualifiedNames.Length; index++)
            typesAddional[index - 1] = System.Type.GetType(arrAssemblyQualifiedNames[index]);
          var xmlNodeReader = new XmlNodeReader(xmlData);
          var xmlSerializer = new XmlSerializer(typeNotificationEventArgs, typesAddional);
          object notificationEventArgs = xmlSerializer.Deserialize(xmlNodeReader);
          xmlNodeReader.Close();

          return notificationEventArgs;
        }

        #endregion
    }
}