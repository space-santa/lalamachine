using System;

namespace LibLala.DomainPrimitives
{
    internal class DiscNumber
    {
        public DiscNumber(uint number)
        {
            if (number > 99 || number < 1)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(number));
            }
            Value = number;
        }

        public uint Value { get; }
        public override string ToString() => Value.ToString(Constants.CULTURE);
    }
}
