using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// Structure type code generator.
    /// </summary>
    /// <typeparam name="T">Structure type.</typeparam>
    public abstract class StructureCodeGenerator<T> : PrimitiveCodeGenerator<T>, IStructureCodeGenerator<T>
        where T : struct
    {
        /// <summary>
        /// Calculate message size of value.
        /// </summary>
        /// <param name="value">Value of <typeparamref name="T"/>.</param>
        /// <returns></returns>
        public abstract int CalculateSize(T value);

        /// <summary>
        /// Read value from Protobuf parse context.
        /// </summary>
        /// <param name="parser">Protobuf parse context.</param>
        /// <returns>Return value of <typeparamref name="T"/>.</returns>
        public abstract T ReadValue(ref ParseContext parser);

        /// <summary>
        /// Write value to Protobuf write context.
        /// </summary>
        /// <param name="writer">Protobuf write context.</param>
        /// <param name="value">Value of <typeparamref name="T"/>.</param>
        public abstract void WriteValue(ref WriteContext writer, T value);

        /// <summary>
        /// Get or set the type of code generator is a enum type.
        /// </summary>
        public bool IsEnum { get; set; }
    }
}
