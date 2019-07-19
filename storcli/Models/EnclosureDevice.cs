using System.Collections.Generic;

namespace storcli.Models
{
    public class EnclosureDevice
    {
        public int Eid { set; get; }
        public int NrSlots { set; get; }
        public int NrPDs { set; get; }
        public string Port { get; set; }

        public List<PhysicalDrive> PhysicalDrives { set; get; }
    }
}
