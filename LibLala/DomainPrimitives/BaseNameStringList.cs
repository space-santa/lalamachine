using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LibLala.DomainPrimitives
{
    public class BaseNameStringList<T> : IComparable<BaseNameStringList<T>> where T : BaseNameString
    {
        private List<T> Names { get; }

        public BaseNameStringList() { Names = new List<T>(); }

        public BaseNameStringList(IList<string> nameStrings)
        {
            ArgumentNullException.ThrowIfNull(nameStrings);

            Names = new List<T>();
            foreach (var name in nameStrings)
            {
                Add((T?)Activator.CreateInstance(typeof(T), name));
            }
        }

        public BaseNameStringList(string nameCsvString)
        {
            if (string.IsNullOrEmpty(nameCsvString))
            {
                throw new ArgumentNullException(paramName: nameof(nameCsvString));
            }

            var localName = nameCsvString.Split(',');
            Names = new List<T>();
            for (var i = 0; i < localName.Length; ++i)
            {
                Add((T?)Activator.CreateInstance(typeof(T), localName[i]));
            }
        }

        public void Add(T? name)
        {
            ArgumentNullException.ThrowIfNull(name, paramName: nameof(name));
            if (Names.Contains(name)) { return; }
            Names.Add(name);
        }

        public IList<string> ToStringList()
        {
            var stringList = new List<string>();

            foreach (var name in Names)
            {
                stringList.Add(name.ToString());
            }

            return stringList;
        }

        public string ToCsvString()
        {
            return string.Join(", ", Names);
        }

        public int CompareTo(BaseNameStringList<T>? other)
        {
            ArgumentNullException.ThrowIfNull(other, paramName: nameof(other));
            return string.Compare(ToCsvString(), other.ToCsvString(), StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null)
            {
                return false;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator ==(BaseNameStringList<T> left, BaseNameStringList<T> right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(BaseNameStringList<T> left, BaseNameStringList<T> right)
        {
            return !(left == right);
        }

        public static bool operator <(BaseNameStringList<T> left, BaseNameStringList<T> right)
        {
            return left is null ? right is object : left.CompareTo(right) < 0;
        }

        public static bool operator <=(BaseNameStringList<T> left, BaseNameStringList<T> right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        public static bool operator >(BaseNameStringList<T> left, BaseNameStringList<T> right)
        {
            return left is object && left.CompareTo(right) > 0;
        }

        public static bool operator >=(BaseNameStringList<T> left, BaseNameStringList<T> right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }
    }
}
