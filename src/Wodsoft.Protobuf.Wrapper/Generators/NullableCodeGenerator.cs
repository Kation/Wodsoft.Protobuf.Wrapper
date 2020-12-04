using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// Nullable code generator.
    /// </summary>
    /// <typeparam name="T">Structor type.</typeparam>
    public sealed class NullableCodeGenerator<T> : NonstandardPrimitiveCodeGenerator<T?>
        where T : struct
    {
        private IStructureCodeGenerator<T> _codeGenerator;

        /// <summary>
        /// Create a nullable code generator.
        /// </summary>
        /// <param name="codeGenerator">Structure code generator.</param>
        public NullableCodeGenerator(IStructureCodeGenerator<T> codeGenerator)
        {
            _codeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
        }

        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            var next = ilGenerator.DefineLabel();
            var end = ilGenerator.DefineLabel();
            ilGenerator.Emit(OpCodes.Ldloca, valueVariable.LocalIndex);
            ilGenerator.Emit(OpCodes.Call, typeof(T?).GetProperty("HasValue").GetMethod);
            ilGenerator.Emit(OpCodes.Brtrue, next);
            ilGenerator.Emit(OpCodes.Ldc_I4_0);
            ilGenerator.Emit(OpCodes.Br, end);
            ilGenerator.MarkLabel(next);
            _codeGenerator.GenerateCalculateSizeCode(ilGenerator, valueVariable);
            ilGenerator.MarkLabel(end);
        }

        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            _codeGenerator.GenerateReadCode(ilGenerator);
            ilGenerator.Emit(OpCodes.Newobj, typeof(T?).GetConstructor(new Type[] { typeof(T) }));
        }

        public override void GenerateWriteCode(ILGenerator ilGenerator, LocalBuilder valueVariable, int fieldNumber)
        {
            var end = ilGenerator.DefineLabel();
            ilGenerator.Emit(OpCodes.Ldloca, valueVariable.LocalIndex);
            ilGenerator.Emit(OpCodes.Call, typeof(T?).GetProperty("HasValue").GetMethod);
            ilGenerator.Emit(OpCodes.Brfalse, end);
            _codeGenerator.GenerateWriteCode(ilGenerator, valueVariable, fieldNumber);
            ilGenerator.MarkLabel(end);
        }

        protected override int CalculateSize(T? value)
        {
            if (value.HasValue)
                return _codeGenerator.CalculateSize(value.Value);
            else
                return 0;
        }

        protected override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {

        }

        protected override T? ReadValue(ref ParseContext parser)
        {
            return _codeGenerator.ReadValue(ref parser);
        }

        protected override void WriteValue(ref WriteContext writer, T? value)
        {
            if (value.HasValue)
                _codeGenerator.WriteValue(ref writer, value.Value);
        }
    }
}
