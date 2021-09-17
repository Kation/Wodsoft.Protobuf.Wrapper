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
        private static IStructureCodeGenerator<T> _CodeGenerator = (IStructureCodeGenerator<T>)MessageBuilder.GetCodeGenerator<T>();

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
