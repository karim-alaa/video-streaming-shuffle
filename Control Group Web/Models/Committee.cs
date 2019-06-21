using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Control_Group_Web.Models
{
    public class Committee
    {
        public string name { get; set; }
        public int views { get; set; }
        public bool active { get; set; }

        public Committee()
        {
            this.views = 0;
            this.active = false;
        }
    }
}