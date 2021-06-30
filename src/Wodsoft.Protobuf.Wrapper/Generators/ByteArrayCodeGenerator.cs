using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// Byte array (byte[]) code generator.
    /// </summary>
    public class ByteArrayCodeGenerator : NonstandardPrimitiveCodeGenerator<byte[]>
    {
        public override WireFormat.WireType WireType => WireFormat.WireType.LengthDelimited;

        protected override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Newobj, typeof(ReadOnlyMemory<byte>).GetConstructor(new Type[] { typeof(byte[]) }));
            ilGenerator.Emit(OpCodes.Call, typeof(UnsafeByteOperations).GetMethod("UnsafeWrap", BindingFlags.Public | BindingFlags.Static));
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeBytesSize), BindingFlags.Static | BindingFlags.Public));
        }

        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            var bytesLocal = ilGenerator.DeclareLocal(typeof(ByteString));
            var segmentLocal = ilGenerator.DeclareLocal(typeof(ArraySegment<byte>));
            var hasArrayLabel = ilGenerator.DefineLabel();
            var endIfLabel = ilGenerator.DefineLabel();
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadBytes)));
            ilGenerator.Emit(OpCodes.Stloc, bytesLocal);
            ilGenerator.Emit(OpCodes.Ldloc, bytesLocal);
            ilGenerator.Emit(OpCodes.Call, typeof(ByteString).GetProperty("Memory").GetMethod);
            ilGenerator.Emit(OpCodes.Ldloca, segmentLocal);
            ilGenerator.Emit(OpCodes.Call, typeof(MemoryMarshal).GetMethod("TryGetArray", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(typeof(byte)));
            ilGenerator.Emit(OpCodes.Brtrue_S, hasArrayLabel);
            ilGenerator.Emit(OpCodes.Ldloc, bytesLocal);
            ilGenerator.Emit(OpCodes.Call, typeof(ByteString).GetMethod("ToByteArray"));
            ilGenerator.Emit(OpCodes.Br_S, endIfLabel);
            ilGenerator.MarkLabel(hasArrayLabel);
            ilGenerator.Emit(OpCodes.Ldloca, segmentLocal);
            ilGenerator.Emit(OpCodes.Call, typeof(ArraySegment<byte>).GetProperty("Array").GetMethod);
            ilGenerator.MarkLabel(endIfLabel);
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
            var bytes = context.ReadBytes();
            if (MemoryMarshal.TryGetArray(bytes.Memory, out var segment))
                return segment.Array;
            else
                return bytes.ToByteArray();
        }

        protected override void WriteValue(ref WriteContext context, byte[] value)
        {
            context.WriteBytes(UnsafeByteOperations.UnsafeWrap(value));
        }
    }
}
