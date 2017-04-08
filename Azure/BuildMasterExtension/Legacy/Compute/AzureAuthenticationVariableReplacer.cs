using System.Collections.Generic;
using System.Linq;
using Inedo.BuildMaster.Extensibility;

namespace Inedo.BuildMasterExtensions.Azure
{
    internal sealed class AzureAuthenticationVariableReplacer : ICustomVariableReplacer
    {
        public AzureAuthenticationVariableReplacer()
        {
        }

        public IEnumerable<VariableExpandingField> GetFieldsToExpand(object instance)
        {
            var credentials = (AzureAuthentication)instance;
            if (credentials == null)
                return Enumerable.Empty<VariableExpandingField>();

            return new[]
            {
                new VariableExpandingField(credentials.SubscriptionID, v => credentials.SubscriptionID = v),
                new VariableExpandingField(credentials.PEMENcoded, v => credentials.PEMENcoded = v),
                new VariableExpandingField(credentials.CertificateName, v => credentials.CertificateName = v),
                new VariableExpandingField(credentials.ConfigFileName, v => credentials.ConfigFileName = v)
            };
        }
    }
}