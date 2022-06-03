using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;
using TrackerLibrary.DataAccess.TextHelpers;

namespace TrackerLibrary.DataAccess
{
    public class TextConnector : IDataConnection
    {

        public void CreatePerson(PersonModel model)
        {
            var people = GlobalConfig.PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();

            var currentId = 1;

            if (people.Count > 0)
            {
                currentId = people.OrderByDescending(x => x.Id).First().Id + 1;
            }

            model.Id = currentId;

            people.Add(model);

            people.SaveToPeopleFile();

        }

        public void CreatePrize(PrizeModel model)
        {
            // Load the text file
            // Convert the text to List<PrizeModel>
            var prizes = GlobalConfig.PrizesFile.FullFilePath().LoadFile().ConvertToPrizeModels();
            
            // Find the max ID
            var currentId = 1;

            if (prizes.Count > 0)
            {
                currentId = prizes.OrderByDescending(prize => prize.Id).First().Id + 1;
            }
            model.Id = currentId;

            // Add the new record with the new ID (max + 1)
            prizes.Add(model);

            // Convert the prizes to list <string>
            // Save the list<string> to the text file
            prizes.SaveToPrizeFile();

        }
        
        public List<PersonModel> GetPerson_All()
        {
            return GlobalConfig.PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();
        }

        public List<TournamentModel> GetTournament_All()
        {
            return GlobalConfig.TournamentFile.FullFilePath().LoadFile().ConvertToTournamentModels();
        }

        public void CreateTeam(TeamModel model)
        {
            var teams = GlobalConfig.TeamFile.FullFilePath().LoadFile().ConvertToTeamModels();

            var currentId = 1;

            if (teams.Count > 0)
            {
                currentId = teams.OrderByDescending(team => team.Id).First().Id + 1;
            }
            model.Id = currentId;

            teams.Add(model);

            teams.SaveToTeamFile();

        }

        public void UpdateMatchup(MatchupModel model)
        {
            model.UpdateMatchUpToFile();
        }

        public List<TeamModel> GetTeams_All()
        {
            return GlobalConfig.TeamFile.FullFilePath().LoadFile().ConvertToTeamModels();

        }

        public void CreateTournament(TournamentModel model)
        {
            var tournaments = GlobalConfig.TournamentFile.FullFilePath().LoadFile().ConvertToTournamentModels();

            var currentId = 1;

            if (tournaments.Count > 0)
            {
                currentId = tournaments.OrderByDescending(x => x.Id).First().Id + 1;
            }

            model.Id = currentId;

            model.SaveRoundsToFile();

            tournaments.Add(model);

            tournaments.SaveToTournamentFile();

        }

    }
}
