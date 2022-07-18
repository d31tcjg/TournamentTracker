using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using TrackerLibrary.Models;
using System.ComponentModel.DataAnnotations;



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

        }

        public static void UpdateTournamentResults(TournamentModel model)
        {
            int startingRound = model.CheckCurrentRound();

            List<MatchupModel> toScore = new List<MatchupModel>();

            foreach (var round in model.Rounds)
            {
                foreach (var rm in round)
                {
                    if ((rm.Entries.Any(x => x.Score != 0 ) || rm.Entries.Count == 1))
                    {
                        toScore.Add(rm);
                    }
                }
            }

            MarkWinnerInMatchups(toScore);

            AdvanceWinners(toScore, model);

            toScore.ForEach(x => GlobalConfig.Connection.UpdateMatchup(x));

            int endingRound = model.CheckCurrentRound();

            if (endingRound > startingRound)
            {
                // Alert users
                model.AlertUsersToNewRound();
            }
        }

        public static void AlertUsersToNewRound(this TournamentModel model)
        {
            int currentRoundNumber = model.CheckCurrentRound();

            List<MatchupModel> currentRound = model.Rounds.Where(x => x.First().MatchupRound == currentRoundNumber).First();

            foreach (var matchup in currentRound)
            {
                foreach (var me in matchup.Entries)
                {
                    foreach (var p in me.TeamCompeting.TeamMembers)
                    {
                        AlertPersonToNewRound(p, me.TeamCompeting.TeamName,
                            matchup.Entries.Where(x => x.TeamCompeting != me.TeamCompeting).FirstOrDefault());
                    }
                }
            }
        }

        private static void AlertPersonToNewRound(PersonModel p, string teamName, MatchupEntryModel competitor)
        {
            // (new EmailAddressAttribute().IsValid("youremailhere@test.test")); 
            // .NET BuiltIn Email Verification 

            if ( new EmailAddressAttribute().IsValid(p.EmailAddress))
            {
                string to = string.Empty;
                string subject = string.Empty;
                StringBuilder body = new StringBuilder();

                if (competitor != null)
                {
                    subject = $"You have a new matchup with {competitor.TeamCompeting.TeamName}";

                    body.AppendLine("<h1>You have a new matchup</h1>");
                    body.Append("<strong>Competitor: ");
                    body.Append(competitor.TeamCompeting.TeamName);
                    body.AppendLine();
                    body.AppendLine();
                    body.AppendLine("Have a great time!");
                    body.AppendLine("~Tournament Tracker");
                }
                else
                {
                    subject = "You have a bye week this round";

                    body.AppendLine("Enjoy your round off!");
                    body.AppendLine("~Tournament Tracker");
                }

                to = p.EmailAddress;

                EmailLogic.SendEmail(to, subject, body.ToString());
            }

            //.NET Built In Phone Validation, same as email, internally uses RegEx
            if (new PhoneAttribute().IsValid(p.CellphoneNumber))
            {
                if (competitor != null)
                {
                    SMSLogic.SendSMSMessge(p.CellphoneNumber, $"You have a new matchup with this {competitor.TeamCompeting.TeamName}");
                }
                else
                {
                    SMSLogic.SendSMSMessge(p.CellphoneNumber, $"You have a bye week for the first round.");
                }
            }

        }

        private static int CheckCurrentRound(this TournamentModel model)
        {
            int output = 1;

            foreach (var round in model.Rounds)
            {
                if (round.All(x => x.Winner != null))
                {
                    output += 1;
                }
                else
                {
                    return output;
                }
            }

            // Tournament is complete
            CompleteTournament(model);

            return output - 1;
        }

        private static void CompleteTournament(TournamentModel model)
        {
            GlobalConfig.Connection.CompleteTournament(model);
            TeamModel winners = model.Rounds.Last().First().Winner;
            TeamModel runnerUp = model.Rounds.Last().First().Entries.Where(x => x.TeamCompeting != winners).First().TeamCompeting;

            decimal winnerPrize = 0;
            decimal runnerUpPrize = 0;

            if (model.Prizes.Count > 0)
            {
                decimal totalIncome = model.EnteredTeams.Count * model.EntryFee;

                PrizeModel firstPlacePrize = model.Prizes.Where(x => x.PlaceNumber == 1).FirstOrDefault();
                PrizeModel secondPlacePrize = model.Prizes.Where(x => x.PlaceNumber == 2).FirstOrDefault();

                if (firstPlacePrize != null)
                {
                    winnerPrize = firstPlacePrize.CalculatePrizePayout(totalIncome);
                }

                if (secondPlacePrize != null)
                {
                    runnerUpPrize = secondPlacePrize.CalculatePrizePayout(totalIncome);
                }
            }

            // Send Email to all tournament

            string subject = string.Empty;
            StringBuilder body = new StringBuilder();

            subject = $"In { model.TournamentName }, {winners.TeamName} has won!";

            body.AppendLine("<h1>We have a winner</h1>");
            body.AppendLine("<p>Congratulations to our winner on a great tournament.</p>");
            body.AppendLine(Environment.NewLine);

            if (winnerPrize > 0)
            {
                body.AppendLine($"<p>{winners.TeamName} will receive ${winnerPrize}</p>");
            }

            if (runnerUpPrize > 0)
            {
                body.AppendLine($"<p>{runnerUp.TeamName} will receive ${runnerUpPrize}</p>");
            }

            body.AppendLine("<p>Thanks for a great Tournament everyone!</p>");
            body.AppendLine("~Tournament Tracker");
            
            var bcc = (from t in model.EnteredTeams
                                from p in t.TeamMembers
                                where p.EmailAddress.Length > 0
                                select p.EmailAddress).ToList();

            //foreach (var t in model.EnteredTeams)
            //{
            //    foreach (var p in t.TeamMembers)
            //    {
            //        if (p.EmailAddress.Length > 0)
            //        {
            //            bcc.Add(p.EmailAddress);
            //        }
            //    }
            //}

            EmailLogic.SendEmail(new List<string>(),bcc ,subject, body.ToString());

            // Complete Tournament
            model.CompleteTournament();

        }

        private static decimal CalculatePrizePayout(this PrizeModel prize, decimal totalIncome)
        {
            decimal output = 0;

            if (prize.PrizeAmount > 0)
            {
                output = prize.PrizeAmount;
            }
            else
            {
                output = Decimal.Multiply(totalIncome, Convert.ToDecimal(prize.PrizePercentage / 100));
            }

            return output;
        }

        private static void AdvanceWinners(List<MatchupModel> models, TournamentModel tournament)
        {
            foreach (MatchupModel m in models)
            {
                foreach (List<MatchupModel> round in tournament.Rounds)
                {
                    foreach (MatchupModel rm in round)
                    {
                        foreach (MatchupEntryModel me in rm.Entries)
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
            // greater or lesser
            string greaterWins = ConfigurationManager.AppSettings["greaterWins"];

            foreach (MatchupModel m in models)
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
                    else if (m.Entries[1].Score < m.Entries[0].Score)
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
