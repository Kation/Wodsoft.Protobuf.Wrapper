using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// Single code generator.
    /// </summary>
    public class SingleCodeGenerator : StructureCodeGenerator<float>
    {
        /// <inheritdoc/>
        public override int CalculateSize(float value)
        {
            return CodedOutputStream.ComputeFloatSize(value);
        }

        /// <inheritdoc/>
        public override FieldCodec<float> CreateFieldCodec(int fieldNumber)
        {
            return FieldCodec.ForFloat(WireFormat.MakeTag(fieldNumber, WireType));
        }

        /// <inheritdoc/>
        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeFloatSize), BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(float) }, null));
        }

        /// <inheritdoc/>
        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadFloat), Array.Empty<Type>()));
        }

        /// <inheritdoc/>
        public override float ReadValue(ref ParseContext parser)
        {
            return parser.ReadFloat();
        }

        /// <inheritdoc/>
        public override void WriteValue(ref WriteContext writer, float value)
        {
            writer.WriteFloat(value);
        }

        /// <inheritdoc/>
        public override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write bool value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteFloat), new Type[] { typeof(float) }));
        }
    }
}
