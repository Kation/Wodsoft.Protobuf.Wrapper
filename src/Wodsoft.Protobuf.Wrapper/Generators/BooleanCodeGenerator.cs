using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// Boolean code generator.
    /// </summary>
    public class BooleanCodeGenerator : StructureCodeGenerator<bool>
    {
        /// <inheritdoc/>
        public override int CalculateSize(bool value)
        {
            return CodedOutputStream.ComputeBoolSize(value);
        }

        /// <inheritdoc/>
        public override FieldCodec<bool> CreateFieldCodec(int fieldNumber)
        {
            return FieldCodec.ForBool(WireFormat.MakeTag(fieldNumber, WireType));
        }

        /// <inheritdoc/>
        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeBoolSize), BindingFlags.Static | BindingFlags.Public));
        }

        /// <inheritdoc/>
        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadBool)));
        }

        /// <inheritdoc/>
        public override bool ReadValue(ref ParseContext parser)
        {
            return parser.ReadBool();
        }

        /// <inheritdoc/>
        public override void WriteValue(ref WriteContext writer, bool value)
        {
            writer.WriteBool(value);
        }

        /// <inheritdoc/>
        public override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write bool value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteBool)));
        }
    }
}
