using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace UpdaterLibrary.Interfaces
{
    public interface ICommitService
    {
        public Task<IEnumerable<GitHubCommit>?> GetAllCommitsAsync();

        public Task<GitHubCommit> GetCommitInfoAsync(string commitSha);
    }
}
