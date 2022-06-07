using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCUI.Models
{
    public class TournamentMVCDetailsModel
    {
        /// <summary>
        /// The name given to this tournament. 
        /// </summary>
        [Display(Name = "Tournament Name")]
        public string TournamentName { get; set; }

        public List<RoundMVCModel> Rounds { get; set; } = new List<RoundMVCModel>();
    }
}