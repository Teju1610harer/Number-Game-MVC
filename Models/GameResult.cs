using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_NumberGame.Models
{
    public class GameResult
    {
        public int Id { get; set; }
        public string PlayerName { get; set; }
        public int GuessNumber { get; set; }
        public int Attempts { get; set; }
        public DateTime ResultDate { get; set; }
    }
}