using System;
using System.Collections.Generic;
using System.Linq;

namespace IERIC.SumariosIERIC.Domain.ValueObjects
{
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        public static bool operator ==(ValueObject? left, ValueObject? right)
        {
            if (left is null && right is null)
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(ValueObject? left, ValueObject? right) =>
            !(left == right);

        public virtual bool Equals(ValueObject? other) =>
            other is not null && ValuesAreEqual(other);

        public override bool Equals(object? obj) =>
            obj is ValueObject valueObject && ValuesAreEqual(valueObject);

        public override int GetHashCode() =>
            GetAtomicValues().Aggregate(
                default(int),
                (hashcode, value) =>
                    HashCode.Combine(hashcode, value.GetHashCode()));

        protected abstract IEnumerable<object> GetAtomicValues();

        private bool ValuesAreEqual(ValueObject valueObject) =>
            GetAtomicValues().SequenceEqual(valueObject.GetAtomicValues());

        public ValueObject Clone()
        {
            return (ValueObject)MemberwiseClone();
        }
    }
}