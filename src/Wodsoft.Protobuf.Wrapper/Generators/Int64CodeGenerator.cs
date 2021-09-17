using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// Int64 code generator.
    /// </summary>
    public class Int64CodeGenerator : StructureCodeGenerator<long>
    {
        /// <inheritdoc/>
        public override int CalculateSize(long value)
        {
            return CodedOutputStream.ComputeInt64Size(value);
        }

        /// <inheritdoc/>
        public override FieldCodec<long> CreateFieldCodec(int fieldNumber)
        {
            return FieldCodec.ForInt64(WireFormat.MakeTag(fieldNumber, WireType));
        }

        /// <inheritdoc/>
        protected override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeInt64Size), BindingFlags.Static | BindingFlags.Public));
        }

        /// <inheritdoc/>
        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadInt64)));
        }
        /// <inheritdoc/>

        public override long ReadValue(ref ParseContext parser)
        {
            return parser.ReadInt64();
        }

        /// <inheritdoc/>
        public override void WriteValue(ref WriteContext writer, long value)
        {
            writer.WriteInt64(value);
        }

        /// <inheritdoc/>
        protected override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write bool value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteInt64)));
        }
    }
}
