using Newtonsoft.Json;

namespace storcli.Storcli
{
    public class PhysicalDrive
    {
        public int Slot { set; get; }
        public string SerialNumber { set; get; }
        public string Model { set; get; }
        public int MediaErrorCount { set; get; }
        public int PredictiveFailureCount { set; get; }
        public string Size { set; get; }
        public string State { set; get; }
        public string DigitalGroup { set; get; }
        public int ControllerId { set; get; }
        public int EnclosureId { set; get; }
        public string ProducerId { set; get; }

        public PhysicalDrive() { }
        public PhysicalDrive(int id, int enclosure, int ctrl)
        {
            Slot = id;
            EnclosureId = enclosure;
            ControllerId = ctrl;
        }

        public PhysicalDrive(int slot)
        {
            Slot = slot;
        }


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
