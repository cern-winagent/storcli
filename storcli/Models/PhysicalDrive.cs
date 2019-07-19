namespace storcli.Models
{
    public class PhysicalDrive
    {
        public string SerialNumber { set; get; }
        public string Size { set; get; }
        public string ProductId { set; get; }
        public string VendorId { set; get; }
        public int MediaErrorCount { set; get; }
        public int PredictiveFailureCount { set; get; }
        public int Slot { set; get; }
        public string AssignmentType { get; set; }
        public int? VirtualDriveDigitalGroup { set; get; }
    }
}
