using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using MVC_NumberGame.Models;

namespace MVC_NumberGame.Controllers
{
    public class HomeController : Controller
    {
        string cs = ConfigurationManager
            .ConnectionStrings["GameDBConnection"]
            .ConnectionString;

        // =============================
        // SESSION GAME STATE
        // =============================

        int RandomNumber
        {
            get { return Session["RandomNumber"] == null ? 0 : (int)Session["RandomNumber"]; }
            set { Session["RandomNumber"] = value; }
        }

        int Attempts
        {
            get { return Session["Attempts"] == null ? 0 : (int)Session["Attempts"]; }
            set { Session["Attempts"] = value; }
        }

        int Level
        {
            get { return Session["Level"] == null ? 1 : (int)Session["Level"]; }
            set { Session["Level"] = value; }
        }

        int Score
        {
            get { return Session["Score"] == null ? 0 : (int)Session["Score"]; }
            set { Session["Score"] = value; }
        }

        int Combo
        {
            get { return Session["Combo"] == null ? 0 : (int)Session["Combo"]; }
            set { Session["Combo"] = value; }
        }

        int XP
        {
            get { return Session["XP"] == null ? 0 : (int)Session["XP"]; }
            set { Session["XP"] = value; }
        }

        // 🔥 Dynamic Difficulty
        int MaxRange => Level * 100;
        int MaxAttempts => Math.Max(5, 11 - Level);

        // =============================
        // HOME PAGE
        // =============================
        public ActionResult Index()
        {
            if (Session["RandomNumber"] == null)
            {
                StartNewGame();
            }

            return View();
        }

        // =============================
        // AAA GAME ENGINE
        // =============================
        [HttpPost]
        public JsonResult PlayGame(Player player)
        {
            Attempts++;

            string message = "";
            bool win = false;
            bool gameOver = false;
            bool levelUp = false;

            // ===== WIN =====
            if (player.GuessNumber == RandomNumber)
            {
                win = true;
                levelUp = true;

                message = "🎉 Correct Guess! NEXT LEVEL";

                Combo++;

                Score += (100 * Level) + (Combo * 20);
                XP += 25;

                SaveResult(
                    string.IsNullOrEmpty(player.PlayerName) ? "Player" : player.PlayerName,
                    player.GuessNumber,
                    Attempts
                );

                NextLevel();
            }
            else if (player.GuessNumber > RandomNumber)
            {
                message = "📉 Too High!";
                Combo = 0;
            }
            else
            {
                message = "📈 Too Low!";
                Combo = 0;
            }

            // ===== GAME OVER =====
            if (Attempts >= MaxAttempts)
            {
                gameOver = true;
                message = "💀 GAME OVER!";
                StartNewGame();
            }

            return Json(new
            {
                msg = message,
                win = win,
                levelUp = levelUp,
                attempt = Attempts,
                level = Level,
                score = Score,
                gameOver = gameOver,
                range = MaxRange,
                maxAttempts = MaxAttempts,
                combo = Combo
            });
        }

        // =============================
        // NEXT LEVEL ENGINE
        // =============================
        void NextLevel()
        {
            Level++;
            Attempts = 0;

            RandomNumber =
                new Random().Next(1, MaxRange + 1);
        }

        // =============================
        // START NEW GAME
        // =============================
        void StartNewGame()
        {
            Level = 1;
            Score = 0;
            Attempts = 0;

            RandomNumber =
                new Random().Next(1, 101);
        }

        // =============================
        // SAVE RESULT
        // =============================
        void SaveResult(string name, int guess, int attempts)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query =
                "INSERT INTO GameResults(PlayerName,GuessNumber,Attempts) VALUES(@name,@guess,@attempts)";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@guess", guess);
                cmd.Parameters.AddWithValue("@attempts", attempts);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // =============================
        // LEADERBOARD
        // =============================
        public ActionResult Leaderboard()
        {
            List<GameResult> list = new List<GameResult>();

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd =
                    new SqlCommand("SELECT * FROM GameResults ORDER BY Attempts ASC", con);

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new GameResult
                    {
                        Id = Convert.ToInt32(dr["Id"]),
                        PlayerName = dr["PlayerName"].ToString(),
                        GuessNumber = Convert.ToInt32(dr["GuessNumber"]),
                        Attempts = Convert.ToInt32(dr["Attempts"]),
                        ResultDate = Convert.ToDateTime(dr["ResultDate"])
                    });
                }
            }

            return View(list);
        }
    }
}