using System.ComponentModel;
using System.Net;
using Inedo.BuildMaster.Web;
using Inedo.Documentation;

namespace Inedo.BuildMasterExtensions.Azure
{
    [DisplayName("Delete Deployment")]
    [Description("Deletes a deployment by slot or by name from a cloud service in Windows Azure.")]
    [Tag("windows-azure")]
    [CustomEditor(typeof(DeleteDeploymentActionEditor))]
    public class DeleteDeploymentAction : AzureComputeActionBase
    {
        public DeleteDeploymentAction()
        {
            this.UsesServiceName = true;
            this.UsesDeploymentName = true;
            this.UsesSlotName = true;
            this.UsesWaitForCompletion = true;
        }

        public override ExtendedRichDescription GetActionDescription()
        {
            return new ExtendedRichDescription(
                new RichDescription(
                    "Delete deployment ",
                    new Hilite(AH.CoalesceString(this.DeploymentName, this.SlotName))
                ),
                new RichDescription(
                    "for service ",
                    new Hilite(this.ServiceName)
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
            var currentDeployment = AzureRequest(RequestType.Get, null,
                    "https://management.core.windows.net/{0}/services/hostedservices/{1}/deploymentslots/{2}",
                    this.ServiceName, this.SlotName);
            if (string.IsNullOrEmpty((string)currentDeployment.Document.Root.Element(ns + "Name")))
            {
                this.LogInformation("There is currently nothing deployed to the {0} deployment slot.", this.SlotName);
                return null;
            }

            AzureResponse resp = null;
            if (string.IsNullOrEmpty(this.DeploymentName))
                resp = AzureRequest(RequestType.Delete, null, "https://management.core.windows.net/{0}/services/hostedservices/{1}/deploymentslots/{2}",
                    this.ServiceName, this.SlotName);
            else
                resp = AzureRequest(RequestType.Delete, null, "https://management.core.windows.net/{0}/services/hostedservices/{1}/deployments/{2}",
                    this.ServiceName, this.DeploymentName);
            if (HttpStatusCode.Accepted != resp.StatusCode)
            {
                LogError("Error deleting deployment named {0}. Error code is: {1}, error description: {2}", this.ServiceName, resp.ErrorCode, resp.ErrorMessage);
                return null;
            }
            return resp.Headers.Get("x-ms-request-id");
        }
    }
}
