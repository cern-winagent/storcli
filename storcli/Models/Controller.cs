using System.Collections.Generic;

namespace storcli.Models
{
    public class Controller
    {
        public int ControllerNumber { set; get; }
        public string Model { set; get; }
        public string Type { set; get; }
        public string SerialNumber { set; get; }

        public List<EnclosureDevice> EnclosureDevices { set; get; }
        public List<VirtualDrive> VirtualDrives { set; get; }
    }
}
