namespace Rx_Patient_PrescriptionSync.Model
{
    public class TokenRequestModel
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RequestedLocation { get; set; } = string.Empty;
        public string RequestedScope { get; set; } = string.Empty;
    }
}
