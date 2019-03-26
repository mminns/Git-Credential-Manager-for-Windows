using System;
using System.Collections.Generic;

namespace Microsoft.Alm.Authentication
{
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay, nq}")]
    public struct SecureData : IEquatable<SecureData>
    {
        internal static readonly SecureDataComparer Comparer = SecureDataComparer.Instance;

        internal SecureData(string key, string name, byte[] data)
        {
            _data = data;
            _key = key;
            _name = name;
        }

        private byte[] _data;
        private string _key;
        private string _name;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public byte[] Data
        {
            get { return _data; }
        }

        public string Key
        {
            get { return _key; }
        }

        public string Name
        {
            get { return _name; }
        }

        internal string DebuggerDisplay
        {
            get { return $"{nameof(SecureData)}: \"{_key}\" => \"{_name}\" : [{_data.Length}]"; }
        }

        public bool Equals(SecureData other)
            => Comparer.Equals(this, other);

        public override bool Equals(object obj)
        {
            return (obj is SecureData a && Equals(a))
                   || base.Equals(obj);
        }

        public override int GetHashCode()
            => Comparer.GetHashCode(this);

        public static bool operator ==(SecureData lhs, SecureData rhs)
            => Comparer.Equals(lhs, rhs);

        public static bool operator !=(SecureData lhs, SecureData rhs)
            => !Comparer.Equals(lhs, rhs);
    }

    internal class SecureDataComparer : IEqualityComparer<SecureData>
    {
        public static readonly SecureDataComparer Instance = new SecureDataComparer();

        private SecureDataComparer()
        { }

        public bool Equals(SecureData lhs, SecureData rhs)
        {
            if (lhs.Data?.Length != rhs.Data?.Length
                || !StringComparer.Ordinal.Equals(lhs.Key, rhs.Key)
                || !StringComparer.Ordinal.Equals(lhs.Name, rhs.Name))
                return false;

            if (lhs.Data is null && rhs.Data is null)
                return true;

            if (lhs.Data is null || rhs.Data is null)
                return false;

            for (int i = 0; i < lhs.Data.Length; i += 1)
            {
                if (lhs.Data[i] != rhs.Data[i])
                    return false;
            }

            return true;
        }

        public int GetHashCode(SecureData obj)
        {
            int keyHash = obj.Key?.GetHashCode() ?? 0;
            int nameHash = obj.Name?.GetHashCode() ?? 0;

            unchecked
            {
                return (keyHash & (int)0xFFFF0000) | (nameHash & 0x0000FFFF);
            }
        }
    }
}