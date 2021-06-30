using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// UInt64 code generator.
    /// </summary>
    public class UInt64CodeGenerator : StructureCodeGenerator<ulong>
    {
        public override int CalculateSize(ulong value)
        {
            return CodedOutputStream.ComputeUInt64Size(value);
        }

        public override FieldCodec<ulong> CreateFieldCodec(int fieldNumber)
        {
            return FieldCodec.ForUInt64(WireFormat.MakeTag(fieldNumber, WireType));
        }

        protected override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeUInt64Size), BindingFlags.Static | BindingFlags.Public));
        }

        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadUInt64)));
        }

        public override ulong ReadValue(ref ParseContext parser)
        {
            return parser.ReadUInt64();
        }

        public override void WriteValue(ref WriteContext writer, ulong value)
        {
            writer.WriteUInt64(value);
        }

        protected override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write bool value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteUInt64)));
        }
    }
}
