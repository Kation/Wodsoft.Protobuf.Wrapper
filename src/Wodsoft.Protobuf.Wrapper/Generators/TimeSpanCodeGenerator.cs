using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// TimeSpan code generator.
    /// </summary>
    public class TimeSpanCodeGenerator : NonstandardStructureCodeGenerator<TimeSpan>
    {
        public override WireFormat.WireType WireType => WireFormat.WireType.LengthDelimited;

        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(Google.Protobuf.WellKnownTypes.Duration).GetMethod("FromTimeSpan", BindingFlags.Public | BindingFlags.Static));
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeMessageSize), BindingFlags.Static | BindingFlags.Public));
        }

        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            var value = ilGenerator.DeclareLocal(typeof(Google.Protobuf.WellKnownTypes.Duration));
            ilGenerator.Emit(OpCodes.Newobj, typeof(Google.Protobuf.WellKnownTypes.Duration).GetConstructor(Array.Empty<Type>()));
            ilGenerator.Emit(OpCodes.Stloc, value);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, value);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadMessage)));
            ilGenerator.Emit(OpCodes.Ldloc, value);
            ilGenerator.Emit(OpCodes.Call, typeof(Google.Protobuf.WellKnownTypes.Duration).GetMethod("ToTimeSpan"));
        }

        protected override int CalculateSize(TimeSpan value)
        {            
            return CodedOutputStream.ComputeMessageSize(Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(value));
        }

        protected override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write DateTime value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(Google.Protobuf.WellKnownTypes.Duration).GetMethod("FromTimeSpan", BindingFlags.Public | BindingFlags.Static));
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteMessage)));
        }

        protected override TimeSpan ReadValue(ref ParseContext context)
        {
            var duration = new Google.Protobuf.WellKnownTypes.Duration();
            context.ReadMessage(duration);
            return duration.ToTimeSpan();
        }

        protected override void WriteValue(ref WriteContext context, TimeSpan value)
        {
            context.WriteMessage(Google.Protobuf.WellKnownTypes.Duration.FromTimeSpan(value));
        }
    }
}
