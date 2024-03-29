﻿using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// Class object code generator.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ClassCodeGenerator<T> : PrimitiveCodeGenerator<T>
        where T : class
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
        public override void GenerateWriteCode(ILGenerator ilGenerator, LocalBuilder valueVariable, int fieldNumber)
        {
            var end = ilGenerator.DefineLabel();
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Brfalse, end);
            base.GenerateWriteCode(ilGenerator, valueVariable, fieldNumber);
            ilGenerator.MarkLabel(end);
        }
    }
}
