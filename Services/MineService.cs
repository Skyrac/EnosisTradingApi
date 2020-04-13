using API.Models;
using API.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Services
{
    public class MineService : IMine
    {
        ICosmosDatabase<UserEntity> _userEntityContext;
        ICosmosDatabase<MineEntity> _mineEntityContext;
        private ConcurrentDictionary<UserEntity, List<MineEntity>> miners = new ConcurrentDictionary<UserEntity, List<MineEntity>>();
        private ConcurrentBag<UserEntity> _updateTasks = new ConcurrentBag<UserEntity>();

        public MineService(ICosmosDatabase<UserEntity> userEntityContext, ICosmosDatabase<MineEntity> mineEntityContext)
        {
            _userEntityContext = userEntityContext;
            _mineEntityContext = mineEntityContext;

            //Test();
        }

        public async Task<double> GetBoundPoints(UserEntity user)
        {
            if((user = miners.Keys.Where(key => key.id == user.id).First()) == null) {
                AddMiner(user);
            }
            if(user == null)
            {
                return -1;
            }
            if(user.passive_time <= 0)
            {
                return user.used_bound_points;
            }
            IEnumerable<MineEntity> mines;
            if(miners[user].Count > 0)
            {
                mines = miners[user];
            } else
            {
                mines = await GetUserMinesAsync(user);
                miners[user].AddRange(mines);
            }
            var collectedPoints = 0.0f;
            foreach (var mine in mines)
            {
                collectedPoints += CheckMine(mine.last_check, user.passive_activation, mine);
            }

            new Task(() => { UpdateEntities(user, mines); }).Start();
            return user.used_bound_points - collectedPoints;
        }

        public void UpdateUserPassiveTime(UserEntity user)
        {
            var now = DateTime.Now;
            user.passive_time = Math.Max(user.passive_time - now.Subtract(user.last_check).Seconds, 0);
            user.last_check = now;
        }

        public async Task<IEnumerable<MineEntity>> GetUserMinesAsync(UserEntity user)
        {
            var mines = await _mineEntityContext.GetItemsAsync(string.Format("SELECT * FROM {0} WHERE ({0}.remaining_time > 0 OR {0}.remaining_time = -717) AND {0}.user = '{1}'", nameof(MineEntity), user.id));
            return mines;
        }

        public async Task<float> GetUserPowerAsync(UserEntity user)
        {
            if (miners.Count == 0 || (user = miners.Keys.Where(key => key.id == user.id).First()) == null)
            {
                AddMiner(user);
            }
            IEnumerable<MineEntity> mines;
            if (miners[user].Count > 0)
            {
                mines = miners[user];
            }
            else
            {
                mines = await GetUserMinesAsync(user);
                miners[user].AddRange(mines);
            }
            UpdateUserPassiveTime(user);
            var power = 0.0f;
            foreach(var mine in mines)
            {
                if (user.passive_time > 0)
                {
                    CheckMine(mine.last_check, user.passive_activation, mine);
                }
                if (mine.remaining_time > 0 || mine.remaining_time == -717)
                {
                    power += mine.power;
                }
            }
            if (user.passive_time > 0)
            {
                new Task(() => { UpdateEntities(user, mines); }).Start();
            }
            return power;
        }



        public void UpdateEntities(UserEntity user, IEnumerable<MineEntity> mines) {
            _userEntityContext.UpdateItemAsync(user.id, user);
            _mineEntityContext.BulkUpdateAsync(mines);
        }

        public float CheckMine(DateTime lastMineCheck, DateTime userPassiveActivation, MineEntity mine)
        {
            var current = DateTime.Now;
            var timeBetween = (lastMineCheck.CompareTo(userPassiveActivation) < 0 ? current.Subtract(userPassiveActivation) : current.Subtract(lastMineCheck)).Seconds;
            mine.remaining_time -= mine.remaining_time == -717 ? 0 : timeBetween;
            mine.last_check = current;
            mine.mined_points += mine.power / 60 * timeBetween;
            return mine.mined_points;
        }

        private async void Test()
        {
            List<Task> tasks = new List<Task>();
            for (var i = 0; i < 5; i++)
            {
                var user = new UserEntity();
                user.passive_time = 200000000 / 2;
                user.passive_activation = DateTime.Now;
                tasks.Add(_userEntityContext.AddItemAsync(user));
                for (var z = 0; z < 5; z++)
                {
                    var contract = new MineEntity();
                    contract.user = user.id;
                    contract.power = 500;
                    contract.remaining_time = 200000000;
                    contract.start_date = DateTime.Now;
                    contract.last_check = DateTime.Now;
                    tasks.Add(_mineEntityContext.AddItemAsync(contract));
                }
            }
            await Task.WhenAll(tasks);
            
        }

        public void AddMiner(UserEntity user)
        {
            if(!miners.Where(entry => entry.Key.id == user.id).Any())
            {
                miners.TryAdd(user, new List<MineEntity>());
            }
        }

    }
}
