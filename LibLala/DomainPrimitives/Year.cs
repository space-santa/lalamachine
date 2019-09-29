using System;

namespace LibLala.DomainPrimitives
{
    internal class Year
    {
        public Year(uint year)
        {
            if (year > 9999) { throw new ArgumentOutOfRangeException(paramName: nameof(year)); }
            Value = year;
        }

        public uint Value { get; }
        public override string ToString() => Value.ToString(Constants.CULTURE);
    }
}
