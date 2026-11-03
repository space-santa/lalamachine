namespace LibLala.DomainPrimitives
{
    public class BeatsPerMinute
    {
        public BeatsPerMinute(uint value)
        {
            Value = value;
        }

        public uint Value { get; }
        // 0 means the file doesn't have this tag, so there is nothing to show.
        public override string ToString() => Value == 0 ? "" : Value.ToString(Constants.CULTURE);
    }
}
