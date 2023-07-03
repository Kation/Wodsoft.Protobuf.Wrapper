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
    /// Decimal code generator.
    /// </summary>
    public class DecimalCodeGenerator : NonstandardStructureCodeGenerator<decimal>
    {
        /// <inheritdoc/>
        public override WireFormat.WireType WireType => WireFormat.WireType.LengthDelimited;

        /// <inheritdoc/>
        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloca, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(Guid).GetMethod("ToByteArray"));
            ilGenerator.Emit(OpCodes.Call, typeof(ByteString).GetMethod("CopyFrom", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(byte[]) }, null));
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeBytesSize), BindingFlags.Static | BindingFlags.Public));
        }
#if NETSTANDARD2_0
        private static MethodInfo _MemoryMarshalCast = typeof(MemoryMarshal).GetMember("Cast").Cast<MethodInfo>().First(t =>
        {
            if (!t.IsGenericMethodDefinition)
                return false;
            if (t.GetGenericArguments().Length != 2)
                return false;
            var parameters = t.GetParameters();
            if (parameters.Length != 1)
                return false;
            if (!parameters[0].ParameterType.IsGenericType)
                return false;
            if (parameters[0].ParameterType.GetGenericTypeDefinition() != typeof(ReadOnlySpan<>))
                return false;
            return true;
        });
#else
        private static MethodInfo _MemoryMarshalCast = typeof(MemoryMarshal).GetMethod("Cast", 2, BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(ReadOnlySpan<>).MakeGenericType(Type.MakeGenericMethodParameter(0)) }, null);
#endif
        /// <inheritdoc/>
        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadBytes)));
            ilGenerator.Emit(OpCodes.Call, typeof(ByteString).GetProperty("Span").GetGetMethod());
            ilGenerator.Emit(OpCodes.Call, _MemoryMarshalCast.MakeGenericMethod(typeof(byte), typeof(int)));
            var readOnlySpan = ilGenerator.DeclareLocal(typeof(ReadOnlySpan<int>));
            ilGenerator.Emit(OpCodes.Stloc, readOnlySpan);
            ilGenerator.Emit(OpCodes.Ldloca, readOnlySpan);
            ilGenerator.Emit(OpCodes.Call, typeof(ReadOnlySpan<int>).GetMethod("ToArray"));
            ilGenerator.Emit(OpCodes.Newobj, typeof(decimal).GetConstructor(new Type[] { typeof(int[]) }));
        }

        /// <inheritdoc/>
        protected override decimal GetDefaultValue() => default;

        /// <inheritdoc/>
        protected override int CalculateSize(decimal value)
        {
            return CodedOutputStream.ComputeBytesSize(ByteString.CopyFrom(MemoryMarshal.Cast<int, byte>(decimal.GetBits(value))));
        }

        /// <inheritdoc/>
        public override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write bool value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(decimal).GetMethod("GetBits", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(decimal) }, null));
            ilGenerator.Emit(OpCodes.Call, typeof(ReadOnlySpan<int>).GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(int[]) }, null));
            ilGenerator.Emit(OpCodes.Call, _MemoryMarshalCast.MakeGenericMethod(typeof(int), typeof(byte)));
            ilGenerator.Emit(OpCodes.Call, typeof(ByteString).GetMethod("CopyFrom", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(ReadOnlySpan<byte>) }, null));
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteBytes)));
        }

        /// <inheritdoc/>
        protected override decimal ReadValue(ref ParseContext context)
        {
            return new decimal(MemoryMarshal.Cast<byte, int>(context.ReadBytes().Span).ToArray());
        }

        /// <inheritdoc/>
        protected override void WriteValue(ref WriteContext context, decimal value)
        {
            context.WriteBytes(ByteString.CopyFrom(MemoryMarshal.Cast<int, byte>(decimal.GetBits(value))));
        }
    }
}
