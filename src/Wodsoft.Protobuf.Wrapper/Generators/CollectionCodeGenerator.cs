using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// Collection object code generator.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TList"></typeparam>
    public class CollectionCodeGenerator<T, TList> : NonstandardPrimitiveCodeGenerator<TList>
        where TList : IEnumerable<T>
    {
        private static readonly ICodeGenerator<T> _CodeGenerator = MessageBuilder.GetCodeGenerator<T>();

        /// <inheritdoc/>
        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        protected override int CalculateSize(TList value)
        {
            var codec = _CodeGenerator.CreateFieldCodec(1);
            var size = value.Sum(t => codec.CalculateSizeWithTag(t));
            var length = CodedOutputStream.ComputeLengthSize(size);
            return size + length;
        }

        /// <inheritdoc/>
        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        protected override TList ReadValue(ref ParseContext parser)
        {
            //var collection = new RepeatedField<T>();
            //collection.AddEntriesFrom(ref parser, _codeGenerator.CreateFieldCodec(2));
            //return collection.ToArray();
            var messsage = new CollectionMessage(_CodeGenerator.CreateFieldCodec(1), WireFormat.MakeTag(1, _CodeGenerator.WireType));
            parser.ReadMessage(messsage);
            return (TList)(object)messsage.ToList();
        }

        /// <inheritdoc/>
        protected override void WriteValue(ref WriteContext writer, TList value)
        {
            var messsage = new CollectionMessage(value, _CodeGenerator.CreateFieldCodec(1), WireFormat.MakeTag(1, _CodeGenerator.WireType));
            writer.WriteMessage(messsage);
            //var collection = new RepeatedField<T>();
            //collection.AddRange(value);
            //collection.WriteTo(ref writer, _codeGenerator.CreateFieldCodec(2));
        }

        private class CollectionMessage : IMessage, IBufferMessage
        {
            MessageDescriptor IMessage.Descriptor => null;

            private readonly List<T> _values;
            private readonly FieldCodec<T> _codec;
            private readonly uint _tag;

            public CollectionMessage(TList values, FieldCodec<T> codec, uint tag) : this(codec, tag)
            {
                _values.AddRange(values);
            }

            public CollectionMessage(FieldCodec<T> codec, uint tag)
            {
                _values = new List<T>();
                _codec = codec;
                _tag = tag;
            }

            int IMessage.CalculateSize()
            {
                var size = _values.Sum(t => _codec.CalculateSizeWithTag(t));
                return size;
            }

            public List<T> ToList()
            {
                return _values;
            }

            void IMessage.MergeFrom(CodedInputStream input)
            {
                throw new NotSupportedException();
            }

            void IMessage.WriteTo(CodedOutputStream output)
            {
                throw new NotSupportedException();
            }

            void IBufferMessage.InternalMergeFrom(ref ParseContext ctx)
            {
                uint tag;
                while ((tag = ctx.ReadTag()) != 0)
                {
                    if (tag == _tag)
                        _values.Add(_codec.Read(ref ctx));
                }
            }

            void IBufferMessage.InternalWriteTo(ref WriteContext ctx)
            {
                foreach (var item in _values)
                {
                    _codec.WriteTagAndValue(ref ctx, item);
                }
            }
        }
    }
}
