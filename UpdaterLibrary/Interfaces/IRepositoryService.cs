using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace UpdaterLibrary.Interfaces
{
    public interface IRepositoryService
    {
        public Task<Repository?> GetRepositoryAsync();
    }
}
