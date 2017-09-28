namespace artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin
{
    using System;
    using System.IO;
    using System.Xml;

    using artiso.TFSEventWorkflows.LoggingLib;

    using log4net;
    using log4net.Appender;
    using log4net.Config;
    using log4net.Repository.Hierarchy;

    using Microsoft.TeamFoundation.Framework.Server;
    using Microsoft.TeamFoundation.WorkItemTracking.Server;

    public class WorkflowRunnerJob : ITeamFoundationJobExtension
    {
        #region Static Fields

        private static readonly object JobLock = new object();

        #endregion

        #region Fields

        private readonly WorkflowRunner workflowRunner;

        #endregion

        #region Constructors and Destructors

        public WorkflowRunnerJob()
        {
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
                string strConfigFile = this.workflowRunner.ExecutionPath.FullName + ".config";
                XmlConfigurator.Configure(new Uri(strConfigFile));
                AppendSuffixToLoggingPath("_JobAgent");

                this.LogInfo(string.Format("Enter Job for WorkitemChangedEvent"));

                lock (JobLock)
                {
                    XmlNode xmlData = jobDefinition.Data;
                    WorkItemChangedEvent workItemChangedEvent = WorkItemChangedEventSerializer.DeserializeXml(xmlData);
                    this.workflowRunner.ProcessEvent(requestContext, workItemChangedEvent);
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

            this.LogInfo(string.Format("Leave Job for WorkitemChangedEvent"));

            return TeamFoundationJobExecutionResult.Succeeded;
        }

        #endregion

        #region Methods

        private static void AppendSuffixToLoggingPath(string suffix)
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            foreach (var appender in hierarchy.Root.Appenders)
            {
                if (appender is FileAppender)
                {
                    var fileAppender = (FileAppender)appender;
                    string logFileLocation;

                    var fileInfo = new FileInfo(fileAppender.File);
                    if (fileInfo.Name.Contains("."))
                    {
                        logFileLocation = fileInfo.FullName.Insert(fileInfo.FullName.LastIndexOf('.'), suffix);
                    }
                    else
                    {
                        logFileLocation = string.Format("{0}_{1}", fileInfo.FullName, suffix);
                    }

                    fileAppender.File = logFileLocation;
                    fileAppender.ActivateOptions();
                    break;
                }
            }
        }

        #endregion
    }
}