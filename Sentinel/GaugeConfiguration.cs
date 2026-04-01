namespace Sentinel
{
    /// <summary>
    /// Gauge configuration parameters
    /// </summary>
    public class GaugeConfiguration
    {

        #region Initialisers

        public GaugeConfiguration(double minimum, double maximum, double greenOrangeTransition, double orangeRedTransition)
        {
            Minimum = minimum;
            Maximum = maximum;
            GreenOrangeTransition = greenOrangeTransition;
            OrangeRedTransition = orangeRedTransition;
        }

        public GaugeConfiguration()
        {
        }

        #endregion

        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double GreenOrangeTransition { get; set; }
        public double OrangeRedTransition { get; set; }

    }
}
