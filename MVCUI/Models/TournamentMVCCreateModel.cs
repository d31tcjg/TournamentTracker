using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrackerLibrary.Models;

namespace MVCUI.Models
{
    public class TournamentMVCCreateModel
    {
        /// <summary>
        /// The name given to this tournament. 
        /// </summary>
        [Display(Name = "Tournament Name")]
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string TournamentName { get; set; }

        /// <summary>
        /// The amount of money each team needs to put up to enter.
        /// </summary>
        [Display(Name = "Entry Fee")]
        [Required]
        [DataType(DataType.Currency)]
        public decimal EntryFee { get; set; }

        /// <summary>
        /// The set of teams that have been entered. 
        /// </summary>
        [Display(Name = "Entered Teams")]
        public List<SelectListItem> EnteredTeams { get; set; } = new List<SelectListItem>();

        public List<string> SelectedEnteredTeams { get; set; } = new List<string>();

        /// <summary>
        /// The list of prizes for the various places. 
        /// </summary>
        [Display(Name = "Prizes")]

        public List<SelectListItem> Prizes { get; set; } = new List<SelectListItem>();

        public List<string> SelectedPrizes { get; set; } = new List<string>();


    }
}
