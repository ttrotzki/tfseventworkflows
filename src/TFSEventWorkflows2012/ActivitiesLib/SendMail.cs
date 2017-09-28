using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Microsoft.TeamFoundation.Framework.Server;
using artiso.TFSEventWorkflows.LoggingLib;

namespace artiso.TFSEventWorkflows.TFSActivitiesLib
{
  #if UsingIVssRequestContext
  using TeamFoundationRequestContext = Microsoft.TeamFoundation.Framework.Server.IVssRequestContext;
  #else
  using TeamFoundationRequestContext = Microsoft.TeamFoundation.Framework.Server.TeamFoundationRequestContext;
  #endif

  public sealed class SendMail : CodeActivity
    {
        [RequiredArgument]
        public InArgument<string> Host { get; set; }

        [RequiredArgument]
        public InArgument<int> Port { get; set; }

        [RequiredArgument]
        public InArgument<string> FromAdress { get; set; }

        [RequiredArgument]
        public InArgument<string> FromDisplayName { get; set; }

        [RequiredArgument]
        public InArgument<string> ToAdress { get; set; }

        [RequiredArgument]
        public InArgument<string> Subject { get; set; }

        [RequiredArgument]
        public InArgument<string> Body { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string argHost = context.GetValue(this.Host);
            int argPort = context.GetValue(this.Port);
            string argFromAdress = context.GetValue(this.FromAdress);
            string argFromDisplayName = context.GetValue(this.FromDisplayName);
            string argToAdress = context.GetValue(this.ToAdress);
            string argSubject = context.GetValue(this.Subject);
            string argBody = context.GetValue(this.Body);

            // Command line argument must the the SMTP host.
            SmtpClient client = new SmtpClient(argHost, argPort);
            client.UseDefaultCredentials = true;

            // Specify the e-mail sender. 
            // Create a mailing address that includes a UTF8 character 
            // in the display name.
            MailAddress from = new MailAddress(argFromAdress, argFromDisplayName);
            // Set destinations for the e-mail message.
            MailAddress to = new MailAddress(argToAdress);
            // Specify the message content.
            MailMessage message = new MailMessage(from, to);
            message.Subject = argSubject;
            message.SubjectEncoding = System.Text.Encoding.UTF8;
            message.Body = argBody;
            message.BodyEncoding = System.Text.Encoding.UTF8;
          
            // send the mail
            client.Send(message);
        }
    }

    public sealed class TfsSendMail : CodeActivity
    {
        [RequiredArgument]
        public InArgument<TeamFoundationRequestContext> RequestContext { get; set; }

        [RequiredArgument]
        public InArgument<string> ToAdress { get; set; }

        [RequiredArgument]
        public InArgument<string> Subject { get; set; }

        [RequiredArgument]
        public InArgument<bool> IsBodyHtml { get; set; }

        [RequiredArgument]
        public InArgument<string> Body { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            TeamFoundationRequestContext requestContextCollection = context.GetValue(this.RequestContext);
            string argToAdress = context.GetValue(this.ToAdress);
            string argSubject = context.GetValue(this.Subject);
            bool argIsBodyHtml = context.GetValue(this.IsBodyHtml);
            string argBody = context.GetValue(this.Body);

            // services on the configuration
            #if UsingOrganizationServiceHost
            TeamFoundationRequestContext requestContextConfiguration = requestContextCollection.ServiceHost.OrganizationServiceHost.CreateServicingContext();
            #else
            TeamFoundationRequestContext requestContextConfiguration = requestContextCollection.ServiceHost.ApplicationServiceHost.CreateServicingContext();
            #endif
            TeamFoundationRegistryService srvRegistryConfiguration = requestContextConfiguration.GetService<TeamFoundationRegistryService>();

            // read the parameters from TFS registry
            var arrRegEntries = srvRegistryConfiguration.ReadEntries(requestContextConfiguration, "/Service/Integration/Settings/*").ToArray();
            var keyvalues = new List<string>() 
            {
              "EmailEnabled",
              "EmailNotificationFromAddress",
              // "SmtpAnonymousAuth",
              "SmtpCertThumbprint",
              "SmtpEnableSsl",
              "SmtpPassword",
              "SmtpPort",
              "SmtpServer",
              "SmtpUser"
            }.Select(name => 
            {
              string path = "/Service/Integration/Settings/" + name;
              string value = null;
              try
              {
                // value = srvRegistryConfiguration.ReadEntries(requestContextConfiguration, path).First().Value;
                value = arrRegEntries.Where(regEntry => regEntry.Name == name).Select(regEntry => regEntry.Value).FirstOrDefault();
              }
              catch (Exception e)
              {
                this.LogInfo(string.Format("registry value '{0}' not found", path));
                this.LogInfo(e.Message);
              }
              return new { key = name, value = value };
            });

            // dispose the request context
            requestContextConfiguration.Dispose();

            foreach(var keyvalue in keyvalues)
            {
              if(keyvalue.value == null)
                this.LogInfo(string.Format("registry value '{0}' not found", keyvalue.key));
            }

            bool TfsEmailEnabled = bool.Parse(keyvalues.Where(kv => kv.key == "EmailEnabled").First().value);
            if (!TfsEmailEnabled)
            {
              this.LogInfo(string.Format("TFS Email Alter Settings are disabled"));
              return;
            }

            string TfsSmtpServer = keyvalues.Where(kv => kv.key == "SmtpServer").First().value;
            int TfsSmtpPort = int.Parse(keyvalues.Where(kv => kv.key == "SmtpPort").First().value);
            string TfsEmailNotificationFromAddress = keyvalues.Where(kv => kv.key == "EmailNotificationFromAddress").First().value;
            string TfsSmtpUser = keyvalues.Where(kv => kv.key == "SmtpUser").First().value;
            string TfsSmtpPassword = keyvalues.Where(kv => kv.key == "SmtpPassword").First().value;

            this.LogInfo(string.Format("TFS SmptServer: {0}:{1}", TfsSmtpServer, TfsSmtpPort));
            this.LogInfo(string.Format("TFS SmptUser: {0} (from address: {1})", TfsSmtpUser, TfsEmailNotificationFromAddress));

            // setup connection to the SMTP host.
            SmtpClient client = new SmtpClient(TfsSmtpServer, TfsSmtpPort);
            if (string.IsNullOrWhiteSpace(TfsSmtpUser))
              client.UseDefaultCredentials = true;
            else
              client.Credentials = new System.Net.NetworkCredential(TfsSmtpUser, TfsSmtpPassword);

            // Specify the e-mail sender. 
            // Create a mailing address that includes a UTF8 character 
            // in the display name.
            MailAddress from = new MailAddress(TfsEmailNotificationFromAddress, TfsEmailNotificationFromAddress);
            // Set destinations for the e-mail message.
            MailAddress to = new MailAddress(argToAdress);
            // Specify the message content.
            MailMessage message = new MailMessage(from, to);
            message.Subject = argSubject;
            message.SubjectEncoding = System.Text.Encoding.UTF8;
            message.Body = argBody;
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.IsBodyHtml = argIsBodyHtml;
          
            // send the mail
            client.Send(message);
        }
    }
}
