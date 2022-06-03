using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
    public static class TournamentLogic
    {
        // Order our list randomly
        // Check if it is big enough if not, add in byes - 2*2*2*2 - 2^4
        // Create our first round of matchups
        // Create every round after that - 8 matchups - 4 matchups - 2 matchups - 1 matchup


        public static void CreateRounds(TournamentModel model)
        {
            var randomizedTeams = RandomizeTeamOrder(model.EnteredTeams);
            var rounds = FindNumberOfRounds(randomizedTeams.Count);
            var byes = NumberOfByes(rounds, randomizedTeams.Count);

            model.Rounds.Add(CreateFirstRound(byes, randomizedTeams));
            
            CreateOtherRounds(model, rounds);

            UpdateTournamentResults(model);
        }

        public static void UpdateTournamentResults(TournamentModel model)
        {
            List<MatchupModel> toScore = new List<MatchupModel>();

            foreach (var round in model.Rounds)
            {
                foreach (var rm in round)
                {
                    if (rm.Winner != null && (rm.Entries.Any(x => x.Score != 0 ) || rm.Entries.Count == 1))
                    {
                        toScore.Add(rm);
                    }
                }
            }

            MarkWinnerInMatchups(toScore);

            AdvanceWinners(toScore, model);

            toScore.ForEach(x => GlobalConfig.Connection.UpdateMatchup(x));
        }

        private static void AdvanceWinners(List<MatchupModel> models, TournamentModel tournament)
        {
            foreach (var m in models)
            {
                foreach (var round in tournament.Rounds)
                {
                    foreach (var rm in round)
                    {
                        foreach (var me in rm.Entries)
                        {
                            if (me.ParentMatchup != null)
                            {
                                if (me.ParentMatchup.Id == m.Id)
                                {
                                    me.TeamCompeting = m.Winner;
                                    GlobalConfig.Connection.UpdateMatchup(rm);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void MarkWinnerInMatchups(List<MatchupModel> models)
        {
            //  greater or lesser
            string greaterWins = ConfigurationManager.AppSettings["greaterWins"];

            foreach (var m in models)
            {
                // Checks for bye week entry
                if (m.Entries.Count == 1)
                {
                    m.Winner = m.Entries[0].TeamCompeting;
                    continue;
                }

                // 0 means false, or low score wins
                if (greaterWins == "0")
                {
                    if (m.Entries[0].Score < m.Entries[1].Score)
                    {
                        m.Winner = m.Entries[0].TeamCompeting;
                    }
                    else if(m.Entries[1].Score < m.Entries[0].Score)
                    {
                        m.Winner = m.Entries[1].TeamCompeting;
                    }
                    else
                    {
                        throw new Exception("We do not allow ties in this application.");
                    }
                }
                else
                {
                    // 1 mean true, or high score wins
                    if (m.Entries[0].Score > m.Entries[1].Score)
                    {
                        m.Winner = m.Entries[0].TeamCompeting;
                    }
                    else if (m.Entries[1].Score > m.Entries[0].Score)
                    {
                        m.Winner = m.Entries[1].TeamCompeting;
                    }
                    else
                    {
                        throw new Exception("We do not allow ties in this application.");
                    }
                } 
            }

        }

        private static void CreateOtherRounds(TournamentModel model, int rounds)
        {
            var round = 2;
            var previoudRound = model.Rounds[0];
            var currentRound = new List<MatchupModel>();
            var currentMatchup = new MatchupModel();

            while (round <= rounds)
            {
                foreach (var match in previoudRound)
                {
                    currentMatchup.Entries.Add(new MatchupEntryModel { ParentMatchup = match });

                    if (currentMatchup.Entries.Count > 1)
                    {
                        currentMatchup.MatchupRound = round;
                        currentRound.Add(currentMatchup);
                        currentMatchup = new MatchupModel();
                    }
                }
                model.Rounds.Add(currentRound);
                previoudRound = currentRound;
                currentRound = new List<MatchupModel>();
                round += 1;
            }
        }

        private static List<MatchupModel> CreateFirstRound(int byes, List<TeamModel> teams)
        {
            var output = new List<MatchupModel>();
            var currentMatchup = new MatchupModel();

            foreach (var team in teams)
            {
                currentMatchup.Entries.Add(new MatchupEntryModel { TeamCompeting = team });

                if (byes > 0 || currentMatchup.Entries.Count > 1)
                {
                    currentMatchup.MatchupRound = 1;
                    output.Add(currentMatchup);
                    currentMatchup = new MatchupModel();

                    if (byes > 0)
                    {
                        byes -= 1;
                    }
                }
            }

            return output;
        }

        private static int NumberOfByes(int rounds, int numberOfTeams)
        {
            int output = 0;
            int totalTeams = 1;

            for (int i = 1; i <= rounds; i++)
            {
                totalTeams *= 2;
            }
            
            output = totalTeams - numberOfTeams;

            return output;
        }

        private static int FindNumberOfRounds(int teamCount)
        {
            int output = 1;
            int val = 2;

            while (val < teamCount)
            {
                // output = output +  1
                output += 1;

                // val = val * 2;
                val *= 2;
                
            }

            return output;

        }

        private static List<TeamModel> RandomizeTeamOrder(List<TeamModel> teams)
        {
            // cards.OrderBy( a => Guid.NewGuid()).ToList();
            return teams.OrderBy(team => Guid.NewGuid()).ToList();
        }
    }
}
