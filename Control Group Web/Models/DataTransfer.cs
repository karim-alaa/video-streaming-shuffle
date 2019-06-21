using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Control_Group_Web.Models
{
    public class DataTransfer
    {
        public string data { get; set; }
        public string type { get; set; }
        
        public string Name { get; set; }
        public string email { get; set; }
        public string user_id { get; set; }
        public int Adamin_num { get; set; }
        public List<string> videos_urls { get; set; }


        

    }
}