using API.Models;
using API.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace API.Services
{
    public class MineService : IMine
    {
        ICosmosDatabase<UserEntity> _userEntityContext;
        ICosmosDatabase<MineEntity> _mineEntityContext;
        private const int period = 2;
        Timer _minersWatch = new Timer(period * 1000);
        private Dictionary<UserEntity, List<MineEntity>> miners = new Dictionary<UserEntity, List<MineEntity>>();

        public MineService(ICosmosDatabase<UserEntity> userEntityContext, ICosmosDatabase<MineEntity> mineEntityContext)
        {
            _userEntityContext = userEntityContext;
            _mineEntityContext = mineEntityContext;
            _minersWatch.Elapsed += UpdateMiningContractsAsync;
            _minersWatch.AutoReset = true;
            GetAllMinersAsync();
        }

        private async void UpdateMiningContractsAsync(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Updating Mining Contracts");
            foreach(var user in miners.Keys)
            {
                var generatedPower = 0.0f;
                foreach(var mineContract in miners[user])
                {
                    var time = (mineContract.remaining_time > period ? period : mineContract.remaining_time);
                    generatedPower +=  time * mineContract.power / 60 * period;
                    mineContract.remaining_time -= time;
                    await _mineEntityContext.UpdateItemAsync(mineContract.id, mineContract);
                }
                user.bound_points += generatedPower;
                await _userEntityContext.UpdateItemAsync(user.id, user);
            }
        }

        private async void GetAllMinersAsync()
        {
            await _mineEntityContext.GetItemsAndCallMethodAsync(string.Format("SELECT * FROM {0} WHERE {0}.remaining_time > 0", nameof(MineEntity)), AddMiningContract);
            _minersWatch.Start();
        }

        public void AddMiner(UserEntity user)
        {
            if(!miners.ContainsKey(user))
            {
                Console.WriteLine("Adding User to Miners");
                miners.Add(user, new List<MineEntity>());
            }
        }

        public async void AddMiningContract(MineEntity miningContract)
        {
            UserEntity miner;
            Console.WriteLine("Adding Miner");
            var pair = miners.FirstOrDefault(item => item.Key.id == miningContract.id);
            if ((miner = pair.Key) == null)
            {
                Console.WriteLine("Retrieving Userdata for Miner");
                miner = await _userEntityContext.GetItemAsync(miningContract.user);
                if(miner == null)
                {
                    //Mining Contract löschen?
                    return;
                }
                AddMiner(miner);
            }
            if(miningContract.remaining_time > 0)
            {
                miners[miner].Add(miningContract);
            }
        }

    }
}
