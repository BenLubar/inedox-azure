﻿using System.ComponentModel;
using System.Net;
using System.Text;
using Inedo.BuildMaster.Web;
using Inedo.Documentation;
using Inedo.Serialization;

namespace Inedo.BuildMasterExtensions.Azure
{
    [DisplayName("Create Hosted Service")]
    [Description("Creates a new cloud service in Windows Azure.")]
    [Tag("windows-azure")]
    [CustomEditor(typeof(CreateHostedServiceActionEditor))]
    public class CreateHostedServiceAction : AzureComputeActionBase
    {
        public CreateHostedServiceAction()
        {
            this.UsesServiceName = true;
            this.UsesWaitForCompletion = true;
            this.UsesExtendedProperties = true;
        }

        [Persistent]
        public string Label { get; set; }

        [Persistent]
        public string Description { get; set; }

        [Persistent]
        public string Location { get; set; }

        [Persistent]
        public string AffinityGroup { get; set; }

        public override ExtendedRichDescription GetActionDescription()
        {
            return new ExtendedRichDescription(
                new RichDescription(
                    "Create cloud service ",
                    new Hilite(this.ServiceName)
                ),
                new RichDescription(
                    "for subscription ",
                    new Hilite(this.Credentials?.SubscriptionID)
                )
            );
        }

        protected override void Execute()
        {
            this.ExecuteRemoteCommand(null);
        }

        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            string requestID = string.Empty;
            requestID = MakeRequest();
            if (string.IsNullOrEmpty(requestID))
                return null;
            if (this.WaitForCompletion)
                this.WaitForRequestCompletion(requestID);
            return requestID;
        }

        internal string MakeRequest()
        {
            var resp = AzureRequest(RequestType.Post, BuildRequestDocument(), "https://management.core.windows.net/{0}/services/hostedservices");
            if (HttpStatusCode.Created != resp.StatusCode)
            {
                LogError("Error creating Hosted Service named {0}. Error code is: {1}, error description: {2}", this.ServiceName, resp.ErrorCode, resp.ErrorMessage);
                return null;
            }
            return resp.Headers.Get("x-ms-request-id");
        }

        internal string BuildRequestDocument()
        {
            StringBuilder body = new StringBuilder();
            body.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<CreateHostedService xmlns=\"http://schemas.microsoft.com/windowsazure\">");
            body.AppendFormat("<ServiceName>{0}</ServiceName>\r\n", this.ServiceName);
            body.AppendFormat("<Label>{0}</Label>\r\n", Base64Encode(this.Label));
            body.AppendFormat("<Description>{0}</Description>\r\n", this.Description);
            if (!string.IsNullOrEmpty(this.Location))
                body.AppendFormat("<Location>{0}</Location>\r\n", this.Location);
            else
                body.AppendFormat("<AffinityGroup>{0}</AffinityGroup>\r\n", this.AffinityGroup);
            body.Append(ParseExtendedProperties());
            body.Append("</CreateHostedService>\r\n");
            return body.ToString();
        }
    }
}
