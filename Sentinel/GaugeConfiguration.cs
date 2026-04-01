namespace Sentinel
{
    /// <summary>
    /// Gauge configuration parameters
    /// </summary>
    public class GaugeConfiguration
    {
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double GreenOrangeTransition { get; set; }
        public double OrangeRedTransition { get; set; }
    }
}
