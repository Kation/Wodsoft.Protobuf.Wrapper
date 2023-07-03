using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// Enum code generator.
    /// </summary>
    /// <typeparam name="TEnum">Enum type.</typeparam>
    /// <typeparam name="TUnderlying">Underlying type of Enum.</typeparam>
    public class EnumCodeGenerator<TEnum, TUnderlying> : NonstandardPrimitiveCodeGenerator<TEnum>
        where TEnum : Enum
        where TUnderlying : struct
    {
        private readonly IStructureCodeGenerator<TUnderlying> _codeGenerator;

        /// <summary>
        /// Initialize Enum code generator.
        /// </summary>
        /// <param name="codeGenerator">Code generator for underlying type.</param>
        public EnumCodeGenerator(IStructureCodeGenerator<TUnderlying> codeGenerator)
        {
            _codeGenerator = codeGenerator;
        }

        /// <inheritdoc/>
        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            _codeGenerator.GenerateReadCode(ilGenerator);
        }

        /// <inheritdoc/>
        protected override int CalculateSize(TEnum value)
        {            
            return _codeGenerator.CalculateSize(Unsafe.As<TEnum, TUnderlying>(ref value));
        }

        /// <inheritdoc/>
        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            _codeGenerator.GenerateCalculateSizeCode(ilGenerator, valueVariable);
        }

        /// <inheritdoc/>
        public override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            _codeGenerator.GenerateWriteValueCode(ilGenerator, valueVariable);
        }

        /// <inheritdoc/>
        protected override TEnum ReadValue(ref ParseContext parser)
        {
            var underlying = _codeGenerator.ReadValue(ref parser);
            var value = Unsafe.As<TUnderlying, TEnum>(ref underlying);
            return value;
        }

        /// <inheritdoc/>
        protected override void WriteValue(ref WriteContext writer, TEnum value)
        {
            _codeGenerator.WriteValue(ref writer, Unsafe.As<TEnum, TUnderlying>(ref value));
        }
    }
}
