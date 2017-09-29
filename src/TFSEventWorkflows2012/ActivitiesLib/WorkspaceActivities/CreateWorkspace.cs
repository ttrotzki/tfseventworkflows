using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Activities;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{
    /// <summary>
    /// Create workspace
    /// </summary>
    public class CreateWorkspace : CodeActivity
    {
        /// <summary>
        /// Gets or sets the server URI.
        /// </summary>
        /// <value>The server URI.</value>
        [RequiredArgument]
        public InArgument<Uri> TFSCollectionUrl { get; set; }

        /// <summary>
        /// Gets or sets the server path.
        /// </summary>
        /// <value>The server path.</value>
        [RequiredArgument]
        public InArgument<string> ServerPath { get; set; }

        /// <summary>
        /// Gets or sets the local path.
        /// </summary>
        /// <value>The local path.</value>
        [RequiredArgument]
        public InArgument<string> LocalPath { get; set; }

        /// <summary>
        /// Gets or sets the name of the workspace.
        /// </summary>
        /// <value>The name of the workspace.</value>
        [RequiredArgument]
        public InArgument<string> WorkspaceName { get; set; }

        /// <summary>
        /// Gets or sets the workspace comment.
        /// </summary>
        /// <value>The workspace comment.</value>
        public InArgument<string> WorkspaceComment { get; set; }

        /// <summary>
        /// Gets or sets the workspace.
        /// </summary>
        /// <value>The workspace.</value>
        [RequiredArgument]
        public OutArgument<Workspace> Workspace { get; set; }


        /// <summary>
        /// When implemented in a derived class, performs the execution of the activity.
        /// </summary>
        /// <param name="context">The execution context under which the activity executes.</param>
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                // Get a reference to our Team Foundation Server. 
                Uri tfsUri = context.GetValue(this.TFSCollectionUrl);
                var tfs = new TfsTeamProjectCollection(tfsUri);

                // Get a reference to Version Control. 
                var versionControl = (VersionControlServer)tfs.GetService(typeof(VersionControlServer));

                var workingFolders = new WorkingFolder[1];
                string serverItem = context.GetValue(this.ServerPath);
                string localItem = context.GetValue(this.LocalPath);
                workingFolders[0] = new WorkingFolder(serverItem, localItem, WorkingFolderType.Map);
                string workspaceName = context.GetValue(this.WorkspaceName);
                string workspaceComment = context.GetValue(this.WorkspaceComment);

                try
                {
                    versionControl.GetWorkspace(localItem).Delete();
                }
                catch (Exception)
                {
                }

                string strHost = System.Environment.MachineName;
                string strFQDN = System.Net.Dns.GetHostEntry(strHost).HostName;
                Workspace workspace = versionControl.CreateWorkspace(
                                                    workspaceName,
                                                    versionControl.AuthorizedUser,
                                                    workspaceComment,
                                                    workingFolders,
                                                    strHost);
                context.SetValue(this.Workspace, workspace);
            }
            catch (Exception ex)
            {
                throw new Exception("Activity CreateWorkspace:", ex);
            }
        }
    }
}
