using System.Collections.Generic;
using System.Threading.Tasks;
using UpdaterLibrary.Models;

namespace UpdaterLibrary.Interfaces
{
    public interface IPatchService
    {
        public Task<string> GetCurrentClientVersionAsync();

        public Task<string> GetActualClientVersionAsync();

        public Task<bool> IsClientUpToDateAsync();

        public Task<Queue<CommitFile>> GetUpdateQueueAsync();

        public bool SetClientVersion(string commitSha);
    }
}
