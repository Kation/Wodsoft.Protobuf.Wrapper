using Google.Protobuf;
using Google.Protobuf.Collections;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using Wodsoft.Protobuf.Generators;

namespace Wodsoft.Protobuf
{
    /// <summary>
    /// Message wrapper type builder.
    /// </summary>
    public class MessageBuilder
    {
        internal static MethodInfo MergeFieldFromMethod = typeof(UnknownFieldSet).GetMethod("MergeFieldFrom", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(UnknownFieldSet), typeof(ParseContext).MakeByRefType() }, null);

        static MessageBuilder()
        {
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Wodsoft.ComBoost.Grpc.Dynamic"), AssemblyBuilderAccess.Run);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule("Services");

            MessageBuilder<bool>.CodeGenerator = new BooleanCodeGenerator();
            MessageBuilder<byte>.CodeGenerator = new ByteCodeGenerator();
            MessageBuilder<sbyte>.CodeGenerator = new SByteCodeGenerator();
            MessageBuilder<short>.CodeGenerator = new Int16CodeGenerator();
            MessageBuilder<int>.CodeGenerator = new Int32CodeGenerator();
            MessageBuilder<long>.CodeGenerator = new Int64CodeGenerator();
            MessageBuilder<ushort>.CodeGenerator = new UInt16CodeGenerator();
            MessageBuilder<uint>.CodeGenerator = new UInt32CodeGenerator();
            MessageBuilder<ulong>.CodeGenerator = new UInt64CodeGenerator();
            MessageBuilder<float>.CodeGenerator = new SingleCodeGenerator();
            MessageBuilder<double>.CodeGenerator = new DoubleCodeGenerator();
            MessageBuilder<DateTime>.CodeGenerator = new DateTimeCodeGenerator();
            MessageBuilder<DateTimeOffset>.CodeGenerator = new DateTimeOffsetCodeGenerator();
            MessageBuilder<TimeSpan>.CodeGenerator = new TimeSpanCodeGenerator();
            MessageBuilder<Guid>.CodeGenerator = new GuidCodeGenerator();
            MessageBuilder<string>.CodeGenerator = new StringCodeGenerator();
            MessageBuilder<byte[]>.CodeGenerator = new ByteArrayCodeGenerator();
            MessageBuilder<ByteString>.CodeGenerator = new ByteStringCodeGenerator();

            MessageBuilder<bool?>.CodeGenerator = new NullableCodeGenerator<bool>(new BooleanCodeGenerator());
            MessageBuilder<byte?>.CodeGenerator = new NullableCodeGenerator<byte>(new ByteCodeGenerator());
            MessageBuilder<sbyte?>.CodeGenerator = new NullableCodeGenerator<sbyte>(new SByteCodeGenerator());
            MessageBuilder<short?>.CodeGenerator = new NullableCodeGenerator<short>(new Int16CodeGenerator());
            MessageBuilder<int?>.CodeGenerator = new NullableCodeGenerator<int>(new Int32CodeGenerator());
            MessageBuilder<long?>.CodeGenerator = new NullableCodeGenerator<long>(new Int64CodeGenerator());
            MessageBuilder<ushort?>.CodeGenerator = new NullableCodeGenerator<ushort>(new UInt16CodeGenerator());
            MessageBuilder<uint?>.CodeGenerator = new NullableCodeGenerator<uint>(new UInt32CodeGenerator());
            MessageBuilder<ulong?>.CodeGenerator = new NullableCodeGenerator<ulong>(new UInt64CodeGenerator());
            MessageBuilder<float?>.CodeGenerator = new NullableCodeGenerator<float>(new SingleCodeGenerator());
            MessageBuilder<double?>.CodeGenerator = new NullableCodeGenerator<double>(new DoubleCodeGenerator());
            MessageBuilder<DateTime?>.CodeGenerator = new NullableCodeGenerator<DateTime>(new DateTimeCodeGenerator());
            MessageBuilder<DateTimeOffset?>.CodeGenerator = new NullableCodeGenerator<DateTimeOffset>(new DateTimeOffsetCodeGenerator());
            MessageBuilder<TimeSpan?>.CodeGenerator = new NullableCodeGenerator<TimeSpan>(new TimeSpanCodeGenerator());
            MessageBuilder<Guid?>.CodeGenerator = new NullableCodeGenerator<Guid>(new GuidCodeGenerator());
        }

        internal static readonly AssemblyBuilder AssemblyBuilder;
        internal static readonly ModuleBuilder ModuleBuilder;

        /// <summary>
        /// Set code generator of type <typeparamref name="T"/>.<br/>
        /// The operation is NOT THREAD SAFTY.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="codeGenerator">Code generator of type <typeparamref name="T"/>.</param>
        public static void SetCodeGenerator<T>(ICodeGenerator<T> codeGenerator)
            where T : new()
        {
            MessageBuilder<T>.CodeGenerator = codeGenerator;
        }

        /// <summary>
        /// Get code generator of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <returns>Return code generator if exists, otherwise null.</returns>
        public static ICodeGenerator<T> GetCodeGenerator<T>()
        {
            return MessageBuilder<T>.CodeGenerator;
        }

        internal static ICodeGenerator GetCodeGenerator(Type type)
        {
            return (ICodeGenerator)typeof(MessageBuilder<>).MakeGenericType(type).GetField("CodeGenerator").GetValue(null);
        }

        internal static bool TryGetCodeGenerator(Type type, out ICodeGenerator codeGenerator)
        {
            codeGenerator = (ICodeGenerator)typeof(MessageBuilder<>).MakeGenericType(type).GetField("CodeGenerator").GetValue(null);
            return codeGenerator != null;
        }

        /// <summary>
        /// Set initializer function of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="initializer">Initializer function that return a value of type <typeparamref name="T"/>.</param>
        public static void SetTypeInitializer<T>(Func<T> initializer)
        {
            MessageBuilder<T>.Initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
        }


        /// <summary>
        /// Get dynamic assembly of wrappers.
        /// </summary>
        /// <returns>Return assembly.</returns>
        public static Assembly GetAssembly()
        {
            return AssemblyBuilder;
        }

        /// <summary>
        /// Get message wrapper type of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <returns>Return message wrapper type.</returns>
        public static Type GetMessageType<T>()
        {
            var type = Message<T>.MessageType;
            if (type == null)
                type = MessageTypeBuilder<T>.Type;
            return type;
        }

