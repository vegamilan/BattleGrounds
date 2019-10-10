using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static BattleGrounds.Common.Enumerations;

namespace BattleGrounds.Models
{
    public class Army
    {
        [Key]
        public string Name { get; set; }
        [Range(80, 100)]
        public int NumberOfUnits { get; set; }
        public AttackStrategy AttackingStrategy { get; set; }
    }
}
