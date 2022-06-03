using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess
{
    public class SqlConnector : IDataConnection
    {
        private const string db = "Tournaments";

        public void CreatePerson(PersonModel model)
        {
            using (IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db)))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@FirstName", model.FirstName);
                parameters.Add("@LastName", model.LastName);
                parameters.Add("@EmailAddress", model.EmailAddress);
                parameters.Add("@CellphoneNumber", model.CellphoneNumber);
                parameters.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                 connection.Execute("dbo.spPeople_Insert", parameters, commandType: CommandType.StoredProcedure);

                model.Id = parameters.Get<int>("@id");

            }
        }

        /// <summary>
        /// Saves a new prize to the database
        /// </summary>
        /// <param name="model">The prize information.</param>
        /// <returns>The prize information, including the unique identifier.</returns>
        public void CreatePrize(PrizeModel model)
        {
            using (IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db)))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@PlaceNumber", model.PlaceNumber);
                parameters.Add("@PlaceName", model.PlaceName);
                parameters.Add("@PrizeAmount", model.PrizeAmount);
                parameters.Add("@PrizePercentage", model.PrizePercentage);
                parameters.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spPrizes_Insert", parameters, commandType: CommandType.StoredProcedure);

                model.Id = parameters.Get<int>("@id");

            }
        }

        public void CreateTeam(TeamModel model)
        {
            using (IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db)))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@TeamName", model.TeamName);
                parameters.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTeams_Insert", parameters, commandType: CommandType.StoredProcedure);

                model.Id = parameters.Get<int>("@id");

                foreach (var person in model.TeamMembers)
                {
                    parameters = new DynamicParameters();
                    parameters.Add("@TeamId", model.Id);
                    parameters.Add("@PersonId", person.Id);

                    connection.Execute("dbo.spTeamMembers_Insert", parameters, commandType: CommandType.StoredProcedure);
                }
            }
        }

        public void CreateTournament(TournamentModel model)
        {
            using (IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db)))
            {
                SaveTournaments(model, connection);

                SaveTournamentPrizes(model, connection);

                SaveTournamentEntries(model, connection);

                SaveTournamentRounds(model, connection);

                TournamentLogic.UpdateTournamentResults(model);

            }

        }

        public void UpdateMatchup(MatchupModel model)
        {
            
            using (IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db)))
            {
                //spMatchups_Update @id, @WinnerId
                var parameters = new DynamicParameters();
                if (model.Winner != null)
                {
                    parameters.Add("@id", model.Id);
                    parameters.Add("@WinnerId", model.Winner.Id);

                    connection.Execute("dbo.spMatchups_Update", parameters, commandType: CommandType.StoredProcedure); 
                }

                // spMatchupEntries_Update id, TeamCompetingId, Score
                foreach (var me in model.Entries)
                {
                    if (me.TeamCompeting != null)
                    {
                        parameters = new DynamicParameters();
                        parameters.Add("@id", me.Id);
                        parameters.Add("@TeamCompetingId", me.TeamCompeting.Id);
                        parameters.Add("@Score", me.Score);

                        connection.Execute("dbo.spMatchupEntries_Update", parameters, commandType: CommandType.StoredProcedure); 
                    }
                }
            }
        }

        private void SaveTournamentRounds(TournamentModel model, IDbConnection connection)
        {
            // List<List<MatchupModel>> Rounds
            // List<MatchupEntryModel> Entries

            // Loop through the rounds
            // Loop through the matchups
            // Save the matchup
            // Loop through the entries and save them

            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel matchup in round)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TournamentId", model.Id);
                    parameters.Add("@MatchupRound", matchup.MatchupRound);
                    parameters.Add("@id", 0, DbType.Int32, direction: ParameterDirection.Output);

                    connection.Execute("dbo.spMatchups_Insert", parameters, commandType: CommandType.StoredProcedure);

                    matchup.Id = parameters.Get<int>("@id");

                    foreach (MatchupEntryModel entry in matchup.Entries)
                    {
                        parameters = new DynamicParameters();

                        parameters.Add("@MatchupId", matchup.Id);

                        if (entry.ParentMatchup == null)
                        {
                            parameters.Add("@ParentMatchupId", null);
                        }
                        else
                        {
                            parameters.Add("@ParentMatchupId", entry.ParentMatchup.Id);
                        }

                        if (entry.TeamCompeting == null)
                        {
                            parameters.Add("@TeamCompetingId", null);
                        }
                        else
                        {
                            parameters.Add("@TeamCompetingId", entry.TeamCompeting.Id);
                        }

                        parameters.Add("@id", 0, DbType.Int32, direction: ParameterDirection.Output);

                        connection.Execute("dbo.spMatchupEntries_Insert", parameters,
                            commandType: CommandType.StoredProcedure);
                    }
                }
            }
        }

        private void SaveTournamentEntries(TournamentModel model, IDbConnection connection)
        {
            foreach (TeamModel team in model.EnteredTeams)
            {
                var parameters = new DynamicParameters();
                parameters.Add("@TournamentId", model.Id);
                parameters.Add("@TeamId", team.Id);
                parameters.Add("@id", 0, DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTournamentEntries_Insert", parameters, commandType: CommandType.StoredProcedure);
            }
        }

        private void SaveTournamentPrizes(TournamentModel model, IDbConnection connection)
        {
            foreach (var prize in model.Prizes)
            {
                var parameters = new DynamicParameters();
                parameters.Add("@TournamentId", model.Id);
                parameters.Add("@PrizeId", prize.Id);
                parameters.Add("@id", 0, DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTournamentPrizes_Insert", parameters,
                    commandType: CommandType.StoredProcedure);
            }
        }

        private void SaveTournaments(TournamentModel model, IDbConnection connection)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@TournamentName", model.TournamentName);
            parameters.Add("@EntryFee", model.EntryFee);
            parameters.Add("@id", 0, DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("dbo.spTournaments_Insert", parameters, commandType: CommandType.StoredProcedure);

            model.Id = parameters.Get<int>("@id");
        }

        public List<PersonModel> GetPerson_All()
        {
            List<PersonModel> output;
            using (IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db)))
            {
                output = connection.Query<PersonModel>("dbo.spPeople_GetAll").ToList();
            }

            return output;
        }

        public List<TeamModel> GetTeams_All()
        {
            List<TeamModel> output;
            using (IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db)))
            {
                output = connection.Query<TeamModel>("dbo.spTeam_GetAll").ToList();

                foreach (var team in output)
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@TeamId", team.Id);

                    team.TeamMembers = connection.Query<PersonModel>("dbo.spTeamMembers_GetByTeam", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }

            return output;
        }

        public List<TournamentModel> GetTournament_All()
        {
            List<TournamentModel> output;
            using (IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db)))
            {
                output = connection.Query<TournamentModel>("spTournaments_GetAll").ToList();
                var p = new DynamicParameters();
                foreach (var tournament in output)
                {
                    // Populate Prizes 
                    p = new DynamicParameters();
                    p.Add("@TournamentId", tournament.Id);

                    tournament.Prizes = connection.Query<PrizeModel>("dbo.spPrizes_GetByTournament", p, commandType: CommandType.StoredProcedure).ToList();


                    p = new DynamicParameters();
                    p.Add("@TournamentId", tournament.Id);

                    tournament.EnteredTeams = connection.Query<TeamModel>("dbo.spTeam_GetByTournament", p, commandType: CommandType.StoredProcedure).ToList();

                    foreach (var team in tournament.EnteredTeams)
                    {
                            // Populate Teams
                             p = new DynamicParameters();
                            p.Add("@TeamId", team.Id);

                            team.TeamMembers = connection.Query<PersonModel>("dbo.spTeamMembers_GetByTeam", p,
                                commandType: CommandType.StoredProcedure).ToList();
                    }
                    
                    p = new DynamicParameters();
                    p.Add("@TournamentId", tournament.Id);

                    // Populate Rounds
                    List<MatchupModel> matchups = connection.Query<MatchupModel>("dbo.spMatchups_GetByTournament", p,  commandType: CommandType.StoredProcedure).ToList();

                    foreach (var m in matchups)
                    {
                         p = new DynamicParameters();
                         p.Add("@MatchupId", m.Id);

                        // Populate Rounds
                        m.Entries = connection.Query<MatchupEntryModel>("spMatchupEntries_GetByMatchup", p, commandType: CommandType.StoredProcedure).ToList();

                        // Populate each entry (2 models)
                        // Populate each matchup (1 model)

                        var allTeams = GetTeams_All();

                        if (m.WinnerId > 0)
                        {
                            m.Winner = allTeams.Where(x => x.Id == m.WinnerId).First();
                        }

                        foreach (var me in m.Entries)
                        {
                            if (me.TeamCompetingId > 0)
                            {

                                me.TeamCompeting = allTeams.Where(x => x.Id == me.TeamCompetingId).First();
                            }

                            if (me.ParentMatchupId > 0)
                            {
                                me.ParentMatchup = matchups.Where(x => x.Id == me.ParentMatchupId).First();
                            }
                        }
                    }

                    //List<List<MatchupModel>>

                    var currRow = new List<MatchupModel>();
                    int currRound = 1;

                    foreach (var m in matchups)
                    {
                        if (m.MatchupRound > currRound)
                        {
                            tournament.Rounds.Add(currRow);
                            currRow = new List<MatchupModel>();
                            currRound += 1;
                        }

                        currRow.Add(m);
                    }

                    tournament.Rounds.Add(currRow);
                }

            }

            return output;
        }
    }
}
