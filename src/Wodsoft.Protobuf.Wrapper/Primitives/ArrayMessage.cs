using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wodsoft.Protobuf.Generators;

namespace Wodsoft.Protobuf.Primitives
{
    /// <summary>
    /// Base array message wrapper.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ArrayMessage<T> : Message<T[]>
    {
        private readonly static uint _Tag;
        private readonly static FieldCodec<T> _FieldCodec;

        static ArrayMessage()
        {
            var codeGenerator = MessageBuilder.GetCodeGenerator<T>();
            if (codeGenerator == null)
            {
                codeGenerator = (ICodeGenerator<T>)Activator.CreateInstance(typeof(ObjectCodeGenerator<>).MakeGenericType(typeof(T)));
            }
            _Tag = WireFormat.MakeTag(1, codeGenerator.WireType);
            _FieldCodec = codeGenerator.CreateFieldCodec(1);
        }

        /// <summary>
        /// Initialize array message wrapper.
        /// </summary>
        public ArrayMessage() { }

        /// <summary>
        /// Initialize array message wrapper with a value.
        /// </summary>
        /// <param name="value">Value.</param>
        public ArrayMessage(T[] value) : base(value) { }

        /// <inheritdoc/>
        protected override int CalculateSize()
        {
            if (SourceValue != null)
                return SourceValue.Sum(t => _FieldCodec.CalculateSizeWithTag(t));
            return 0;
        }

        /// <inheritdoc/>
        protected override void Read(ref ParseContext parser)
        {
            List<T> list = new List<T>();
            while (parser.ReadTag() == _Tag)
            {
                list.Add(_FieldCodec.Read(ref parser));
            }
            SourceValue = list.ToArray();
        }

        /// <inheritdoc/>
        protected override void Write(ref WriteContext writer)
        {
            if (SourceValue != null)
            {
                for (int i = 0; i < SourceValue.Length; i++)
                    _FieldCodec.WriteTagAndValue(ref writer, SourceValue[i]);
            }
        }
    }
}
