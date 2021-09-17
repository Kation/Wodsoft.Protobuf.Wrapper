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
        private readonly PropertyInfo _propertyInfo;

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

        /// <inheritdoc/>
        public int FieldNumber { get; }

        /// <inheritdoc/>
        public Type FieldType => _propertyInfo.PropertyType;

        /// <inheritdoc/>
        public string FieldName => _propertyInfo.Name;

        /// <inheritdoc/>
        public void GenerateReadFieldCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Callvirt, _propertyInfo.GetMethod);
        }

        /// <inheritdoc/>
        public void GenerateWriteFieldCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Callvirt, _propertyInfo.SetMethod);
        }
    }
}
