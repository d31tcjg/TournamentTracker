using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess.TextHelpers
{
    // * Load the text file
    // * Convert the text to List<PrizeModel>
    // * Find the max ID
    // * Add the new record with the new ID (max + 1)
    // Convert the prizes to list <string>
    // Save the list<string> to the text filea

    public static class TextConnectorProcessor
    {
        public static string FullFilePath(this string fileName) //PrizeModels.csv
        {
            // C:\data\TournamentTracker\PrizeModels.csv
            return $"{ConfigurationManager.AppSettings["filePath"]}\\{fileName} ";
        }

        public static List<string> LoadFile(this string file)
        {
            if (!File.Exists(file))
            {
                return new List<string>();
            }

            return File.ReadAllLines(file).ToList();
        }

        public static List<PrizeModel> ConvertToPrizeModels(this List<string> lines)
        {
            var output = new List<PrizeModel>();
            foreach (var line in lines)
            {
                string[] cols = line.Split(',');

                var prizeModel = new PrizeModel
                {
                    Id = int.Parse(cols[0]),
                    PlaceNumber = int.Parse(cols[1]),
                    PlaceName = cols[2],
                    PrizeAmount = decimal.Parse(cols[3]),
                    PrizePercentage = double.Parse(cols[4])
                };
                output.Add(prizeModel);
            }
            return output;
        }

        public static List<PersonModel> ConvertToPersonModels(this List<string> lines)
        {
            var output = new List<PersonModel>();
            foreach (var line in lines)
            {
                string[] cols = line.Split(',');

                var personModel = new PersonModel
                {
                    Id = int.Parse(cols[0]),
                    FirstName = cols[1],
                    LastName = cols[2],
                    EmailAddress = cols[3],
                    CellphoneNumber = cols[4]
                };
                output.Add(personModel);
            }
            return output;
        }

        public static List<TeamModel> ConvertToTeamModels(this List<string> lines)
        {
            //id, team name, list of Ids separated by the pipe|
            //3, Tim's Team,1|3|5

            var output = new List<TeamModel>();
            var people = GlobalConfig.PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();


            foreach (var line in lines)
            {
                string[] cols = line.Split(',');
                var team = new TeamModel();
                team.Id = int.Parse(cols[0]);
                team.TeamName = cols[1];

                string[] personIds = cols[2].Split('|');

                foreach (var personId in personIds)
                {
                    team.TeamMembers.Add(people.Where(x => x.Id == int.Parse(personId)).First());
                }
                output.Add(team);
            }

            return output;
        }

        public static List<TournamentModel> ConvertToTournamentModels(this List<string> lines)
        {
            // id=0
            // Tournament = 1
            // EntryFee = 2
            // EnteredTeams = 3
            // Prizes = 4
            // Rounds = 5
            // id,TournamentName,EntryFee,(id|id|id - Entered Teams), (id|id|id - Prizes), (Rounds - id^id^id|id^id^id|id^id^id)
            var output = new List<TournamentModel>();
            var teams = GlobalConfig.TeamFile.FullFilePath().LoadFile().ConvertToTeamModels();
            var prizes = GlobalConfig.PrizesFile.FullFilePath().LoadFile().ConvertToPrizeModels();
            var matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();



            foreach (var line in lines)
            {
                string[] cols = line.Split(',');
                var tournament = new TournamentModel();
                tournament.Id = int.Parse(cols[0]);
                tournament.TournamentName = cols[1];
                tournament.EntryFee = decimal.Parse(cols[2]);

                string[] teamIds = cols[3].Split('|');

                foreach (var id in teamIds)
                {
                    tournament.EnteredTeams.Add(teams.Where(x => x.Id == int.Parse(id)).First());
                }

                if (cols[4].Length > 0)
                {
                    string[] prizeIds = cols[4].Split('|');

                    foreach (var id in prizeIds)
                    {
                        tournament.Prizes.Add(prizes.Where(x => x.Id == int.Parse(id)).First());
                    }
                }

                //Capture Rounds information
                string[] rounds = cols[5].Split('|');


                foreach (var round in rounds)
                {
                    string[] msText = round.Split('^');
                    var matches = new List<MatchupModel>();

                    foreach (var matchupModelTextId in msText)
                    {
                        matches.Add(matchups.Where(x => x.Id == int.Parse(matchupModelTextId)).First());
                    }

                    tournament.Rounds.Add(matches);
                }

                output.Add(tournament);
            }

            return output;
        }


        public static void SaveToPrizeFile(this List<PrizeModel> models)
        {
            var lines = new List<string>();

            foreach (var prize in models)
            {
                lines.Add($"{prize.Id},{prize.PlaceNumber},{prize.PlaceName},{prize.PrizeAmount},{prize.PrizePercentage}");
            }

            File.WriteAllLines(GlobalConfig.PrizesFile.FullFilePath(), lines);
        }

        public static void SaveToPeopleFile(this List<PersonModel> models)
        {
            var lines = new List<string>();

            foreach (var person in models)
            {
                lines.Add($"{person.Id},{person.FirstName},{person.LastName},{person.EmailAddress},{person.CellphoneNumber}");
            }

            File.WriteAllLines(GlobalConfig.PeopleFile.FullFilePath(), lines);
        }

        public static void SaveToTeamFile(this List<TeamModel> models)
        {
            var lines = new List<string>();

            foreach (var team in models)
            {
                lines.Add($"{team.Id},{team.TeamName},{ConvertPeopleListToString(team.TeamMembers)}");
            }

            File.WriteAllLines(GlobalConfig.TeamFile.FullFilePath(), lines);
        }

        public static void SaveRoundsToFile(this TournamentModel model)
        {
            // Loop through each Round
            // Loop through each Matchup
            // Get the id for the new matchup and save the record
            // Loop through each Entry, get the id, and save it

            foreach (var round in model.Rounds)
            {
                foreach (var matchup in round)
                {
                    // Load all of the matchups from file
                    // Get the top id and add one
                    // Store the id
                    // Save the matchup record

                    matchup.SaveMatchupToFile();

                }
            }
        }

        public static List<MatchupEntryModel> ConvertToMatchupEntryModels(this List<string> lines)
        {
            // id = 0, TeamCompeting = 1, Score = 2, ParentMatchup = 3
            var output = new List<MatchupEntryModel>();

            foreach (var line in lines)
            {
                string[] cols = line.Split(',');

                var matchupEntryModel = new MatchupEntryModel();
                matchupEntryModel.Id = int.Parse(cols[0]);

                if (cols[1].Length.Equals(0))
                {
                    matchupEntryModel.TeamCompeting = null;
                }
                else
                {
                    matchupEntryModel.TeamCompeting = LookupTeamById(int.Parse(cols[1]));

                }

                matchupEntryModel.Score = double.Parse(cols[2]);

                var parentId = 0;
                if (int.TryParse(cols[3], out parentId))
                {
                    matchupEntryModel.ParentMatchup = LookupMatchupById(parentId);
                }
                else
                {
                    matchupEntryModel.ParentMatchup = null;
                }
                output.Add(matchupEntryModel);
            }
            return output;
        }

        private static List<MatchupEntryModel> ConvertStringToMatchupEntryModels(string input)
        {
            string[] ids = input.Split('|');
            var output = new List<MatchupEntryModel>();
            var entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile();
            var matchingEntries = new List<string>();

            foreach (var id in ids)
            {
                foreach (var entry in entries)
                {
                    string[] cols = entry.Split(',');

                    if (cols[0].Equals(id))
                    {
                        matchingEntries.Add(entry);
                    }
                }
            }

            output = matchingEntries.ConvertToMatchupEntryModels();

            return output;
        }


        private static TeamModel LookupTeamById(int id)
        {
            List<string> teams = GlobalConfig.TeamFile.FullFilePath().LoadFile();

            foreach (var team in teams)
            {
                string[] cols = team.Split(',');
                if (cols[0].Equals(id.ToString()))
                {
                    var matchingTeams = new List<string>();
                    matchingTeams.Add(team);
                    return matchingTeams.ConvertToTeamModels().First();
                }
            }

            return null;
        }

        private static MatchupModel LookupMatchupById(int id)
        {
            var matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile();

            foreach (var matchup in matchups)
            {
                string[] cols = matchup.Split(',');
                if (cols[0].Equals(id.ToString()))
                {
                    var matchingMatchups = new List<string>();
                    matchingMatchups.Add(matchup);
                    return matchingMatchups.ConvertToMatchupModels().First();
                }
            }

            return null;
        }


        public static List<MatchupModel> ConvertToMatchupModels(this List<string> lines)
        {
            // id = 0, entries = 1(pipe delimited by id), winner = 2, matchupRounds = 3
            var output = new List<MatchupModel>();

            foreach (var line in lines)
            {
                string[] cols = line.Split(',');

                var matchupModel = new MatchupModel()
                {
                    Id = int.Parse(cols[0]),
                    Entries = ConvertStringToMatchupEntryModels(cols[1]),
                    MatchupRound = int.Parse(cols[3]),
                };

                if (!cols[2].Length.Equals(0))
                {
                    matchupModel.Winner = LookupTeamById(int.Parse(cols[2]));
                }


                output.Add(matchupModel);
            }
            return output;
        }

        public static void SaveMatchupToFile(this MatchupModel matchup)
        {

            var matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();

            var currentId = 1;

            if (matchups.Count > 0)
            {
                currentId = matchups.OrderByDescending(match => match.Id).First().Id + 1;
            }

            matchup.Id = currentId;

            matchups.Add(matchup);


            foreach (var entry in matchup.Entries)
            {
                entry.SaveEntryToFile();
            }

            // save to file
            var lines = new List<string>();

            //id=0,entries=1(pipe delimited by id), winner=2, matchupRound=3
            foreach (var match in matchups)
            {
                var winner = string.Empty;
                if (match.Winner != null)
                {
                    winner = match.Winner.Id.ToString();
                }

                lines.Add($"{match.Id},{ConvertMatchupEntryListToString(match.Entries)},{winner}, {matchup.MatchupRound}");
            }

            File.WriteAllLines(GlobalConfig.MatchupFile.FullFilePath(), lines);

        }

        public static void UpdateMatchUpToFile(this MatchupModel matchup)
        {
            var matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();

            MatchupModel oldMatchup = new MatchupModel();

            foreach (var m in matchups)
            {
                if (m.Id == matchup.Id)
                {
                    oldMatchup = m;
                }
            }

            matchups.Remove(oldMatchup);

            matchups.Add(matchup);


            foreach (var entry in matchup.Entries)
            {
                entry.UpdateEntryToFile();
            }

            // save to file
            var lines = new List<string>();

            //id=0,entries=1(pipe delimited by id), winner=2, matchupRound=3
            foreach (var match in matchups)
            {
                var winner = string.Empty;
                if (match.Winner != null)
                {
                    winner = match.Winner.Id.ToString();
                }

                lines.Add($"{match.Id},{ConvertMatchupEntryListToString(match.Entries)},{winner}, {matchup.MatchupRound}");
            }

            File.WriteAllLines(GlobalConfig.MatchupFile.FullFilePath(), lines);
        }

        public static void UpdateEntryToFile(this MatchupEntryModel entry)
        {
            var entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile().ConvertToMatchupEntryModels();
            MatchupEntryModel oldEntry = new MatchupEntryModel();

            foreach (var e in entries)
            {
                if (e.Id == entry.Id)
                {
                    oldEntry = e;
                }
            }

            entries.Remove(oldEntry);

            entries.Add(entry);

            // save to file 
            var lines = new List<string>();
            // id = 0, TeamCompeting = 1, Score = 2, ParentMatchup = 3

            foreach (var e in entries)
            {
                var parent = string.Empty;
                if (e.ParentMatchup != null)
                {
                    parent = e.ParentMatchup.Id.ToString();
                }
                var teamCompeting = string.Empty;
                if (e.TeamCompeting != null)
                {
                    teamCompeting = e.TeamCompeting.Id.ToString();
                }
                lines.Add($"{e.Id},{teamCompeting},{e.Score},{parent}");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntryFile.FullFilePath(), lines);
        }

        public static void SaveEntryToFile(this MatchupEntryModel entry)
        {
            var entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile().ConvertToMatchupEntryModels();

            var currentId = 1;

            if (entries.Count > 0)
            {
                currentId = entries.OrderByDescending(e => e.Id).First().Id + 1;
            }

            entry.Id = currentId;
            entries.Add(entry);

            // save to file 
            var lines = new List<string>();
            // id = 0, TeamCompeting = 1, Score = 2, ParentMatchup = 3

            foreach (var e in entries)
            {
                var parent = string.Empty;
                if (e.ParentMatchup != null)
                {
                    parent = e.ParentMatchup.Id.ToString();
                }
                var teamCompeting = string.Empty;
                if (e.TeamCompeting != null)
                {
                    teamCompeting = e.TeamCompeting.Id.ToString();
                }
                lines.Add($"{e.Id},{teamCompeting},{e.Score},{parent}");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntryFile.FullFilePath(), lines);
        }

        public static void SaveToTournamentFile(this List<TournamentModel> models)
        {
            var lines = new List<string>();

            foreach (var tournament in models)
            {
                lines.Add($"{tournament.Id},{tournament.TournamentName},{tournament.EntryFee},{ConvertTeamListToString(tournament.EnteredTeams)},{ConvertPrizeListToString(tournament.Prizes)},{ConvertRoundListToString(tournament.Rounds)}");
            }

            File.WriteAllLines(GlobalConfig.TournamentFile.FullFilePath(), lines);
        }

        private static string ConvertRoundListToString(List<List<MatchupModel>> rounds)
        {
            //(Rounds - id ^ id ^ id | id ^ id ^ id | id ^ id ^ id)
            var output = string.Empty;

            if (rounds.Count.Equals(0)) return string.Empty;

            foreach (var round in rounds)
            {
                output += $"{ConvertMatchupListToString(round)}|";
            }
            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertMatchupListToString(List<MatchupModel> matchups)
        {
            var output = string.Empty;

            if (matchups.Count.Equals(0)) return string.Empty;

            foreach (var matchup in matchups)
            {
                output += $"{matchup.Id}^";
            }
            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertMatchupEntryListToString(List<MatchupEntryModel> entries)
        {
            var output = string.Empty;

            if (entries.Count.Equals(0)) return string.Empty;

            //2|5|
            foreach (var entry in entries)
            {
                output += $"{entry.Id}|";
            }
            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertPrizeListToString(List<PrizeModel> prizes)
        {
            var output = string.Empty;

            if (prizes.Count.Equals(0)) return string.Empty;

            foreach (var prize in prizes)
            {
                output += $"{prize.Id}|";
            }
            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertTeamListToString(List<TeamModel> teams)
        {
            var output = string.Empty;

            if (teams.Count.Equals(0)) return string.Empty;

            //2|5|
            foreach (var team in teams)
            {
                output += $"{team.Id}|";
            }
            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertPeopleListToString(List<PersonModel> people)
        {
            var output = string.Empty;

            if (people.Count.Equals(0)) return string.Empty;

            //2|5|
            foreach (var person in people)
            {
                output += $"{person.Id}|";
            }
            output = output.Substring(0, output.Length - 1);

            return output;
        }
    }
}
