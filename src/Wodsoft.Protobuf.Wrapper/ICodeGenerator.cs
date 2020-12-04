using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf
{
    /// <summary>
    /// Code generator for dynamic build message wrapper class.
    /// </summary>
    public interface ICodeGenerator
    {
        /// <summary>
        /// Get WireType of type.
        /// </summary>
        WireFormat.WireType WireType { get; }

        /// <summary>
        /// Generate IL code that calculate value's size.<br/>
        /// There must be a INT32 value on the top of stack.
        /// </summary>
        /// <param name="ilGenerator">IL generator.</param>
        /// <param name="valueVariable">Local variable of value.</param>
        void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable);

        /// <summary>
        /// Generate IL code that write tag and value use WriteContext.<br/>
        /// ref WriteContext is at argument 1(OpCodes.Ldarg_1).<br/>
        /// It should be empty in the stack after codes.
        /// </summary>
        /// <param name="ilGenerator">IL generator.</param>
        /// <param name="valueVariable">Local variable of value.</param>
        /// <param name="fieldNumber">Field Number.</param>
        void GenerateWriteCode(ILGenerator ilGenerator, LocalBuilder valueVariable, int fieldNumber);

        /// <summary>
        /// Generate IL code that read value use ParseContext.<br/>
        /// ref ParseContext is at argument 1(OpCodes.Ldarg_1).<br/>
        /// There must be a value on the top of stack. It can be null.
        /// </summary>
        /// <param name="ilGenerator"></param>
        void GenerateReadCode(ILGenerator ilGenerator);
    }

    /// <summary>
    /// Code generator for dynamic build message wrapper class of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of object.</typeparam>
    public interface ICodeGenerator<T> : ICodeGenerator
    {
        /// <summary>
        /// Create FieldCodec for type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="fieldNumber">Number of field.</param>
        /// <returns>Return FieldCodec&lt;<typeparamref name="T"/>&gt;</returns>
        FieldCodec<T> CreateFieldCodec(int fieldNumber);
    }

    /// <summary>
    /// Structure type code generator for dynamic build message wrapper class of type <typeparamref name="T"/>.<br/>
    /// Method are used to create custom FieldCodec&lt;<typeparamref name="T"/>&gt;.
    /// </summary>
    /// <typeparam name="T">Type of structure.</typeparam>
    public interface IStructureCodeGenerator<T> : ICodeGenerator<T>
        where T : struct
    {
        /// <summary>
        /// Calculate message size of value.
        /// </summary>
        /// <param name="value">Value of <typeparamref name="T"/>.</param>
        /// <returns></returns>
        int CalculateSize(T value);

        /// <summary>
        /// Read value from Protobuf parse context.
        /// </summary>
        /// <param name="parser">Protobuf parse context.</param>
        /// <returns>Return value of <typeparamref name="T"/>.</returns>
        T ReadValue(ref ParseContext parser);

        /// <summary>
        /// Write value to Protobuf write context.
        /// </summary>
        /// <param name="writer">Protobuf write context.</param>
        /// <param name="value">Value of <typeparamref name="T"/>.</param>
        void WriteValue(ref WriteContext writer, T value);
    }
}
