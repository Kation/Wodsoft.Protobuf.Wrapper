using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// Primitive code generator.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PrimitiveCodeGenerator<T> : ICodeGenerator<T>
    {
        /// <summary>
        /// Get WireType of type.
        /// </summary>
        public virtual WireFormat.WireType WireType => WireFormat.WireType.Varint;

        /// <summary>
        /// Create FieldCodec for type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="fieldNumber">Number of field.</param>
        /// <returns>Return FieldCodec&lt;<typeparamref name="T"/>&gt;</returns>
        public abstract FieldCodec<T> CreateFieldCodec(int fieldNumber);

        /// <summary>
        /// Generate IL code that calculate value's size.<br/>
        /// Include field length.<br/>
        /// There must be a INT32 value on the top of stack.
        /// </summary>
        /// <param name="ilGenerator">IL generator.</param>
        /// <param name="valueVariable">Local variable of value.</param>
        /// <param name="fieldNumber">Field Number.</param>
        public virtual void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable, int fieldNumber)
        {
            GenerateCalculateSizeCode(ilGenerator, valueVariable);
            ilGenerator.Emit(OpCodes.Ldc_I4, CodedOutputStream.ComputeTagSize(fieldNumber));
            ilGenerator.Emit(OpCodes.Add_Ovf);
        }

        /// <summary>
        /// Generate IL code that calculate value's size.<br/>
        /// Not include filed length.<br/>
        /// There must be a INT32 value on the top of stack.
        /// </summary>
        /// <param name="ilGenerator">IL generator.</param>
        /// <param name="valueVariable">Local variable of value.</param>
        public abstract void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable);

        /// <summary>
        /// Generate IL code that read value use ParseContext.<br/>
        /// ref ParseContext is at argument 1(OpCodes.Ldarg_1).<br/>
        /// There must be a value on the top of stack. It can be null.
        /// </summary>
        /// <param name="ilGenerator"></param>
        public abstract void GenerateReadCode(ILGenerator ilGenerator);

        /// <summary>
        /// Generate IL code that write tag and value use WriteContext.<br/>
        /// ref WriteContext is at argument 1(OpCodes.Ldarg_1).<br/>
        /// It should be empty in the stack after codes.
        /// </summary>
        /// <param name="ilGenerator">IL generator.</param>
        /// <param name="valueVariable">Local variable of value.</param>
        /// <param name="fieldNumber">Field Number.</param>
        public virtual void GenerateWriteCode(ILGenerator ilGenerator, LocalBuilder valueVariable, int fieldNumber)
        {
            GenerateWriteTagCode(ilGenerator, fieldNumber);
            GenerateWriteValueCode(ilGenerator, valueVariable);
        }

        /// <summary>
        /// Generate IL code that write tag of field.<br/>
        /// ref WriteContext is at argument 1(OpCodes.Ldarg_1).<br/>
        /// It should be empty in the stack after codes.
        /// </summary>
        /// <param name="ilGenerator">IL generator.</param>
        /// <param name="fieldNumber">Field Number.</param>
        protected virtual void GenerateWriteTagCode(ILGenerator ilGenerator, int fieldNumber)
        {
            //Write tag
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldc_I4, (int)WireFormat.MakeTag(fieldNumber, WireType));
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteTag), BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(uint) }, null));
        }

        /// <summary>
        /// Generate IL code that write value of field.<br/>
        /// ref WriteContext is at argument 1(OpCodes.Ldarg_1).<br/>
        /// It should be empty in the stack after codes.
        /// </summary>
        /// <param name="ilGenerator">IL generator.</param>
        /// <param name="valueVariable">Local variable of value.</param>
        public abstract void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable);
    }
}
