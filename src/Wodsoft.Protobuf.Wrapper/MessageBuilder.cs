using Google.Protobuf;
using Google.Protobuf.Collections;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using Wodsoft.Protobuf.Generators;

namespace Wodsoft.Protobuf
{
    public class MessageBuilder
    {
        static MessageBuilder()
        {
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Wodsoft.ComBoost.Grpc.Dynamic"), AssemblyBuilderAccess.Run);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule("Services");

            _CodeGenerators[typeof(bool)] = new BooleanCodeGenerator();
            _CodeGenerators[typeof(byte)] = new ByteCodeGenerator();
            _CodeGenerators[typeof(sbyte)] = new SByteCodeGenerator();
            _CodeGenerators[typeof(short)] = new Int16CodeGenerator();
            _CodeGenerators[typeof(int)] = new Int32CodeGenerator();
            _CodeGenerators[typeof(long)] = new Int64CodeGenerator();
            _CodeGenerators[typeof(ushort)] = new UInt16CodeGenerator();
            _CodeGenerators[typeof(uint)] = new UInt32CodeGenerator();
            _CodeGenerators[typeof(ulong)] = new UInt64CodeGenerator();
            _CodeGenerators[typeof(float)] = new SingleCodeGenerator();
            _CodeGenerators[typeof(double)] = new DoubleCodeGenerator();
            _CodeGenerators[typeof(DateTime)] = new DateTimeCodeGenerator();
            _CodeGenerators[typeof(DateTimeOffset)] = new DateTimeOffsetCodeGenerator();
            _CodeGenerators[typeof(TimeSpan)] = new TimeSpanCodeGenerator();
            _CodeGenerators[typeof(Guid)] = new GuidCodeGenerator();
            _CodeGenerators[typeof(string)] = new StringCodeGenerator();

            _CodeGenerators[typeof(bool?)] = new NullableCodeGenerator<bool>(new BooleanCodeGenerator());
            _CodeGenerators[typeof(byte?)] = new NullableCodeGenerator<byte>(new ByteCodeGenerator());
            _CodeGenerators[typeof(sbyte?)] = new NullableCodeGenerator<sbyte>(new SByteCodeGenerator());
            _CodeGenerators[typeof(short?)] = new NullableCodeGenerator<short>(new Int16CodeGenerator());
            _CodeGenerators[typeof(int?)] = new NullableCodeGenerator<int>(new Int32CodeGenerator());
            _CodeGenerators[typeof(long?)] = new NullableCodeGenerator<long>(new Int64CodeGenerator());
            _CodeGenerators[typeof(ushort?)] = new NullableCodeGenerator<ushort>(new UInt16CodeGenerator());
            _CodeGenerators[typeof(uint?)] = new NullableCodeGenerator<uint>(new UInt32CodeGenerator());
            _CodeGenerators[typeof(ulong?)] = new NullableCodeGenerator<ulong>(new UInt64CodeGenerator());
            _CodeGenerators[typeof(float?)] = new NullableCodeGenerator<float>(new SingleCodeGenerator());
            _CodeGenerators[typeof(double?)] = new NullableCodeGenerator<double>(new DoubleCodeGenerator());
            _CodeGenerators[typeof(DateTime?)] = new NullableCodeGenerator<DateTime>(new DateTimeCodeGenerator());
            _CodeGenerators[typeof(DateTimeOffset?)] = new NullableCodeGenerator<DateTimeOffset>(new DateTimeOffsetCodeGenerator());
            _CodeGenerators[typeof(TimeSpan?)] = new NullableCodeGenerator<TimeSpan>(new TimeSpanCodeGenerator());
            _CodeGenerators[typeof(Guid?)] = new NullableCodeGenerator<Guid>(new GuidCodeGenerator());
        }

        internal static readonly AssemblyBuilder AssemblyBuilder;
        internal static readonly ModuleBuilder ModuleBuilder;
        private static readonly ConcurrentDictionary<Type, Type> _TypeCache = new ConcurrentDictionary<Type, Type>();
        private static readonly Dictionary<Type, ICodeGenerator> _CodeGenerators = new Dictionary<Type, ICodeGenerator>();

        /// <summary>
        /// Set code generator of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="codeGenerator">Code generator of type <typeparamref name="T"/>.</param>
        public static void SetCodeGenerator<T>(ICodeGenerator codeGenerator)
            where T : struct
        {
            _CodeGenerators[typeof(T)] = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
        }

