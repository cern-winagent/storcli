using Newtonsoft.Json;
using System.Collections.Generic;

namespace storcli.Storcli
{
    public class Controller
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { set; get; }
        public string SerialNumber { set; get; }
        public int NrDriveGroups { set; get; }
        public int NrPhysicalDrives { set; get; }
        public int NrVirtualDrives { set; get; }
        public List<VirtualDrive> VirtualDrives;
        public List<EnclosureDevice> EnclosureDevices;


        public Controller(int id)
        {
            Id = id;
            VirtualDrives = new List<VirtualDrive>();
            EnclosureDevices = new List<EnclosureDevice>();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
