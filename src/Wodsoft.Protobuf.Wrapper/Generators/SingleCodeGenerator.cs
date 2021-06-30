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
        public override int CalculateSize(float value)
        {
            return CodedOutputStream.ComputeFloatSize(value);
        }

        public override FieldCodec<float> CreateFieldCodec(int fieldNumber)
        {
            return FieldCodec.ForFloat(WireFormat.MakeTag(fieldNumber, WireType));
        }

        protected override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeFloatSize), BindingFlags.Static | BindingFlags.Public));
        }

        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadFloat)));
        }

        public override float ReadValue(ref ParseContext parser)
        {
            return parser.ReadFloat();
        }

        public override void WriteValue(ref WriteContext writer, float value)
        {
            writer.WriteFloat(value);
        }

        protected override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write bool value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteFloat)));
        }
    }
}
