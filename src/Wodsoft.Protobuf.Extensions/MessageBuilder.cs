using Google.Protobuf;
using Google.Protobuf.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;

namespace Wodsoft.Protobuf
{
    public class MessageBuilder
    {
        static MessageBuilder()
        {
            _AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Wodsoft.ComBoost.Grpc.Dynamic"), AssemblyBuilderAccess.Run);
            _ModuleBuilder = _AssemblyBuilder.DefineDynamicModule("Services");
        }

        private static readonly AssemblyBuilder _AssemblyBuilder;
        private static readonly ModuleBuilder _ModuleBuilder;
        private static readonly ConcurrentDictionary<Type, Type> _TypeCache = new ConcurrentDictionary<Type, Type>();

        public static Assembly GetAssembly()
        {
            return _AssemblyBuilder;
        }

        public static Type GetMessageType<T>()
            where T : class, new()
        {
            var type = Message<T>.MessageType;
            if (type == null)
                type = GetMessageType(typeof(T));
            return type;
        }

        public static Type GetMessageType(Type type)
        {
            return _TypeCache.GetOrAdd(type, t =>
            {
                var baseType = typeof(Message<>).MakeGenericType(type);
                var moduleBuilder = _ModuleBuilder;
                var typeBuilder = moduleBuilder.DefineType(type.Namespace + ".Proxy_" + type.Name,
                    TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit,
                    baseType);
                var properties = GetProperties(type);
                BuildMethod(typeBuilder, baseType, type, properties, out var initFields);
                var constructor = BuildConstructor(typeBuilder, baseType, type, initFields);
                BuildEmptyConstructor(typeBuilder, baseType, constructor);
                var messageType = typeBuilder.CreateTypeInfo();
                typeof(Message<>).MakeGenericType(t).GetField("MessageType", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, messageType);
                return messageType;
            });
        }

