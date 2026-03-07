namespace ObsMan
{
    public class ObservingConditionsRule
    {
        public EqualityType EqualityType1 { get; set; } = EqualityType.NotInUse;
        public double Value1 { get; set; } = 0.0;
        public EqualityType EqualityType2 { get; set; } = EqualityType.NotInUse;
        public double Value2 { get; set; } = 0.0;
    }
}
