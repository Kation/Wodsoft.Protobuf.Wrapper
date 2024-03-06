using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// DateTime code generator.
    /// </summary>
    public class DateTimeCodeGenerator : NonstandardStructureCodeGenerator<DateTime>
    {
        /// <inheritdoc/>
        public override WireFormat.WireType WireType => WireFormat.WireType.LengthDelimited;

        /// <inheritdoc/>
        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            var next = ilGenerator.DefineLabel();
            ilGenerator.Emit(OpCodes.Ldloca, valueVariable.LocalIndex);
            ilGenerator.Emit(OpCodes.Call, typeof(DateTime).GetProperty("Kind").GetMethod);
            ilGenerator.Emit(OpCodes.Ldc_I4, (int)DateTimeKind.Utc);
            ilGenerator.Emit(OpCodes.Beq, next);
            ilGenerator.Emit(OpCodes.Ldloca, valueVariable.LocalIndex);
            ilGenerator.Emit(OpCodes.Call, typeof(DateTime).GetMethod(nameof(DateTime.ToUniversalTime), Array.Empty<Type>()));
            ilGenerator.Emit(OpCodes.Stloc, valueVariable);
            ilGenerator.MarkLabel(next);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(Google.Protobuf.WellKnownTypes.Timestamp).GetMethod(nameof(Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime), BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(DateTime) }, null));
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeMessageSize), BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(IMessage) }, null));
        }

        /// <inheritdoc/>
        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            var value = ilGenerator.DeclareLocal(typeof(Google.Protobuf.WellKnownTypes.Timestamp));
            ilGenerator.Emit(OpCodes.Newobj, typeof(Google.Protobuf.WellKnownTypes.Timestamp).GetConstructor(Array.Empty<Type>()));
            ilGenerator.Emit(OpCodes.Stloc, value);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, value);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadMessage)));
            ilGenerator.Emit(OpCodes.Ldloc, value);
            ilGenerator.Emit(OpCodes.Call, typeof(Google.Protobuf.WellKnownTypes.Timestamp).GetMethod(nameof(Google.Protobuf.WellKnownTypes.Timestamp.ToDateTime), Array.Empty<Type>()));
        }

        /// <inheritdoc/>
        protected override int CalculateSize(DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
                value = value.ToUniversalTime();
            return CodedOutputStream.ComputeMessageSize(Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(value));
        }

        /// <inheritdoc/>
        public override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write DateTime value
            var next = ilGenerator.DefineLabel();
            ilGenerator.Emit(OpCodes.Ldloca, valueVariable.LocalIndex);
            ilGenerator.Emit(OpCodes.Call, typeof(DateTime).GetProperty("Kind").GetMethod);
            ilGenerator.Emit(OpCodes.Ldc_I4, (int)DateTimeKind.Utc);
            ilGenerator.Emit(OpCodes.Beq, next);
            ilGenerator.Emit(OpCodes.Ldloca, valueVariable.LocalIndex);
            ilGenerator.Emit(OpCodes.Call, typeof(DateTime).GetMethod(nameof(DateTime.ToUniversalTime), Array.Empty<Type>()));
            ilGenerator.Emit(OpCodes.Stloc, valueVariable);
            ilGenerator.MarkLabel(next);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(Google.Protobuf.WellKnownTypes.Timestamp).GetMethod(nameof(Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime), BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(DateTime) }, null));
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteMessage), new Type[] { typeof(IMessage) }));
        }

        /// <inheritdoc/>
        protected override DateTime ReadValue(ref ParseContext context)
        {
            var timestamp = new Google.Protobuf.WellKnownTypes.Timestamp();
            context.ReadMessage(timestamp);
            return timestamp.ToDateTime();
        }

        /// <inheritdoc/>
        protected override void WriteValue(ref WriteContext context, DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
                value = value.ToUniversalTime();
            context.WriteMessage(Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(value));
        }
    }
}
