using System;
using System.Collections.Generic;

namespace storcli.Models
{
    public class Server
    {
        public string Name { set; get; }
        public string OperatingSystem { set; get; }
        public List<Controller> Controllers { set; get; }
        public int TimeIntervalInSeconds { set; get; }
        public DateTime CurrentDateTime { set; get; }

        public String UnprocessedOutput {set; get;}
        public String StorcliVersion { set; get; }
    }
}