        /// <summary>
        /// Get code generator of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <returns>Return code generator if exists, otherwise null.</returns>
        public static ICodeGenerator GetCodeGenerator<T>()
        {
            _CodeGenerators.TryGetValue(typeof(T), out var value);
            return value;
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
            where T : class, new()
        {
            var type = Message<T>.MessageType;
            if (type == null)
                type = GetMessageType(typeof(T));
            return type;
        }

        /// <summary>
        /// Get message wrapper type.
        /// </summary>
        /// <param name="type">Type of object.</param>
        /// <returns>Return message wrapper type.</returns>
        public static Type GetMessageType(Type type)
        {
            Type[] refTypes = null;
            var wrappedType = _TypeCache.GetOrAdd(type, t =>
            {
                var baseType = typeof(Message<>).MakeGenericType(type);
                var typeBuilder = (TypeBuilder)baseType.GetField("TypeBuilder", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                var properties = GetProperties(type);
                BuildMethod(typeBuilder, baseType, type, properties, out var initFields, out refTypes);
                var constructor = BuildConstructor(typeBuilder, baseType, type, initFields);
                BuildEmptyConstructor(typeBuilder, baseType, type, constructor);
                var messageType = typeBuilder.CreateTypeInfo();
                typeof(Message<>).MakeGenericType(t).GetField("MessageType", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, messageType);
                return messageType.AsType();
            });
            if (refTypes != null)
                foreach (var item in refTypes)
                    if (!_TypeCache.ContainsKey(item))
                        GetMessageType(item);
            return wrappedType;
        }

        private static void BuildEmptyConstructor(TypeBuilder typeBuilder, Type baseType, Type wrapType, ConstructorBuilder constructor)
        {
            var constructorBuilder = (ConstructorBuilder)typeof(Message<>).MakeGenericType(wrapType).GetField("EmptyConstructor", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            var ilGenerator = constructorBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Newobj, baseType.GetGenericArguments()[0].GetConstructor(Array.Empty<Type>()));
            ilGenerator.Emit(OpCodes.Call, constructor);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private static ConstructorBuilder BuildConstructor(TypeBuilder typeBuilder, Type baseType, Type objectType, FieldBuilder[] initFields)
        {
            var constructorBuilder = (ConstructorBuilder)typeof(Message<>).MakeGenericType(objectType).GetField("ValueConstructor", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            var ilGenerator = constructorBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, baseType.GetConstructor(new Type[] { objectType }));

            foreach (var field in initFields)
            {
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Newobj, field.FieldType.GetConstructor(Array.Empty<Type>()));
                ilGenerator.Emit(OpCodes.Stfld, field);
            }
            ilGenerator.Emit(OpCodes.Ret);
            return constructorBuilder;
        }

        private static PropertyInfo[] GetProperties(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(t => t.CanWrite && t.CanRead).ToArray();
            if (properties.Any(t => t.GetCustomAttribute<DataMemberAttribute>() != null))
                properties = properties.Where(t => t.GetCustomAttribute<DataMemberAttribute>() != null).OrderBy(t => t.GetCustomAttribute<DataMemberAttribute>().Order).ToArray();
            return properties;
        }

        private static void BuildMethod(TypeBuilder typeBuilder, Type baseType, Type objectType, PropertyInfo[] properties, out FieldBuilder[] initFields, out Type[] referenceTypes)
        {
            var computeSizeMethodBuilder = typeBuilder.DefineMethod("ComputeSize", MethodAttributes.Static | MethodAttributes.Public, typeof(int), new Type[] { objectType });
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
            var readTagLabels = properties.ToDictionary(t => t, t => readILGenerator.DefineLabel());

            var staticConstructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
        MethodAttributes.RTSpecialName | MethodAttributes.Static, CallingConventions.Standard, null);
            var staticIlGenerator = staticConstructorBuilder.GetILGenerator();

            var sourceFieldInfo = baseType.GetField("SourceValue", BindingFlags.NonPublic | BindingFlags.Instance);

            int index = 0;

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

                foreach (var property in properties)
                {
                    index++;
                    uint tag;
                    if (_CodeGenerators.TryGetValue(property.PropertyType, out var codeGenerator))
                    {
                        tag = WireFormat.MakeTag(index, codeGenerator.WireType);
                    }
                    else
                    {
                        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                        if (type.IsEnum)
                        {
                            tag = WireFormat.MakeTag(index, _CodeGenerators[Enum.GetUnderlyingType(type)].WireType);
                        }
                        else
                            tag = WireFormat.MakeTag(index, WireFormat.WireType.LengthDelimited);
                    }

                    readILGenerator.Emit(OpCodes.Ldloc, readTagVariable);
                    readILGenerator.Emit(OpCodes.Ldc_I4, (int)tag);
                    readILGenerator.Emit(OpCodes.Beq, readTagLabels[property]);
                    if (property.PropertyType.IsGenericType)
                    {
                        var type = property.PropertyType.GetGenericTypeDefinition();
                        if (type == typeof(RepeatedField<>) || type == typeof(IList<>) || type == typeof(ICollection<>) || type == typeof(List<>) || type == typeof(Collection<>))
                        {
                            type = property.PropertyType.GetGenericArguments()[0];
                            if (_CodeGenerators.TryGetValue(type, out codeGenerator) && codeGenerator.WireType == WireFormat.WireType.Varint)
                            {

                                readILGenerator.Emit(OpCodes.Ldloc, readTagVariable);
                                readILGenerator.Emit(OpCodes.Ldc_I4, (int)WireFormat.MakeTag(index, WireFormat.WireType.Varint));
                                readILGenerator.Emit(OpCodes.Beq, readTagLabels[property]);
                            }
                        }
                    }
                    else if (property.PropertyType.IsArray)
                    {
                        if (_CodeGenerators.TryGetValue(property.PropertyType.GetElementType(), out codeGenerator) && codeGenerator.WireType == WireFormat.WireType.Varint)
                        {

                            readILGenerator.Emit(OpCodes.Ldloc, readTagVariable);
                            readILGenerator.Emit(OpCodes.Ldc_I4, (int)WireFormat.MakeTag(index, WireFormat.WireType.Varint));
                            readILGenerator.Emit(OpCodes.Beq, readTagLabels[property]);
                        }
                    }
                }
                readILGenerator.Emit(OpCodes.Br, readWhileStart);
            }

            List<Tuple<FieldInfo, PropertyInfo, Type>> collectionProperties = new List<Tuple<FieldInfo, PropertyInfo, Type>>();
            List<Tuple<FieldInfo, PropertyInfo, Type, Type>> dictionaryProperties = new List<Tuple<FieldInfo, PropertyInfo, Type, Type>>();

            index = 0;
            foreach (var property in properties)
            {
                index++;
                var dataMemberAttribute = property.GetCustomAttribute<DataMemberAttribute>();
                if (dataMemberAttribute != null)
                    index = dataMemberAttribute.Order;

                var computeSizeValueVariable = computeSizeILGenerator.DeclareLocal(property.PropertyType);
                var computeSizeEnd = computeSizeILGenerator.DefineLabel();
                var writeValueVariable = writeILGenerator.DeclareLocal(property.PropertyType);
                var writeEnd = writeILGenerator.DefineLabel();

                readILGenerator.MarkLabel(readTagLabels[property]);

                _CodeGenerators.TryGetValue(property.PropertyType, out var codeGenerator);

                GenerateReadProperty(computeSizeILGenerator, computeSizeValueVariable, sourceFieldInfo, property);
                GenerateReadProperty(writeILGenerator, writeValueVariable, sourceFieldInfo, property);

                if (codeGenerator == null)
                {
                    if (property.PropertyType.IsValueType && (Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType).IsEnum)
                    {
                        var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                        if (underlyingType != null)
                        {
                            //ComputeSize
                            {
                                //IL: if (value.HasValue)
                                computeSizeILGenerator.Emit(OpCodes.Ldloc, computeSizeValueVariable);
                                computeSizeILGenerator.Emit(OpCodes.Call, property.PropertyType.GetProperty("HasValue").GetMethod);
                                computeSizeILGenerator.Emit(OpCodes.Brfalse, computeSizeEnd);
                            }
                            //Write
                            {
                                //IL: if (value.HasValue)
                                writeILGenerator.Emit(OpCodes.Ldloc, writeValueVariable);
                                writeILGenerator.Emit(OpCodes.Call, property.PropertyType.GetProperty("HasValue").GetMethod);
                                writeILGenerator.Emit(OpCodes.Brfalse, writeEnd);
                            }
                        }

                        var type = underlyingType ?? property.PropertyType;

                        var valueType = Enum.GetUnderlyingType(type);
                        codeGenerator = _CodeGenerators[valueType];

                        //Write
                        codeGenerator.GenerateWriteCode(writeILGenerator, writeValueVariable, index);

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
                                readILGenerator.Emit(OpCodes.Newobj, property.PropertyType.GetConstructor(Array.Empty<Type>()));
                            readILGenerator.Emit(OpCodes.Callvirt, property.SetMethod);
                        }

                        //ComputeSize
                        {
                            //IL: size += CodedOutputStream.Compute{type}Size(value);
                            codeGenerator.GenerateCalculateSizeCode(computeSizeILGenerator, computeSizeValueVariable);
                            computeSizeILGenerator.Emit(OpCodes.Ldloc, sizeVariable);
                            computeSizeILGenerator.Emit(OpCodes.Add_Ovf);
                            computeSizeILGenerator.Emit(OpCodes.Stloc, sizeVariable);
                        }
                    }
                    else
                    {
                        GenerateCheckNull(computeSizeILGenerator, computeSizeValueVariable, computeSizeEnd);
                        GenerateCheckNull(writeILGenerator, writeValueVariable, writeEnd);

                        bool isCollection = false;
                        bool isDictionary = false;
                        Type elementType = null;
                        Type elementType2 = null;
                        if (property.PropertyType.IsArray)
                        {
                            isCollection = true;
                            elementType = property.PropertyType.GetElementType();
                        }
                        else if (property.PropertyType.IsGenericType)
                        {
                            var genericType = property.PropertyType.GetGenericTypeDefinition();
                            if (genericType == typeof(IList<>) || genericType == typeof(List<>) || genericType == typeof(ICollection<>) || genericType == typeof(Collection<>) || genericType == typeof(RepeatedField<>))
                            {
                                elementType = property.PropertyType.GetGenericArguments()[0];
                                isCollection = true;
                            }
                            else if (genericType == typeof(Dictionary<,>) || genericType == typeof(IDictionary<,>) || genericType == typeof(MapField<,>))
                            {
                                var types = property.PropertyType.GetGenericArguments();
                                elementType = types[0];
                                elementType2 = types[1];
                                isDictionary = true;
                            }
                        }

                        //Get the tag value of this property
                        var tag = WireFormat.MakeTag(index, WireFormat.WireType.LengthDelimited);

                        if (isCollection)
                        {
                            var collectionType = typeof(RepeatedField<>).MakeGenericType(elementType);

                            _CodeGenerators.TryGetValue(elementType, out codeGenerator);

                            var codecField = typeBuilder.DefineField("_Codec_" + property.Name, typeof(FieldCodec<>).MakeGenericType(elementType), FieldAttributes.Private | FieldAttributes.Static);
                            //static constructor
                            {
                                //IL: _Codec_{PropertyName} = FieldCodec.For{XXX}(tag);
                                if (codeGenerator == null)
                                    GenerateCodecValue(staticIlGenerator, elementType, tag);
                                else
                                {
                                    var generatorType = typeof(ICodeGenerator<>).MakeGenericType(elementType);
                                    staticIlGenerator.Emit(OpCodes.Call, typeof(MessageBuilder).GetMethod("GetCodeGenerator").MakeGenericMethod(elementType));
                                    staticIlGenerator.Emit(OpCodes.Castclass, generatorType);
                                    staticIlGenerator.Emit(OpCodes.Ldc_I4, index);
                                    staticIlGenerator.Emit(OpCodes.Callvirt, generatorType.GetMethod("CreateFieldCodec", new Type[] { typeof(int) }));
                                }
                                staticIlGenerator.Emit(OpCodes.Stsfld, codecField);
                            }

                            if (property.PropertyType == collectionType)
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
                                    readILGenerator.Emit(OpCodes.Callvirt, property.GetMethod);
                                }
                            }
                            else
                            {
                                var field = typeBuilder.DefineField("_" + property.Name, collectionType, FieldAttributes.Private | FieldAttributes.InitOnly);
                                speciallyFields.Add(field);

                                //ComputeSize
                                {
                                    GenerateAddCollection(computeSizeILGenerator, computeSizeValueVariable, field);
                                    computeSizeILGenerator.Emit(OpCodes.Ldarg_0);
                                    computeSizeILGenerator.Emit(OpCodes.Ldfld, field);
                                }
                                //Write
                                {
                                    GenerateAddCollection(writeILGenerator, writeValueVariable, field);
                                    writeILGenerator.Emit(OpCodes.Ldarg_0);
                                    writeILGenerator.Emit(OpCodes.Ldfld, field);
                                }
                                //Read
                                {
                                    //this._{property}
                                    readILGenerator.Emit(OpCodes.Ldarg_0);
                                    readILGenerator.Emit(OpCodes.Ldfld, field);
                                }
                                //add this to move collections to this.Source.{Property}
                                collectionProperties.Add(new Tuple<FieldInfo, PropertyInfo, Type>(field, property, elementType));
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

                            var codecField = typeBuilder.DefineField("_Codec_" + property.Name, typeof(MapField<,>.Codec).MakeGenericType(elementType, elementType2), FieldAttributes.Private | FieldAttributes.Static);
                            //static constructor
                            {
                                _CodeGenerators.TryGetValue(elementType, out codeGenerator);
                                if (codeGenerator == null)
                                {
                                    GenerateCodecValue(staticIlGenerator, elementType, WireFormat.MakeTag(1, WireFormat.WireType.LengthDelimited));
                                }
                                else
                                {
                                    var generatorType = typeof(ICodeGenerator<>).MakeGenericType(elementType);
                                    staticIlGenerator.Emit(OpCodes.Call, typeof(MessageBuilder).GetMethod("GetCodeGenerator").MakeGenericMethod(elementType));
                                    staticIlGenerator.Emit(OpCodes.Castclass, generatorType);
                                    staticIlGenerator.Emit(OpCodes.Ldc_I4, 1);
                                    staticIlGenerator.Emit(OpCodes.Callvirt, generatorType.GetMethod("CreateFieldCodec", new Type[] { typeof(int) }));
                                }
                                _CodeGenerators.TryGetValue(elementType2, out codeGenerator);
                                if (codeGenerator == null)
                                {
                                    GenerateCodecValue(staticIlGenerator, elementType2, WireFormat.MakeTag(1, WireFormat.WireType.LengthDelimited));
                                }
                                else
                                {
                                    var generatorType = typeof(ICodeGenerator<>).MakeGenericType(elementType2);
                                    staticIlGenerator.Emit(OpCodes.Call, typeof(MessageBuilder).GetMethod("GetCodeGenerator").MakeGenericMethod(elementType2));
                                    staticIlGenerator.Emit(OpCodes.Castclass, generatorType);
                                    staticIlGenerator.Emit(OpCodes.Ldc_I4, 2);
                                    staticIlGenerator.Emit(OpCodes.Callvirt, generatorType.GetMethod("CreateFieldCodec", new Type[] { typeof(int) }));
                                }

                                //IL: FieldCodec.For{XXX}(tag);
                                staticIlGenerator.Emit(OpCodes.Ldc_I4, (int)tag);
                                staticIlGenerator.Emit(OpCodes.Newobj, codecField.FieldType.GetConstructors()[0]);
                                staticIlGenerator.Emit(OpCodes.Stsfld, codecField);
                            }

                            if (property.PropertyType == dictionaryType)
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
                                    readILGenerator.Emit(OpCodes.Callvirt, property.GetMethod);
                                }
                            }
                            else
                            {
                                var field = typeBuilder.DefineField("_" + property.Name, dictionaryType, FieldAttributes.Private | FieldAttributes.InitOnly);
                                speciallyFields.Add(field);

                                //ComputeSize
                                {
                                    GenerateAddDictionary(computeSizeILGenerator, computeSizeValueVariable, field);
                                    computeSizeILGenerator.Emit(OpCodes.Ldarg_0);
                                    computeSizeILGenerator.Emit(OpCodes.Ldfld, field);
                                }
                                //Write
                                {
                                    GenerateAddDictionary(writeILGenerator, writeValueVariable, field);
                                    writeILGenerator.Emit(OpCodes.Ldarg_0);
                                    writeILGenerator.Emit(OpCodes.Ldfld, field);
                                }
                                //Read
                                {
                                    //this._{property}
                                    readILGenerator.Emit(OpCodes.Ldarg_0);
                                    readILGenerator.Emit(OpCodes.Ldfld, field);
                                }
                                //add this to move dictionary to this.Source.{Property}
                                dictionaryProperties.Add(new Tuple<FieldInfo, PropertyInfo, Type, Type>(field, property, elementType, elementType2));
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
                            //ComputeSize
                            {
                                computeSizeILGenerator.Emit(OpCodes.Ldloc, computeSizeValueVariable);
                            }
                            //Write
                            {
                                //IL: writer.WriteTag(tag);
                                writeILGenerator.Emit(OpCodes.Ldarg_1);
                                writeILGenerator.Emit(OpCodes.Ldc_I4, (int)tag);
                                writeILGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod("WriteTag", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(uint) }, null));

                                writeILGenerator.Emit(OpCodes.Ldarg_1);
                                writeILGenerator.Emit(OpCodes.Ldloc, writeValueVariable);
                            }

