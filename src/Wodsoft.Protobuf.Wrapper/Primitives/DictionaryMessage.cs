using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
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
    /// <typeparam name="TDictionary"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DictionaryMessage<TDictionary, TKey, TValue> : Message<TDictionary>
        where TDictionary : IDictionary<TKey, TValue>, new()
    {
        private readonly static uint _KeyTag, _ValueTag, _MapTag;
        private readonly static FieldCodec<TKey> _KeyFieldCodec;
        private readonly static FieldCodec<TValue> _ValueFieldCodec;
        private readonly static MapField<TKey, TValue>.Codec _MapCodec;

        static DictionaryMessage()
        {
            var keyCodeGenerator = MessageBuilder.GetCodeGenerator<TKey>();
            if (keyCodeGenerator == null)
            {
                keyCodeGenerator = (ICodeGenerator<TKey>)Activator.CreateInstance(typeof(ObjectCodeGenerator<>).MakeGenericType(typeof(TKey)));
            }
            _KeyTag = WireFormat.MakeTag(1, keyCodeGenerator.WireType);
            _KeyFieldCodec = keyCodeGenerator.CreateFieldCodec(1);

            var valueType = typeof(TValue);
            bool isArray = false, isCollection = false;
            if (valueType.IsArray)
                isArray = true;
            else if (valueType.IsGenericType)
            {
                var genericType = valueType.GetGenericTypeDefinition();
                if (genericType == typeof(IList<>) || genericType == typeof(List<>) || genericType == typeof(ICollection<>) || genericType == typeof(IEnumerable<>))
                {
                    valueType = valueType.GetGenericArguments()[0];
                    isCollection = true;
                }
            }
            if (isArray)
            {
                var valueCodeGenerator = (ICodeGenerator<TValue>)Activator.CreateInstance(typeof(ArrayCodeGenerator<>).MakeGenericType(valueType.GetElementType()));
                _ValueTag = WireFormat.MakeTag(2, valueCodeGenerator.WireType);
                _ValueFieldCodec = valueCodeGenerator.CreateFieldCodec(2);
            }
            else if (isCollection)
            {
                var valueCodeGenerator = (ICodeGenerator<TValue>)Activator.CreateInstance(typeof(CollectionCodeGenerator<,>).MakeGenericType(valueType, typeof(TValue)));
                _ValueTag = WireFormat.MakeTag(2, valueCodeGenerator.WireType);
                _ValueFieldCodec = valueCodeGenerator.CreateFieldCodec(2);
            }
            else
            {
                var valueCodeGenerator = MessageBuilder.GetCodeGenerator<TValue>();
                if (valueCodeGenerator == null)
                {
                    valueCodeGenerator = (ICodeGenerator<TValue>)Activator.CreateInstance(typeof(ObjectCodeGenerator<>).MakeGenericType(typeof(TValue)));
                }
                _ValueTag = WireFormat.MakeTag(2, valueCodeGenerator.WireType);
                _ValueFieldCodec = valueCodeGenerator.CreateFieldCodec(2);
            }
            _MapTag = WireFormat.MakeTag(1, WireFormat.WireType.LengthDelimited);
        }

        /// <summary>
        /// Initialize collection message wrapper.
        /// </summary>
        public DictionaryMessage() { }

        /// <summary>
        /// Initialize collection message wrapper with a value.
        /// </summary>
        /// <param name="value">Value.</param>
        public DictionaryMessage(TDictionary value) : base(value) { }

        /// <inheritdoc/>
        protected override int CalculateSize()
        {
            if (SourceValue != null)
            {
                var size = SourceValue.Sum(t => new DictionaryItemMessage(t).CalculateSize());
            }
            return 0;
        }

        /// <inheritdoc/>
        protected override void Read(ref ParseContext parser)
        {
            if (SourceValue == null)
                SourceValue = new TDictionary();
            else
                SourceValue.Clear();
            while (parser.ReadTag() == _MapTag)
            {
                DictionaryItemMessage message = new DictionaryItemMessage();
                parser.ReadMessage(message);
                SourceValue.Add(message.Pair);
            }
        }

        /// <inheritdoc/>
        protected override void Write(ref WriteContext writer)
        {
            if (SourceValue != null)
            {
                foreach (var entry in SourceValue)
                {
                    writer.WriteTag(_MapTag);
                    var message = new DictionaryItemMessage(entry);
                    writer.WriteMessage(message);
                }
            }
        }

        /// <summary>
        /// Compute value size.
        /// </summary>
        /// <param name="value">value.</param>
        /// <returns></returns>
        public static int ComputeSize(TDictionary value)
        {
            if (value != null)
            {
                var size = value.Sum(t => new DictionaryItemMessage(t).CalculateSize());
            }
            return 0;
        }

        private class DictionaryItemMessage : IMessage, IBufferMessage
        {
            private KeyValuePair<TKey, TValue> _pair;

            MessageDescriptor IMessage.Descriptor => null;

            public DictionaryItemMessage() { }

            public DictionaryItemMessage(KeyValuePair<TKey, TValue> pair)
            {
                _pair = pair;
            }

            public KeyValuePair<TKey, TValue> Pair => _pair;

            public int CalculateSize()
            {
                var keyLength = _KeyFieldCodec.CalculateSizeWithTag(_pair.Key);
                var valueLength = _ValueFieldCodec.CalculateSizeWithTag(_pair.Value);
                //var sizeLength = CodedOutputStream.ComputeLengthSize(keyLength + valueLength);
                return keyLength + valueLength;
            }

            void IBufferMessage.InternalMergeFrom(ref ParseContext ctx)
            {
                TKey key = default;
                TValue value = default;
                uint tag;
                while ((tag = ctx.ReadTag()) != 0)
                {
                    if (tag == _KeyTag)
                        key = _KeyFieldCodec.Read(ref ctx);
                    else if (tag == _ValueTag)
                        value = _ValueFieldCodec.Read(ref ctx);
                }
                _pair = new KeyValuePair<TKey, TValue>(key, value);
            }

            void IBufferMessage.InternalWriteTo(ref WriteContext ctx)
            {
                _KeyFieldCodec.WriteTagAndValue(ref ctx, _pair.Key);
                _ValueFieldCodec.WriteTagAndValue(ref ctx, _pair.Value);
            }

            void IMessage.MergeFrom(CodedInputStream input)
            {
                throw new NotSupportedException();
            }

            void IMessage.WriteTo(CodedOutputStream output)
            {
                throw new NotSupportedException();
            }
        }
    }
}
