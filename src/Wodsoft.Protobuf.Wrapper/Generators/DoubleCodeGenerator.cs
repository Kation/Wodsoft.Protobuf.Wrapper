using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// Double code generator.
    /// </summary>
    public class DoubleCodeGenerator : StructureCodeGenerator<double>
    {
        /// <inheritdoc/>
        public override int CalculateSize(double value)
        {
            return CodedOutputStream.ComputeDoubleSize(value);
        }

        /// <inheritdoc/>
        public override FieldCodec<double> CreateFieldCodec(int fieldNumber)
        {
            return FieldCodec.ForDouble(WireFormat.MakeTag(fieldNumber, WireType));
        }

        /// <inheritdoc/>
        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeDoubleSize), BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(double) }, null));
        }

        /// <inheritdoc/>
        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadDouble), Array.Empty<Type>()));
        }

        /// <inheritdoc/>
        public override double ReadValue(ref ParseContext parser)
        {
            return parser.ReadDouble();
        }

        /// <inheritdoc/>
        public override void WriteValue(ref WriteContext writer, double value)
        {
            writer.WriteDouble(value);
        }

        /// <inheritdoc/>
        public override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write bool value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteDouble), new Type[] { typeof(double) }));
        }
    }
}
