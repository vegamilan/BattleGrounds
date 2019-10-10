using BattleGrounds.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleGrounds
{
    public class BattleGroundsCalculator
    {
        private readonly BattleGroundsDbContext _context;

        public BattleGroundsCalculator(BattleGroundsDbContext context)
        {
            _context = context;

            var armies = _context.Armies;

            while (armies.Count() > 1)
            {
                foreach (var army in armies)
                {
                    switch (army.AttackingStrategy)
                    {
                        case Common.Enumerations.AttackStrategy.Random:
                            break;

                        case Common.Enumerations.AttackStrategy.Strongest:
                            break;

                        case Common.Enumerations.AttackStrategy.Weakest:
                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }
}
