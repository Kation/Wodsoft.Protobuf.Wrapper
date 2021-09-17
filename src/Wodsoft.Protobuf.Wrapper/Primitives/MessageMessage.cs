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
        /// Initialize IMessage message wrapper.
        /// </summary>
        public MessageMessage()
        {

        }

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
            if (SourceValue == null)
                return 0;
            return SourceValue.CalculateSize();
        }

        /// <inheritdoc/>
        protected override void Read(ref ParseContext parser)
        {
            parser.ReadMessage(SourceValue);
        }

        /// <inheritdoc/>
        protected override void Write(ref WriteContext writer)
        {
            writer.WriteMessage(SourceValue);
        }
    }
}
