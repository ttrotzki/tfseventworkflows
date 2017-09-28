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
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Server;
#if UsingILocationService
using Microsoft.VisualStudio.Services.Location.Server;
#endif

using artiso.TFSEventWorkflows.LoggingLib;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{
  #if UsingIVssRequestContext
  using TeamFoundationRequestContext = Microsoft.TeamFoundation.Framework.Server.IVssRequestContext;
  #else
  using TeamFoundationRequestContext = Microsoft.TeamFoundation.Framework.Server.TeamFoundationRequestContext;
  #endif

  public sealed class GetTeamProjectCollection : CodeActivity
  {
    [RequiredArgument]
    public InArgument<TeamFoundationRequestContext> requestContext { get; set; }

    [RequiredArgument]
    public OutArgument<string> teamProjectCollectionUrl { get; set; }

    [RequiredArgument]
    public OutArgument<TfsTeamProjectCollection> teamProjectCollection { get; set; }

    protected override void Execute(CodeActivityContext context)
    {
      TeamFoundationRequestContext requestContext = context.GetValue(this.requestContext);
      #if UsingILocationService
      ILocationService tfLocationService = requestContext.GetService<ILocationService>();
      Uri uriRequestContext = new Uri(tfLocationService.GetLocationData(requestContext, Guid.Empty).GetServerAccessMapping(requestContext).AccessPoint + "/" + requestContext.ServiceHost.Name);
      #else
      var tfLocationService = requestContext.GetService<TeamFoundationLocationService>();
      var accessMapping = tfLocationService.GetServerAccessMapping(requestContext);
      Uri uriRequestContext = new Uri(tfLocationService.GetHostLocation(requestContext, accessMapping));
      #endif
      UriBuilder uriBuilderRequestContext = new UriBuilder(uriRequestContext.Scheme, System.Environment.MachineName, uriRequestContext.Port, uriRequestContext.PathAndQuery);
      string teamProjectCollectionUrl = uriBuilderRequestContext.Uri.AbsoluteUri;
      var teamProjectCollection = new TfsTeamProjectCollection(new Uri(teamProjectCollectionUrl));

      context.SetValue(this.teamProjectCollectionUrl, teamProjectCollectionUrl);
      context.SetValue(this.teamProjectCollection, teamProjectCollection);
    }
  }
}
