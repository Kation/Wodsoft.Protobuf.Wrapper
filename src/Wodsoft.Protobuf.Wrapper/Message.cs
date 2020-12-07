using Google.Protobuf;
using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf
{
    /// <summary>
    /// Base wrapper class of Protobuf.
    /// </summary>
    public abstract class Message : IMessage, IBufferMessage
    {
        MessageDescriptor IMessage.Descriptor => null;

        /// <summary>
        /// Calculate message size of object.
        /// </summary>
        /// <returns></returns>
        protected abstract int CalculateSize();

        /// <summary>
        /// Write message of object.
        /// </summary>
        /// <param name="writer">Protobuf write context.</param>
        protected abstract void Write(ref WriteContext writer);

        /// <summary>
        /// Read message of object.
        /// </summary>
        /// <param name="parser">Protobuf parse context.</param>
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

        /// <summary>
        /// Serialize object into a stream.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="stream">The stream where bytes write into.</param>
        /// <param name="source">The object that need to be serialized.</param>
        public static void Serialize<T>(Stream stream, T source)
            where T : new()
        {
            Message<T>.Serialize(stream, source);
        }

        /// <summary>
        /// Serialize object into a coded output stream.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="output">The stream where bytes write into.</param>
        /// <param name="source">The object that need to be serialized.</param>
        public static void Serialize<T>(CodedOutputStream output, T source)
            where T : new()
        {
            Message<T>.Serialize(output, source);
        }

        /// <summary>
        /// Deserialize object from a stream.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="stream">The stream where contains bytes of serialized object.</param>
        /// <returns>Return deserialized object.</returns>
        public static T Deserialize<T>(Stream stream)
            where T : new()
        {
            return Message<T>.Deserialize(stream);
        }

        /// <summary>
        /// Deserialize object from a coded input stream.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="input">The stream where contains data of serialized object.</param>
        /// <returns>Return deserialized object.</returns>
        public static T Deserialize<T>(CodedInputStream input)
            where T : new()
        {
            return Message<T>.Deserialize(input);
        }
    }

    /// <summary>
    /// Base generic wrapper class of Protobuf.
    /// </summary>
    /// <typeparam name="T">Type of object that need to wrap.</typeparam>
    public abstract class Message<T> : Message
        where T : new()
    {
        internal static readonly ConstructorBuilder EmptyConstructor;
        internal static readonly ConstructorBuilder ValueConstructor;
        internal static Type MessageType;
        internal static readonly TypeBuilder TypeBuilder;

        static Message()
        {
            var type = typeof(T);
            TypeBuilder = MessageBuilder.ModuleBuilder.DefineType(
                type.Namespace + ".Proxy_" + type.Name,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit,
                typeof(Message<T>));
            ValueConstructor = TypeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig,
                CallingConventions.Standard | CallingConventions.HasThis,
                new Type[] { typeof(T) });
            EmptyConstructor = TypeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig, 
                CallingConventions.Standard | CallingConventions.HasThis, 
                Array.Empty<Type>());
        }

        /// <summary>
        /// Create a message wrapper with a new <typeparamref name="T"/>() as source.
        /// </summary>
        public Message()
        {
            Source = new T();
        }

        /// <summary>
        /// Create a message wrapper with source.
        /// </summary>
        /// <param name="source">Source of object.</param>
        public Message(T source)
        {
            Source = source;
        }

        /// <summary>
        /// Source value of <typeparamref name="T"/>.
        /// </summary>
        protected T SourceValue;
        /// <summary>
        /// Get or set object source for wrapper.
        /// Source can not be null.
        /// </summary>
        public T Source
        {
            get => SourceValue; set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Source));
                SourceValue = value;
            }
        }

        /// <summary>
        /// Serialize object into a stream.
        /// </summary>
        /// <param name="stream">The stream where bytes write into.</param>
        /// <param name="source">The object that need to be serialized.</param>
        public static void Serialize(Stream stream, T source)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            Serialize(new CodedOutputStream(stream, true), source);
        }

        /// <summary>
        /// Serialize object into a coded output stream.
        /// </summary>
        /// <param name="output">The stream where bytes write into.</param>
        /// <param name="source">The object that need to be serialized.</param>
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

        /// <summary>
        /// Deserialize object from a stream.
        /// </summary>
        /// <param name="stream">The stream where contains bytes of serialized object.</param>
        /// <returns>Return deserialized object.</returns>
        public static T Deserialize(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            return Deserialize(new CodedInputStream(stream, true));
        }

        /// <summary>
        /// Deserialize object from a coded input stream.
        /// </summary>
        /// <param name="input">The stream where contains data of serialized object.</param>
        /// <returns>Return deserialized object.</returns>
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
                return null;
            var type = MessageBuilder.GetMessageType<T>();
            var message = (Message<T>)Activator.CreateInstance(type, source);
            return message;
        }

        public static implicit operator T(Message<T> message)
        {
            if (message == null)
                return default(T);
            return message.SourceValue;
        }
    }
}
