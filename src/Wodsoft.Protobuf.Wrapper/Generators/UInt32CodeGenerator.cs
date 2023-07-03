using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// UInt32 code generator.
    /// </summary>
    public class UInt32CodeGenerator : StructureCodeGenerator<uint>
    {
        /// <inheritdoc/>
        public override int CalculateSize(uint value)
        {
            return CodedOutputStream.ComputeUInt32Size(value);
        }

        /// <inheritdoc/>
        public override FieldCodec<uint> CreateFieldCodec(int fieldNumber)
        {
            return FieldCodec.ForUInt32(WireFormat.MakeTag(fieldNumber, WireType));
        }

        /// <inheritdoc/>
        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeUInt32Size), BindingFlags.Static | BindingFlags.Public));
        }

        /// <inheritdoc/>
        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadUInt32)));
        }

        /// <inheritdoc/>
        public override uint ReadValue(ref ParseContext parser)
        {
            return parser.ReadUInt32();
        }

        /// <inheritdoc/>
        public override void WriteValue(ref WriteContext writer, uint value)
        {
            writer.WriteUInt32(value);
        }

        /// <inheritdoc/>
        public override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write bool value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteUInt32)));
        }
    }
}
