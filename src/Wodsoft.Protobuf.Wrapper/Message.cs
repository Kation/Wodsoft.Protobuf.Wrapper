using Google.Protobuf;
using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using Wodsoft.Protobuf.Primitives;

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
        {
            Message<T>.Serialize(output, source);
        }

        /// <summary>
        /// Serialize object to byte array.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="source">The object that need to be serialized.</param>
        /// <returns></returns>
        public static byte[] SerializeToBytes<T>(T source)
        {
            MemoryStream stream = new MemoryStream();
            Serialize(stream, source);
            return stream.ToArray();
        }

        /// <summary>
        /// Deserialize object from a stream.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="stream">The stream where contains bytes of serialized object.</param>
        /// <returns>Return deserialized object.</returns>
        public static T Deserialize<T>(Stream stream)
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
        {
            return Message<T>.Deserialize(input);
        }

        /// <summary>
        /// Deserialize object from a byte array.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="data">Byte array data.</param>
        /// <returns></returns>
        public static T DeserializeFromBytes<T>(byte[] data)
        {
            CodedInputStream stream = new CodedInputStream(data);
            return Deserialize<T>(stream);
        }

        /// <summary>
        /// Create a message wrapper with new value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Message<T> New<T>()
        {
            if (Message<T>.MessageType == null)
                MessageBuilder.GetMessageType<T>();
            return Message<T>.GetMessageWithoutValue();
        }
    }

    /// <summary>
    /// Base generic wrapper class of Protobuf.
    /// </summary>
    /// <typeparam name="T">Type of object that need to wrap.</typeparam>
    public abstract class Message<T> : Message
    {
        internal static readonly ConstructorBuilder EmptyConstructor;
        internal static readonly ConstructorBuilder ValueConstructor;
        internal static Type MessageType;
        internal static readonly TypeBuilder TypeBuilder;

        internal static Func<Message<T>> GetMessageWithoutValue;
        internal static Func<T, Message<T>> GetMessageWithValue;

        private static IMessageFieldProvider _FieldProvider = GeneralMessageFieldProvider.Instance;
        /// <summary>
        /// Get or set field provider.<br/>
        /// Value can not be null.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Value can not be null.</exception>
        public static IMessageFieldProvider FieldProvider
        {
            get => _FieldProvider;
            set
            {
                _FieldProvider = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        static Message()
        {
            var type = typeof(T);
            if (typeof(IMessage).IsAssignableFrom(type))
            {
                MessageType = typeof(MessageMessage<>).MakeGenericType(type);
                var constructor = type.GetConstructor(Array.Empty<Type>());
                if (constructor == null)
                    GetMessageWithoutValue = Expression.Lambda<Func<Message<T>>>(Expression.New(MessageType.GetConstructor(new Type[] { typeof(T) }), Expression.Call(typeof(MessageBuilder).GetMethod(nameof(MessageBuilder.NewObject)).MakeGenericMethod(type)))).Compile();
                else
                    GetMessageWithoutValue = Expression.Lambda<Func<Message<T>>>(Expression.New(MessageType.GetConstructor(new Type[] { typeof(T) }), Expression.New(constructor))).Compile();
                var valueParameter = Expression.Parameter(typeof(T), "value");
                GetMessageWithValue = Expression.Lambda<Func<T, Message<T>>>(Expression.New(MessageType.GetConstructor(new Type[] { typeof(T) }), valueParameter), valueParameter).Compile();
            }
            else
            {
                if (type == typeof(string))
                {
                    MessageType = typeof(StringMessage);
                    GetMessageWithoutValue = Expression.Lambda<Func<Message<T>>>(Expression.New(MessageType.GetConstructor(Array.Empty<Type>()))).Compile();
                    var valueParameter = Expression.Parameter(typeof(T), "value");
                    GetMessageWithValue = Expression.Lambda<Func<T, Message<T>>>(Expression.New(MessageType.GetConstructor(new Type[] { typeof(T) }), valueParameter), valueParameter).Compile();
                }
                else if (type.IsValueType && Nullable.GetUnderlyingType(type) == null && MessageBuilder.GetCodeGenerator<T>() != null)
                {
                    MessageType = typeof(StructureMessage<>).MakeGenericType(type);
                    GetMessageWithoutValue = Expression.Lambda<Func<Message<T>>>(Expression.New(MessageType.GetConstructor(Array.Empty<Type>()))).Compile();
                    var valueParameter = Expression.Parameter(typeof(T), "value");
                    GetMessageWithValue = Expression.Lambda<Func<T, Message<T>>>(Expression.New(MessageType.GetConstructor(new Type[] { typeof(T) }), valueParameter), valueParameter).Compile();
                }
                else if (type.IsEnum)
                {
                    var underlyingType = Enum.GetUnderlyingType(type);
                    MessageType = typeof(EnumMessage<,>).MakeGenericType(type, underlyingType);
                    GetMessageWithoutValue = Expression.Lambda<Func<Message<T>>>(Expression.New(MessageType.GetConstructor(Array.Empty<Type>()))).Compile();
                    var valueParameter = Expression.Parameter(typeof(T), "value");
                    GetMessageWithValue = Expression.Lambda<Func<T, Message<T>>>(Expression.New(MessageType.GetConstructor(new Type[] { typeof(T) }), valueParameter), valueParameter).Compile();
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    MessageType = typeof(CollectionMessage<,>).MakeGenericType(type, type.GetGenericArguments()[0]);
                    GetMessageWithoutValue = Expression.Lambda<Func<Message<T>>>(Expression.New(MessageType.GetConstructor(Array.Empty<Type>()))).Compile();
                    var valueParameter = Expression.Parameter(typeof(T), "value");
                    GetMessageWithValue = Expression.Lambda<Func<T, Message<T>>>(Expression.New(MessageType.GetConstructor(new Type[] { typeof(T) }), valueParameter), valueParameter).Compile();
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var underlyingType = Nullable.GetUnderlyingType(type);
                    MessageType = typeof(NullableMessage<>).MakeGenericType(underlyingType);
                    GetMessageWithoutValue = Expression.Lambda<Func<Message<T>>>(Expression.New(MessageType.GetConstructor(Array.Empty<Type>()))).Compile();
                    var valueParameter = Expression.Parameter(typeof(T), "value");
                    GetMessageWithValue = Expression.Lambda<Func<T, Message<T>>>(Expression.New(MessageType.GetConstructor(new Type[] { typeof(T) }), valueParameter), valueParameter).Compile();
                }
                else if (type.IsArray)
                {
                    MessageType = typeof(ArrayMessage<>).MakeGenericType(type.GetElementType());
                    GetMessageWithoutValue = Expression.Lambda<Func<Message<T>>>(Expression.New(MessageType.GetConstructor(Array.Empty<Type>()))).Compile();
                    var valueParameter = Expression.Parameter(typeof(T), "value");
                    GetMessageWithValue = Expression.Lambda<Func<T, Message<T>>>(Expression.New(MessageType.GetConstructor(new Type[] { typeof(T) }), valueParameter), valueParameter).Compile();
                }
                else
                {
                    TypeBuilder = MessageBuilder.ModuleBuilder.DefineType(
                        type.Namespace + ".Proxy_" + type.Name + "_" + type.GetHashCode(),
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
            }
        }

        /// <summary>
        /// Create a message wrapper with a new <typeparamref name="T"/>() as source.
        /// </summary>
        public Message()
        {
            SourceValue = default;
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
                return;
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
                return;
            if (source is IMessage m)
                output.WriteRawMessage(m);
            else
            {
                Message<T> message = source;
                output.WriteRawMessage(message);
            }
            output.Flush();
        }

        /// <summary>
        /// Serialize object to byte array.
        /// </summary>
        /// <param name="source">The object that need to be serialized.</param>
        /// <returns></returns>
        public static byte[] SerializeToBytes(T source)
        {
            MemoryStream stream = new MemoryStream();
            Serialize(stream, source);
            return stream.ToArray();
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
            if (input.IsAtEnd)
                return default;
            if (typeof(IMessage).IsAssignableFrom(typeof(T)))
            {
                T message = Activator.CreateInstance<T>();
                input.ReadRawMessage((IMessage)message);
                return message;
            }
            else
            {
                if (GetMessageWithoutValue == null)
                {
                    if (MessageType == null)
                        MessageBuilder.GetMessageType(typeof(T));
                    GetMessageWithoutValue = Expression.Lambda<Func<Message<T>>>(Expression.New(MessageType.GetConstructor(Array.Empty<Type>()))).Compile();
                }
                Message<T> message = GetMessageWithoutValue();
                input.ReadRawMessage(message);
                return message.Source;
            }
        }

        /// <summary>
        /// Deserialize object from a byte array.
        /// </summary>
        /// <param name="data">Byte array data.</param>
        /// <returns></returns>
        public static T DeserializeFromBytes(byte[] data)
        {
            CodedInputStream stream = new CodedInputStream(data);
            return Deserialize(stream);
        }

        /// <summary>
        /// Convert a value into Protobuf IMessage wrapper type implicit.<br/>
        /// A null will be return if the source value is null.
        /// </summary>
        /// <param name="source">Source value.</param>
        public static implicit operator Message<T>(T source)
        {
            if (source == null)
                return null;
            if (GetMessageWithValue == null)
            {
                if (MessageType == null)
                    MessageBuilder.GetMessageType(typeof(T));
                var valueParameter = Expression.Parameter(typeof(T), "value");
                GetMessageWithValue = Expression.Lambda<Func<T, Message<T>>>(Expression.New(MessageType.GetConstructor(new Type[] { typeof(T) }), valueParameter), valueParameter).Compile();
            }
            var message = GetMessageWithValue(source);
            return message;
        }

        /// <summary>
        /// Convert a Protobuf IMessage wrapper type back to its source type implicit.<br/>
        /// A default value will be return if the message is null.
        /// </summary>
        /// <param name="message"></param>
        public static implicit operator T(Message<T> message)
        {
            if (message == null)
                return default;
            return message.SourceValue;
        }
    }
}
