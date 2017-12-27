using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Linq
{
    [Serializable]
    public sealed class ParseException : Exception
    {
        // Fields
        private int position;

        // Methods
        public ParseException(string message, int position) : base(message) => this.position = position;

        public override string ToString() => $"{Message} (at index {position})";

        // Properties
        public int Position => position;
    }
}
