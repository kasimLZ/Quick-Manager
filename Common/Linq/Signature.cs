using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Linq
{
    internal class Signature : IEquatable<Signature>
    {
        // Fields
        public int hashCode;
        public DynamicProperty[] properties;

        // Methods
        public Signature(IEnumerable<DynamicProperty> properties)
        {
            this.properties = properties.ToArray();
            hashCode = 0;
            foreach (DynamicProperty property in properties)
                hashCode ^= property.Name.GetHashCode() ^ property.Type.GetHashCode();
        }

        public bool Equals(Signature other)
        {
            if (properties.Length != other.properties.Length) return false;

            for (int i = 0; i < properties.Length; i++)
            {
                if ((properties[i].Name != other.properties[i].Name) || (properties[i].Type != other.properties[i].Type)) return false;
            }
            return true;
        }

        public override bool Equals(object obj) => ((obj is Signature) && Equals((Signature)obj));

        public override int GetHashCode() => hashCode;
    }


}
