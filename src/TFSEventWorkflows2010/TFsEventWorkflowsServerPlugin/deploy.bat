echo OFF
set TARGETDIR=%1
set TFSPLUGINDIR="C:\Program Files\Microsoft Team Foundation Server 2010\Application Tier\Web Services\bin\Plugins\"

copy %TARGETDIR%\log4net.dll %TFSPLUGINDIR%\log4net.dll
copy %TARGETDIR%\artiso.TFSEventWorkflows.LoggingLib.dll %TFSPLUGINDIR%\artiso.TFSEventWorkflows.LoggingLib.dll
copy %TARGETDIR%\artiso.TFSEventWorkflows.TFSActivitiesLib.dll %TFSPLUGINDIR%\artiso.TFSEventWorkflows.TFSActivitiesLib.dll
copy %TARGETDIR%\artiso.TFSEventWorkflows.WorkflowsLib.dll %TFSPLUGINDIR%\artiso.TFSEventWorkflows.WorkflowsLib.dll
copy %TARGETDIR%\artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin.dll %TFSPLUGINDIR%\artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin.dll
copy %TARGETDIR%\artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin.dll.config %TFSPLUGINDIR%\artiso.TFSEventWorkflows.TFSEventWorkflowsServerPlugin.dll.config
copy %TARGETDIR%\artiso.TFSEventWorkflows.CompositesLib.dll %TFSPLUGINDIR%\artiso.TFSEventWorkflows.CompositesLib.dll
copy %TARGETDIR%\..\..\..\WorkflowsLib\DeploymentWorkflow.xaml %TFSPLUGINDIR%\DeploymentWorkflow.xaml