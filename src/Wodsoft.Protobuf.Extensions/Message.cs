using Google.Protobuf;
using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Wodsoft.Protobuf
{
    public abstract class Message<T> : IMessage, IBufferMessage
        where T : class, new()
    {
        internal static Type MessageType;

        public Message()
        {
            Source = new T();
        }

        public Message(T source)
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
            Message<T> message = source;
            output.WriteRawMessage(message);
            output.Flush();
        }

        public static T Deserialize(Stream stream)
        {
            return Deserialize(new CodedInputStream(stream, true));
        }

        public static T Deserialize(CodedInputStream input)
        {
            Message<T> message = (Message<T>)Activator.CreateInstance(MessageBuilder.GetMessageType<T>());
            input.ReadRawMessage(message);
            return message.Source;
        }

        MessageDescriptor IMessage.Descriptor => null;

        protected abstract int CalculateSize();

        protected abstract void Write(ref WriteContext writer);

        protected abstract void Read(ref ParseContext parser);

        public static implicit operator Message<T>(T source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            var type = MessageBuilder.GetMessageType<T>();
            var message = (Message<T>)Activator.CreateInstance(type, source);
            return message;
        }

        public static implicit operator T(Message<T> message)
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
