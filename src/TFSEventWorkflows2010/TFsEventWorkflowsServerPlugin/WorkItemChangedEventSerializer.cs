namespace artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin
{
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    using Microsoft.TeamFoundation.WorkItemTracking.Server;

    internal class WorkItemChangedEventSerializer
    {
        #region Public Methods and Operators

        public static WorkItemChangedEvent DeserializeXml(XmlNode xmlData)
        {
            var xmlNodeReader = new XmlNodeReader(xmlData);
            var xmlSerializer = new XmlSerializer(typeof(WorkItemChangedEvent));
            var workItemChangedEvent = (WorkItemChangedEvent)xmlSerializer.Deserialize(xmlNodeReader);
            xmlNodeReader.Close();

            return workItemChangedEvent;
        }

        public static XmlNode SerializeXml(WorkItemChangedEvent workItemChangedEvent)
        {
            if (null == workItemChangedEvent)
            {
                return null;
            }

            var xmlSerializer = new XmlSerializer(typeof(WorkItemChangedEvent));
            var memoryStream = new MemoryStream();
            xmlSerializer.Serialize(memoryStream, workItemChangedEvent);

            memoryStream.Position = 0;
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(memoryStream);
            memoryStream.Close();

            return xmlDocument.ChildNodes[1];
        }

        #endregion
    }
}