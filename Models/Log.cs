using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleGrounds.Models
{
    public class Log
    {
        public int Id { get; set; }
        public DateTimeOffset DateOfCreation { get; set; }
        public string Description { get; set; }

        public Log(string description)
        {
            DateOfCreation = DateTimeOffset.Now;
            Description = description;
        }
    }
}
