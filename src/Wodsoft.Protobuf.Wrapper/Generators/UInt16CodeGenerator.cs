using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// UInt16 code generator.
    /// </summary>
    public class UInt16CodeGenerator : NonstandardStructureCodeGenerator<ushort>
    {
        /// <inheritdoc/>
        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeInt32Size), BindingFlags.Static | BindingFlags.Public));
        }

        /// <inheritdoc/>
        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadInt32)));
            ilGenerator.Emit(OpCodes.Conv_U2);
        }

        /// <inheritdoc/>
        protected override int CalculateSize(ushort value)
        {
            return CodedOutputStream.ComputeInt32Size(value);
        }

        /// <inheritdoc/>
        public override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write bool value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteInt32)));
        }

        /// <inheritdoc/>
        protected override ushort ReadValue(ref ParseContext context)
        {
            return (ushort)context.ReadInt32();
        }

        /// <inheritdoc/>
        protected override void WriteValue(ref WriteContext context, ushort value)
        {
            context.WriteInt32(value);
        }
    }
}
