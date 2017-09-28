using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TfsEventWorkflowsConfiguration
{
  using Microsoft.TeamFoundation.Client;
  using Microsoft.TeamFoundation.Framework.Client;
  using Microsoft.TeamFoundation.Framework.Common;

  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    private TfsConfigurationServer srvConfiguration = null;
    private ITeamFoundationRegistry tfsRegServiceConfiguration = null;

    private void btnConnect_Click(object sender, EventArgs e)
    {
      if (txtServerUrl.Enabled)
      {
        string strServerUrl = txtServerUrl.Text;
        try
        {
          // get the configuartion service
          #if UsingVssClientCredentials
          // TODO: find a way for TFS 2017 to always ask for credentials
          srvConfiguration = new TfsConfigurationServer(new Uri(strServerUrl), new Microsoft.VisualStudio.Services.Client.VssClientCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(true), Microsoft.VisualStudio.Services.Common.CredentialPromptType.PromptIfNeeded));
          #else
          srvConfiguration = new TfsConfigurationServer(new Uri(strServerUrl), null, new UICredentialsProvider(this));
          #endif

          // get the registry service
          tfsRegServiceConfiguration = srvConfiguration.GetService<ITeamFoundationRegistry>();

          ReadRegistry();
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message);
          return;
        }

        EnableControls(true);
      }
      else
      {
        EnableControls(false);
      }
    }


    private void btnSave_Click(object sender, EventArgs e)
    {
      try
      {
        WriteRegistry();
      }
      catch (AccessCheckException ex)
      {
        MessageBox.Show(ex.Message);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void EnableControls(bool Enable)
    {
      grpServer.Enabled = !Enable;
      grpWorkflowActivities.Enabled = Enable;
      grpWorkflows.Enabled = Enable;
      grpCollectionSpecific.Enabled = Enable;

      btnSave.Enabled = Enable;
      btnConnect.Text = Enable ? "Disconnect" : "Connect";

    }

    private void ReadRegistry()
    {
      txtWorkflowActivitiesGlobalCollection.Text = tfsRegServiceConfiguration.GetValue(TfsRegistryPath.WorkflowActivitiesGlobalCollection, "").Trim(new char[] { '/' });
      txtWorkflowActivitiesGlobalPath.Text =       tfsRegServiceConfiguration.GetValue(TfsRegistryPath.WorkflowActivitiesGlobalPath, "").Trim(new char[] { '/' });

      txtWorkflowsGlobalCollection.Text =          tfsRegServiceConfiguration.GetValue(TfsRegistryPath.WorkflowsGlobalCollection, "").Trim(new char[] { '/' });
      txtWorkflowsGlobalPath.Text =                tfsRegServiceConfiguration.GetValue(TfsRegistryPath.WorkflowsGlobalPath, "").Trim(new char[] { '/' });
      // WorkflowsCollectionPath.Text =            tfsRegServiceConfiguration.GetValue(TfsRegistryPath.WorkflowsCollectionPath, "").Trim(new char[] { '/' });
      txtWorkflowsProjectReleativePath.Text =      tfsRegServiceConfiguration.GetValue(TfsRegistryPath.WorkflowsProjectReleativePath, "").Trim(new char[] { '/' });
      txtWorkflowsConfigFileNameWebService.Text =  tfsRegServiceConfiguration.GetValue(TfsRegistryPath.WorkflowsConfigFileNameWebService, "").Trim(new char[] { '/' });
      txtWorkflowsConfigFileNameJobAgent.Text =    tfsRegServiceConfiguration.GetValue(TfsRegistryPath.WorkflowsConfigFileNameJobAgent, "").Trim(new char[] { '/' });

      cbCollection.ValueMember = "";
      cbCollection.DisplayMember = "";
      cbCollection.DataSource = null;

      dictWorkflowsCollectionPath = new Dictionary<string, string>();

      ReadOnlyCollection<CatalogNode> collectionNodes = srvConfiguration.CatalogNode.QueryChildren(
        new[] { CatalogResourceTypes.ProjectCollection }, false, CatalogQueryOptions.None);
      foreach (Microsoft.TeamFoundation.Framework.Client.CatalogNode collectionNode in collectionNodes)
      {
        string strCollection = collectionNode.Resource.DisplayName;
        Guid quidCollection = new Guid(collectionNode.Resource.Properties["InstanceId"]);
        Microsoft.TeamFoundation.Client.TfsTeamProjectCollection collection = srvConfiguration.GetTeamProjectCollection(quidCollection);
        Microsoft.TeamFoundation.Framework.Client.ITeamFoundationRegistry tfsRegServiceCollection = collection.GetService<Microsoft.TeamFoundation.Framework.Client.ITeamFoundationRegistry>();

        string strWorkflowsCollectionPath = tfsRegServiceCollection.GetValue(TfsRegistryPath.WorkflowsCollectionPath, "").Trim(new char[] { '/' });
        dictWorkflowsCollectionPath.Add(strCollection, strWorkflowsCollectionPath);
      }

      cbCollection.DataSource = new BindingSource(dictWorkflowsCollectionPath, null);
      cbCollection.DisplayMember = "Key";
      cbCollection.ValueMember = "Value";
    }

    private void WriteRegistry()
    {
      System.Collections.Generic.List<RegistryEntry> regEntries = new List<RegistryEntry>();
      regEntries.Add(new RegistryEntry(TfsRegistryPath.WorkflowActivitiesGlobalCollection, txtWorkflowActivitiesGlobalCollection.Text));
      regEntries.Add(new RegistryEntry(TfsRegistryPath.WorkflowActivitiesGlobalPath, txtWorkflowActivitiesGlobalPath.Text));
      regEntries.Add(new RegistryEntry(TfsRegistryPath.WorkflowsGlobalCollection, txtWorkflowsGlobalCollection.Text));
      regEntries.Add(new RegistryEntry(TfsRegistryPath.WorkflowsGlobalPath, txtWorkflowsGlobalPath.Text));
      regEntries.Add(new RegistryEntry(TfsRegistryPath.WorkflowsProjectReleativePath, txtWorkflowsProjectReleativePath.Text));
      regEntries.Add(new RegistryEntry(TfsRegistryPath.WorkflowsConfigFileNameWebService, txtWorkflowsConfigFileNameWebService.Text));
      regEntries.Add(new RegistryEntry(TfsRegistryPath.WorkflowsConfigFileNameJobAgent, txtWorkflowsConfigFileNameJobAgent.Text));

      tfsRegServiceConfiguration.WriteEntries(regEntries);

      ReadOnlyCollection<CatalogNode> collectionNodes = srvConfiguration.CatalogNode.QueryChildren(
        new[] { CatalogResourceTypes.ProjectCollection }, false, CatalogQueryOptions.None);
      foreach (Microsoft.TeamFoundation.Framework.Client.CatalogNode collectionNode in collectionNodes)
      {
        string strCollection = collectionNode.Resource.DisplayName;
        Guid quidCollection = new Guid(collectionNode.Resource.Properties["InstanceId"]);
        Microsoft.TeamFoundation.Client.TfsTeamProjectCollection collection = srvConfiguration.GetTeamProjectCollection(quidCollection);
        Microsoft.TeamFoundation.Framework.Client.ITeamFoundationRegistry tfsRegServiceCollection = collection.GetService<Microsoft.TeamFoundation.Framework.Client.ITeamFoundationRegistry>();

        string strWorkflowsCollectionPath = dictWorkflowsCollectionPath[strCollection];
        regEntries.Clear();
        regEntries.Add(new RegistryEntry(TfsRegistryPath.WorkflowsCollectionPath, strWorkflowsCollectionPath));
        tfsRegServiceCollection.WriteEntries(regEntries);
      }
    }

    private Dictionary<string, string> dictWorkflowsCollectionPath = null;

    private void cbCollection_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (cbCollection.SelectedIndex != -1)
      {
        string strWorkflowsCollectionPath = ((KeyValuePair<string, string>) cbCollection.SelectedItem).Value;
        txtWorkflowsCollectionPath.Text = strWorkflowsCollectionPath;
      }
    }

    private void txtWorkflowsCollectionPath_TextChanged(object sender, EventArgs e)
    {
      if (cbCollection.SelectedIndex != -1)
      {
        string strWorkflowsCollectionPath = txtWorkflowsCollectionPath.Text;
        dictWorkflowsCollectionPath[((KeyValuePair<string, string>)cbCollection.SelectedItem).Key] = strWorkflowsCollectionPath;
      }
    }
  }

    // TFS registry paths
  internal class TfsRegistryPath
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


}
