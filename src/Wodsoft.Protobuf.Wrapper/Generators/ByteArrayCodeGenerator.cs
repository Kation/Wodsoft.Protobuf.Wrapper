using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// Byte array (byte[]) code generator.
    /// </summary>
    public class ByteArrayCodeGenerator : NonstandardPrimitiveCodeGenerator<byte[]>
    {
        public override WireFormat.WireType WireType => WireFormat.WireType.LengthDelimited;

        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Newobj, typeof(ReadOnlyMemory<byte>).GetConstructor(new Type[] { typeof(byte[]) }));
            ilGenerator.Emit(OpCodes.Call, typeof(UnsafeByteOperations).GetMethod("UnsafeWrap", BindingFlags.Public | BindingFlags.Static));
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeBytesSize), BindingFlags.Static | BindingFlags.Public));
        }

        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadBytes)));
            ilGenerator.Emit(OpCodes.Call, typeof(ByteString).GetMethod("ToByteArray"));
        }

        protected override byte[] GetDefaultValue() => Array.Empty<byte>();

        protected override int CalculateSize(byte[] value)
        {
            return CodedOutputStream.ComputeBytesSize(ByteString.CopyFrom(value));
        }

        public override void GenerateWriteCode(ILGenerator ilGenerator, LocalBuilder valueVariable, int fieldNumber)
        {
            var end = ilGenerator.DefineLabel();
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Brfalse, end);
            base.GenerateWriteCode(ilGenerator, valueVariable, fieldNumber);
            ilGenerator.MarkLabel(end);
        }

        protected override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write bool value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Newobj, typeof(ReadOnlyMemory<byte>).GetConstructor(new Type[] { typeof(byte[]) }));
            ilGenerator.Emit(OpCodes.Call, typeof(UnsafeByteOperations).GetMethod("UnsafeWrap", BindingFlags.Public | BindingFlags.Static));
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteBytes)));
        }

        protected override byte[] ReadValue(ref ParseContext context)
        {
            return context.ReadBytes().ToByteArray();
        }

        protected override void WriteValue(ref WriteContext context, byte[] value)
        {
            context.WriteBytes(UnsafeByteOperations.UnsafeWrap(value));
        }
    }
}
