using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Build.Client;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib.BuildActivities
{
    public sealed class GetDropLocation : CodeActivity
    {
        [RequiredArgument]
        public InArgument<Uri> TFSCollectionUrl { get; set; }

        [RequiredArgument]
        public InArgument<string> TeamProjectName { get; set; }

        [RequiredArgument]
        public InArgument<string> BuildDefinitionName { get; set; }

        [RequiredArgument]
        public OutArgument<string> LastGoodBuildDropLocation { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            // Get a reference to our Team Foundation Server. 
            Uri tfsUri = context.GetValue(this.TFSCollectionUrl);
            string tfsProjectName = context.GetValue(this.TeamProjectName);
            string buildDefinitionName = context.GetValue(this.BuildDefinitionName);
            var tfs = new TfsTeamProjectCollection(tfsUri);

            // Get a reference to Version Control. 
            var buildServer = (IBuildServer)tfs.GetService(typeof(IBuildServer));
            var buildDefinition = buildServer.GetBuildDefinition(tfsProjectName, buildDefinitionName);
            var lastGoodBuildDropLocation = buildServer.GetBuild(buildDefinition.LastGoodBuildUri).DropLocation;

            context.SetValue(this.LastGoodBuildDropLocation, lastGoodBuildDropLocation);
        }
    }
}
