using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Build.Client;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib.BuildActivities
{
  public sealed class GetBuildDefinitionXml : CodeActivity
  {
    /// <summary>
    /// BuildDefinition to use.
    /// </summary>
    /// <value>BuildDefinition</value>
    [RequiredArgument]
    public InArgument<IBuildDefinition> BuildDefinition { get; set; }

    /// <summary>
    /// The returned XML representation of the BuildDefinition.
    /// </summary>
    /// <value>returned XML</value>
    [RequiredArgument]
    public OutArgument<string> Xml { get; set; }

    protected override void Execute(CodeActivityContext context)
    {
      // get BuildDefinition and copy it to serializable class
      var buildDefinition = context.GetValue(this.BuildDefinition);
      var _buildDefintion = new _BuildDefinition(buildDefinition);

      // serialize the BuildDefinition
      var xmlSerializer = new XmlSerializer(typeof(_BuildDefinition));
      var memoryStream = new MemoryStream();
      xmlSerializer.Serialize(memoryStream, _buildDefintion);

      // load BuildDefinition as XmlDocument
      memoryStream.Position = 0;
      var xmlDocument = new XmlDocument();
      xmlDocument.Load(memoryStream);
      memoryStream.Close();

      // convert ProcessParameters from Text to Xml
      XmlNode nodeProcessParameters = xmlDocument.SelectSingleNode("descendant::ProcessParameters");
      string xmlProcessParameters = XElement.Parse(nodeProcessParameters.InnerText).ToString(SaveOptions.DisableFormatting);
      nodeProcessParameters.InnerText = "";
      nodeProcessParameters.InnerXml = xmlProcessParameters;

      // store XmlDocument as well formated string
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);
      XmlTextWriter xtw = null;
      try
      {
        xtw = new XmlTextWriter(sw);
        xtw.Formatting = Formatting.Indented;
        xmlDocument.ChildNodes[1].WriteTo(xtw);
      }
      finally
      {
        //clean up even if error
        if (xtw != null)
          xtw.Close();
      }

      string strXml = sb.ToString(); 
      context.SetValue(this.Xml, strXml);
    }

    #region Serializable implementation of IBuildDefinition
    public class _BuildDefinition
    {
      public _BuildDefinition() { }
      public _BuildDefinition(IBuildDefinition bd)
      {
        this.AttachedProperties = new List<KeyValue>();
        foreach (var kv in bd.AttachedProperties)
          this.AttachedProperties.Add(new KeyValue() {  Key=kv.Key, Value=kv.Value.ToString() });

        this.BatchSize = bd.BatchSize;
        this.BuildControllerUri = bd.BuildControllerUri.AbsoluteUri;
        this.ContinuousIntegrationQuietPeriod = bd.ContinuousIntegrationQuietPeriod;
        this.ContinuousIntegrationType = bd.ContinuousIntegrationType;
        this.DateCreated = bd.DateCreated;
        this.DefaultDropLocation = bd.DefaultDropLocation;
        this.Description = bd.Description;
        this.Id = bd.Id;
        this.ProcessTemplateServerPath = bd.Process.ServerPath;
        this.ProcessParameters = bd.ProcessParameters;
        this.QueueStatus = bd.QueueStatus;
        this.TriggerType = bd.TriggerType;

        this.RetentionPolicyList = new List<RetentionPolicy>();
        foreach (var rp in bd.RetentionPolicyList)
          this.RetentionPolicyList.Add(new RetentionPolicy() { BuildReason = rp.BuildReason, BuildStatus = rp.BuildStatus, DeleteOptions = rp.DeleteOptions, NumberToKeep = rp.NumberToKeep });

        this.Schedules = new List<Schedule>();
        foreach (var s in bd.Schedules)
          this.Schedules.Add(new Schedule() { DaysToBuild=s.DaysToBuild, StartTime=s.StartTime, TimeZone=s.TimeZone.DisplayName, Type=s.Type });

        this.Workspace = new WorkspaceTemplate() { LastModifiedBy = bd.Workspace.LastModifiedBy, LastModifiedDate = bd.Workspace.LastModifiedDate, Mappings = new List<WorkspaceMapping>() };
        foreach (var m in bd.Workspace.Mappings)
          this.Workspace.Mappings.Add(new WorkspaceMapping() { Depth=m.Depth, LocalItem=m.LocalItem, MappingType=m.MappingType, ServerItem=m.ServerItem });
      }

      public List<KeyValue> AttachedProperties { get; set;  }
      public int BatchSize { get; set; }
      public string BuildControllerUri { get; set; }
      public int ContinuousIntegrationQuietPeriod { get; set; }
      public ContinuousIntegrationType ContinuousIntegrationType { get; set; }
      public DateTime DateCreated { get; set; }
      public string DefaultDropLocation { get; set; }
      public string Description { get; set; }
      public string Id { get; set;  }
      public string ProcessTemplateServerPath { get; set; } 
      public string ProcessParameters { get; set; }
      public DefinitionQueueStatus QueueStatus { get; set; }
      public List<RetentionPolicy> RetentionPolicyList { get; set; }
      public List<Schedule> Schedules { get; set; }
      public DefinitionTriggerType TriggerType { get; set; }
      public WorkspaceTemplate Workspace { get; set; }
    }

    public class KeyValue
    {
      public string Key { get; set; }
      public string Value { get; set; }
    }

    public class WorkspaceTemplate
    {
      public string LastModifiedBy { get; set; }
      public DateTime LastModifiedDate { get; set; }
      public List<WorkspaceMapping> Mappings { get; set; }
    }

    public class WorkspaceMapping
    {
      public WorkspaceMappingDepth Depth { get; set; }
      public string LocalItem { get; set; }
      public WorkspaceMappingType MappingType { get; set; }
      public string ServerItem { get; set; }
    }

    public class RetentionPolicy
    {
      public BuildReason BuildReason { get; set; }
      public BuildStatus BuildStatus { get; set; }
      public DeleteOptions DeleteOptions { get; set; }
      public int NumberToKeep { get; set; }
    }

    public class Schedule
    {
      public ScheduleDays DaysToBuild { get; set; }
      public int StartTime { get; set; }
      public string TimeZone { get; set; }
      public ScheduleType Type { get; set; }
    }
    #endregion
  }
}
