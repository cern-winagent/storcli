using Newtonsoft.Json;

namespace storcli.Storcli
{
    public class EnclosureDevice
    {
        public int EID { set; get; }
        public int Slots { set; get; }
        public int PDs { set; get; }
        public string Port { set; get; }

        public EnclosureDevice(int eid, int slots, int pds, string port)
        {
            EID = eid;
            Slots = slots;
            PDs = pds;
            Port = port;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
