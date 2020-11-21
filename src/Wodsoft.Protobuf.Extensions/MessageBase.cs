using Google.Protobuf;
using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf
{
    public abstract class MessageBase<T> : IMessage, IBufferMessage
        where T : class, new()
    {
        public MessageBase()
        {
            Source = new T();
        }

        public MessageBase(T source)
        {
            Source = source;
        }

        protected T SourceValue;
        public T Source
        {
            get => SourceValue; set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Source));
                SourceValue = value;
            }
        }

        MessageDescriptor IMessage.Descriptor => null;

        protected abstract int CalculateSize();

        protected abstract void Write(ref WriteContext writer);

        protected abstract void Read(ref ParseContext parser);

        public static implicit operator MessageBase<T>(T source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            var type = MessageBuilder.GetMessageType<T>();
            var message = (MessageBase<T>)Activator.CreateInstance(type, source);
            return message;
        }

        public static implicit operator T(MessageBase<T> message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            return message.SourceValue;
        }

        #region IMessage Implementation

        int IMessage.CalculateSize()
        {
            return CalculateSize();
        }

        void IMessage.MergeFrom(CodedInputStream input)
        {
            input.ReadRawMessage(this);
        }

        void IMessage.WriteTo(CodedOutputStream output)
        {
            output.WriteRawMessage(this);
        }

        #endregion

        #region IBufferMessage Implementation

        void IBufferMessage.InternalMergeFrom(ref ParseContext ctx)
        {
            Read(ref ctx);
        }

        void IBufferMessage.InternalWriteTo(ref WriteContext ctx)
        {
            Write(ref ctx);
        }

        #endregion
    }
}
