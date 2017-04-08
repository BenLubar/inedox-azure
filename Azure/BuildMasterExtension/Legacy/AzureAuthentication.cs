using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Inedo.BuildMasterExtensions.Azure
{
    [Serializable]
    public class AzureAuthentication
    {
        public string SubscriptionID { get; set; }

        public string PEMENcoded { get; set; }

        public string CertificateName { get; set; }

        public string ConfigFileName { get; set; }

        public bool HasCertificate => !string.IsNullOrEmpty(this.PEMENcoded) || !string.IsNullOrEmpty(this.CertificateName) || !string.IsNullOrEmpty(this.ConfigFileName);

        public X509Certificate2 Certificate
        {
            get
            {
                if (!string.IsNullOrEmpty(this.PEMENcoded))
                    return this.GetCertificateFromString(this.PEMENcoded);

                if (!string.IsNullOrEmpty(this.CertificateName))
                    return this.GetCertificateFromStore(this.CertificateName);

                return null;
            }
        }

        internal X509Certificate2 GetCertificateFromStore(string name)
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.Find(X509FindType.FindBySubjectName, name, false);

                if (certs.Count == 0)
                    throw new InvalidOperationException($"Cannot find a certificate named \"{name}\" in the machine store.");

                return certs[0];
            }
            finally
            {
                store.Close();
            }
        }

        internal X509Certificate2 GetCertificateFromString(string pemEncodedCertificate)
        {
            try
            {
                var file = Path.GetTempFileName();
                File.WriteAllText(file, pemEncodedCertificate, Encoding.ASCII);
                return new X509Certificate2(file);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred decoding the certificate from its PEM-encoded value, error was: " + ex.Message, ex);
            }
        }
    }
}
