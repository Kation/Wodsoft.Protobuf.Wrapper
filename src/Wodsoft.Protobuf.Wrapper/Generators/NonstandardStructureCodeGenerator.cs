using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// Nonstandard structure code generator.<br/>
    /// Nonstandard means that type of <typeparamref name="T"/> is not support by Protobuf.
    /// </summary>
    /// <typeparam name="T">Structure type.</typeparam>
    public abstract class NonstandardStructureCodeGenerator<T> : NonstandardPrimitiveCodeGenerator<T>, IStructureCodeGenerator<T>
        where T : struct
    {
        int IStructureCodeGenerator<T>.CalculateSize(T value) => CalculateSize(value);

        T IStructureCodeGenerator<T>.ReadValue(ref ParseContext parser) => ReadValue(ref parser);

        void IStructureCodeGenerator<T>.WriteValue(ref WriteContext writer, T value) => WriteValue(ref writer, value);
    }
}
