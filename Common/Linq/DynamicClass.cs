using System.Reflection;
using System.Text;

namespace Common.Linq
{
    public abstract class DynamicClass
    {
        // Methods
        protected DynamicClass()
        {
        }

        public override string ToString()
        {
            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            for (int i = 0; i < properties.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }
                builder.Append(properties[i].Name);
                builder.Append("=");
                builder.Append(properties[i].GetValue(this, null));
            }
            builder.Append("}");
            return builder.ToString();
        }
    }


}
