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
    /// DateOnly code generator.
    /// </summary>
    public class DateOnlyCodeGenerator : NonstandardStructureCodeGenerator<DateOnly>
    {
        /// <inheritdoc/>
        public override WireFormat.WireType WireType => WireFormat.WireType.LengthDelimited;

        /// <inheritdoc/>
        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            var next = ilGenerator.DefineLabel();
            ilGenerator.Emit(OpCodes.Ldloca, valueVariable.LocalIndex);
            ilGenerator.Emit(OpCodes.Call, typeof(DateOnly).GetProperty(nameof(DateOnly.DayNumber)).GetMethod);
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeInt32Size), BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(int) }, null));
        }

        /// <inheritdoc/>
        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadInt32)));
            ilGenerator.Emit(OpCodes.Call, typeof(DateOnly).GetMethod(nameof(DateOnly.FromDayNumber), BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(int) }, null));
        }

        /// <inheritdoc/>
        protected override int CalculateSize(DateOnly value)
        {
            return CodedOutputStream.ComputeInt32Size(value.DayNumber);
        }

        /// <inheritdoc/>
        public override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write DateOnly value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloca, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(DateOnly).GetProperty(nameof(DateOnly.DayNumber)).GetMethod);
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteInt32), new Type[] { typeof(int) }));
        }

        /// <inheritdoc/>
        protected override DateOnly ReadValue(ref ParseContext context)
        {
            return DateOnly.FromDayNumber(context.ReadInt32());
        }

        /// <inheritdoc/>
        protected override void WriteValue(ref WriteContext context, DateOnly value)
        {
            context.WriteInt32(value.DayNumber);
        }
    }
}
#endif