using API.Models;

namespace API.Services
{
    public interface IMine
    {
        public void AddMiner(UserEntity user);
        public void AddMiningContract(MineEntity miningContract);
    }
}
