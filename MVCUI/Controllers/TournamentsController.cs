using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCUI.Models;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace MVCUI.Controllers
{
    public class TournamentsController : Controller
    {
        // GET: Tournaments
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Details(int id, int roundId = 0)
        {
            List<TournamentModel> tournaments = GlobalConfig.Connection.GetTournament_All();

            try
            {
                TournamentMVCDetailsModel input = new TournamentMVCDetailsModel();
                TournamentModel t = tournaments.Where(x => x.Id == id).First();

                input.TournamentName = t.TournamentName;

                var orderedRounds = t.Rounds.OrderBy(x => x.First().MatchupRound).ToList();
                
                bool activeFound = false;

                for (int i = 0; i < orderedRounds.Count ; i++)
                {
                    RoundStatus status = RoundStatus.Locked;

                    if (!activeFound)
                    {
                        if (orderedRounds[i].TrueForAll(x => x.Winner != null))
                        {
                            status = RoundStatus.Complete;
                        }
                        else
                        {
                            status = RoundStatus.Active;
                            activeFound = true;
                        } 
                    }

                    input.Rounds.Add(new RoundMVCModel{ RoundName = "Round " + (i + 1), Status = status, RoundNumber = i + 1});
                }
                return View(input);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: People/Create
        public ActionResult Create()
        {
            TournamentMVCCreateModel input = new TournamentMVCCreateModel();
            List<TeamModel> allTeams = GlobalConfig.Connection.GetTeams_All();
            List<PrizeModel> allPrizes = GlobalConfig.Connection.GetPrizes_All();

            input.EnteredTeams = allTeams.Select(x => new SelectListItem{ Text = x.TeamName, Value = x.Id.ToString() }).ToList();
            input.Prizes = allPrizes.Select(x => new SelectListItem { Text = x.PlaceName, Value = x.Id.ToString() }).ToList();

            return View(input);
        }

        // POST: People/Create
        [ValidateAntiForgeryToken()]
        [HttpPost]
        public ActionResult Create(TournamentMVCCreateModel model)
        {
            try
            {
                if (ModelState.IsValid && model.SelectedEnteredTeams.Count > 0)
                {
                    TournamentModel t = new TournamentModel()
                    {
                        TournamentName = model.TournamentName,
                        EntryFee = model.EntryFee,
                        EnteredTeams = model.SelectedEnteredTeams.Select(x => new TeamModel{ Id = int.Parse(x)}).ToList(),
                        Prizes = model.SelectedPrizes.Select(x => new PrizeModel{ Id = int.Parse(x)}).ToList()
                    };

                    // Wire our matchups
                    TournamentLogic.CreateRounds(t);

                    GlobalConfig.Connection.CreateTournament(t);

                    t.AlertUsersToNewRound();

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Create");
                }
            }
            catch
            {
                return View();
            }
        }
    }
}