        /// <summary>
        /// Get message wrapper type.
        /// </summary>
        /// <param name="type">Type of object.</param>
        /// <returns>Return message wrapper type.</returns>
        public static Type GetMessageType(Type type)
        {
            return (Type)typeof(MessageTypeBuilder<>).MakeGenericType(type).GetField("Type").GetValue(null);
        }

        internal static void GenerateCodecValue(ILGenerator staticILGenerator, Type elementType, int fieldNumber)
        {
            if (typeof(IMessage).IsAssignableFrom(elementType))
            {
                staticILGenerator.Emit(OpCodes.Ldc_I4, (int)WireFormat.MakeTag(fieldNumber, WireFormat.WireType.LengthDelimited));
                staticILGenerator.Emit(OpCodes.Ldtoken, typeof(Func<>).MakeGenericType(elementType));
                staticILGenerator.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[1] { typeof(RuntimeTypeHandle) }));
                var cons = elementType.GetConstructor(Array.Empty<Type>());
                if (cons == null)
                    staticILGenerator.Emit(OpCodes.Ldtoken, typeof(MessageBuilder).GetMethod(nameof(MessageBuilder.NewObject), BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(elementType));
                else
                    staticILGenerator.Emit(OpCodes.Ldtoken, typeof(MessageBuilder).GetMethod("MessageFactory", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(elementType));
                staticILGenerator.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetMethodFromHandle", new Type[1] { typeof(RuntimeMethodHandle) }));
                staticILGenerator.Emit(OpCodes.Castclass, typeof(MethodInfo));
                staticILGenerator.Emit(OpCodes.Call, typeof(Delegate).GetMethod(nameof(Delegate.CreateDelegate), BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(Type), typeof(MethodInfo) }, null));
                staticILGenerator.Emit(OpCodes.Newobj, typeof(MessageParser<>).MakeGenericType(elementType).GetConstructor(new Type[] { typeof(Func<>).MakeGenericType(elementType) }));
                staticILGenerator.Emit(OpCodes.Call, typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForMessage)).MakeGenericMethod(elementType));
            }
            else
            {
                var codeGeneratorType = typeof(ObjectCodeGenerator<>).MakeGenericType(elementType);
                staticILGenerator.Emit(OpCodes.Newobj, codeGeneratorType.GetConstructor(Array.Empty<Type>()));
                staticILGenerator.Emit(OpCodes.Ldc_I4, fieldNumber);
                staticILGenerator.Emit(OpCodes.Call, codeGeneratorType.GetMethod("CreateFieldCodec"));
            }
        }

        private static T MessageFactory<T>()
            where T : IMessage, new()
        {
            return new T();
        }

        internal static void GenerateCheckNull(ILGenerator ilGenerator, LocalBuilder valueVariable, Label end)
        {
            //IL:if (value == null) goto end;
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Brfalse, end);
        }

        internal static void GenerateAddCollection(ILGenerator ilGenerator, LocalBuilder valueVariable, FieldInfo field)
        {
            var skip = ilGenerator.DefineLabel();
            //IL: if (collection != value)
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, field);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Beq, skip);

            //IL: collection.Clear();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, field);
            ilGenerator.Emit(OpCodes.Call, field.FieldType.GetMethod("Clear"));

            //IL: collection.AddRange(value);
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, field);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, field.FieldType.GetMethod("AddRange"));

            ilGenerator.MarkLabel(skip);
        }

        internal static void GenerateAddDictionary(ILGenerator ilGenerator, LocalBuilder valueVariable, FieldInfo field)
        {
            var skip = ilGenerator.DefineLabel();
            //IL: if (dictionary != value)
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, field);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Beq, skip);

            //IL: dictionary.Clear();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, field);
            ilGenerator.Emit(OpCodes.Call, field.FieldType.GetMethod("Clear"));
            //IL: dictionary.Add(value);
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, field);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, field.FieldType.GetMethods().Where(t => t.Name == "Add").OrderBy(t => t.GetParameters().Length).First());

            ilGenerator.MarkLabel(skip);
        }

        /// <summary>
        /// New a object with type initializer.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <returns>Return initialized object.</returns>
        public static T NewObject<T>()
        {
            var initializer = MessageBuilder<T>.Initializer;
            if (initializer == null)
                throw new NotSupportedException("Type initializer not found. Need to set type initializer first.");
            return initializer();
        }
    }

    internal class MessageBuilder<T>
    {
        public static ICodeGenerator<T> CodeGenerator;
        public static Func<T> Initializer;
    }

    internal class MessageTypeBuilder<T>
    {
        public static Type Type;

        static MessageTypeBuilder()
        {
            var type = typeof(T);
            var typeBuilder = Message<T>.TypeBuilder;
            var fieldProvider = Message<T>.FieldProvider;
            var properties = fieldProvider.GetFields(type);
            BuildMethod(typeBuilder, properties, out var initFields, out var refTypes);
            var constructor = BuildConstructor(initFields);
            BuildEmptyConstructor(constructor);
            var messageType = typeBuilder.CreateTypeInfo();
            ObjectCodeGenerator<T>.ComputeSize = messageType.GetMethod("ComputeSize");
            ObjectCodeGenerator<T>.EmptyConstructor = messageType.GetConstructor(Array.Empty<Type>());
            ObjectCodeGenerator<T>.WrapConstructor = messageType.GetConstructor(new Type[] { type });
            var finalType = messageType.AsType();
            {
                Message<T>.GetMessageWithoutValue = Expression.Lambda<Func<Message<T>>>(Expression.New(finalType.GetConstructor(Array.Empty<Type>()))).Compile();
                var valueParameter = Expression.Parameter(type, "value");
                Message<T>.GetMessageWithValue = Expression.Lambda<Func<T, Message<T>>>(Expression.New(finalType.GetConstructor(new Type[] { type }), valueParameter), valueParameter).Compile();
            }
            Message<T>.MessageType = messageType;
            Type = finalType;
            foreach (var item in refTypes)
                typeof(MessageBuilder<>).MakeGenericType(item).GetField("Type", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        }

        private static void BuildEmptyConstructor(ConstructorBuilder constructor)
        {
            var wrapType = typeof(T);
            var constructorBuilder = Message<T>.EmptyConstructor;
            var ilGenerator = constructorBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            if (wrapType.IsValueType)
            {
                var valueVariable = ilGenerator.DeclareLocal(wrapType);
                ilGenerator.Emit(OpCodes.Ldloca_S, valueVariable);
                ilGenerator.Emit(OpCodes.Initobj, wrapType);
                ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            }
            else
            {
                var cons = wrapType.GetConstructor(Array.Empty<Type>());
                if (cons == null)
                    ilGenerator.Emit(OpCodes.Call, typeof(MessageBuilder).GetMethod(nameof(MessageBuilder.NewObject), BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(wrapType));
                else
                    ilGenerator.Emit(OpCodes.Newobj, cons);
            }
            ilGenerator.Emit(OpCodes.Call, constructor);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private static ConstructorBuilder BuildConstructor(FieldBuilder[] initFields)
        {
            var constructorBuilder = Message<T>.ValueConstructor;
            var ilGenerator = constructorBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, typeof(Message<T>).GetConstructor(new Type[] { typeof(T) }));

            foreach (var field in initFields)
            {
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Newobj, field.FieldType.GetConstructor(Array.Empty<Type>()));
                ilGenerator.Emit(OpCodes.Stfld, field);
            }
            ilGenerator.Emit(OpCodes.Ret);
            return constructorBuilder;
        }

        private static void BuildMethod(TypeBuilder typeBuilder, IEnumerable<IMessageField> fields, out FieldBuilder[] initFields, out Type[] referenceTypes)
        {
            var baseType = typeof(Message<T>);
            var objectType = typeof(T);
            MethodBuilder computeSizeMethodBuilder;
            if (objectType.IsValueType)
                computeSizeMethodBuilder = typeBuilder.DefineMethod("ComputeSize", MethodAttributes.Static | MethodAttributes.Public, typeof(int), new Type[] { objectType.MakeByRefType() });
            else
                computeSizeMethodBuilder = typeBuilder.DefineMethod("ComputeSize", MethodAttributes.Static | MethodAttributes.Public, typeof(int), new Type[] { objectType });
            typeof(ObjectCodeGenerator<>).MakeGenericType(objectType).GetField(nameof(ObjectCodeGenerator<object>.ComputeSize), BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, computeSizeMethodBuilder);
            var computeSizeILGenerator = computeSizeMethodBuilder.GetILGenerator();
            var sizeVariable = computeSizeILGenerator.DeclareLocal(typeof(int));

            var writeMethodBuilder = typeBuilder.DefineMethod("Write", MethodAttributes.Family | MethodAttributes.Virtual, null, new Type[] { typeof(WriteContext).MakeByRefType() });
            var writeILGenerator = writeMethodBuilder.GetILGenerator();
            typeBuilder.DefineMethodOverride(writeMethodBuilder, baseType.GetMethod("Write", BindingFlags.NonPublic | BindingFlags.Instance));

            var readMethodBuilder = typeBuilder.DefineMethod("Read", MethodAttributes.Family | MethodAttributes.Virtual, null, new Type[] { typeof(ParseContext).MakeByRefType() });
            var readILGenerator = readMethodBuilder.GetILGenerator();
            typeBuilder.DefineMethodOverride(readMethodBuilder, baseType.GetMethod("Read", BindingFlags.NonPublic | BindingFlags.Instance));
            var readTagVariable = readILGenerator.DeclareLocal(typeof(uint));
            var readWhileStart = readILGenerator.DefineLabel();
            var readEnd = readILGenerator.DefineLabel();
            var readTagLabels = fields.ToDictionary(t => t, t => readILGenerator.DefineLabel());

            var staticConstructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
        MethodAttributes.RTSpecialName | MethodAttributes.Static, CallingConventions.Standard, null);
            var staticIlGenerator = staticConstructorBuilder.GetILGenerator();

            var sourceFieldInfo = baseType.GetField("SourceValue", BindingFlags.NonPublic | BindingFlags.Instance);

            List<Type> referenceWrapTypes = new List<Type>();

            List<FieldBuilder> speciallyFields = new List<FieldBuilder>();

            //ComputeSize
            {
                computeSizeILGenerator.Emit(OpCodes.Ldc_I4_0);
                computeSizeILGenerator.Emit(OpCodes.Stloc, sizeVariable);
            }
            //Read
            {
                readILGenerator.MarkLabel(readWhileStart);
                readILGenerator.Emit(OpCodes.Ldarg_1);
                readILGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod("ReadTag"));
                readILGenerator.Emit(OpCodes.Stloc, readTagVariable);

                readILGenerator.Emit(OpCodes.Ldloc, readTagVariable);
                readILGenerator.Emit(OpCodes.Ldc_I4_0);
                readILGenerator.Emit(OpCodes.Beq, readEnd);

                foreach (var field in fields)
                {
                    uint tag;
                    if (MessageBuilder.TryGetCodeGenerator(field.FieldType, out var codeGenerator))
                    {
                        tag = WireFormat.MakeTag(field.FieldNumber, codeGenerator.WireType);
                    }
                    else
                    {
                        var type = Nullable.GetUnderlyingType(field.FieldType) ?? field.FieldType;
                        if (type.IsEnum)
                        {
                            tag = WireFormat.MakeTag(field.FieldNumber, MessageBuilder.GetCodeGenerator(Enum.GetUnderlyingType(type)).WireType);
                        }
                        else
                            tag = WireFormat.MakeTag(field.FieldNumber, WireFormat.WireType.LengthDelimited);
                    }

                    readILGenerator.Emit(OpCodes.Ldloc, readTagVariable);
                    readILGenerator.Emit(OpCodes.Ldc_I4, (int)tag);
                    readILGenerator.Emit(OpCodes.Beq, readTagLabels[field]);
                    if (field.FieldType.IsGenericType)
                    {
                        var type = field.FieldType.GetGenericTypeDefinition();
                        if (type == typeof(RepeatedField<>) || type == typeof(IList<>) || type == typeof(ICollection<>) || type == typeof(List<>) || type == typeof(Collection<>) || type == typeof(IEnumerable<>))
                        {
                            type = field.FieldType.GetGenericArguments()[0];
                            if (MessageBuilder.TryGetCodeGenerator(type, out codeGenerator) && codeGenerator.WireType == WireFormat.WireType.Varint)
                            {

                                readILGenerator.Emit(OpCodes.Ldloc, readTagVariable);
                                readILGenerator.Emit(OpCodes.Ldc_I4, (int)WireFormat.MakeTag(field.FieldNumber, WireFormat.WireType.Varint));
                                readILGenerator.Emit(OpCodes.Beq, readTagLabels[field]);
                            }
                        }
                    }
                    else if (field.FieldType.IsArray)
                    {
                        if (MessageBuilder.TryGetCodeGenerator(field.FieldType.GetElementType(), out codeGenerator) && codeGenerator.WireType == WireFormat.WireType.Varint)
                        {

                            readILGenerator.Emit(OpCodes.Ldloc, readTagVariable);
                            readILGenerator.Emit(OpCodes.Ldc_I4, (int)WireFormat.MakeTag(field.FieldNumber, WireFormat.WireType.Varint));
                            readILGenerator.Emit(OpCodes.Beq, readTagLabels[field]);
                        }
                    }
                }
                readILGenerator.Emit(OpCodes.Ldnull);
                readILGenerator.Emit(OpCodes.Ldarg_1);
                readILGenerator.Emit(OpCodes.Call, MessageBuilder.MergeFieldFromMethod);
                readILGenerator.Emit(OpCodes.Pop);
                readILGenerator.Emit(OpCodes.Br, readWhileStart);
            }

            List<Tuple<FieldInfo, IMessageField, Type>> collectionProperties = new List<Tuple<FieldInfo, IMessageField, Type>>();
            List<Tuple<FieldInfo, IMessageField, Type, Type>> dictionaryProperties = new List<Tuple<FieldInfo, IMessageField, Type, Type>>();

            foreach (var field in fields)
            {
                var computeSizeValueVariable = computeSizeILGenerator.DeclareLocal(field.FieldType);
                var computeSizeEnd = computeSizeILGenerator.DefineLabel();
                var writeValueVariable = writeILGenerator.DeclareLocal(field.FieldType);
                var writeEnd = writeILGenerator.DefineLabel();

                readILGenerator.MarkLabel(readTagLabels[field]);

                MessageBuilder.TryGetCodeGenerator(field.FieldType, out var codeGenerator);

                //GenerateReadProperty(computeSizeILGenerator, computeSizeValueVariable, sourceFieldInfo, property);
                //ComputeSize
                {
                    computeSizeILGenerator.Emit(OpCodes.Ldarg_0);
                    field.GenerateReadFieldCode(computeSizeILGenerator);
                    computeSizeILGenerator.Emit(OpCodes.Stloc, computeSizeValueVariable);
                }
                //Write
                {
                    writeILGenerator.Emit(OpCodes.Ldarg_0);
                    if (sourceFieldInfo.FieldType.IsValueType)
                        writeILGenerator.Emit(OpCodes.Ldflda, sourceFieldInfo);
                    else
                        writeILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);
                    field.GenerateReadFieldCode(writeILGenerator);
                    writeILGenerator.Emit(OpCodes.Stloc, writeValueVariable);
                }
                //GenerateReadProperty(writeILGenerator, writeValueVariable, sourceFieldInfo, property);

                if (codeGenerator == null)
                {
                    if (field.FieldType.IsValueType && (Nullable.GetUnderlyingType(field.FieldType) ?? field.FieldType).IsEnum)
                    {
                        var underlyingType = Nullable.GetUnderlyingType(field.FieldType);
                        if (underlyingType != null)
                        {
                            //ComputeSize
                            {
                                //IL: if (value.HasValue)
                                computeSizeILGenerator.Emit(OpCodes.Ldloca, computeSizeValueVariable);
                                computeSizeILGenerator.Emit(OpCodes.Call, field.FieldType.GetProperty("HasValue").GetMethod);
                                computeSizeILGenerator.Emit(OpCodes.Brfalse, computeSizeEnd);
                            }
                            //Write
                            {
                                //IL: if (value.HasValue)
                                writeILGenerator.Emit(OpCodes.Ldloca, writeValueVariable);
                                writeILGenerator.Emit(OpCodes.Call, field.FieldType.GetProperty("HasValue").GetMethod);
                                writeILGenerator.Emit(OpCodes.Brfalse, writeEnd);
                            }
                        }

                        var type = underlyingType ?? field.FieldType;

                        var valueType = Enum.GetUnderlyingType(type);
                        codeGenerator = MessageBuilder.GetCodeGenerator(valueType);

                        if (underlyingType != null)
                        {
                            //ComputeSize
                            {
                                var underlyingVariable = computeSizeILGenerator.DeclareLocal(underlyingType);
                                computeSizeILGenerator.Emit(OpCodes.Ldloca, computeSizeValueVariable);
                                computeSizeILGenerator.Emit(OpCodes.Call, field.FieldType.GetProperty("Value").GetMethod);
                                computeSizeILGenerator.Emit(OpCodes.Stloc, underlyingVariable);
                                codeGenerator.GenerateCalculateSizeCode(computeSizeILGenerator, underlyingVariable, field.FieldNumber);
                            }
                            //Write
                            {
                                var underlyingVariable = writeILGenerator.DeclareLocal(underlyingType);
                                writeILGenerator.Emit(OpCodes.Ldloca, writeValueVariable);
                                writeILGenerator.Emit(OpCodes.Call, field.FieldType.GetProperty("Value").GetMethod);
                                writeILGenerator.Emit(OpCodes.Stloc, underlyingVariable);
                                codeGenerator.GenerateWriteCode(writeILGenerator, underlyingVariable, field.FieldNumber);
                            }
                        }
                        else
                        {
                            //ComputeSize
                            codeGenerator.GenerateCalculateSizeCode(computeSizeILGenerator, computeSizeValueVariable, field.FieldNumber);
                            //Write
                            codeGenerator.GenerateWriteCode(writeILGenerator, writeValueVariable, field.FieldNumber);
                        }

                        //Read
                        {
                            //IL: this.Source.{Property} = value;
                            readILGenerator.Emit(OpCodes.Ldarg_0);
                            readILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);

                            //IL: Enum.ToObject(typeof({type}), parser.ReadXXX());
                            readILGenerator.Emit(OpCodes.Ldtoken, type);
                            readILGenerator.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[1] { typeof(RuntimeTypeHandle) }));
                            codeGenerator.GenerateReadCode(readILGenerator);
                            if (Enum.GetUnderlyingType(type) == typeof(byte))
                            {
                                readILGenerator.Emit(OpCodes.Conv_U1);
                            }
                            else if (Enum.GetUnderlyingType(type) == typeof(sbyte))
                            {
                                readILGenerator.Emit(OpCodes.Conv_I1);
                            }
                            else if (Enum.GetUnderlyingType(type) == typeof(short))
                            {
                                readILGenerator.Emit(OpCodes.Conv_I2);
                            }
                            else if (Enum.GetUnderlyingType(type) == typeof(ushort))
                            {
                                readILGenerator.Emit(OpCodes.Conv_U2);
                            }
                            readILGenerator.Emit(OpCodes.Call, typeof(Enum).GetMethod("ToObject", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(Type), Enum.GetUnderlyingType(type) }, null));
                            readILGenerator.Emit(OpCodes.Unbox_Any, type);

                            if (underlyingType != null)
                                readILGenerator.Emit(OpCodes.Newobj, field.FieldType.GetConstructor(new Type[] { underlyingType }));
                            field.GenerateWriteFieldCode(readILGenerator);
                        }

                        //ComputeSize
                        {
                            //IL: size += CodedOutputStream.Compute{type}Size(value);
                            computeSizeILGenerator.Emit(OpCodes.Ldloc, sizeVariable);
                            computeSizeILGenerator.Emit(OpCodes.Add_Ovf);
                            computeSizeILGenerator.Emit(OpCodes.Stloc, sizeVariable);
                        }
                    }
                    else
                    {
                        if (!field.FieldType.IsValueType)
                        {
                            MessageBuilder.GenerateCheckNull(computeSizeILGenerator, computeSizeValueVariable, computeSizeEnd);
                            MessageBuilder.GenerateCheckNull(writeILGenerator, writeValueVariable, writeEnd);
                        }

                        bool isCollection = false;
                        bool isDictionary = false;
                        Type elementType = null;
                        Type elementType2 = null;
                        if (field.FieldType.IsArray)
                        {
                            isCollection = true;
                            elementType = field.FieldType.GetElementType();
                        }
                        else if (field.FieldType.IsGenericType)
                        {
                            var genericType = field.FieldType.GetGenericTypeDefinition();
                            if (genericType == typeof(IList<>) || genericType == typeof(List<>) || genericType == typeof(ICollection<>) || genericType == typeof(Collection<>) || genericType == typeof(RepeatedField<>) || genericType == typeof(IEnumerable<>))
                            {
                                elementType = field.FieldType.GetGenericArguments()[0];
                                isCollection = true;
                            }
                            else if (genericType == typeof(Dictionary<,>) || genericType == typeof(IDictionary<,>) || genericType == typeof(MapField<,>))
                            {
                                var types = field.FieldType.GetGenericArguments();
                                elementType = types[0];
                                elementType2 = types[1];
                                isDictionary = true;
                            }
                        }

                        //Get the tag value of this property
                        var tag = WireFormat.MakeTag(field.FieldNumber, WireFormat.WireType.LengthDelimited);

                        if (isCollection)
                        {
                            var collectionType = typeof(RepeatedField<>).MakeGenericType(elementType);

                            MessageBuilder.TryGetCodeGenerator(elementType, out codeGenerator);

                            var codecField = typeBuilder.DefineField("_Codec_" + field.FieldName, typeof(FieldCodec<>).MakeGenericType(elementType), FieldAttributes.Private | FieldAttributes.Static);
                            //static constructor
                            {
                                //IL: _Codec_{PropertyName} = FieldCodec.For{XXX}(tag);
                                if (codeGenerator == null)
                                    MessageBuilder.GenerateCodecValue(staticIlGenerator, elementType, field.FieldNumber);
                                else
                                {
                                    var generatorType = typeof(ICodeGenerator<>).MakeGenericType(elementType);
                                    staticIlGenerator.Emit(OpCodes.Call, typeof(MessageBuilder).GetMethod("GetCodeGenerator").MakeGenericMethod(elementType));
                                    staticIlGenerator.Emit(OpCodes.Castclass, generatorType);
                                    staticIlGenerator.Emit(OpCodes.Ldc_I4, field.FieldNumber);
                                    staticIlGenerator.Emit(OpCodes.Callvirt, generatorType.GetMethod("CreateFieldCodec", new Type[] { typeof(int) }));
                                }
                                staticIlGenerator.Emit(OpCodes.Stsfld, codecField);
                            }

                            if (field.FieldType == collectionType)
                            {
                                //ComputeSize
                                {
                                    //IL: value.CalculateSize(_Codec_{PropertyName});
                                    computeSizeILGenerator.Emit(OpCodes.Ldloc, computeSizeValueVariable);
                                }
                                //Write
                                {
                                    //IL: value.WriteTo(ref writer, _Codec_{PropertyName});
                                    writeILGenerator.Emit(OpCodes.Ldloc, writeValueVariable);
                                }
                                //Read
                                {
                                    //this.Source.{property}
                                    readILGenerator.Emit(OpCodes.Ldarg_0);
                                    readILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);
                                    field.GenerateReadFieldCode(readILGenerator);
                                }
                            }
                            else
                            {
                                var collectionField = typeBuilder.DefineField("_" + field.FieldName, collectionType, FieldAttributes.Private | FieldAttributes.InitOnly);
                                speciallyFields.Add(collectionField);

                                //ComputeSize
                                {
                                    var collectionVariable = computeSizeILGenerator.DeclareLocal(collectionType);
                                    computeSizeILGenerator.Emit(OpCodes.Newobj, collectionType.GetConstructor(Array.Empty<Type>()));
                                    computeSizeILGenerator.Emit(OpCodes.Stloc, collectionVariable);
                                    //IL: collection.AddRange(value);
                                    computeSizeILGenerator.Emit(OpCodes.Ldloc, collectionVariable);
                                    computeSizeILGenerator.Emit(OpCodes.Ldloc, computeSizeValueVariable);
                                    computeSizeILGenerator.Emit(OpCodes.Call, collectionField.FieldType.GetMethod("AddRange"));

                                    computeSizeILGenerator.Emit(OpCodes.Ldloc, collectionVariable);
                                }
                                //Write
                                {
                                    MessageBuilder.GenerateAddCollection(writeILGenerator, writeValueVariable, collectionField);
                                    writeILGenerator.Emit(OpCodes.Ldarg_0);
                                    writeILGenerator.Emit(OpCodes.Ldfld, collectionField);
                                }
                                //Read
                                {
                                    //this._{property}
                                    readILGenerator.Emit(OpCodes.Ldarg_0);
                                    readILGenerator.Emit(OpCodes.Ldfld, collectionField);
                                }
                                //add this to move collections to this.Source.{Property}
                                collectionProperties.Add(new Tuple<FieldInfo, IMessageField, Type>(collectionField, field, elementType));
                            }
                            //ComputeSize
                            {
                                //IL: size += collection.CalculateSize(_Codec_{PropertyName});
                                computeSizeILGenerator.Emit(OpCodes.Ldsfld, codecField);
                                computeSizeILGenerator.Emit(OpCodes.Call, collectionType.GetMethod("CalculateSize"));
                                computeSizeILGenerator.Emit(OpCodes.Ldloc, sizeVariable);
                                computeSizeILGenerator.Emit(OpCodes.Add_Ovf);
                                computeSizeILGenerator.Emit(OpCodes.Stloc, sizeVariable);
                            }
                            //Write
                            {
                                //IL: WriteTo(ref writer, _Codec_{PropertyName});
                                writeILGenerator.Emit(OpCodes.Ldarg_1);
                                writeILGenerator.Emit(OpCodes.Ldsfld, codecField);
                                writeILGenerator.Emit(OpCodes.Call, collectionType.GetMethod("WriteTo", new Type[] { typeof(WriteContext).MakeByRefType(), codecField.FieldType }));
                            }
                            //Read
                            {
                                //IL: .AddEntriesFrom(ref parser, _Codec_{PropertyName});
                                readILGenerator.Emit(OpCodes.Ldarg_1);
                                readILGenerator.Emit(OpCodes.Ldsfld, codecField);
                                readILGenerator.Emit(OpCodes.Call, collectionType.GetMethod("AddEntriesFrom", new Type[] { typeof(ParseContext).MakeByRefType(), codecField.FieldType }));
                            }
                        }
                        else if (isDictionary)
                        {
                            var dictionaryType = typeof(MapField<,>).MakeGenericType(elementType, elementType2);
                            Type valueType = elementType2;
                            bool isArray = false;
                            if (elementType2.IsArray && elementType2 != typeof(byte[]))
                            {
                                valueType = elementType2.GetElementType();
                                isArray = true;
                            }
                            else if (elementType2.IsGenericType)
                            {
                                var genericType = elementType2.GetGenericTypeDefinition();
                                if (genericType == typeof(IList<>) || genericType == typeof(List<>) || genericType == typeof(ICollection<>) || genericType == typeof(IEnumerable<>))
                                {
                                    valueType = elementType2.GetGenericArguments()[0];
                                    isCollection = true;
                                }
                            }
                            var codecField = typeBuilder.DefineField("_Codec_" + field.FieldName, typeof(MapField<,>.Codec).MakeGenericType(elementType, elementType2), FieldAttributes.Private | FieldAttributes.Static);
                            //static constructor
                            {
                                MessageBuilder.TryGetCodeGenerator(elementType, out codeGenerator);
                                if (codeGenerator == null)
                                {
                                    MessageBuilder.GenerateCodecValue(staticIlGenerator, elementType, 1);
                                }
                                else
                                {
                                    var generatorType = typeof(ICodeGenerator<>).MakeGenericType(elementType);
                                    staticIlGenerator.Emit(OpCodes.Call, typeof(MessageBuilder).GetMethod("GetCodeGenerator").MakeGenericMethod(elementType));
                                    staticIlGenerator.Emit(OpCodes.Castclass, generatorType);
                                    staticIlGenerator.Emit(OpCodes.Ldc_I4, 1);
                                    staticIlGenerator.Emit(OpCodes.Callvirt, generatorType.GetMethod("CreateFieldCodec", new Type[] { typeof(int) }));
                                }
                                if (isArray)
                                {
                                    var generatorType = typeof(ICodeGenerator<>).MakeGenericType(elementType2);
                                    staticIlGenerator.Emit(OpCodes.Newobj, typeof(ArrayCodeGenerator<>).MakeGenericType(valueType).GetConstructor(Array.Empty<Type>()));
                                    staticIlGenerator.Emit(OpCodes.Ldc_I4, 2);
                                    staticIlGenerator.Emit(OpCodes.Callvirt, generatorType.GetMethod("CreateFieldCodec", new Type[] { typeof(int) }));
                                }
                                else if (isCollection)
                                {
                                    var generatorType = typeof(ICodeGenerator<>).MakeGenericType(elementType2);
                                    staticIlGenerator.Emit(OpCodes.Newobj, typeof(CollectionCodeGenerator<,>).MakeGenericType(valueType, elementType2).GetConstructor(Array.Empty<Type>()));
                                    staticIlGenerator.Emit(OpCodes.Ldc_I4, 2);
                                    staticIlGenerator.Emit(OpCodes.Callvirt, generatorType.GetMethod("CreateFieldCodec", new Type[] { typeof(int) }));
                                }
                                else
                                {
                                    MessageBuilder.TryGetCodeGenerator(elementType2, out codeGenerator);
                                    if (codeGenerator == null)
                                    {
                                        MessageBuilder.GenerateCodecValue(staticIlGenerator, elementType2, 2);
                                    }
                                    else
                                    {
                                        var generatorType = typeof(ICodeGenerator<>).MakeGenericType(elementType2);
                                        staticIlGenerator.Emit(OpCodes.Call, typeof(MessageBuilder).GetMethod("GetCodeGenerator").MakeGenericMethod(elementType2));
                                        staticIlGenerator.Emit(OpCodes.Castclass, generatorType);
                                        staticIlGenerator.Emit(OpCodes.Ldc_I4, 2);
                                        staticIlGenerator.Emit(OpCodes.Callvirt, generatorType.GetMethod("CreateFieldCodec", new Type[] { typeof(int) }));
                                    }
                                }

                                //IL: FieldCodec.For{XXX}(tag);
                                staticIlGenerator.Emit(OpCodes.Ldc_I4, (int)tag);
                                staticIlGenerator.Emit(OpCodes.Newobj, codecField.FieldType.GetConstructors()[0]);
                                staticIlGenerator.Emit(OpCodes.Stsfld, codecField);
                            }

                            if (field.FieldType == dictionaryType)
                            {
                                //ComputeSize
                                {
                                    //IL: value.CalculateSize(_Codec_{PropertyName});
                                    computeSizeILGenerator.Emit(OpCodes.Ldloc, computeSizeValueVariable);
                                }
                                //Write
                                {
                                    //IL: value.WriteTo(ref writer, _Codec_{PropertyName});
                                    writeILGenerator.Emit(OpCodes.Ldloc, writeValueVariable);
                                }
                                //Read
                                {
                                    //this.Source.{property}
                                    readILGenerator.Emit(OpCodes.Ldarg_0);
                                    readILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);
                                    field.GenerateReadFieldCode(readILGenerator);
                                }
                            }
                            else
                            {
                                var dictionaryField = typeBuilder.DefineField("_" + field.FieldName, dictionaryType, FieldAttributes.Private | FieldAttributes.InitOnly);
                                speciallyFields.Add(dictionaryField);

                                //ComputeSize
                                {

                                    var dictionaryVariable = computeSizeILGenerator.DeclareLocal(dictionaryType);
                                    computeSizeILGenerator.Emit(OpCodes.Newobj, dictionaryType.GetConstructor(Array.Empty<Type>()));
                                    computeSizeILGenerator.Emit(OpCodes.Stloc, dictionaryVariable);
                                    //IL: collection.AddRange(value);
                                    computeSizeILGenerator.Emit(OpCodes.Ldloc, dictionaryVariable);
                                    computeSizeILGenerator.Emit(OpCodes.Ldloc, computeSizeValueVariable);
                                    computeSizeILGenerator.Emit(OpCodes.Call, dictionaryField.FieldType.GetMethod("Add", new Type[] { typeof(IDictionary<,>).MakeGenericType(elementType, elementType2) }));

                                    computeSizeILGenerator.Emit(OpCodes.Ldloc, dictionaryVariable);
                                }
                                //Write
                                {
                                    MessageBuilder.GenerateAddDictionary(writeILGenerator, writeValueVariable, dictionaryField);
                                    writeILGenerator.Emit(OpCodes.Ldarg_0);
                                    writeILGenerator.Emit(OpCodes.Ldfld, dictionaryField);
                                }
                                //Read
                                {
                                    //this._{property}
                                    readILGenerator.Emit(OpCodes.Ldarg_0);
                                    readILGenerator.Emit(OpCodes.Ldfld, dictionaryField);
                                }
                                //add this to move dictionary to this.Source.{Property}
                                dictionaryProperties.Add(new Tuple<FieldInfo, IMessageField, Type, Type>(dictionaryField, field, elementType, elementType2));
                            }

                            //ComputeSize
                            {
                                computeSizeILGenerator.Emit(OpCodes.Ldsfld, codecField);
                                computeSizeILGenerator.Emit(OpCodes.Call, dictionaryType.GetMethod("CalculateSize"));
                                computeSizeILGenerator.Emit(OpCodes.Ldloc, sizeVariable);
                                computeSizeILGenerator.Emit(OpCodes.Add_Ovf);
                                computeSizeILGenerator.Emit(OpCodes.Stloc, sizeVariable);
                            }
                            //Write
                            {
                                //IL: WriteTo(ref writer, _Codec_{PropertyName});
                                writeILGenerator.Emit(OpCodes.Ldarg_1);
                                writeILGenerator.Emit(OpCodes.Ldsfld, codecField);
                                writeILGenerator.Emit(OpCodes.Call, dictionaryType.GetMethod("WriteTo", new Type[] { typeof(WriteContext).MakeByRefType(), codecField.FieldType }));
                            }
                            //Read
                            {
                                //IL: .AddEntriesFrom(ref parser, _Codec_{PropertyName});
                                readILGenerator.Emit(OpCodes.Ldarg_1);
                                readILGenerator.Emit(OpCodes.Ldsfld, codecField);
                                readILGenerator.Emit(OpCodes.Call, dictionaryType.GetMethod("AddEntriesFrom", new Type[] { typeof(ParseContext).MakeByRefType(), codecField.FieldType }));
                            }
                        }
                        else
                        {
                            if (typeof(IMessage).IsAssignableFrom(field.FieldType))
                            {
                                //ComputeSize
                                {
                                    computeSizeILGenerator.Emit(OpCodes.Ldloc, computeSizeValueVariable);
                                    computeSizeILGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeMessageSize), BindingFlags.Public | BindingFlags.Static));
                                    computeSizeILGenerator.Emit(OpCodes.Ldc_I4, CodedOutputStream.ComputeTagSize(field.FieldNumber));
                                    computeSizeILGenerator.Emit(OpCodes.Add_Ovf);
                                    computeSizeILGenerator.Emit(OpCodes.Ldloc, sizeVariable);
                                    computeSizeILGenerator.Emit(OpCodes.Add_Ovf);
                                    computeSizeILGenerator.Emit(OpCodes.Stloc, sizeVariable);
                                }
                                //Write
                                {
                                    //IL: writer.WriteTag(tag);
                                    writeILGenerator.Emit(OpCodes.Ldarg_1);
                                    writeILGenerator.Emit(OpCodes.Ldc_I4, (int)tag);
                                    writeILGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod("WriteTag", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(uint) }, null));

                                    writeILGenerator.Emit(OpCodes.Ldarg_1);
                                    writeILGenerator.Emit(OpCodes.Ldloc, writeValueVariable);
                                    writeILGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteMessage)));
                                }
                                //Read
                                {
                                    var valueVariable = readILGenerator.DeclareLocal(field.FieldType);
                                    var afterNew = readILGenerator.DefineLabel();
                                    readILGenerator.Emit(OpCodes.Ldarg_0);
                                    readILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);
                                    field.GenerateReadFieldCode(readILGenerator);
                                    readILGenerator.Emit(OpCodes.Stloc, valueVariable);

                                    readILGenerator.Emit(OpCodes.Ldloc, valueVariable);
                                    readILGenerator.Emit(OpCodes.Brtrue, afterNew);

                                    if (field.FieldType.GetConstructor(Array.Empty<Type>()) == null)
                                        readILGenerator.Emit(OpCodes.Call, typeof(MessageBuilder).GetMethod(nameof(MessageBuilder.NewObject), BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(field.FieldType));
                                    else
                                        readILGenerator.Emit(OpCodes.Newobj, field.FieldType.GetConstructor(Array.Empty<Type>()));
                                    readILGenerator.Emit(OpCodes.Stloc, valueVariable);

                                    readILGenerator.MarkLabel(afterNew);
                                    readILGenerator.Emit(OpCodes.Ldarg_1);
                                    readILGenerator.Emit(OpCodes.Ldloc, valueVariable);
                                    readILGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadMessage)));

                                    readILGenerator.Emit(OpCodes.Ldarg_0);
                                    readILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);
                                    readILGenerator.Emit(OpCodes.Ldloc, valueVariable);
                                    field.GenerateWriteFieldCode(readILGenerator);
                                }
                            }
                            else
                            {
                                codeGenerator = (ICodeGenerator)Activator.CreateInstance(typeof(ObjectCodeGenerator<>).MakeGenericType(field.FieldType));

                                codeGenerator.GenerateCalculateSizeCode(computeSizeILGenerator, computeSizeValueVariable, field.FieldNumber);
                                computeSizeILGenerator.Emit(OpCodes.Ldloc, sizeVariable);
                                computeSizeILGenerator.Emit(OpCodes.Add_Ovf);
                                computeSizeILGenerator.Emit(OpCodes.Stloc, sizeVariable);

                                codeGenerator.GenerateWriteCode(writeILGenerator, writeValueVariable, field.FieldNumber);
                                readILGenerator.Emit(OpCodes.Ldarg_0);
                                if (sourceFieldInfo.FieldType.IsValueType)
                                    readILGenerator.Emit(OpCodes.Ldflda, sourceFieldInfo);
                                else
                                    readILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);
                                codeGenerator.GenerateReadCode(readILGenerator);
                                field.GenerateWriteFieldCode(readILGenerator);
                            }
                        }
                    }
                }
                else
                {
                    //ComputeSize
                    codeGenerator.GenerateCalculateSizeCode(computeSizeILGenerator, computeSizeValueVariable, field.FieldNumber);
                    computeSizeILGenerator.Emit(OpCodes.Ldloc, sizeVariable);
                    computeSizeILGenerator.Emit(OpCodes.Add_Ovf);
                    computeSizeILGenerator.Emit(OpCodes.Stloc, sizeVariable);

                    //Write
                    codeGenerator.GenerateWriteCode(writeILGenerator, writeValueVariable, field.FieldNumber);

                    //Read
                    //IL : this.Source.{Property} = parser.Read{xxx}();
                    readILGenerator.Emit(OpCodes.Ldarg_0);
                    if (sourceFieldInfo.FieldType.IsValueType)
                        readILGenerator.Emit(OpCodes.Ldflda, sourceFieldInfo);
                    else
                        readILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);
                    codeGenerator.GenerateReadCode(readILGenerator);
                    field.GenerateWriteFieldCode(readILGenerator);
                }


                computeSizeILGenerator.MarkLabel(computeSizeEnd);
                writeILGenerator.MarkLabel(writeEnd);
                readILGenerator.Emit(OpCodes.Br, readWhileStart);
            }

            readILGenerator.MarkLabel(readEnd);

            foreach (var item in collectionProperties)
            {
                readILGenerator.Emit(OpCodes.Ldarg_0);
                readILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);
                readILGenerator.Emit(OpCodes.Ldarg_0);
                readILGenerator.Emit(OpCodes.Ldfld, item.Item1);
                var type = item.Item3;
                var nullable = Nullable.GetUnderlyingType(type) != null;
                if (nullable)
                    type = Nullable.GetUnderlyingType(type);
                //if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort))
                //{
                //    readILGenerator.Emit(OpCodes.Call, typeof(MessageHelper).GetMethod("ConvertTo" + type.Name, BindingFlags.Public | BindingFlags.Static, null, new Type[] { nullable ? typeof(RepeatedField<int?>) : typeof(RepeatedField<int>) }, null));
                //    if (item.Item2.PropertyType.IsArray)
                //    {
                //        readILGenerator.Emit(OpCodes.Call, typeof(System.Linq.Enumerable).GetMethod("ToArray").MakeGenericMethod(item.Item3));
                //    }
                //    else
                //    {
                //        readILGenerator.Emit(OpCodes.Newobj, typeof(List<>).MakeGenericType(item.Item3).GetConstructor(new Type[] { typeof(IEnumerable<>).MakeGenericType(item.Item3) }));
                //        if (item.Item2.PropertyType == typeof(Collection<>).MakeGenericType(item.Item3))
                //            readILGenerator.Emit(OpCodes.Newobj, typeof(Collection<>).MakeGenericType(item.Item3).GetConstructor(new Type[] { typeof(IList<>).MakeGenericType(item.Item3) }));
                //    }
                //}
                if (!item.Item2.FieldType.IsAssignableFrom(item.Item1.FieldType))
                {
                    if (item.Item2.FieldType.IsArray)
                        readILGenerator.Emit(OpCodes.Call, typeof(System.Linq.Enumerable).GetMethod("ToArray").MakeGenericMethod(item.Item3));
                    else if (item.Item2.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                        readILGenerator.Emit(OpCodes.Newobj, item.Item2.FieldType.GetConstructor(new Type[] { typeof(IEnumerable<>).MakeGenericType(item.Item3) }));
                    else
                        readILGenerator.Emit(OpCodes.Newobj, item.Item2.FieldType.GetConstructor(new Type[] { typeof(IList<>).MakeGenericType(item.Item3) }));

                }
                item.Item2.GenerateWriteFieldCode(readILGenerator);
            }
            foreach (var item in dictionaryProperties)
            {
                readILGenerator.Emit(OpCodes.Ldarg_0);
                readILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);
                readILGenerator.Emit(OpCodes.Ldarg_0);
                readILGenerator.Emit(OpCodes.Ldfld, item.Item1);
                if (!item.Item2.FieldType.IsAssignableFrom(item.Item1.FieldType))
                {
                    readILGenerator.Emit(OpCodes.Newobj, item.Item2.FieldType.GetConstructor(new Type[] { typeof(IDictionary<,>).MakeGenericType(item.Item3, item.Item4) }));
                }
                item.Item2.GenerateWriteFieldCode(readILGenerator);
            }

            //ComputeSize
            {
                computeSizeILGenerator.Emit(OpCodes.Ldloc, sizeVariable);
                computeSizeILGenerator.Emit(OpCodes.Ret);
            }
            //CalculateSize
            {
                var calculateSizeMethodBuilder = typeBuilder.DefineMethod("CalculateSize", MethodAttributes.Family | MethodAttributes.Virtual, typeof(int), Array.Empty<Type>());
                var calculateSizeILGenerator = calculateSizeMethodBuilder.GetILGenerator();
                typeBuilder.DefineMethodOverride(calculateSizeMethodBuilder, baseType.GetMethod("CalculateSize", BindingFlags.NonPublic | BindingFlags.Instance));

                calculateSizeILGenerator.Emit(OpCodes.Ldarg_0);
                if (objectType.IsValueType)
                    calculateSizeILGenerator.Emit(OpCodes.Ldflda, sourceFieldInfo);
                else
                    calculateSizeILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);
                calculateSizeILGenerator.Emit(OpCodes.Call, computeSizeMethodBuilder);
                calculateSizeILGenerator.Emit(OpCodes.Ret);
            }

            readILGenerator.Emit(OpCodes.Ret);
            writeILGenerator.Emit(OpCodes.Ret);
            staticIlGenerator.Emit(OpCodes.Ret);

            initFields = speciallyFields.ToArray();
            referenceTypes = referenceWrapTypes.ToArray();
        }
    }
}
