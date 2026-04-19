using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_NumberGame.Models
{
    public class Player
    {
        public string PlayerName { get; set; }
        public int GuessNumber { get; set; }

        public int Level { get; set; }
        public int Score { get; set; }
        public int TimeLeft { get; set; }
    }
}