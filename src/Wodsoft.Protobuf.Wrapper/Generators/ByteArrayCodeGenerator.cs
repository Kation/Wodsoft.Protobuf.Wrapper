using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <inheritdoc/>
        public override WireFormat.WireType WireType => WireFormat.WireType.LengthDelimited;

        /// <inheritdoc/>
        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable, int fieldNumber)
        {
            var end = ilGenerator.DefineLabel();
            var next = ilGenerator.DefineLabel();
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Brtrue, next);
            ilGenerator.Emit(OpCodes.Ldc_I4_0);
            ilGenerator.Emit(OpCodes.Br, end);
            ilGenerator.MarkLabel(next);
            GenerateCalculateSizeCode(ilGenerator, valueVariable);
            ilGenerator.Emit(OpCodes.Ldc_I4, CodedOutputStream.ComputeTagSize(fieldNumber));
            ilGenerator.Emit(OpCodes.Add_Ovf);
            ilGenerator.MarkLabel(end);
        }

        /// <inheritdoc/>
        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Newobj, typeof(ReadOnlyMemory<byte>).GetConstructor(new Type[] { typeof(byte[]) }));
            ilGenerator.Emit(OpCodes.Call, typeof(UnsafeByteOperations).GetMethod(nameof(UnsafeByteOperations.UnsafeWrap), BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(ReadOnlyMemory<byte>) }, null));
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeBytesSize), BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(ByteString) }, null));
        }

        /// <inheritdoc/>
        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            var bytesLocal = ilGenerator.DeclareLocal(typeof(ByteString));
            var segmentLocal = ilGenerator.DeclareLocal(typeof(ArraySegment<byte>));
            var hasArrayLabel = ilGenerator.DefineLabel();
            var endIfLabel = ilGenerator.DefineLabel();
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadBytes), Array.Empty<Type>()));
            ilGenerator.Emit(OpCodes.Stloc, bytesLocal);
            ilGenerator.Emit(OpCodes.Ldloc, bytesLocal);
            ilGenerator.Emit(OpCodes.Call, typeof(ByteString).GetProperty("Memory").GetMethod);
            ilGenerator.Emit(OpCodes.Ldloca, segmentLocal);
#if NETSTANDARD2_1
            ilGenerator.Emit(OpCodes.Call, typeof(MemoryMarshal).GetMethod(nameof(MemoryMarshal.TryGetArray), BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(ReadOnlyMemory<>).MakeGenericType(Type.MakeGenericMethodParameter(0)), typeof(ArraySegment<>).MakeGenericType(Type.MakeGenericMethodParameter(0)).MakeByRefType() }, null).MakeGenericMethod(typeof(byte)));
#else
ilGenerator.Emit(OpCodes.Call, typeof(MemoryMarshal).GetMethods()
    .First(t=>t.Name == nameof(MemoryMarshal.TryGetArray)
        && t.IsGenericMethodDefinition && t.GetGenericArguments().Length == 1 && t.GetParameters().Length == 2
        && t.GetParameters()[0].ParameterType.IsGenericTypeDefinition && t.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(ReadOnlyMemory<>)
        && t.GetParameters()[1].ParameterType.IsGenericTypeDefinition && t.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(ArraySegment<>)).MakeGenericMethod(typeof(byte)));
#endif
            ilGenerator.Emit(OpCodes.Brtrue_S, hasArrayLabel);
            ilGenerator.Emit(OpCodes.Ldloc, bytesLocal);
            ilGenerator.Emit(OpCodes.Call, typeof(ByteString).GetMethod(nameof(ByteString.ToByteArray), Array.Empty<Type>()));
            ilGenerator.Emit(OpCodes.Br_S, endIfLabel);
            ilGenerator.MarkLabel(hasArrayLabel);
            ilGenerator.Emit(OpCodes.Ldloca, segmentLocal);
            ilGenerator.Emit(OpCodes.Call, typeof(ArraySegment<byte>).GetProperty("Array").GetMethod);
            ilGenerator.MarkLabel(endIfLabel);
        }

        /// <inheritdoc/>
        protected override byte[] GetDefaultValue() => Array.Empty<byte>();

        /// <inheritdoc/>
        protected override int CalculateSize(byte[] value)
        {
            return CodedOutputStream.ComputeBytesSize(ByteString.CopyFrom(value));
        }

        /// <inheritdoc/>
        public override void GenerateWriteCode(ILGenerator ilGenerator, LocalBuilder valueVariable, int fieldNumber)
        {
            var end = ilGenerator.DefineLabel();
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Brfalse, end);
            base.GenerateWriteCode(ilGenerator, valueVariable, fieldNumber);
            ilGenerator.MarkLabel(end);
        }

        /// <inheritdoc/>
        public override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write bool value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Newobj, typeof(ReadOnlyMemory<byte>).GetConstructor(new Type[] { typeof(byte[]) }));
            ilGenerator.Emit(OpCodes.Call, typeof(UnsafeByteOperations).GetMethod(nameof(UnsafeByteOperations.UnsafeWrap), BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(ReadOnlyMemory<byte>) }, null));
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteBytes), new Type[] { typeof(ByteString) }));
        }

        /// <inheritdoc/>
        protected override byte[] ReadValue(ref ParseContext context)
        {
            var bytes = context.ReadBytes();
            if (MemoryMarshal.TryGetArray(bytes.Memory, out var segment))
                return segment.Array;
            else
                return bytes.ToByteArray();
        }

        /// <inheritdoc/>
        protected override void WriteValue(ref WriteContext context, byte[] value)
        {
            context.WriteBytes(UnsafeByteOperations.UnsafeWrap(value));
        }
    }
}
