using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Linq
{
    public class DynamicProperty
    {
        // Fields
        private string name;
        private Type type;

        // Methods
        public DynamicProperty(string name, Type type)
        {
            this.name = name ?? throw new ArgumentNullException("name");
            this.type = type ?? throw new ArgumentNullException("type");
        }

        // Properties
        public string Name => name;

        public Type Type => type;
    }



}
