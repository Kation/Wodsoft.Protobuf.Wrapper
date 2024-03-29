﻿using Google.Protobuf;
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
        /// <inheritdoc/>
        public override WireFormat.WireType WireType => WireFormat.WireType.LengthDelimited;

        /// <inheritdoc/>
        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            ilGenerator.Emit(OpCodes.Ldloca, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(Guid).GetMethod(nameof(Guid.ToByteArray), Array.Empty<Type>()));
            ilGenerator.Emit(OpCodes.Call, typeof(ByteString).GetMethod(nameof(ByteString.CopyFrom), BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(byte[]) }, null));
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeBytesSize), BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(ByteString) }, null));
        }

        /// <inheritdoc/>
        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadBytes), Array.Empty<Type>()));
            ilGenerator.Emit(OpCodes.Call, typeof(ByteString).GetMethod(nameof(ByteString.ToByteArray), Array.Empty<Type>()));
            ilGenerator.Emit(OpCodes.Newobj, typeof(Guid).GetConstructor(new Type[] { typeof(byte[]) }));
        }

        /// <inheritdoc/>
        protected override Guid GetDefaultValue() => Guid.Empty;

        /// <inheritdoc/>
        protected override int CalculateSize(Guid value)
        {
            return CodedOutputStream.ComputeBytesSize(ByteString.CopyFrom(value.ToByteArray()));
        }

        /// <inheritdoc/>
        public override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            //Write bool value
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloca, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(Guid).GetMethod(nameof(Guid.ToByteArray), Array.Empty<Type>()));
            ilGenerator.Emit(OpCodes.Call, typeof(ByteString).GetMethod(nameof(ByteString.CopyFrom), BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(byte[]) }, null));
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteBytes), new Type[] { typeof(ByteString) }));
        }

        /// <inheritdoc/>
        protected override Guid ReadValue(ref ParseContext context)
        {
            return new Guid(context.ReadBytes().ToByteArray());
        }

        /// <inheritdoc/>
        protected override void WriteValue(ref WriteContext context, Guid value)
        {
            context.WriteBytes(ByteString.CopyFrom(value.ToByteArray()));
        }
    }
}
