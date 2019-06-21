using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Control_Group_Web.Models
{
    public class School
    {
        public string name { get; set; }
        public int views { get; set; }
        public bool active { get; set; }

        public School()
        {
            this.views = 0;
            this.active = false;
        }
    }
}