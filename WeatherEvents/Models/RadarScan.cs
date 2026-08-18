namespace WeatherEvents.Models
{
    public class RadarScan
    {
        public long Id { get; set; }      // Primary key
        public string ScanId { get; set; } = string.Empty;  // From DMI
        public DateTime ScanTime { get; set; }              // From DMI
        public string StationId { get; set; } = string.Empty;
        public double[]? Bbox { get; set; }
        public string? DownloadUrl { get; set; }
        public DateTime IngestedAt { get; set; }
    }
}
