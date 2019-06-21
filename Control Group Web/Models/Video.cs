using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Control_Group_Web.Models
{
    public class Video
    {
        public string url { get; set; }
        public bool seen { get; set; }
        public bool active { get; set; }

        public Video()
        {
            this.seen = false;
            this.active = false;
        }
    }
}