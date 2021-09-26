using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Primitives
{
    /// <summary>
    /// IMessage message wrapper.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageMessage<T> : Message<T>
        where T : IMessage
    {
        /// <summary>
        /// Initialize IMessage message wrapper with a value.
        /// </summary>
        /// <param name="value">Value.</param>
        public MessageMessage(T value) : base(value)
        {

        }

        /// <inheritdoc/>
        protected override int CalculateSize()
        {
            return SourceValue.CalculateSize();
        }

        /// <inheritdoc/>
        protected override void Read(ref ParseContext parser)
        {
            if (SourceValue is IBufferMessage bufferMessage)
                bufferMessage.InternalMergeFrom(ref parser);
            else
                parser.ReadMessage(SourceValue);
        }

        /// <inheritdoc/>
        protected override void Write(ref WriteContext writer)
        {
            if (SourceValue is IBufferMessage bufferMessage)
                bufferMessage.InternalWriteTo(ref writer);
            else
                writer.WriteMessage(SourceValue);
        }
    }
}
