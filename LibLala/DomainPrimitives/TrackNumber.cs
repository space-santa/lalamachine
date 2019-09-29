using System;

namespace LibLala.DomainPrimitives
{
    internal class TrackNumber
    {
        public TrackNumber(uint number)
        {
            if (number == 0) { number = 1; }
            if (number > 999 || number < 1)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(number));
            }
            Value = number;
        }

        public uint Value { get; }
        public override string ToString() => Value.ToString(Constants.CULTURE);
    }
}
