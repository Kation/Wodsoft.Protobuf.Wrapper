using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf
{
    /// <summary>
    /// Protobuf message field definition for property of CLR type.
    /// </summary>
    public class PropertyMessageField : IMessageField
    {
        private PropertyInfo _propertyInfo;

        /// <summary>
        /// Create message field.
        /// </summary>
        /// <param name="fieldNumber">Field number.</param>
        /// <param name="propertyInfo">Property info of object.</param>
        public PropertyMessageField(int fieldNumber, PropertyInfo propertyInfo)
        {
            FieldNumber = fieldNumber;
            _propertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
        }

        public int FieldNumber { get; }

        public Type FieldType => _propertyInfo.PropertyType;

        public string FieldName => _propertyInfo.Name;

        public void GenerateReadFieldCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Callvirt, _propertyInfo.GetMethod);
        }

        public void GenerateWriteFieldCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Callvirt, _propertyInfo.SetMethod);
        }
    }
}
