namespace Dynamensions.EasyVideoConverter.Core.Models
{
    public class PrepareProgress(string statusMessage, bool showPercent = true)
    {
        public string StatusMessage { get; set; } = statusMessage;
        public long Current { get; set; }
        public long Total { get; set; }
        public bool ShowPercent { get; set; } = showPercent;
        public double Percent
        {
            get
            {
                return Current == 0 ? 0 : ((double)Current / (double)Total) * 100;
            }

        }

        public string? PercentMessage { get; set; }
    }
}
