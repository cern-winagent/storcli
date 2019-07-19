using Newtonsoft.Json;

namespace storcli.Storcli
{
    public class VirtualDrive
    {
        public int Id { set; get; }
        public int DigitalGroup { set; get; }
        public string Type { set; get; }
        public string Size { set; get; }
        public string State { set; get; }
        public VirtualDrive(int id, int dg, string type, string size, string state)
        {
            Id = id;
            DigitalGroup = dg;
            Type = type;
            Size = size;
            State = state;
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
