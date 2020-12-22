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
        private FieldInfo _fieldInfo;

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

        public int FieldNumber { get; }

        public Type FieldType => _fieldInfo.FieldType;

        public string FieldName => _fieldInfo.Name;

        public void GenerateReadFieldCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldfld, _fieldInfo);
        }

        public void GenerateWriteFieldCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Stfld, _fieldInfo);
        }
    }
}
