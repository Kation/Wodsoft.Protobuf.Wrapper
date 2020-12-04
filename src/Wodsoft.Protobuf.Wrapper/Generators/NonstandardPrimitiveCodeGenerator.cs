using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// Nonstandard primitive code generator.<br/>
    /// Nonstandard means that type of <typeparamref name="T"/> is not support by Protobuf.
    /// </summary>
    /// <typeparam name="T">Object type.</typeparam>
    public abstract class NonstandardPrimitiveCodeGenerator<T> : PrimitiveCodeGenerator<T>
    {
        /// <summary>
        /// Create FieldCodec for type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="fieldNumber">Number of field.</param>
        /// <returns>Return FieldCodec&lt;<typeparamref name="T"/>&gt;</returns>
        public override FieldCodec<T> CreateFieldCodec(int fieldNumber)
        {
            var readerType = typeof(FieldCodec).Assembly.GetType("Google.Protobuf.ValueReader`1").MakeGenericType(typeof(T));
            var writerType = typeof(FieldCodec).Assembly.GetType("Google.Protobuf.ValueWriter`1").MakeGenericType(typeof(T));
            var constructor = typeof(FieldCodec<T>).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { readerType, writerType, typeof(Func<T, int>), typeof(uint), typeof(T) }, null);
            return (FieldCodec<T>)constructor.Invoke(new object[]{
                Delegate.CreateDelegate(readerType, this, GetType().GetMethod("ReadValue", BindingFlags.NonPublic | BindingFlags.Instance)),
                Delegate.CreateDelegate(writerType, this, GetType().GetMethod("WriteValue", BindingFlags.NonPublic | BindingFlags.Instance)),
                (Func<T, int>)CalculateSize,
                WireFormat.MakeTag(fieldNumber, WireType),
                default(T)
            });
        }

        /// <summary>
        /// Calculate message size of value.
        /// </summary>
        /// <param name="value">Value of <typeparamref name="T"/>.</param>
        /// <returns></returns>
        protected abstract int CalculateSize(T value);

        /// <summary>
        /// Get default value of <typeparamref name="T"/>.
        /// </summary>
        /// <returns>Return value of <typeparamref name="T"/>.</returns>
        protected virtual T GetDefaultValue() => default(T);

        /// <summary>
        /// Read value from Protobuf parse context.
        /// </summary>
        /// <param name="parser">Protobuf parse context.</param>
        /// <returns>Return value of <typeparamref name="T"/>.</returns>
        protected abstract T ReadValue(ref ParseContext parser);

        /// <summary>
        /// Write value to Protobuf write context.
        /// </summary>
        /// <param name="writer">Protobuf write context.</param>
        /// <param name="value">Value of <typeparamref name="T"/>.</param>
        protected abstract void WriteValue(ref WriteContext writer, T value);
    }
}
