using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf
{
    /// <summary>
    /// Protobuf message field definition for field of CLR type.
    /// </summary>
    public class FieldMessageField : IMessageField
    {
        private readonly FieldInfo _fieldInfo;

        /// <summary>
        /// Create message field.
        /// </summary>
        /// <param name="fieldNumber">Field number.</param>
        /// <param name="fieldInfo">Field info of object.</param>
        public FieldMessageField(int fieldNumber, FieldInfo fieldInfo)
        {
            FieldNumber = fieldNumber;
            _fieldInfo = fieldInfo ?? throw new ArgumentNullException(nameof(fieldNumber));
        }

        /// <inheritdoc/>
        public int FieldNumber { get; }

        /// <inheritdoc/>
        public Type FieldType => _fieldInfo.FieldType;

        /// <inheritdoc/>
        public string FieldName => _fieldInfo.Name;

        /// <inheritdoc/>
        public void GenerateReadFieldCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldfld, _fieldInfo);
        }

        /// <inheritdoc/>
        public void GenerateWriteFieldCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Stfld, _fieldInfo);
        }
    }
}
