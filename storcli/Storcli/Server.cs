using Newtonsoft.Json;
using System.Collections.Generic;

namespace storcli.Storcli
{
    public class Server
    {
        public string HostName { set; get; }
        public string OperatingSystem { set; get; }
        public int NrControllers { set; get; }
        public List<Controller> Controllers { set; get; }
        public List<PhysicalDrive> PhysicalDrives { set; get; }

        public Server()
        {
            Controllers = new List<Controller>();
            PhysicalDrives = new List<PhysicalDrive>();
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
