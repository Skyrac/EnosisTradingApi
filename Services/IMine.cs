using API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services
{
    public interface IMine
    {
        public void AddMiner(UserEntity user);
        public Task<IEnumerable<MineEntity>> GetUserMinesAsync(UserEntity user);
        public Task<float> GetUserPowerAsync(UserEntity user);
        public Task<double> GetBoundPoints(UserEntity user);
    }
}
