using Google.Protobuf;
using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Wodsoft.Protobuf
{
    public abstract class Message : IMessage, IBufferMessage
    {
        MessageDescriptor IMessage.Descriptor => null;

        protected abstract int CalculateSize();

        protected abstract void Write(ref WriteContext writer);

        protected abstract void Read(ref ParseContext parser);

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

        public static void Serialize<T>(Stream stream, T source)
            where T : class, new()
        {
            Message<T>.Serialize(stream, source);
        }

        public static void Serialize<T>(CodedOutputStream output, T source)
            where T : class, new()
        {
            Message<T>.Serialize(output, source);
        }
        public static T Deserialize<T>(Stream stream)
            where T : class, new()
        {
            return Message<T>.Deserialize(stream);
        }

        public static T Deserialize<T>(CodedInputStream input)
            where T : class, new()
        {
            return Message<T>.Deserialize(input);
        }
    }

    public abstract class Message<T> : Message
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
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            Serialize(new CodedOutputStream(stream, true), source);
        }

        public static void Serialize(CodedOutputStream output, T source)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            Message<T> message = source;
            output.WriteRawMessage(message);
            output.Flush();
        }

        public static T Deserialize(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            return Deserialize(new CodedInputStream(stream, true));
        }

        public static T Deserialize(CodedInputStream input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            Message<T> message = (Message<T>)Activator.CreateInstance(MessageBuilder.GetMessageType<T>());
            input.ReadRawMessage(message);
            return message.Source;
        }


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
    }
}
