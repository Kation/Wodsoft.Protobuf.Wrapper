using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using Wodsoft.Protobuf.Generators;

namespace Wodsoft.Protobuf.Primitives
{
    /// <summary>
    /// Nullable type message wrapper.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NullableMessage<T> : Message<T?>
        where T : struct
    {
        /// <summary>
        /// Initialize string message wrapper.
        /// </summary>
        public NullableMessage() { }

        /// <summary>
        /// Initialize string message wrapper with a value.
        /// </summary>
        /// <param name="value">Value.</param>
        public NullableMessage(T? value) : base(value) { }

        /// <inheritdoc/>
        protected override int CalculateSize()
        {
            if (SourceValue.HasValue)
                return ((IMessage)((Message<T>)SourceValue.Value)).CalculateSize();
            return 0;
        }

        /// <inheritdoc/>
        protected override void Read(ref ParseContext parser)
        {
            var message = Message.New<T>();
            parser.ReadMessage(message);
            SourceValue = message.Source;
        }

        /// <inheritdoc/>
        protected override void Write(ref WriteContext writer)
        {
            if (SourceValue.HasValue)
            {
                var message = (Message<T>)SourceValue.Value;
                writer.WriteMessage(message);
            }
        }
    }
}
