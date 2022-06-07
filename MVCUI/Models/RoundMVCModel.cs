using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCUI.Models
{
    public class RoundMVCModel
    {
        public int RoundNumber { get; set; }
        public string RoundName { get; set; }
        public RoundStatus Status { get; set; }
    }
}