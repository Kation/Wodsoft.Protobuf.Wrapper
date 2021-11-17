using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Wodsoft.Protobuf
{
    /// <summary>
    /// Protobuf message field provider.<br/>
    /// Get message fields definition of .NET types.
    /// </summary>
    public class GeneralMessageFieldProvider : IMessageFieldProvider
    {
        /// <summary>
        /// Get instance of <see cref="GeneralMessageFieldProvider">GeneralMessageFieldProvider</see>.
        /// </summary>
        public readonly static GeneralMessageFieldProvider Instance = new GeneralMessageFieldProvider();

        /// <inheritdoc/>
        public IEnumerable<IMessageField> GetFields(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(t => t.CanWrite && t.CanRead && t.GetMethod.GetParameters().Length == 0).OrderBy(t => t.Name) .ToArray();
            if (properties.Any(t => t.GetCustomAttribute<DataMemberAttribute>() != null))
            {
                var fields = properties.Where(t => t.GetCustomAttribute<DataMemberAttribute>() != null).Select(t => new PropertyMessageField(t.GetCustomAttribute<DataMemberAttribute>().Order, t)).OrderBy(t => t.FieldNumber).ToArray();
                foreach(var item in fields.GroupBy(t => t.FieldNumber))
                {
                    if (item.Count() > 1)
                        throw new InvalidDataContractException($"There are many properties of \"{type.FullName}\" with same order {item.Key}.");
                }
                return fields;
            }
            else
            {
                int i = 1;
                return properties.Select(t => new PropertyMessageField(i++, t)).ToArray();
            }
        }
    }
}
