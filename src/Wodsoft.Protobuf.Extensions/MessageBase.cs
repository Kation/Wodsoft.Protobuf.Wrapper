using Google.Protobuf;
using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
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

        //public void Serialize(Stream stream)
        //{
        //    using (CodedOutputStream cos = new CodedOutputStream(stream, true))
        //    {
        //        cos.WriteRawMessage(this);
        //    }
        //}

        public static void Serialize(Stream stream, T source)
        {
            Serialize(new CodedOutputStream(stream, true), source);
        }

        public static void Serialize(CodedOutputStream output, T source)
        {
            MessageBase<T> message = source;
            output.WriteRawMessage(message);
            output.Flush();
        }

        public static T Deserialize(Stream stream)
        {
            return Deserialize(new CodedInputStream(stream, true));
        }

        public static T Deserialize(CodedInputStream input)
        {
            MessageBase<T> message = (MessageBase<T>)Activator.CreateInstance(MessageBuilder.GetMessageType<T>());
            input.ReadRawMessage(message);
            return message.Source;
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
