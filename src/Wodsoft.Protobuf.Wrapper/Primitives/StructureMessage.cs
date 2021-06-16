using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Primitives
{
    public class StructureMessage<T> : Message<T>
        where T : struct
    {
        private static IStructureCodeGenerator<T> _CodeGenerator = (IStructureCodeGenerator<T>)MessageBuilder.GetCodeGenerator<T>();

        public StructureMessage() { }

        public StructureMessage(T value) : base(value) { }

        protected override int CalculateSize()
        {
            return _CodeGenerator.CalculateSize(SourceValue);
        }

        protected override void Read(ref ParseContext parser)
        {
            SourceValue = _CodeGenerator.ReadValue(ref parser);
        }

        protected override void Write(ref WriteContext writer)
        {
            _CodeGenerator.WriteValue(ref writer, SourceValue);
        }
    }
}
