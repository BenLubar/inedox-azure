using System;
using System.IO;
using System.Linq;
using System.Text;
using Inedo.Agents;
using Inedo.BuildMaster.ConfigurationFiles;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Extensibility.Variables;
using Inedo.IO;
using Inedo.Serialization;

namespace Inedo.BuildMasterExtensions.Azure
{
    public abstract class AzureActionWithConfigBase : AzureComputeActionBase
    {
        [Persistent]
        public string ConfigurationFilePath { get; set; }

        [Persistent]
        public string ConfigurationFileName { get; set; }

        [Persistent]
        public string ConfigurationFileContents { get; set; }

        [Persistent]
        public int ConfigurationFileId { get; set; }

        [Persistent]
        public string InstanceName { get; set; }

        protected string GetConfigurationFileContents()
        {
            if (!string.IsNullOrEmpty(this.ConfigurationFileContents))
                return this.ConfigurationFileContents;

            if (!string.IsNullOrEmpty(this.ConfigurationFilePath))
            {
                var configFileDir = this.ResolveLegacyPath(this.ConfigurationFilePath);
                var configFile = PathEx.Combine(configFileDir, this.ConfigurationFileName);

                var fileOps = this.Context.Agent.GetService<IFileOperationsExecuter>();

                if (!fileOps.FileExists(configFile))
                {
                    this.LogError("Configuration file {0} does not exist.", configFile);
                    return null;
                }

                if (this.TestConfigurer != null)
                    return fileOps.ReadAllText(configFile);

                var tree = VariableExpressionTree.Parse(fileOps.ReadAllText(configFile), Domains.VariableSupportCodes.All);
                var variableContext = FindVariableEvaluationContext(this.Context);
                return tree.Evaluate(variableContext);
            }

            return this.GetConfigText(this.ConfigurationFileId, this.InstanceName);
        }

        protected virtual byte[] GetConfigurationFileContents(int configurationFileId, string instanceName, int? versionNumber)
        {
            var deployer = new ConfigurationFileDeployer(
                new ConfigurationFileDeploymentOptions
                {
                    ConfigurationFileId = configurationFileId,
                    InstanceName = instanceName,
                    VersionNumber = versionNumber
                }
            );

            using (var memoryStream = new MemoryStream())
            {
                var writer = new StreamWriter(memoryStream, new UTF8Encoding(false));
                deployer.MessageLogged += (s, e) => this.Log(e.Level, e.Message);
                if (!deployer.Write((IGenericBuildMasterContext)this.Context, writer))
                    return null;

                writer.Flush();
                return memoryStream.ToArray();
            }
        }

        private string GetConfigText(int configurationFileId, string instanceName)
        {
            this.LogDebug("Loading configuration file instance {0}...", instanceName);

            var version = DB.Releases_GetRelease(this.Context.ApplicationId, this.Context.ReleaseNumber)
                .ReleaseConfigurationFiles
                .FirstOrDefault(r => r.ConfigurationFile_Id == configurationFileId);

            var file = this.GetConfigurationFileContents(configurationFileId, instanceName, version != null ? (int?)version.Version_Number : null);
            if (file == null)
                return null;

            this.LogDebug("Configuration file found.");
            return Encoding.UTF8.GetString(file);
        }

        private static ILegacyVariableEvaluationContext FindVariableEvaluationContext(IAgentBasedActionExecutionContext context)
        {
            var wrappedContext = new SimpleBuildMasterContext(context);

            var type = Type.GetType("Inedo.BuildMaster.Variables.StandardVariableEvaluationContext,BuildMaster", false);

            if (type != null)
            {
                return (ILegacyVariableEvaluationContext)Activator.CreateInstance(type, wrappedContext, context.Variables);
            }
            else
            {
                type = Type.GetType("Inedo.BuildMaster.Variables.LegacyVariableEvaluationContext,BuildMaster", true);
                return (ILegacyVariableEvaluationContext)Activator.CreateInstance(type, wrappedContext, context.Variables);
            }
        }
    }
}
