using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// Guid code generator.
    /// </summary>
    public class GuidCodeGenerator : NonstandardStructureCodeGenerator<Guid>
    {
        public override WireFormat.WireType WireType => WireFormat.WireType.LengthDelimited;

        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(ByteString).GetMethod("CopyFrom", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(byte[]) }, null));
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeBytesSize), BindingFlags.Static | BindingFlags.Public));
        }

        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadBytes)));
            ilGenerator.Emit(OpCodes.Call, typeof(ByteString).GetMethod("ToByteArray"));
            ilGenerator.Emit(OpCodes.Newobj, typeof(Guid).GetConstructor(new Type[] { typeof(byte[]) }));
        }

        protected override Guid GetDefaultValue() => Guid.Empty;

        protected override int CalculateSize(Guid value)
        {
            return CodedOutputStream.ComputeBytesSize(ByteString.CopyFrom(value.ToByteArray()));
        }

        protected override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write bool value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(Guid).GetMethod("ToByteArray"));
            ilGenerator.Emit(OpCodes.Call, typeof(ByteString).GetMethod("CopyFrom", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(byte[]) }, null));
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteBytes)));
        }

        protected override Guid ReadValue(ref ParseContext context)
        {
            return new Guid(context.ReadBytes().ToByteArray());
        }

        protected override void WriteValue(ref WriteContext context, Guid value)
        {
            context.WriteBytes(ByteString.CopyFrom(value.ToByteArray()));
        }
    }
}
