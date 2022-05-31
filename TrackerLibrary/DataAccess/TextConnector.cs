﻿using System;
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
        private const string PrizesFile = "PrizeModels.csv";
        private const string PeopleFile = "PersonModels.csv";
        private const string TeamFile = "TeamModels.csv";
        private const string TournamentFile = "TournamentModels.csv";
        private const string MatchupFile = "MatchupModels.csv";
        private const string MatchupEntriesFile = "MatchupEntriesModels.csv";

        public PersonModel CreatePerson(PersonModel model)
        {
            var people = PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();

            var currentId = 1;

            if (people.Count > 0)
            {
                currentId = people.OrderByDescending(person => person.Id).First().Id + 1;
            }

            model.Id = currentId;

            people.Add(model);

            people.SaveToPeopleFile();

            return model;
        }

        public PrizeModel CreatePrize(PrizeModel model)
        {
            // Load the text file
            // Convert the text to List<PrizeModel>
            var prizes = PrizesFile.FullFilePath().LoadFile().ConvertToPrizeModels();
            
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

            return model;
        }
        
        public List<PersonModel> GetPerson_All()
        {
            return PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();
        }

        public List<TournamentModel> GetTournament_All()
        {
            return TournamentFile.FullFilePath().LoadFile().ConvertToTournamentModels();
        }

        public TeamModel CreateTeam(TeamModel model)
        {
            var teams = TeamFile.FullFilePath().LoadFile().ConvertToTeamModels();

            var currentId = 1;

            if (teams.Count > 0)
            {
                currentId = teams.OrderByDescending(team => team.Id).First().Id + 1;
            }
            model.Id = currentId;

            teams.Add(model);

            teams.SaveToTeamFile();

            return model;
        }

        public void UpdateMatchup(MatchupModel model)
        {
            model.UpdateMatchUpToFile();
        }

        public List<TeamModel> GetTeams_All()
        {
            return TeamFile.FullFilePath().LoadFile().ConvertToTeamModels();

        }

        public void CreateTournament(TournamentModel model)
        {
            var tournaments = TournamentFile.FullFilePath().LoadFile().ConvertToTournamentModels();

            var currentId = 1;

            if (tournaments.Count > 0)
            {
                currentId = tournaments.OrderByDescending(team => team.Id).First().Id + 1;
            }

            model.Id = currentId;

            model.SaveRoundsToFile();

            tournaments.Add(model);

            tournaments.SaveToTournamentFile();

        }
    }
}
