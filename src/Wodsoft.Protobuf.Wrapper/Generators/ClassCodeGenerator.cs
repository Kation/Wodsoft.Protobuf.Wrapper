using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    public abstract class ClassCodeGenerator<T> : PrimitiveCodeGenerator<T>
        where T : class
    {
        public override WireFormat.WireType WireType => WireFormat.WireType.LengthDelimited;

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
