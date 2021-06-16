using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Primitives
{
    public class StringMessage : Message<string>
    {
        private static readonly ICodeGenerator<string> _CodeGenerator = MessageBuilder.GetCodeGenerator<string>();

        public StringMessage() { }

        public StringMessage(string value) : base(value) { }

        protected override int CalculateSize()
        {
            return CodedOutputStream.ComputeStringSize(SourceValue);
        }

        protected override void Read(ref ParseContext parser)
        {
            SourceValue = parser.ReadString();
        }

        protected override void Write(ref WriteContext writer)
        {
            writer.WriteString(SourceValue);
        }
    }
}