        private static void BuildEmptyConstructor(TypeBuilder typeBuilder, Type baseType, ConstructorBuilder constructor)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig, CallingConventions.Standard | CallingConventions.HasThis,
                Array.Empty<Type>());
            var ilGenerator = constructorBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Newobj, baseType.GetGenericArguments()[0].GetConstructor(Array.Empty<Type>()));
            ilGenerator.Emit(OpCodes.Call, constructor);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private static ConstructorBuilder BuildConstructor(TypeBuilder typeBuilder, Type baseType, Type objectType, FieldBuilder[] initFields)
        {
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig, CallingConventions.Standard | CallingConventions.HasThis,
                new Type[] { objectType });
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
            foreach (var property in properties)
                TypeCheck(property);
            return properties;
        }

        private static void BuildMethod(TypeBuilder typeBuilder, Type baseType, Type objectType, PropertyInfo[] properties, out FieldBuilder[] initFields)
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
                    if (_ValueTypes.Contains(property.PropertyType) || _NullableValueTypes.Contains(property.PropertyType) || property.PropertyType.IsEnum || Nullable.GetUnderlyingType(property.PropertyType)?.IsEnum == true)
                        tag = WireFormat.MakeTag(index, WireFormat.WireType.Varint);
                    else
                        tag = WireFormat.MakeTag(index, WireFormat.WireType.LengthDelimited);

                    readILGenerator.Emit(OpCodes.Ldloc, readTagVariable);
                    readILGenerator.Emit(OpCodes.Ldc_I4, (int)tag);
                    readILGenerator.Emit(OpCodes.Beq, readTagLabels[property]);
                    if (property.PropertyType.IsGenericType)
                    {
                        var type = property.PropertyType.GetGenericTypeDefinition();
                        if (type == typeof(RepeatedField<>) || type == typeof(IList<>) || type == typeof(ICollection<>) || type == typeof(List<>) || type == typeof(Collection<>))
                        {
                            type = property.PropertyType.GetGenericArguments()[0];
                            type = Nullable.GetUnderlyingType(type) ?? type;
                            if (_ValueTypes.Contains(type))
                            {

                                readILGenerator.Emit(OpCodes.Ldloc, readTagVariable);
                                readILGenerator.Emit(OpCodes.Ldc_I4, (int)WireFormat.MakeTag(index, WireFormat.WireType.Varint));
                                readILGenerator.Emit(OpCodes.Beq, readTagLabels[property]);
                            }
                        }
                    }
                }
                readILGenerator.Emit(OpCodes.Br, readWhileStart);
            }

            List<Tuple<FieldInfo, PropertyInfo, Type>> collectionProperties = new List<Tuple<FieldInfo, PropertyInfo, Type>>();

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

                GenerateReadProperty(computeSizeILGenerator, computeSizeValueVariable, sourceFieldInfo, property);
                GenerateReadProperty(writeILGenerator, writeValueVariable, sourceFieldInfo, property);

                if (property.PropertyType.IsValueType)
                {
                    var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
                    if (underlyingType != null)
                    {
                        //ComputeSize
                        {
                            //IL: if (value.HasValue)
                            computeSizeILGenerator.Emit(OpCodes.Ldloc, computeSizeValueVariable);
                            computeSizeILGenerator.Emit(OpCodes.Callvirt, property.PropertyType.GetProperty("HasValue").GetMethod);
                            computeSizeILGenerator.Emit(OpCodes.Brfalse, computeSizeEnd);
                        }
                        //Write
                        {
                            //IL: if (value.HasValue)
                            writeILGenerator.Emit(OpCodes.Ldloc, writeValueVariable);
                            writeILGenerator.Emit(OpCodes.Callvirt, property.PropertyType.GetProperty("HasValue").GetMethod);
                            writeILGenerator.Emit(OpCodes.Brfalse, writeEnd);
                        }
                    }

                    var type = underlyingType ?? property.PropertyType;

                    //Write
                    {
                        //Get the tag value of this property
                        var tag = WireFormat.MakeTag(index, WireFormat.WireType.Varint);
                        //IL: writer.WriteTag(tag);
                        writeILGenerator.Emit(OpCodes.Ldarg_1);
                        writeILGenerator.Emit(OpCodes.Ldc_I4, (int)tag);
                        writeILGenerator.Emit(OpCodes.Call, _WriteTag);

                        //IL: writer.Write{XXX}(value);
                        writeILGenerator.Emit(OpCodes.Ldarg_1);
                    }
                    //Read
                    {
                        //IL: this.Source.{Property} = value;
                        readILGenerator.Emit(OpCodes.Ldarg_0);
                        readILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);

                        //Convert back values
                        if (type == typeof(Guid))
                        {
                            readILGenerator.Emit(OpCodes.Ldarg_1);
                            readILGenerator.Emit(OpCodes.Call, _ReadMethodMap[type]);
                            readILGenerator.Emit(OpCodes.Call, typeof(ByteString).GetMethod("ToByteArray"));
                            readILGenerator.Emit(OpCodes.Newobj, typeof(Guid).GetConstructor(new Type[] { typeof(byte[]) }));
                        }
                        else if (type == typeof(DateTime))
                        {
                            //IL: dateTime = new Google.Protobuf.WellKnownTypes.Timestamp();
                            var dateTimeVariable = readILGenerator.DeclareLocal(typeof(Google.Protobuf.WellKnownTypes.Timestamp));
                            readILGenerator.Emit(OpCodes.Newobj, typeof(Google.Protobuf.WellKnownTypes.Timestamp).GetConstructor(Array.Empty<Type>()));
                            readILGenerator.Emit(OpCodes.Stloc, dateTimeVariable);
                            readILGenerator.Emit(OpCodes.Ldloc, dateTimeVariable);

                            //IL: parser.ReadMessage(dateTime);
                            readILGenerator.Emit(OpCodes.Ldarg_1);
                            readILGenerator.Emit(OpCodes.Call, _ReadMethodMap[type]);

                            //IL: dateTime.ToDateTime();
                            readILGenerator.Emit(OpCodes.Ldloc);
                            readILGenerator.Emit(OpCodes.Call, typeof(Google.Protobuf.WellKnownTypes.Timestamp).GetMethod("ToDateTime"));
                        }
                        else if (type == typeof(DateTimeOffset))
                        {
                            //IL: dateTime = new Google.Protobuf.WellKnownTypes.Timestamp();
                            var dateTimeVariable = readILGenerator.DeclareLocal(typeof(Google.Protobuf.WellKnownTypes.Timestamp));
                            readILGenerator.Emit(OpCodes.Newobj, typeof(Google.Protobuf.WellKnownTypes.Timestamp).GetConstructor(Array.Empty<Type>()));
                            readILGenerator.Emit(OpCodes.Stloc, dateTimeVariable);
                            readILGenerator.Emit(OpCodes.Ldloc, dateTimeVariable);

                            //IL: parser.ReadMessage(dateTime);
                            readILGenerator.Emit(OpCodes.Ldarg_1);
                            readILGenerator.Emit(OpCodes.Call, _ReadMethodMap[type]);

                            //IL: dateTime.ToDateTime();
                            readILGenerator.Emit(OpCodes.Ldloc);
                            readILGenerator.Emit(OpCodes.Call, typeof(Google.Protobuf.WellKnownTypes.Timestamp).GetMethod("ToDateTimeOffset"));
                        }
                        else if (type == typeof(TimeSpan))
                        {
                            //IL: timespan = new Google.Protobuf.WellKnownTypes.Duration();
                            var timespanVariable = readILGenerator.DeclareLocal(typeof(Google.Protobuf.WellKnownTypes.Duration));
                            readILGenerator.Emit(OpCodes.Newobj, typeof(Google.Protobuf.WellKnownTypes.Duration).GetConstructor(Array.Empty<Type>()));
                            readILGenerator.Emit(OpCodes.Stloc, timespanVariable);
                            readILGenerator.Emit(OpCodes.Ldloc, timespanVariable);

                            //IL: parser.ReadMessage(timespan);
                            readILGenerator.Emit(OpCodes.Ldarg_1);
                            readILGenerator.Emit(OpCodes.Call, _ReadMethodMap[type]);

                            //IL: dateTime.ToDateTime();
                            readILGenerator.Emit(OpCodes.Ldloc);
                            readILGenerator.Emit(OpCodes.Call, typeof(Google.Protobuf.WellKnownTypes.Duration).GetMethod("ToTimeSpan"));
                        }
                        else if (type.IsEnum)
                        {
                            //IL: parser.ReadXXX();
                            readILGenerator.Emit(OpCodes.Ldtoken, type);
                            readILGenerator.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[1] { typeof(RuntimeTypeHandle) }));
                            readILGenerator.Emit(OpCodes.Ldarg_1);
                            readILGenerator.Emit(OpCodes.Call, _ReadMethodMap[Enum.GetUnderlyingType(type)]);
                            if (Enum.GetUnderlyingType(type) == typeof(byte) || Enum.GetUnderlyingType(type) == typeof(sbyte) || Enum.GetUnderlyingType(type) == typeof(short) || Enum.GetUnderlyingType(type) == typeof(ushort))
                            {
                                readILGenerator.Emit(OpCodes.Box, typeof(int));
                                readILGenerator.Emit(OpCodes.Ldtoken, Enum.GetUnderlyingType(type));
                                readILGenerator.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[1] { typeof(RuntimeTypeHandle) }));
                                readILGenerator.Emit(OpCodes.Call, typeof(Convert).GetMethod("ChangeType", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object), typeof(Type) }, null));
                                readILGenerator.Emit(OpCodes.Unbox_Any, Enum.GetUnderlyingType(type));
                            }
                            readILGenerator.Emit(OpCodes.Call, typeof(Enum).GetMethod("ToObject", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(Type), Enum.GetUnderlyingType(type) }, null));
                            //readILGenerator.Emit(OpCodes.Ldtoken, type);
                            //readILGenerator.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[1] { typeof(RuntimeTypeHandle) }));
                            //readILGenerator.Emit(OpCodes.Call, typeof(Convert).GetMethod("ChangeType", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object), typeof(Type) }, null));
                            readILGenerator.Emit(OpCodes.Unbox_Any, type);
                        }
                        else if (type == typeof(sbyte) || type == typeof(byte) || type == typeof(short) || type == typeof(ushort))
                        {
                            readILGenerator.Emit(OpCodes.Ldarg_1);
                            readILGenerator.Emit(OpCodes.Call, _ReadMethodMap[type]);
                            readILGenerator.Emit(OpCodes.Box, typeof(int));
                            readILGenerator.Emit(OpCodes.Ldtoken, type);
                            readILGenerator.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[1] { typeof(RuntimeTypeHandle) }));
                            readILGenerator.Emit(OpCodes.Call, typeof(Convert).GetMethod("ChangeType", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object), typeof(Type) }, null));
                            readILGenerator.Emit(OpCodes.Unbox_Any, type);
                        }
                        else
                        {
                            readILGenerator.Emit(OpCodes.Ldarg_1);
                            readILGenerator.Emit(OpCodes.Call, _ReadMethodMap[type]);
                        }
                        if (underlyingType != null)
                            readILGenerator.Emit(OpCodes.Newobj, property.PropertyType.GetConstructor(Array.Empty<Type>()));
                        readILGenerator.Emit(OpCodes.Callvirt, property.SetMethod);
                    }
                    GenerateConvertValue(computeSizeILGenerator, computeSizeValueVariable, type, underlyingType != null);
                    type = GenerateConvertValue(writeILGenerator, writeValueVariable, type, underlyingType != null);

                    //ComputeSize
                    {
                        //IL: size += CodedOutputStream.Compute{type}Size(value);
                        computeSizeILGenerator.Emit(OpCodes.Call, _ComputeMethodMap[type]);
                        computeSizeILGenerator.Emit(OpCodes.Ldloc, sizeVariable);
                        computeSizeILGenerator.Emit(OpCodes.Add_Ovf);
                        computeSizeILGenerator.Emit(OpCodes.Stloc, sizeVariable);
                    }
                    //Write
                    {
                        writeILGenerator.Emit(OpCodes.Call, _WriteMethodMap[type]);
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
                        var sourceElementType = elementType;
                        if (elementType == typeof(byte) || elementType == typeof(sbyte) || elementType == typeof(short) || elementType == typeof(ushort))
                            elementType = typeof(int);
                        if (elementType == typeof(byte?) || elementType == typeof(sbyte?) || elementType == typeof(short?) || elementType == typeof(ushort?))
                            elementType = typeof(int?);
                        var collectionType = typeof(RepeatedField<>).MakeGenericType(elementType);

                        var codecField = typeBuilder.DefineField("_Codec_" + property.Name, typeof(FieldCodec<>).MakeGenericType(elementType), FieldAttributes.Private | FieldAttributes.Static);
                        //static constructor
                        {
                            //IL: _Codec_{PropertyName} = FieldCodec.For{XXX}(tag);
                            staticIlGenerator.Emit(OpCodes.Ldc_I4, (int)tag);
                            if (_ValueTypes.Contains(elementType))
                            {
                                //IL: FieldCodec.For{XXX}(tag)
                                staticIlGenerator.Emit(OpCodes.Call, _CodecMethodMap[elementType]);
                            }
                            else if (_NullableValueTypes.Contains(elementType))
                            {
                                var type = Nullable.GetUnderlyingType(elementType);
                                //if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort))
                                //    type = typeof(int);
                                //IL: FieldCodec.ForStructWrapper<>(tag)
                                staticIlGenerator.Emit(OpCodes.Call, typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForStructWrapper), BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(uint) }, null).MakeGenericMethod(type));
                            }
                            else if (elementType == typeof(string))
                            {
                                staticIlGenerator.Emit(OpCodes.Call, typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForString), BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(uint) }, null));
                            }
                            else if (elementType == typeof(ByteString) || elementType == typeof(byte[]))
                            {
                                staticIlGenerator.Emit(OpCodes.Call, typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForBytes), BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(uint) }, null));
                            }
                            else
                            {
                                //IL: FieldCodec.ForMessage(tag, {elementType}.Parser);
                                staticIlGenerator.Emit(OpCodes.Ldnull);
                                staticIlGenerator.Emit(OpCodes.Call, (elementType.IsAssignableFrom(typeof(IMessage)) ? elementType : GetMessageType(elementType)).GetProperty("Parser", BindingFlags.Public | BindingFlags.Static).GetMethod);
                                staticIlGenerator.Emit(OpCodes.Call, typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForMessage)).MakeGenericMethod(elementType));
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
                                if (sourceElementType == elementType)
                                {
                                    GenerateAddCollection(computeSizeILGenerator, computeSizeValueVariable, field);
                                }
                                else
                                {
                                    var enumerableVariable = computeSizeILGenerator.DeclareLocal(typeof(IEnumerable<int>));
                                    computeSizeILGenerator.Emit(OpCodes.Ldloc, computeSizeValueVariable);
                                    computeSizeILGenerator.Emit(OpCodes.Call, typeof(MessageHelper).GetMethod("ConvertToInt", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(IEnumerable<>).MakeGenericType(sourceElementType) }, null));
                                    computeSizeILGenerator.Emit(OpCodes.Stloc, enumerableVariable);
                                    GenerateAddCollection(computeSizeILGenerator, enumerableVariable, field);
                                }
                                computeSizeILGenerator.Emit(OpCodes.Ldarg_0);
                                computeSizeILGenerator.Emit(OpCodes.Ldfld, field);
                            }
                            //Write
                            {
                                if (sourceElementType == elementType)
                                {
                                    GenerateAddCollection(writeILGenerator, writeValueVariable, field);
                                }
                                else
                                {
                                    var enumerableVariable = writeILGenerator.DeclareLocal(Nullable.GetUnderlyingType(sourceElementType) == null ? typeof(IEnumerable<int>) : typeof(IEnumerable<int?>));
                                    writeILGenerator.Emit(OpCodes.Ldloc, writeValueVariable);
                                    writeILGenerator.Emit(OpCodes.Call, typeof(MessageHelper).GetMethod("ConvertToInt", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(IEnumerable<>).MakeGenericType(sourceElementType) }, null));
                                    writeILGenerator.Emit(OpCodes.Stloc, enumerableVariable);
                                    GenerateAddCollection(writeILGenerator, enumerableVariable, field);
                                }
                                writeILGenerator.Emit(OpCodes.Ldarg_0);
                                writeILGenerator.Emit(OpCodes.Ldfld, field);
                            }
                            //Read
                            {
                                //this._{property}
                                readILGenerator.Emit(OpCodes.Ldarg_0);
                                readILGenerator.Emit(OpCodes.Ldfld, field);
                            }

                            collectionProperties.Add(new Tuple<FieldInfo, PropertyInfo, Type>(field, property, sourceElementType));
                        }
                        //ComputeSize
                        {
                            //IL: size += collection.CalculateSize(_Codec_{PropertyName});
                            computeSizeILGenerator.Emit(OpCodes.Ldsfld, codecField);
                            computeSizeILGenerator.Emit(OpCodes.Callvirt, collectionType.GetMethod("CalculateSize"));
                            computeSizeILGenerator.Emit(OpCodes.Ldloc, sizeVariable);
                            computeSizeILGenerator.Emit(OpCodes.Add_Ovf);
                            computeSizeILGenerator.Emit(OpCodes.Stloc, sizeVariable);
                        }
                        //Write
                        {
                            //IL: WriteTo(ref writer, _Codec_{PropertyName});
                            writeILGenerator.Emit(OpCodes.Ldarg_1);
                            writeILGenerator.Emit(OpCodes.Ldsfld, codecField);
                            writeILGenerator.Emit(OpCodes.Callvirt, collectionType.GetMethod("WriteTo", new Type[] { typeof(WriteContext).MakeByRefType(), typeof(FieldCodec<>).MakeGenericType(elementType) }));
                        }
                        //Read
                        {
                            //IL: .AddEntriesFrom(ref parser, _Codec_{PropertyName});
                            readILGenerator.Emit(OpCodes.Ldarg_1);
                            readILGenerator.Emit(OpCodes.Ldsfld, codecField);
                            readILGenerator.Emit(OpCodes.Callvirt, collectionType.GetMethod("AddEntriesFrom", new Type[] { typeof(ParseContext).MakeByRefType(), codecField.FieldType }));
                        }
                    }
                    else if (isDictionary)
                    {
                        var dictionaryType = typeof(MapField<,>).MakeGenericType(elementType, elementType2);

                        var codecField = typeBuilder.DefineField("_Codec_" + property.Name, typeof(MapField<,>).MakeGenericType(elementType, elementType2).GetNestedType("Codec"), FieldAttributes.Private | FieldAttributes.Static);
                        //static constructor
                        {
                            //IL: FieldCodec.For{XXX}(tag);
                            if (_ValueTypes.Contains(elementType))
                            {
                                var keyTag = WireFormat.MakeTag(index, WireFormat.WireType.Varint);
                                //IL: FieldCodec.ForStructWrapper(keyTag)
                                staticIlGenerator.Emit(OpCodes.Ldc_I4, (int)keyTag);
                                staticIlGenerator.Emit(OpCodes.Call, typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForStructWrapper)).MakeGenericMethod(elementType));
                            }
                            else
                            {
                                var keyTag = WireFormat.MakeTag(index, WireFormat.WireType.LengthDelimited);
                                //IL: FieldCodec.ForMessage(keyTag, {elementType}.Parser);
                                staticIlGenerator.Emit(OpCodes.Ldc_I4, (int)keyTag);
                                staticIlGenerator.Emit(OpCodes.Ldnull);
                                staticIlGenerator.Emit(OpCodes.Call, (elementType.IsAssignableFrom(typeof(IMessage)) ? elementType : GetMessageType(elementType)).GetProperty("Parser", BindingFlags.Public | BindingFlags.Static).GetMethod);
                                staticIlGenerator.Emit(OpCodes.Call, typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForMessage)).MakeGenericMethod(elementType));
                            }

                            if (_ValueTypes.Contains(elementType2))
                            {
                                var valueTag = WireFormat.MakeTag(index, WireFormat.WireType.Varint);
                                //IL: FieldCodec.ForStructWrapper(keyTag)
                                staticIlGenerator.Emit(OpCodes.Ldc_I4, (int)valueTag);
                                staticIlGenerator.Emit(OpCodes.Call, typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForStructWrapper)).MakeGenericMethod(elementType2));
                            }
                            else
                            {
                                var valueTag = WireFormat.MakeTag(index, WireFormat.WireType.LengthDelimited);
                                //IL: FieldCodec.ForMessage(keyTag, {elementType}.Parser);
                                staticIlGenerator.Emit(OpCodes.Ldc_I4, (int)valueTag);
                                staticIlGenerator.Emit(OpCodes.Ldnull);
                                staticIlGenerator.Emit(OpCodes.Call, (elementType.IsAssignableFrom(typeof(IMessage)) ? elementType2 : GetMessageType(elementType2)).GetProperty("Parser", BindingFlags.Public | BindingFlags.Static).GetMethod);
                                staticIlGenerator.Emit(OpCodes.Call, typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForMessage)).MakeGenericMethod(elementType2));
                            }

                            staticIlGenerator.Emit(OpCodes.Ldc_I4, (int)tag);
                            staticIlGenerator.Emit(OpCodes.Newobj, typeof(MapField<,>).MakeGenericType(elementType, elementType2).GetNestedType("Codec").GetConstructor(Array.Empty<Type>()));
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
                        }

                        //ComputeSize
                        {
                            computeSizeILGenerator.Emit(OpCodes.Ldsfld, codecField);
                            computeSizeILGenerator.Emit(OpCodes.Callvirt, dictionaryType.GetMethod("CalculateSize"));
                            computeSizeILGenerator.Emit(OpCodes.Ldloc, sizeVariable);
                            computeSizeILGenerator.Emit(OpCodes.Add_Ovf);
                            computeSizeILGenerator.Emit(OpCodes.Stloc, sizeVariable);
                        }
                        //Write
                        {
                            //IL: WriteTo(ref writer, _Codec_{PropertyName});
                            writeILGenerator.Emit(OpCodes.Ldarg_1);
                            writeILGenerator.Emit(OpCodes.Ldsfld, codecField);
                            writeILGenerator.Emit(OpCodes.Callvirt, dictionaryType.GetMethod("WriteTo", new Type[] { typeof(WriteContext), typeof(MapField<,>).MakeGenericType(elementType, elementType2).GetNestedType("Codec") }));
                        }
                        //Read
                        {
                            //IL: .AddEntriesFrom(ref parser, _Codec_{PropertyName});
                            readILGenerator.Emit(OpCodes.Ldarg_1);
                            readILGenerator.Emit(OpCodes.Ldsfld, codecField);
                            readILGenerator.Emit(OpCodes.Callvirt, dictionaryType.GetMethod("AddEntriesFrom", new Type[] { typeof(ParseContext).MakeByRefType(), codecField.FieldType }));
                        }
                    }
                    else if (property.PropertyType == typeof(string) || property.PropertyType == typeof(byte[]) || property.PropertyType == typeof(ByteString))
                    {
                        //ComputeSize
                        {
                            //IL: size += CodedOutputStream.Compute{type}Size(value);
                            computeSizeILGenerator.Emit(OpCodes.Ldloc, computeSizeValueVariable);
                            if (property.PropertyType == typeof(byte[]))
                                computeSizeILGenerator.Emit(OpCodes.Call, typeof(ByteString).GetMethod("CopyFrom", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(byte[]) }, null));
                            computeSizeILGenerator.Emit(OpCodes.Call, property.PropertyType == typeof(string) ? _ComputeStringSizeMethodInfo : _ComputeBytesSizeMethodInfo);
                            computeSizeILGenerator.Emit(OpCodes.Ldloc, sizeVariable);
                            computeSizeILGenerator.Emit(OpCodes.Add_Ovf);
                            computeSizeILGenerator.Emit(OpCodes.Stloc, sizeVariable);
                        }
                        //Write
                        {
                            //IL: writer.WriteTag(tag);
                            writeILGenerator.Emit(OpCodes.Ldarg_1);
                            writeILGenerator.Emit(OpCodes.Ldc_I4, (int)tag);
                            writeILGenerator.Emit(OpCodes.Call, _WriteTag);

                            //IL writer.Write{XXX}(source.{Property});
                            writeILGenerator.Emit(OpCodes.Ldarg_1);
                            writeILGenerator.Emit(OpCodes.Ldloc, writeValueVariable);
                            if (property.PropertyType == typeof(byte[]))
                                writeILGenerator.Emit(OpCodes.Call, typeof(ByteString).GetMethod("CopyFrom", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(byte[]) }, null));
                            writeILGenerator.Emit(OpCodes.Callvirt, _WriteMethodMap[property.PropertyType]);
                        }
                        //Read
                        {
                            //IL: this.Source.{Property} = value;
                            readILGenerator.Emit(OpCodes.Ldarg_0);
                            readILGenerator.Emit(OpCodes.Ldfld, sourceFieldInfo);
                            readILGenerator.Emit(OpCodes.Ldarg_1);
                            readILGenerator.Emit(OpCodes.Callvirt, _ReadMethodMap[property.PropertyType]);
                            if (property.PropertyType == typeof(byte[]))
                                readILGenerator.Emit(OpCodes.Call, typeof(ByteString).GetMethod("ToByteArray"));
                            readILGenerator.Emit(OpCodes.Callvirt, property.SetMethod);
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
                            writeILGenerator.Emit(OpCodes.Call, _WriteTag);

                            writeILGenerator.Emit(OpCodes.Ldarg_1);
                            writeILGenerator.Emit(OpCodes.Ldloc, writeValueVariable);
                        }

                        if (!property.PropertyType.IsAssignableFrom(typeof(IMessage)))
                        {
                            //ComputeSize
                            {
                                computeSizeILGenerator.Emit(OpCodes.Newobj, GetMessageType(property.PropertyType).GetConstructor(new Type[] { property.PropertyType }));
                            }
                            //Write
                            {
                                writeILGenerator.Emit(OpCodes.Newobj, GetMessageType(property.PropertyType).GetConstructor(new Type[] { property.PropertyType }));
                            }
                        }

                        //ComputeSize
                        {
                            computeSizeILGenerator.Emit(OpCodes.Call, _ComputeMessageSizeMethodInfo);
                            computeSizeILGenerator.Emit(OpCodes.Ldloc, sizeVariable);
                            computeSizeILGenerator.Emit(OpCodes.Add_Ovf);
                            computeSizeILGenerator.Emit(OpCodes.Stloc, sizeVariable);
                        }
                        //Write
                        {
                            writeILGenerator.Emit(OpCodes.Callvirt, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteGroup)));

                        }
                    }
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
                if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort))
                {
                    readILGenerator.Emit(OpCodes.Call, typeof(MessageHelper).GetMethod("ConvertTo" + type.Name, BindingFlags.Public | BindingFlags.Static, null, new Type[] { nullable ? typeof(RepeatedField<int?>) : typeof(RepeatedField<int>) }, null));
                    if (item.Item2.PropertyType.IsArray)
                    {
                        readILGenerator.Emit(OpCodes.Call, typeof(System.Linq.Enumerable).GetMethod("ToArray").MakeGenericMethod(item.Item3));
                    }
                    else
                    {
                        readILGenerator.Emit(OpCodes.Newobj, typeof(List<>).MakeGenericType(item.Item3).GetConstructor(new Type[] { typeof(IEnumerable<>).MakeGenericType(item.Item3) }));
                        if (item.Item2.PropertyType == typeof(Collection<>).MakeGenericType(item.Item3))
                            readILGenerator.Emit(OpCodes.Newobj, typeof(Collection<>).MakeGenericType(item.Item3).GetConstructor(new Type[] { typeof(IList<>).MakeGenericType(item.Item3) }));
                    }
                }
                else if (!item.Item2.PropertyType.IsAssignableFrom(item.Item1.FieldType))
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
        }

        private static void GenerateReadProperty(ILGenerator ilGenerator, LocalBuilder valueVariable, FieldInfo souceField, PropertyInfo property)
        {
            //IL: value = source.{Property};
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, souceField);
            ilGenerator.Emit(OpCodes.Callvirt, property.GetMethod);
            ilGenerator.Emit(OpCodes.Stloc, valueVariable);
        }

        private static Type GenerateConvertValue(ILGenerator ilGenerator, LocalBuilder valueVariable, Type type, bool nullable)
        {
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            if (nullable)
                ilGenerator.Emit(OpCodes.Callvirt, type.GetProperty("Value").GetMethod);
            if (type == typeof(Guid))
            {
                ilGenerator.Emit(OpCodes.Call, typeof(Guid).GetMethod("ToByteArray"));
                ilGenerator.Emit(OpCodes.Call, typeof(ByteString).GetMethod("CopyFrom", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(byte[]) }, null));
            }
            else if (type == typeof(DateTime))
                ilGenerator.Emit(OpCodes.Call, typeof(Google.Protobuf.WellKnownTypes.Timestamp).GetMethod("FromDateTime", BindingFlags.Public | BindingFlags.Static));
            else if (type == typeof(DateTimeOffset))
                ilGenerator.Emit(OpCodes.Call, typeof(Google.Protobuf.WellKnownTypes.Timestamp).GetMethod("FromDateTimeOffset", BindingFlags.Public | BindingFlags.Static));
            else if (type == typeof(TimeSpan))
                ilGenerator.Emit(OpCodes.Call, typeof(Google.Protobuf.WellKnownTypes.Duration).GetMethod("FromTimespan", BindingFlags.Public | BindingFlags.Static));
            else if (type.IsEnum)
            {
                var enumType = type;
                type = Enum.GetUnderlyingType(type);
                if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort))
                {
                    ilGenerator.Emit(OpCodes.Box, enumType);
                    ilGenerator.Emit(OpCodes.Ldtoken, type);
                    ilGenerator.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[1] { typeof(RuntimeTypeHandle) }));
                    ilGenerator.Emit(OpCodes.Call, typeof(Convert).GetMethod("ChangeType", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object), typeof(Type) }, null));
                    ilGenerator.Emit(OpCodes.Unbox_Any, type);
                }
            }
            //else if (type == typeof(sbyte) || type == typeof(byte) || type == typeof(short) || type == typeof(ushort))
            //{
            //    ilGenerator.Emit(OpCodes.Ldtoken, typeof(int));
            //    ilGenerator.Emit(OpCodes.Call, typeof(Convert).GetMethod("ChangeType", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object), typeof(Type) }, null));
            //}
            return type;
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
            ilGenerator.Emit(OpCodes.Callvirt, field.FieldType.GetMethod("AddRange"));

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
            ilGenerator.Emit(OpCodes.Callvirt, field.FieldType.GetMethods().Where(t => t.Name == "Add").OrderBy(t => t.GetParameters().Length).First());

            ilGenerator.MarkLabel(skip);
        }


        #region CalculateSize

        private static readonly MethodInfo _ComputeDoubleSizeMethodInfo = typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeDoubleSize), BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo _ComputeFloatSizeMethodInfo = typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeFloatSize), BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo _ComputeInt32SizeMethodInfo = typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeInt32Size), BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo _ComputeInt64SizeMethodInfo = typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeInt64Size), BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo _ComputeUInt32SizeMethodInfo = typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeUInt32Size), BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo _ComputeUInt64SizeMethodInfo = typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeUInt64Size), BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo _ComputeBoolSizeMethodInfo = typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeBoolSize), BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo _ComputeStringSizeMethodInfo = typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeStringSize), BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo _ComputeBytesSizeMethodInfo = typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeBytesSize), BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo _ComputeMessageSizeMethodInfo = typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeMessageSize), BindingFlags.Static | BindingFlags.Public);
        private static readonly Dictionary<Type, MethodInfo> _ComputeMethodMap = new Dictionary<Type, MethodInfo>
        {
            { typeof(byte), _ComputeInt32SizeMethodInfo },
            { typeof(sbyte), _ComputeInt32SizeMethodInfo },
            { typeof(short), _ComputeInt32SizeMethodInfo },
            { typeof(ushort), _ComputeInt32SizeMethodInfo },
            { typeof(double), _ComputeDoubleSizeMethodInfo },
            { typeof(float), _ComputeFloatSizeMethodInfo },
            { typeof(int), _ComputeInt32SizeMethodInfo },
            { typeof(long), _ComputeInt64SizeMethodInfo },
            { typeof(uint), _ComputeUInt32SizeMethodInfo },
            { typeof(ulong), _ComputeUInt64SizeMethodInfo },
            { typeof(bool), _ComputeBoolSizeMethodInfo },
            { typeof(string), _ComputeStringSizeMethodInfo },
            { typeof(ByteString), _ComputeBytesSizeMethodInfo },
        };
        private static readonly Dictionary<Type, MethodInfo> _CodecMethodMap = new Dictionary<Type, MethodInfo>
        {
            { typeof(byte), typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForInt32), BindingFlags.Public| BindingFlags.Static, null, new Type[]{ typeof(uint) }, null) },
            { typeof(sbyte), typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForInt32), BindingFlags.Public| BindingFlags.Static, null, new Type[]{ typeof(uint) }, null) },
            { typeof(short), typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForInt32), BindingFlags.Public| BindingFlags.Static, null, new Type[]{ typeof(uint) }, null) },
            { typeof(ushort), typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForInt32), BindingFlags.Public| BindingFlags.Static, null, new Type[]{ typeof(uint) }, null) },
            { typeof(double), typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForDouble), BindingFlags.Public| BindingFlags.Static, null, new Type[]{ typeof(uint) }, null) },
            { typeof(float), typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForFloat), BindingFlags.Public| BindingFlags.Static, null, new Type[]{ typeof(uint) }, null) },
            { typeof(int), typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForInt32), BindingFlags.Public| BindingFlags.Static, null, new Type[]{ typeof(uint) }, null) },
            { typeof(long), typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForInt64), BindingFlags.Public| BindingFlags.Static, null, new Type[]{ typeof(uint) }, null) },
            { typeof(uint), typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForUInt32), BindingFlags.Public| BindingFlags.Static, null, new Type[]{ typeof(uint) }, null) },
            { typeof(ulong), typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForUInt64), BindingFlags.Public| BindingFlags.Static, null, new Type[]{ typeof(uint) }, null) },
            { typeof(bool), typeof(FieldCodec).GetMethod(nameof(FieldCodec.ForBool), BindingFlags.Public| BindingFlags.Static, null, new Type[]{ typeof(uint) }, null) },
        };

        #endregion

        #region Write

        private static readonly Dictionary<Type, MethodInfo> _WriteMethodMap = new Dictionary<Type, MethodInfo>
        {
            { typeof(byte), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteInt32)) },
            { typeof(sbyte), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteInt32)) },
            { typeof(short), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteInt32)) },
            { typeof(ushort), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteInt32)) },
            { typeof(double), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteDouble)) },
            { typeof(float), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteFloat)) },
            { typeof(int), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteInt32)) },
            { typeof(long), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteInt64)) },
            { typeof(uint), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteUInt32)) },
            { typeof(ulong), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteUInt64)) },
            { typeof(bool), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteBool)) },
            { typeof(string), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteString)) },
            { typeof(ByteString), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteBytes)) },
            { typeof(Guid), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteBytes)) },
            { typeof(DateTime), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteMessage)) },
            { typeof(DateTimeOffset), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteMessage)) },
            { typeof(TimeSpan), typeof(WriteContext).GetMethod(nameof(WriteContext.WriteMessage)) },
        };
        private static readonly MethodInfo _WriteTag = typeof(WriteContext).GetMethod("WriteTag", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(uint) }, null);

        #endregion

        #region Read

        private static readonly Dictionary<Type, MethodInfo> _ReadMethodMap = new Dictionary<Type, MethodInfo>
        {
            { typeof(byte), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadInt32)) },
            { typeof(sbyte), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadInt32)) },
            { typeof(short), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadInt32)) },
            { typeof(ushort), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadInt32)) },
            { typeof(double), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadDouble)) },
            { typeof(float), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadFloat)) },
            { typeof(int), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadInt32)) },
            { typeof(long), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadInt64)) },
            { typeof(uint), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadUInt32)) },
            { typeof(ulong), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadUInt64)) },
            { typeof(bool), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadBool)) },
            { typeof(string), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadString)) },
            { typeof(ByteString), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadBytes)) },
            { typeof(Guid), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadBytes)) },
            { typeof(DateTime), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadMessage)) },
            { typeof(DateTimeOffset), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadMessage)) },
            { typeof(TimeSpan), typeof(ParseContext).GetMethod(nameof(ParseContext.ReadMessage)) },
        };

        #endregion

        private static readonly Type[] _ValueTypes = new Type[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(double), typeof(float), typeof(int), typeof(long), typeof(uint), typeof(ulong), typeof(bool) };
        private static readonly Type[] _NullableValueTypes = new Type[] { typeof(byte?), typeof(sbyte?), typeof(short?), typeof(ushort?), typeof(double?), typeof(float?), typeof(int?), typeof(long?), typeof(uint?), typeof(ulong?), typeof(bool?) };
        private static readonly Type[] _ScalarValueTypes = new Type[] { typeof(double), typeof(float), typeof(int), typeof(long), typeof(uint), typeof(ulong),
            typeof(bool), typeof(Guid), typeof(string), typeof(ByteString), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(byte[]),
            typeof(double?), typeof(float?), typeof(int?), typeof(long?), typeof(uint?), typeof(ulong?), typeof(bool?), typeof(Guid?)};
        private static Exception TypeCheck(PropertyInfo property)
        {
            Type type = property.PropertyType;
            if (_ScalarValueTypes.Contains(type))
                return null;
            if (type.IsEnum)
                return null;
            if (type.IsAssignableFrom(typeof(IMessage)))
                return null;
            if (type.IsArray)
                type = type.GetElementType();
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>) || type.GetGenericTypeDefinition() == typeof(Collection<>) || type.GetGenericTypeDefinition() == typeof(Nullable<>)))
                type = type.GetGenericArguments()[0];
            Exception ex = null;
            if (type.IsClass)
                ex = TypeCheck(type);
            return new NotSupportedException($"Type \"{property.PropertyType}\" of \"{property.DeclaringType.FullName}.{property.Name}\" is not support by grpc.", ex);
        }

        private static readonly ConcurrentDictionary<Type, Exception> _TypeChecks = new ConcurrentDictionary<Type, Exception>();
        private static Exception TypeCheck(Type type)
        {
            return _TypeChecks.GetOrAdd(type, _ =>
            {
                Exception ex;
                foreach (var prop in GetProperties(type))
                {
                    ex = TypeCheck(prop);
                    if (ex != null)
                        return ex;
                }
                return null;
            });
        }
    }
}
