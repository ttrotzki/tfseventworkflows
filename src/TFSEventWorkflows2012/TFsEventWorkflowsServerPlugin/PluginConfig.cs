using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using log4net.Config;

using artiso.TFSEventWorkflows.LoggingLib;

namespace artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin
{
  public class PluginConfig
  {
    private static PluginConfig singleton = null;
    private static object criticalSection = new object();
    public static PluginConfig Config
    {
      get
      {
        lock(criticalSection)
        {
          if (singleton == null)
            singleton = new PluginConfig();
        }
        return singleton;
      }
    }

    private PluginConfig()
    {
      InJobAgent = AppDomain.CurrentDomain.BaseDirectory.ToLower().Contains("tfsjobagent");
      ExecutionPath = new FileInfo(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

      string strConfigFile = ExecutionPath.FullName + ".config";
      XmlConfigurator.Configure(new Uri(strConfigFile));

      this.LogInfo("Plugin Reloaded");
    }

    public bool InJobAgent;
    public FileInfo ExecutionPath;
  }
}
