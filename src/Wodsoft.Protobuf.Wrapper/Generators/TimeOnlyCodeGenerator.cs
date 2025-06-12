using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

#if NET6_0_OR_GREATER
namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// TimeOnly code generator.
    /// </summary>
    public class TimeOnlyCodeGenerator : NonstandardStructureCodeGenerator<TimeOnly>
    {
        /// <inheritdoc/>
        public override WireFormat.WireType WireType => WireFormat.WireType.LengthDelimited;

        /// <inheritdoc/>
        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            var next = ilGenerator.DefineLabel();
            ilGenerator.Emit(OpCodes.Ldloca, valueVariable.LocalIndex);
            ilGenerator.Emit(OpCodes.Call, typeof(TimeOnly).GetProperty(nameof(TimeOnly.Ticks)).GetMethod);
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeInt64Size), BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(long) }, null));
        }

        /// <inheritdoc/>
        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadInt64)));
            ilGenerator.Emit(OpCodes.Newobj, typeof(TimeOnly).GetConstructor(new Type[] { typeof(long) }));
        }

        /// <inheritdoc/>
        protected override int CalculateSize(TimeOnly value)
        {
            return CodedOutputStream.ComputeInt64Size(value.Ticks);
        }

        /// <inheritdoc/>
        public override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write DateOnly value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloca, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(TimeOnly).GetProperty(nameof(TimeOnly.Ticks)).GetMethod);
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteInt64), new Type[] { typeof(long) }));
        }

        /// <inheritdoc/>
        protected override TimeOnly ReadValue(ref ParseContext context)
        {
            return new TimeOnly(context.ReadInt64());
        }

        /// <inheritdoc/>
        protected override void WriteValue(ref WriteContext context, TimeOnly value)
        {
            context.WriteInt64(value.Ticks);
        }
    }
}
#endif