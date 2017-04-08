using Inedo.BuildMaster.Configuration;
using Inedo.IO;

namespace Inedo.BuildMasterExtensions.Azure
{
    internal static class LegacyPathResolver
    {
        public static string GetWorkingDirectory(int executionId, int? deployableId, string overridePath)
        {
            if (overridePath == null)
                overridePath = string.Empty;

            if (overridePath.StartsWith("~\\") || overridePath.StartsWith("~/"))
            {
                return PathEx.Combine(
                    GetExecutionBaseDirectory(executionId),
                    overridePath.Substring(2)
                );
            }
            else
            {
                return PathEx.Combine(
                    GetExecutionBaseDirectory(executionId),
                    PathEx.Combine("_D" + (deployableId ?? 0), "WRK", overridePath)
                );
            }
        }

        public static string GetExecutionBaseDirectory(int executionId)
        {
            string relativePath = "_E" + executionId;
            string baseWorkingDir = CoreConfig.BaseWorkingDirectory;
            return PathEx.Combine(baseWorkingDir, relativePath);
        }
    }
}
