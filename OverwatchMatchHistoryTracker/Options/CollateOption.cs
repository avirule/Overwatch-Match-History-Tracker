namespace OverwatchMatchHistoryTracker.Options
{
    public abstract class CollateOption
    {
        protected string _Name;
        protected string _Role;
        protected string _Outcome;

        public CollateOption() => _Role = _Outcome = _Name = string.Empty;
    }
}
