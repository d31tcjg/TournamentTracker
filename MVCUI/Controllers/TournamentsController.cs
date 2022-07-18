using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult EditTournamentMatchup(MatchupMVCModel model)
        {
            
            try
            {
                if (ModelState.IsValid)
                {
                    List<TournamentModel> tournaments = GlobalConfig.Connection.GetTournament_All();
                    TournamentModel t = tournaments.Where(x => x.Id == model.TournamentId).First();
                    MatchupModel foundMatchup = new MatchupModel();

                    foreach (var round in t.Rounds)
                    {
                        foreach (var matchup in round)
                        {
                            if (matchup.Id == model.MatchupId)
                            {
                                foundMatchup = matchup;
                            }
                        }
                    }

                    for (int i = 0; i < foundMatchup.Entries.Count; i++)
                    {
                        if (i == 0)
                        {
                            foundMatchup.Entries[i].Score = model.FirstTeamScore;
                        }
                        else if (i == 1)
                        {
                            foundMatchup.Entries[i].Score = model.SecondTeamScore;
                        }
                    }

                    TournamentLogic.UpdateTournamentResults(t);

                }
            }
            catch 
            {

            }
            return RedirectToAction("Details", "Tournaments", new { id = model.TournamentId, roundId = model.RoundNumber });
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

                            if (roundId == 0)
                            {
                                roundId = i + 1;
                            }
                        } 
                    }

                    input.Rounds.Add(new RoundMVCModel{ RoundName = "Round " + (i + 1), Status = status, RoundNumber = i + 1});
                }

                input.Matchups = GetMatchups(orderedRounds[roundId - 1], id, roundId);

                return View(input);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private List<MatchupMVCModel> GetMatchups(List<MatchupModel> input, int tournamentId, int roundId = 0)
        {
            List<MatchupMVCModel> output = new List<MatchupMVCModel>();

            foreach (var item in input)
            {
                int teamTwoId = 0;
                string teamOneName = String.Empty;
                string teamTwoName = "Bye";
                double teamTwoScore = 0;

                teamOneName = item.Entries[0].TeamCompeting == null ? "To Be Determined" : item.Entries[0].TeamCompeting.TeamName;

                if (item.Entries.Count > 1)
                {
                    teamTwoId = item.Entries[1].Id;
                    teamTwoName = item.Entries[1].TeamCompeting == null ? "To Be Determined" : item.Entries[1].TeamCompeting.TeamName;
                    teamTwoScore = item.Entries[1].Score;
                }

                output.Add(new MatchupMVCModel
                {
                    MatchupId = item.Id,
                    TournamentId = tournamentId,
                    RoundNumber = roundId,
                    FirstTeamMatchupEntryId = item.Entries[0].Id,
                    FirstTeamName = teamOneName,
                    FirstTeamScore = item.Entries[0].Score,
                    SecondTeamMatchupEntryId = teamTwoId,
                    SecondTeamName = teamTwoName,
                    SecondTeamScore = teamTwoScore
                });
            }

            return output;
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
                    List<TeamModel> allTeams = GlobalConfig.Connection.GetTeams_All();
                    List<PrizeModel> allPrizes = GlobalConfig.Connection.GetPrizes_All();

                    TournamentModel t = new TournamentModel()
                    {
                        TournamentName = model.TournamentName,
                        EntryFee = model.EntryFee,
                        EnteredTeams = model.SelectedEnteredTeams.Select(x => allTeams.Where(y => y.Id == int.Parse(x)).First()).ToList(),
                        Prizes = model.SelectedPrizes.Select(x => allPrizes.Where(y => y.Id == int.Parse(x)).First()).ToList()

                        //EnteredTeams = model.SelectedEnteredTeams.Select(x => new TeamModel { Id = int.Parse(x) }).ToList(),
                        //Prizes = model.SelectedPrizes.Select(x => new PrizeModel { Id = int.Parse(x) }).ToList()
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