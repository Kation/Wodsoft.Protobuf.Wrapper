using Google.Protobuf;
using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wodsoft.Protobuf.Generators;

namespace Wodsoft.Protobuf.Primitives
{
    /// <summary>
    /// Base collection message wrapper.
    /// </summary>
    /// <typeparam name="TCollection"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    public class CollectionMessage<TCollection, TElement> : Message<TCollection>
        where TCollection : IList<TElement>, new()
    {
        private readonly static uint _Tag;
        private readonly static FieldCodec<TElement> _FieldCodec;

        static CollectionMessage()
        {
            var codeGenerator = MessageBuilder.GetCodeGenerator<TElement>();
            if (codeGenerator == null)
            {
                codeGenerator = (ICodeGenerator<TElement>)Activator.CreateInstance(typeof(ObjectCodeGenerator<>).MakeGenericType(typeof(TElement)));
            }
            _Tag = WireFormat.MakeTag(1, codeGenerator.WireType);
            _FieldCodec = codeGenerator.CreateFieldCodec(1);
        }

        /// <summary>
        /// Initialize collection message wrapper.
        /// </summary>
        public CollectionMessage() { }

        /// <summary>
        /// Initialize collection message wrapper with a value.
        /// </summary>
        /// <param name="value">Value.</param>
        public CollectionMessage(TCollection value) : base(value) { }

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
            if (SourceValue == null)
                SourceValue = new TCollection();
            else
                SourceValue.Clear();
            while (parser.ReadTag() == _Tag)
            {
                SourceValue.Add(_FieldCodec.Read(ref parser));
            }
        }

        /// <inheritdoc/>
        protected override void Write(ref WriteContext writer)
        {
            if (SourceValue != null)
            {
                for (int i = 0; i < SourceValue.Count; i++)
                    _FieldCodec.WriteTagAndValue(ref writer, SourceValue[i]);
            }
        }
    }
}
