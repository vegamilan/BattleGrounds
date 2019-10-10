using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BattleGrounds.Models;
using Hangfire;
using System.Text;
using static BattleGrounds.Common.Constants;

namespace BattleGrounds.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ArmiesController : ControllerBase
    {
        private readonly BattleGroundsDbContext _context;
        private readonly IBackgroundJobClient _backgroundJob;

        public ArmiesController(BattleGroundsDbContext context, IBackgroundJobClient backgroundJob)
        {
            _context = context;
            _backgroundJob = backgroundJob;
        }

        private GameOptions GetGameInitializedOptions()
        {
            var gameOptions = _context.GameOptions.Where(go => go.Key == BATTLE_INITIALIZED_KEY).FirstOrDefault();
            if (gameOptions == null)
            {
                _context.GameOptions.Add(new GameOptions());
                _context.SaveChanges();
            }

            return gameOptions;
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<string>> PostArmy(Army army)
        {
            if (GetGameInitializedOptions().Value == "True")
            {
                return "Can't add army because battle has already been initialized.";
            }

            _context.Armies.Add(army);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ArmyExists(army.Name))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetArmy", new { id = army.Name }, army);
        }

        private bool ArmyExists(string id)
        {
            return _context.Armies.Any(e => e.Name == id);
        }

        // GET: api/start
        [HttpGet]
        public async Task<ActionResult<string>> Start()
        {
            var armies = _context.Armies;

            // Checking if game can start
            if (armies.Count() < 10)
            {
                return "Game can't start without minimum 10 armies added";
            }

            _context.GameOptions.Where(go => go.Key == BATTLE_INITIALIZED_KEY).First().Value = "True";
            _context.SaveChanges();

            foreach (var army in armies)
            {
                _backgroundJob.Enqueue(() => PrepareAttack(army));
            }
            
            return "Game started.";
        }

        public void PrepareAttack(Army army)
        {
            // If game stopped attacks will stop as well
            if (GetGameInitializedOptions().Value == "False")
            {
                return;
            }

            army = _context.Armies.First(a => a.Name == army.Name);

            if (army.NumberOfUnits > 0)
            {
                var activeArmies = _context.Armies.Where(a => a.Name != army.Name && a.NumberOfUnits > 0);

                // Checking if game has finished
                if (activeArmies.Count() < 1)
                {
                    string message = "Game is finished, winner army is: " + army.Name;
                    Log log = new Log(message);
                    _context.Logs.Add(log);
                    _context.SaveChanges();

                    return;
                }

                Army targetArmy = null;

                switch (army.AttackingStrategy)
                {
                    case Common.Enumerations.AttackStrategy.Random:
                        Random random = new Random();
                        targetArmy = activeArmies.ToArray()[random.Next(0, activeArmies.Count() - 1)];
                        break;

                    case Common.Enumerations.AttackStrategy.Strongest:
                        targetArmy = activeArmies.OrderByDescending(a => a.NumberOfUnits).ToArray()[0];
                        break;

                    case Common.Enumerations.AttackStrategy.Weakest:
                        targetArmy = activeArmies.OrderBy(a => a.NumberOfUnits).ToArray()[0];
                        break;

                    default:
                        break;
                }

                DateTimeOffset timeOfAttack = DateTimeOffset.Now.AddMilliseconds(army.NumberOfUnits * 10);

                Log logAttack = new Log(army.Name + " with " + army.NumberOfUnits + " units will attack " + targetArmy.Name + " at " + timeOfAttack.ToString());
                _context.Logs.Add(logAttack);

                _context.SaveChanges();

                _backgroundJob.Schedule(() => Attack(army, targetArmy.Name), timeOfAttack);
            }
        }

        public void Attack(Army army, string targetArmyName)
        {
            // If game stopped attacks will stop as well
            if (GetGameInitializedOptions().Value == "False")
            {
                return;
            }

            army = _context.Armies.First(a => a.Name == army.Name);

            if (army.NumberOfUnits > 0)
            {
                Random random = new Random();
                StringBuilder message = new StringBuilder();
                message.Append(army.Name).Append(" attacked ").Append(targetArmyName);

                if (random.Next(0, 100) < army.NumberOfUnits) // Successful attack
                {
                    var targetArmy = _context.Armies.First(a => a.Name == targetArmyName);
                    var damage = (int)Math.Ceiling(army.NumberOfUnits * 0.5);

                    targetArmy.NumberOfUnits -= damage;

                    message.Append(" with ").Append(damage.ToString()).Append(" damage.");
                }
                else
                {
                    message.Append(" but missed.");
                }

                Log log = new Log(message.ToString());
                _context.Logs.Add(log);
                _context.SaveChanges();

                PrepareAttack(army);
            }
        }

        // GET: api/getlog
        [HttpGet]
        public async Task<ActionResult<string>> GetLog()
        {
            StringBuilder fullBattleLog = new StringBuilder();

            var logs = _context.Logs.OrderBy(l => l.DateOfCreation);
            foreach (var log in logs)
            {
                fullBattleLog.AppendLine(log.Description);
            }

            return fullBattleLog.ToString();
        }

        // GET: api/reset
        [HttpGet]
        public async Task<ActionResult<string>> Reset()
        {
            _context.Logs.RemoveRange(_context.Logs);
            _context.Armies.RemoveRange(_context.Armies);
            await _context.SaveChangesAsync();

            var gameOptions = _context.GameOptions.Where(go => go.Key == BATTLE_INITIALIZED_KEY).FirstOrDefault();
            if (gameOptions == null)
            {
                _context.GameOptions.Add(new GameOptions());
            }
            else
            {
                gameOptions.Value = "False";
            }

            _context.SaveChanges();

            return "Game resetted";
        }
    }
}
