﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary.Models
{
    public class TeamModel
    {
        public int Id { get; set; }

        [Display(Name = "Team Name")]
        public string TeamName { get; set; }

        [Display(Name = "Team Member List")]
        public List<PersonModel> TeamMembers { get; set; } = new List<PersonModel>();
    }
}
