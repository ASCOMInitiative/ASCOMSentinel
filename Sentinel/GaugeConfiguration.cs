namespace Sentinel
{
    /// <summary>
    /// Gauge configuration parameters
    /// </summary>
    public class GaugeConfiguration
    {

        #region Initialisers

        public GaugeConfiguration(PropertyName propertyName, double minimum, double maximum, double greenOrangeTransition, double orangeRedTransition, int majorSegments, int displayOrder = 0)
        {
            PropertyName = propertyName;
            Minimum = minimum;
            Maximum = maximum;
            GreenOrangeTransition = greenOrangeTransition;
            OrangeRedTransition = orangeRedTransition;
            MajorSegments = majorSegments;
            DisplayOrder = displayOrder;
        }

        public GaugeConfiguration()
        {
        }

        #endregion  

        public PropertyName PropertyName { get; set; } = PropertyName.WindDirection;

        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double GreenOrangeTransition { get; set; }
        public double OrangeRedTransition { get; set; }

        public int MajorSegments { get; set; } = 10;

        /// <summary>
        /// Display order of this gauge on the Display page (1-13). Lower values are displayed first.
        /// </summary>
        public int DisplayOrder { get; set; }

    }
}
