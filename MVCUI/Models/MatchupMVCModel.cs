using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Web;

namespace MVCUI.Models
{
    public class MatchupMVCModel
    {
        public int MatchupId { get; set; }
        public int TournamentId { get; set; }
        public int RoundNumber { get; set; }
        public int FirstTeamMatchupEntryId { get; set; }
        public string FirstTeamName { get; set; }
        public double FirstTeamScore { get; set; }
        public int SecondTeamMatchupEntryId { get; set; }
        public string SecondTeamName { get; set; }
        public double SecondTeamScore { get; set; }
    }
}