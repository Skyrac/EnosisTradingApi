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

            Test();
        }

        public async Task<double> GetBoundPoints(string user_id)
        {
            UserEntity user;
            if((user = miners.Keys.Where(key => key.id == user_id).First()) == null) {

                user = await _userEntityContext.GetItemAsync(user_id);
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
                mines = await _mineEntityContext.GetItemsAsync(string.Format("SELECT * FROM {0} WHERE {0}.remaining_time > 0 AND {0}.user = '{1}'", nameof(MineEntity), user.id));
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

        public void UpdateEntities(UserEntity user, IEnumerable<MineEntity> mines) {
            _userEntityContext.UpdateItemAsync(user.id, user);
            _mineEntityContext.BulkUpdateAsync(mines);
        }

        public float CheckMine(DateTime last, DateTime start, MineEntity mine)
        {
            var current = DateTime.Now;
            var timeBetween = (last.CompareTo(start) < 0 ? current.Subtract(start) : current.Subtract(last)).Seconds;
            mine.remaining_time -= timeBetween;
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
