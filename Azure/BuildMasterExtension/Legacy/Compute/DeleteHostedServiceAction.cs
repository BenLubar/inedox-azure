using System.ComponentModel;
using System.Net;
using Inedo.BuildMaster.Web;
using Inedo.Documentation;

namespace Inedo.BuildMasterExtensions.Azure
{
    [DisplayName("Delete Hosted Service")]
    [Description("Deletes a cloud service in Windows Azure.")]
    [Tag("windows-azure")]
    [CustomEditor(typeof(DeleteHostedServiceActionEditor))]
    public class DeleteHostedServiceAction : AzureComputeActionBase
    {
        public DeleteHostedServiceAction()
        {
            this.UsesServiceName = true;
        }

        public override ExtendedRichDescription GetActionDescription()
        {
            return new ExtendedRichDescription(
                new RichDescription(
                    "Delete cloud service ",
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
            var resp = AzureRequest(RequestType.Delete, null, "https://management.core.windows.net/{0}/services/hostedservices/{1}", this.ServiceName);
            if (HttpStatusCode.OK != resp.StatusCode)
            {
                LogError("Error deleting Hosted Service named {0}. Error code is: {1}, error description: {2}", this.ServiceName, resp.ErrorCode, resp.ErrorMessage);
                return null;
            }
            return resp.Headers.Get("x-ms-request-id");
        }
    }
}
