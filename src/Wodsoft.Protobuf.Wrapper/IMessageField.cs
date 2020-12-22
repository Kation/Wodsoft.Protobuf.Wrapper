using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf
{
    /// <summary>
    /// Protobuf message field definition.
    /// </summary>
    public interface IMessageField
    {
        /// <summary>
        /// Get field number.
        /// </summary>
        int FieldNumber { get; }

        /// <summary>
        /// Get field CLR type.
        /// </summary>
        Type FieldType { get; }

        /// <summary>
        /// Get field name.
        /// </summary>
        string FieldName { get; }

        /// <summary>
        /// Generate IL code that read field value of object.<br/>
        /// The top of the stack is a reference of object.<br/>
        /// There must be a value on the top of the stack. It can be null.
        /// </summary>
        /// <param name="ilGenerator">IL generator.</param>
        void GenerateReadFieldCode(ILGenerator ilGenerator);

        /// <summary>
        /// Generate IL code that write field value of object.<br/>
        /// The top of the stack is value that need to write to the field.<br/>
        /// The second of the stack is a reference of object.<br/>
        /// </summary>
        /// <param name="ilGenerator">IL generator.</param>
        void GenerateWriteFieldCode(ILGenerator ilGenerator);
    }
}
