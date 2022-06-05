﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary.Models
{
    public class PersonModel
    {
        /// <summary>
        /// The unique identifier for the person. 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The first name of the person
        /// </summary>
        [Display(Name = "First Name")]
        [StringLength(100, MinimumLength = 2)]
        [Required]
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of the person.
        /// </summary>
        [Display(Name = "Last Name")]
        [StringLength(100, MinimumLength = 2)]
        [Required]
        public string LastName { get; set; }

        /// <summary>
        /// The primary email address of the person.
        /// </summary>
        [Display(Name = "Email Address")]
        [StringLength(200, MinimumLength = 6)]
        [EmailAddress(ErrorMessage = "Email is not valid")]
        [Required]
        public string EmailAddress { get; set; }

        /// <summary>
        /// The primary cell phone number of the person.
        /// </summary>
        [Display(Name = "Cellphone Number")]
        [StringLength(100, MinimumLength = 10)]
        [Phone(ErrorMessage = "Phone Number is not valid")]
        [Required]
        public string CellphoneNumber { get; set; }

        [Display(Name = "Full Name")]
        public string FullName
        {
            get => $"{ FirstName } { LastName }";
        }
        


    }
}
