namespace UpdaterLibrary.Interfaces
{
    public interface IVersionService
    {
        public string GetCurrentClientVersion();

        public bool IsClientUpToDate();

        public bool UpdateClientVersion(string commitSha);
    }
}
