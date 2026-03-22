namespace Sentinel
{
    public class GaugeConfiguration
    {
        public double Minimum { get; set; } = 0.0;
        public double Maximum { get; set; } = 0.0;
        public double OrangeGreenTransition { get; set; } = 0.0;
        public double RedOrangeTransition { get; set; } = 0.0;
    }
}
