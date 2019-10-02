using System;

namespace LibLala.DomainPrimitives
{
    public class Year
    {
        public Year(int? year)
        {
            if (year > 9999) { throw new ArgumentOutOfRangeException(paramName: nameof(year)); }
            // Because SQLite can't change the column to be nullable now.
            // TODO: Evaluate other ways to store the lib that doesn't require migrations.
            if (year is null || year < 0) { year = -1; }
            Value = (int)year;
        }

        public int Value { get; }
        public override string ToString() => Value.ToString(Constants.CULTURE);
    }
}