                            if (!typeof(IMessage).IsAssignableFrom(property.PropertyType))
                            {
                                if (!referenceWrapTypes.Contains(property.PropertyType))
                                    referenceWrapTypes.Add(property.PropertyType);
                                var wrappedType = typeof(Message<>).MakeGenericType(property.PropertyType);
                                var valueConstructor = (ConstructorBuilder)wrappedType.GetField("ValueConstructor", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                                //ComputeSize
                                {
                                    computeSizeILGenerator.Emit(OpCodes.Newobj, valueConstructor);
                                }
                                //Write
                                {
                                    writeILGenerator.Emit(OpCodes.Newobj, valueConstructor);
                                }
                                //Read
                                {
                                    var valueVariable = readILGenerator.DeclareLocal(wrappedType);
                                    readILGenerator.Emit(OpCodes.Newobj, (ConstructorBuilder)wrappedType.GetField("EmptyConstructor", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
                                    readILGenerator.Emit(OpCodes.Stloc, valueVariable);
                                    readILGenerator.Emit(OpCodes.Ldarg_1);
                                    readILGenerator.Emit(OpCodes.Ldloc, valueVariable);
                                    readILGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadMessage)));

                                    readILGenerator.Emit(OpCodes.Ldarg_0);
                                    readILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);
                                    readILGenerator.Emit(OpCodes.Ldloc, valueVariable);
                                    readILGenerator.Emit(OpCodes.Call, wrappedType.GetProperty("Source").GetMethod);
                                    readILGenerator.Emit(OpCodes.Callvirt, property.SetMethod);
                                }
                            }
                            else
                            {
                                //Read
                                {
                                    var valueVariable = readILGenerator.DeclareLocal(property.PropertyType);
                                    var afterNew = readILGenerator.DefineLabel();
                                    readILGenerator.Emit(OpCodes.Ldarg_0);
                                    readILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);
                                    readILGenerator.Emit(OpCodes.Callvirt, property.GetMethod);
                                    readILGenerator.Emit(OpCodes.Stloc, valueVariable);

                                    readILGenerator.Emit(OpCodes.Ldloc, valueVariable);
                                    readILGenerator.Emit(OpCodes.Brtrue, afterNew);

                                    readILGenerator.Emit(OpCodes.Newobj, property.PropertyType.GetConstructor(Array.Empty<Type>()));
                                    readILGenerator.Emit(OpCodes.Stloc, valueVariable);

                                    readILGenerator.MarkLabel(afterNew);
                                    readILGenerator.Emit(OpCodes.Ldarg_1);
                                    readILGenerator.Emit(OpCodes.Ldloc, valueVariable);
                                    readILGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadMessage)));

                                    readILGenerator.Emit(OpCodes.Ldarg_0);
                                    readILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);
                                    readILGenerator.Emit(OpCodes.Ldloc, valueVariable);
                                    readILGenerator.Emit(OpCodes.Callvirt, property.SetMethod);
                                }
                            }

                            //ComputeSize
                            {
                                computeSizeILGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeMessageSize), BindingFlags.Public | BindingFlags.Static));
                                computeSizeILGenerator.Emit(OpCodes.Ldloc, sizeVariable);
                                computeSizeILGenerator.Emit(OpCodes.Add_Ovf);
                                computeSizeILGenerator.Emit(OpCodes.Ldc_I4_1);
                                computeSizeILGenerator.Emit(OpCodes.Add_Ovf);
                                computeSizeILGenerator.Emit(OpCodes.Stloc, sizeVariable);
                            }
                            //Write
                            {
                                writeILGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteMessage)));
                            }
                        }
                    }
                }
                else
                {
                    //ComputeSize
                    codeGenerator.GenerateCalculateSizeCode(computeSizeILGenerator, computeSizeValueVariable);
                    computeSizeILGenerator.Emit(OpCodes.Ldloc, sizeVariable);
                    computeSizeILGenerator.Emit(OpCodes.Add_Ovf);
                    computeSizeILGenerator.Emit(OpCodes.Stloc, sizeVariable);

                    //Write
                    codeGenerator.GenerateWriteCode(writeILGenerator, writeValueVariable, index);

                    //Read
                    //IL : this.Source.{Property} = parser.Read{xxx}();
                    readILGenerator.Emit(OpCodes.Ldarg_0);
                    readILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);
                    codeGenerator.GenerateReadCode(readILGenerator);
                    readILGenerator.Emit(OpCodes.Callvirt, property.SetMethod);
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
                if (!item.Item2.PropertyType.IsAssignableFrom(item.Item1.FieldType))
                {
                    if (item.Item2.PropertyType.IsArray)
                        readILGenerator.Emit(OpCodes.Call, typeof(System.Linq.Enumerable).GetMethod("ToArray").MakeGenericMethod(item.Item3));
                    else if (item.Item2.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        readILGenerator.Emit(OpCodes.Newobj, item.Item2.PropertyType.GetConstructor(new Type[] { typeof(IEnumerable<>).MakeGenericType(item.Item3) }));
                    else
                        readILGenerator.Emit(OpCodes.Newobj, item.Item2.PropertyType.GetConstructor(new Type[] { typeof(IList<>).MakeGenericType(item.Item3) }));

                }
                readILGenerator.Emit(OpCodes.Callvirt, item.Item2.SetMethod);
            }
            foreach (var item in dictionaryProperties)
            {
                readILGenerator.Emit(OpCodes.Ldarg_0);
                readILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);
                readILGenerator.Emit(OpCodes.Ldarg_0);
                readILGenerator.Emit(OpCodes.Ldfld, item.Item1);
                if (!item.Item2.PropertyType.IsAssignableFrom(item.Item1.FieldType))
                {
                    readILGenerator.Emit(OpCodes.Newobj, item.Item2.PropertyType.GetConstructor(new Type[] { typeof(IDictionary<,>).MakeGenericType(item.Item3, item.Item4) }));
                }
                readILGenerator.Emit(OpCodes.Callvirt, item.Item2.SetMethod);
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
                calculateSizeILGenerator.Emit(OpCodes.Call, computeSizeMethodBuilder);
                calculateSizeILGenerator.Emit(OpCodes.Ret);
            }

            readILGenerator.Emit(OpCodes.Ret);
            writeILGenerator.Emit(OpCodes.Ret);
            staticIlGenerator.Emit(OpCodes.Ret);

            initFields = speciallyFields.ToArray();
            referenceTypes = referenceWrapTypes.ToArray();
        }

        private static void GenerateCodecValue(ILGenerator staticIlGenerator, Type elementType, uint tag)
        {
            staticIlGenerator.Emit(OpCodes.Ldc_I4, (int)tag);
            //IL: FieldCodec.ForMessage(tag, {elementType}.Parser);
            staticIlGenerator.Emit(OpCodes.Ldnull);
            staticIlGenerator.Emit(OpCodes.Call, (elementType.IsAssignableFrom(typeof(IMessage)) ? elementType : GetMessageType(elementType)).GetProperty("Parser", BindingFlags.Public | BindingFlags.Static).GetMethod);
            staticIlGenerator.Emit(OpCodes.Call, typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForMessage)).MakeGenericMethod(elementType));
        }

        private static void GenerateReadProperty(ILGenerator ilGenerator, LocalBuilder valueVariable, FieldInfo souceField, PropertyInfo property)
        {
            //IL: value = source.{Property};
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, souceField);
            ilGenerator.Emit(OpCodes.Callvirt, property.GetMethod);
            ilGenerator.Emit(OpCodes.Stloc, valueVariable);
        }

        private static void GenerateCheckNull(ILGenerator ilGenerator, LocalBuilder valueVariable, Label end)
        {
            //IL:if (value == null) goto end;
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Brfalse, end);
        }

        private static void GenerateAddCollection(ILGenerator ilGenerator, LocalBuilder valueVariable, FieldInfo field)
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

        private static void GenerateAddDictionary(ILGenerator ilGenerator, LocalBuilder valueVariable, FieldInfo field)
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
    }
}
