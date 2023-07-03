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
        /// <inheritdoc/>
        public override int CalculateSize(ulong value)
        {
            return CodedOutputStream.ComputeUInt64Size(value);
        }

        /// <inheritdoc/>
        public override FieldCodec<ulong> CreateFieldCodec(int fieldNumber)
        {
            return FieldCodec.ForUInt64(WireFormat.MakeTag(fieldNumber, WireType));
        }

        /// <inheritdoc/>
        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeUInt64Size), BindingFlags.Static | BindingFlags.Public));
        }

        /// <inheritdoc/>
        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadUInt64)));
        }

        /// <inheritdoc/>
        public override ulong ReadValue(ref ParseContext parser)
        {
            return parser.ReadUInt64();
        }

        /// <inheritdoc/>
        public override void WriteValue(ref WriteContext writer, ulong value)
        {
            writer.WriteUInt64(value);
        }

        /// <inheritdoc/>
        public override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write bool value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteUInt64)));
        }
    }
}
