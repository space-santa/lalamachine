using System;

namespace LibLala.DomainPrimitives
{
    public class BaseNameString
    {
        private readonly string _name = "";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        public BaseNameString(string name)
        {
            ArgumentNullException.ThrowIfNull(name);

            if (name.Length > 256)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(name), message: "Name must not be longer than 256 characters.");
            }

            _name = name.Trim();
        }

        public override string ToString()
        {
            return _name;
        }

        public static bool operator ==(BaseNameString lhs, BaseNameString rhs)
        {
            if (lhs is null || rhs is null) { return false; }
            return lhs.ToString().Equals(rhs.ToString(), StringComparison.Ordinal);
        }

        public static bool operator !=(BaseNameString lhs, BaseNameString rhs)
        {
            if (lhs is null || rhs is null) { return false; }
            return !lhs.ToString().Equals(rhs.ToString(), StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
        {
            var newObject = obj as BaseNameString;

            if (newObject is { })
            {
                return this == newObject;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode(StringComparison.Ordinal);
        }
    }
}
