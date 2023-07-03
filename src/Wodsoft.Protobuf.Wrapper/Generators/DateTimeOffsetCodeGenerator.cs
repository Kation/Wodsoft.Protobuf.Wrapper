using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// DateTimeOffset code generator.
    /// </summary>
    public class DateTimeOffsetCodeGenerator : NonstandardStructureCodeGenerator<DateTimeOffset>
    {
        /// <inheritdoc/>
        public override WireFormat.WireType WireType => WireFormat.WireType.LengthDelimited;

        /// <inheritdoc/>
        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(Google.Protobuf.WellKnownTypes.Timestamp).GetMethod("FromDateTimeOffset", BindingFlags.Public | BindingFlags.Static));
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeMessageSize), BindingFlags.Static | BindingFlags.Public));
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
            ilGenerator.Emit(OpCodes.Call, typeof(Google.Protobuf.WellKnownTypes.Timestamp).GetMethod("ToDateTimeOffset"));
        }

        /// <inheritdoc/>
        protected override int CalculateSize(DateTimeOffset value)
        {            
            return CodedOutputStream.ComputeMessageSize(Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(value));
        }

        /// <inheritdoc/>
        public override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write DateTime value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(Google.Protobuf.WellKnownTypes.Timestamp).GetMethod("FromDateTimeOffset", BindingFlags.Public | BindingFlags.Static));
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteMessage)));
        }

        /// <inheritdoc/>
        protected override DateTimeOffset ReadValue(ref ParseContext context)
        {
            var timestamp = new Google.Protobuf.WellKnownTypes.Timestamp();
            context.ReadMessage(timestamp);
            return timestamp.ToDateTime();
        }

        /// <inheritdoc/>
        protected override void WriteValue(ref WriteContext context, DateTimeOffset value)
        {
            context.WriteMessage(Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(value));
        }
    }
}
