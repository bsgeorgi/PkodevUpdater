using UpdaterLibrary.Interfaces;

namespace UpdaterLibrary.Services
{
    public class VersionService : IVersionService
    {
        public VersionService()
        {

        }

        public string GetCurrentClientVersion()
        {
            // TODO: retrieve ClientCommitAt from appsetting.json
            // If ClientCommitAt is empty, then set it as
            // The very first commit hash from the repository
            // Using UpdateClientVersion method
            return string.Empty;
        }

        public bool IsClientUpToDate()
        {
            // TODO: Check if client is up to date by comparing
            // ClientCommitAt value from appsettings.json against latest
            // Commit Sha from HEAD
            return false;
        }

        public bool UpdateClientVersion(string commitSha)
        {
            // TODO: overwrite ClientCommitAt value in appsettings.json
            // With commitSha value
            // Return true if successful
            return true;
        }
    }
}
