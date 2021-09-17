using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Primitives
{
    /// <summary>
    /// Base structure message wrapper.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StructureMessage<T> : Message<T>
        where T : struct
    {
        private readonly static IStructureCodeGenerator<T> _CodeGenerator;

        static StructureMessage()
        {
            _CodeGenerator = (IStructureCodeGenerator<T>)MessageBuilder.GetCodeGenerator<T>();
            if (_CodeGenerator == null)
                throw new NotSupportedException($"Type of \"{typeof(T).FullName}\" does not supported by StructureMessage<T>, because there is no code generator found.");
        }

        /// <summary>
        /// Initialize structure message wrapper.
        /// </summary>
        public StructureMessage() { }

        /// <summary>
        /// Initialize strcuture message wrapper with a value.
        /// </summary>
        /// <param name="value">Value.</param>
        public StructureMessage(T value) : base(value) { }

        /// <inheritdoc/>
        protected override int CalculateSize()
        {
            return _CodeGenerator.CalculateSize(SourceValue);
        }

        /// <inheritdoc/>
        protected override void Read(ref ParseContext parser)
        {
            SourceValue = _CodeGenerator.ReadValue(ref parser);
        }

        /// <inheritdoc/>
        protected override void Write(ref WriteContext writer)
        {
            _CodeGenerator.WriteValue(ref writer, SourceValue);
        }
    }
}
