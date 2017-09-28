using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using Microsoft.TeamFoundation.Build.Server;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Framework.Server;

using artiso.TFSEventWorkflows.LoggingLib;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{
  #if UsingIVssRequestContext
  using TeamFoundationRequestContext = Microsoft.TeamFoundation.Framework.Server.IVssRequestContext;
  #else
  using TeamFoundationRequestContext = Microsoft.TeamFoundation.Framework.Server.TeamFoundationRequestContext;
  #endif

  public sealed class QueueToJobAgent : CodeActivity
  {
    [RequiredArgument]
    public InArgument<object> notificationEventArgs { get; set; }

    [RequiredArgument]
    public InArgument<TeamFoundationRequestContext> requestContext { get; set; }

    protected override void Execute(CodeActivityContext context)
    {
      object notificationEventArgs = context.GetValue(this.notificationEventArgs);
      XmlNode xmlData = SerializeXml(notificationEventArgs);
      TeamFoundationRequestContext requestContext = context.GetValue(this.requestContext);

      // Handle the notification by queueing the information we need for a job
      var jobService = requestContext.GetService<TeamFoundationJobService>();
      var jobGuid = jobService.QueueOneTimeJob(
          requestContext,
          "TFSEventWorkflow Job",
          "artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin.WorkflowRunnerJob",
          xmlData,
          false);
      int result = jobService.QueueJobsNow(requestContext, new List<Guid>() { jobGuid }, true);
    }

    private XmlNode SerializeXml(object notificationEventArgs)
    {
      if (null == notificationEventArgs)
      {
        return null;
      }

      Type typeNotificationEventArgs = notificationEventArgs.GetType();
      var xmlSerializer = new XmlSerializer(typeNotificationEventArgs);
      var memoryStream = new MemoryStream();
      xmlSerializer.Serialize(memoryStream, notificationEventArgs);

      memoryStream.Position = 0;
      var xmlDocument = new XmlDocument();
      xmlDocument.Load(memoryStream);
      memoryStream.Close();

      XmlNode xmlNode = xmlDocument.ChildNodes[1];
      XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("assemblyQualifiedName");
      xmlAttribute.Value = typeNotificationEventArgs.AssemblyQualifiedName;
      xmlNode.Attributes.Append(xmlAttribute);

      return xmlNode;
    }
  }
}
