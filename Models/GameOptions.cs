using System.ComponentModel.DataAnnotations;
using static BattleGrounds.Common.Constants;

namespace BattleGrounds.Models
{
    public class GameOptions
    {
        [Key]
        public string Key { get; set; }
        public string Value { get; set; }

        public GameOptions()
        {
            Key = BATTLE_INITIALIZED_KEY;
            Value = "False";
        }
    }
}
