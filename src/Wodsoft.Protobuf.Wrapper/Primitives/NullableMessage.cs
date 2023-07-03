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
        /// Initialize nullable message wrapper.
        /// </summary>
        public NullableMessage() { }

        /// <summary>
        /// Initialize nullable message wrapper with a value.
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
            ((IBufferMessage)message).InternalMergeFrom(ref parser);
            SourceValue = message.Source;
        }

        /// <inheritdoc/>
        protected override void Write(ref WriteContext writer)
        {
            if (SourceValue.HasValue)
            {
                IBufferMessage message = (Message<T>)SourceValue.Value;
                message.InternalWriteTo(ref writer);
            }
        }

        /// <summary>
        /// Compute value size.
        /// </summary>
        /// <param name="value">value.</param>
        /// <returns></returns>
        public static int ComputeSize(T? value)
        {
            if (value.HasValue)
                return ((IMessage)((Message<T>)value.Value)).CalculateSize();
            return 0;
        }
    }
}
