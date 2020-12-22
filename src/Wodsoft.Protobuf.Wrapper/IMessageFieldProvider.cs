using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf
{
    /// <summary>
    /// Protobuf message field provider.
    /// Get message fields definition of .NET types.
    /// </summary>
    public interface IMessageFieldProvider
    {
        /// <summary>
        /// Get message fields of a type.
        /// </summary>
        /// <param name="type">Object type.</param>
        /// <returns>Return message fields.</returns>
        IEnumerable<IMessageField> GetFields(Type type);
    }
}
