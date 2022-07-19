using Google.Protobuf;
using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Wodsoft.Protobuf.Primitives;
using Xunit;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerClass, DisableTestParallelization = true)]
namespace Wodsoft.Protobuf.Wrapper.Test
{
    [Collection("SerializationTest")]
    public class SerializationTest
    {
        [Fact]
        public void Simply_Class_Test()
        {
            SimplyModel model = new SimplyModel
            {
                IntValue = 100,
                DoubleValue = 222.222,
                StringValue = "This is a simply object",
                BooleanValue = true,
                ByteValue = 200,
                FloatValue = 1.2f,
                LongValue = 34234,
                SByteValue = -97,
                UInt32Value = 203947109,
                UInt64Value = 21389172947123912,
                ShortValue = -16234,
                UShortValue = 36312,
                DateTime = new DateTime(2020, 7, 6, 6, 54, 10, DateTimeKind.Utc),
                DateTimeOffset = new DateTimeOffset(2020, 6, 5, 23, 11, 2, TimeSpan.FromHours(8)),
                TimeSpan = TimeSpan.FromHours(2.3),
                BytesValue = new byte[] { 1, 2, 3, 4, 5 },
                GuidValue = Guid.NewGuid()
            };

            MemoryStream stream = new MemoryStream();
            Message.Serialize(stream, model);

            stream.Position = 0;
            var model2 = Message.Deserialize<SimplyModel>(stream);

            Assert.Equal(model.IntValue, model2.IntValue);
            Assert.Equal(model.DoubleValue, model2.DoubleValue);
            Assert.Equal(model.StringValue, model2.StringValue);
            Assert.Equal(model.BooleanValue, model2.BooleanValue);
            Assert.Equal(model.ByteValue, model2.ByteValue);
            Assert.Equal(model.FloatValue, model2.FloatValue);
            Assert.Equal(model.LongValue, model2.LongValue);
            Assert.Equal(model.SByteValue, model2.SByteValue);
            Assert.Equal(model.UInt32Value, model2.UInt32Value);
            Assert.Equal(model.UInt64Value, model2.UInt64Value);
            Assert.Equal(model.ShortValue, model2.ShortValue);
            Assert.Equal(model.UShortValue, model2.UShortValue);
            Assert.Equal(model.DateTime, model2.DateTime);
            Assert.Equal(model.DateTimeOffset, model2.DateTimeOffset);
            Assert.Equal(model.TimeSpan, model2.TimeSpan);
            Assert.Equal(model.BytesValue, model2.BytesValue);
            Assert.Equal(model.GuidValue, model2.GuidValue);
        }

        [Fact]
        public void Nullable_Class_Test()
        {
            NullableModel model = new NullableModel
            {
                IntValue = 100,
                DoubleValue = 222.222,
                StringValue = "This is a simply object",
                BooleanValue = true,
                ByteValue = 200,
                FloatValue = 1.2f,
                LongValue = 34234,
                SByteValue = -97,
                UInt32Value = 203947109,
                UInt64Value = 21389172947123912,
                ShortValue = -16234,
                UShortValue = 36312,
                DateTime = new DateTime(2020, 7, 6, 6, 54, 10, DateTimeKind.Utc),
                DateTimeOffset = new DateTimeOffset(2020, 6, 5, 23, 11, 2, TimeSpan.FromHours(8)),
                TimeSpan = TimeSpan.FromHours(2.3),
                BytesValue = new byte[] { 1, 2, 3, 4, 5 },
                EnumValue = ByteEnum.Two
            };

            MemoryStream stream = new MemoryStream();
            Message.Serialize(stream, model);

            stream.Position = 0;
            var model2 = Message.Deserialize<NullableModel>(stream);

            Assert.Equal(model.IntValue, model2.IntValue);
            Assert.Equal(model.DoubleValue, model2.DoubleValue);
            Assert.Equal(model.StringValue, model2.StringValue);
            Assert.Equal(model.BooleanValue, model2.BooleanValue);
            Assert.Equal(model.ByteValue, model2.ByteValue);
            Assert.Equal(model.FloatValue, model2.FloatValue);
            Assert.Equal(model.LongValue, model2.LongValue);
            Assert.Equal(model.SByteValue, model2.SByteValue);
            Assert.Equal(model.UInt32Value, model2.UInt32Value);
            Assert.Equal(model.UInt64Value, model2.UInt64Value);
            Assert.Equal(model.ShortValue, model2.ShortValue);
            Assert.Equal(model.UShortValue, model2.UShortValue);
            Assert.Equal(model.DateTime, model2.DateTime);
            Assert.Equal(model.DateTimeOffset, model2.DateTimeOffset);
            Assert.Equal(model.TimeSpan, model2.TimeSpan);
            Assert.Equal(model.BytesValue, model2.BytesValue);
            Assert.Equal(model.EnumValue, model2.EnumValue);
        }

        [Fact]
        public void Nullable_Primitives_Test()
        {
            {
                MemoryStream stream = new MemoryStream();
                byte? value = 155;
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                var result = Message<byte?>.Deserialize(stream);
                Assert.Equal(value, result);
            }
            {
                MemoryStream stream = new MemoryStream();
                ByteEnum? value = ByteEnum.Two;
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                var result = Message<ByteEnum?>.Deserialize(stream);
                Assert.Equal(value, result);
            }
            {
                MemoryStream stream = new MemoryStream();
                int? value = null;
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                var result = Message<int?>.Deserialize(stream);
                Assert.Equal(value, result);
            }
        }

        [Fact]
        public void Enum_Class_Test()
        {
            EnumModel model = new EnumModel
            {
                ByteEnum = ByteEnum.Two,
                SByteEnum = SByteEnum.Disabled,
                Int16Enum = Int16Enum.One,
                Int32Enum = Int32Enum.Disabled,
                Int64Enum = Int64Enum.Disabled,
                UInt16Enum = UInt16Enum.Normal,
                UInt32Enum = UInt32Enum.Three,
                UInt64Enum = UInt64Enum.Three
            };

            MemoryStream stream = new MemoryStream();
            Message.Serialize(stream, model);

            stream.Position = 0;
            var model2 = Message<EnumModel>.Deserialize(stream);

            Assert.Equal(model.ByteEnum, model2.ByteEnum);
            Assert.Equal(model.SByteEnum, model2.SByteEnum);
            Assert.Equal(model.Int16Enum, model2.Int16Enum);
            Assert.Equal(model.Int32Enum, model2.Int32Enum);
            Assert.Equal(model.Int64Enum, model2.Int64Enum);
            Assert.Equal(model.UInt16Enum, model2.UInt16Enum);
            Assert.Equal(model.UInt32Enum, model2.UInt32Enum);
            Assert.Equal(model.UInt64Enum, model2.UInt64Enum);
        }

        [Fact]
        public void Collection_Class_Test()
        {
            CollectionModel model = new CollectionModel();
            model.StringList = new List<string>();
            model.StringList.Add("1");
            model.StringList.Add("2");
            model.StringList.Add("3");
            model.StringList2 = new List<string>();
            model.StringList2.Add("4");
            model.StringList2.Add("5");
            model.StringList2.Add("6");
            model.IntList = new List<int>();
            model.IntList.Add(10);
            model.IntList.Add(20);
            model.IntList.Add(30);
            model.IntEnumerable = new int[] { 15, 25, 35 };
            model.NullableIntList = new List<int?>();
            model.NullableIntList.Add(50);
            model.NullableIntList.Add(6);
            model.NullableIntList.Add(-9);
            model.ByteList = new List<byte>();
            model.ByteList.Add(23);
            model.ByteList.Add(45);
            model.NullableByteList = new List<byte?>();
            model.NullableByteList.Add(123);
            model.NullableByteList.Add(254);
            model.NullableByteList.Add(0);
            model.SByteCollection = new List<sbyte>();
            model.SByteCollection.Add(-45);
            model.SByteCollection.Add(21);
            model.SByteCollection2 = new Collection<sbyte>();
            model.SByteCollection2.Add(-87);
            model.SByteCollection2.Add(88);
            model.NullableShortCollection = new List<short?>();
            model.NullableShortCollection.Add(-8752);
            model.NullableShortCollection.Add(5782);
            model.NullableShortCollection2 = new Collection<short?>();
            model.NullableShortCollection2.Add(-4278);
            model.NullableShortCollection2.Add(7886);
            model.UShortArray = new ushort[] { 16588, 65521 };
            model.NullableUShortArray = new ushort?[] { 32556, 1565 };
            model.LongArray = new long[] { 16581231238, 6545232521 };
            model.NullableLongArray = new long?[] { 325512312341232346, 1512312312433443465 };
            model.TimeSpanCollection = new List<TimeSpan>();
            model.TimeSpanCollection.Add(TimeSpan.FromHours(1.1));
            model.TimeSpanCollection.Add(TimeSpan.FromHours(0.2));
            model.TimeSpanCollection.Add(TimeSpan.FromHours(123));
            model.TimeSpanCollection.Add(TimeSpan.FromHours(42349));

            var data = Message.SerializeToBytes(model);
            var model2 = Message<CollectionModel>.DeserializeFromBytes(data);

            Assert.Equal(model.StringList, model2.StringList);
            Assert.Equal(model.StringList2, model2.StringList2);
            Assert.Equal(model.IntList, model2.IntList);
            Assert.Equal(model.NullableIntList, model2.NullableIntList);
            Assert.Equal(model.ByteList, model2.ByteList);
            Assert.Equal(model.NullableByteList, model2.NullableByteList);
            Assert.Equal(model.SByteCollection, model2.SByteCollection);
            Assert.Equal(model.SByteCollection2, model2.SByteCollection2);
            Assert.Equal(model.NullableShortCollection, model2.NullableShortCollection);
            Assert.Equal(model.NullableShortCollection2, model2.NullableShortCollection2);
            Assert.Equal(model.IntEnumerable, model2.IntEnumerable);
            Assert.Equal<ushort>(model.UShortArray, model2.UShortArray);
            Assert.Equal<ushort?>(model.NullableUShortArray, model2.NullableUShortArray);
            Assert.Equal<long>(model.LongArray, model2.LongArray);
            Assert.Equal<long?>(model.NullableLongArray, model2.NullableLongArray);
            Assert.Equal<TimeSpan>(model.TimeSpanCollection, model2.TimeSpanCollection);
        }

        [Fact]
        public void Dictionary_Class_Test()
        {
            DictionaryModel model = new DictionaryModel();
            model.StringMap = new Dictionary<string, string>();
            model.StringMap["a"] = "1";
            model.StringMap["b"] = "2";
            model.ByteMap = new Dictionary<byte, string>();
            model.ByteMap[2] = "2";
            model.ByteMap[4] = "4";
            model.ShortMap = new Dictionary<string, short?>();
            model.ShortMap["a"] = 123;
            model.ShortMap["b"] = 456;
            model.StringArrayMap = new Dictionary<string, string[]>();
            model.StringArrayMap["a"] = new string[] { "a", "b", "c" };
            model.StringListMap = new Dictionary<string, List<string>>();
            model.StringListMap["a"] = new List<string> { "a", "b", "c" };


            MemoryStream stream = new MemoryStream();
            Message.Serialize(stream, model);

            stream.Position = 0;
            var model2 = Message<DictionaryModel>.Deserialize(stream);

            Assert.Equal<KeyValuePair<string, string>>(model.StringMap, model2.StringMap);
            Assert.Equal<KeyValuePair<byte, string>>(model.ByteMap, model2.ByteMap);
            Assert.Equal<KeyValuePair<string, short?>>(model.ShortMap, model2.ShortMap);
            Assert.Equal(model.StringArrayMap.Count, model2.StringArrayMap.Count);
            Assert.Equal(model.StringArrayMap["a"], model2.StringArrayMap["a"]);
            Assert.Equal(model.StringListMap["a"], model2.StringListMap["a"]);
        }

        [Fact]
        public void Object_Class_Test()
        {
            ContentModel model = new ContentModel();
            model.Title = "This is a test content";
            model.Read = 1000;
            model.User = new UserModel
            {
                UserId = "test",
                UserName = "Test User"
            };

            MemoryStream stream = new MemoryStream();
            Message.Serialize(stream, model);

            stream.Position = 0;
            var model2 = Message<ContentModel>.Deserialize(stream);

            Assert.Equal(model.Title, model2.Title);
            Assert.Equal(model.Read, model2.Read);
            Assert.NotNull(model2.User);
            Assert.Equal(model.User.UserId, model2.User.UserId);
            Assert.Equal(model.User.UserName, model2.User.UserName);
        }

        [Fact]
        public void Message_Class_Test()
        {
            MessageModel model = new MessageModel();
            model.Title = "This is a protobuf message included model";
            model.Number = 99;
            model.ByteString = ByteString.CopyFrom(new byte[] { 123, 34, 12, 200 });
            model.Model = new ProtobufModel
            {
                IntValue = 100,
                StringValue = "Hello"
            };
            model.Models = new List<ProtobufModel>
            {
                new ProtobufModel
                {
                    IntValue = 200,
                    StringValue = "Hello"
                },
                new ProtobufModel
                {
                    IntValue = 210,
                    StringValue = "Hello"
                }
            };

            MemoryStream stream = new MemoryStream();
            Message.Serialize(stream, model);

            stream.Position = 0;
            var model2 = Message<MessageModel>.Deserialize(stream);

            Assert.Equal(model.Title, model2.Title);
            Assert.Equal(model.Number, model2.Number);
            Assert.Equal(model.ByteString, model2.ByteString);
            Assert.NotNull(model2.Model);
            Assert.Equal(model.Model.IntValue, model2.Model.IntValue);
            Assert.Equal(model.Model.StringValue, model2.Model.StringValue);
        }

        [Fact]
        public void Message_Value_Test()
        {
            ProtobufModel model = new ProtobufModel
            {
                IntValue = 100,
                StringValue = "Hello"
            };

            MemoryStream stream = new MemoryStream();
            {
                //Message.Serialize(stream, model);

                ((IMessage)(Message<ProtobufModel>)model).WriteTo(stream);
                //Message.Serialize(stream, model);

                stream.Position = 0;
                Assert.Equal(stream.Length, ((IMessage)(Message<ProtobufModel>)model).CalculateSize());
                var model2 = new MessageMessage<ProtobufModel>(new ProtobufModel());
                ((IMessage)model2).MergeFrom(stream);

                Assert.Equal(model.IntValue, model2.Source.IntValue);
                Assert.Equal(model.StringValue, model2.Source.StringValue);
            }

            {
                var message = (Message<ProtobufModel>)model;
                stream.Position = 0;
                Message.Serialize(stream, model);

                stream.Position = 0;
                var model2 = Message<ProtobufModel>.Deserialize(stream);

                Assert.Equal(model.IntValue, model2.IntValue);
                Assert.Equal(model.StringValue, model2.StringValue);
            }
        }

        [Fact]
        public void Object_Class_Collection_Test()
        {
            ObjectCollectionModel model = new ObjectCollectionModel();
            model.Users = new List<UserModel>();
            model.Users.Add(new UserModel
            {
                UserId = "a",
                UserName = "a"
            });
            model.Users.Add(new UserModel
            {
                UserId = "b",
                UserName = "b"
            });

            MemoryStream stream = new MemoryStream();
            Message.Serialize(stream, model);

            stream.Position = 0;
            var model2 = Message<ObjectCollectionModel>.Deserialize(stream);

            Assert.NotNull(model2.Users);
            Assert.Equal(model.Users[0].UserId, model.Users[0].UserId);
            Assert.Equal(model.Users[0].UserName, model.Users[0].UserName);
            Assert.Equal(model.Users[1].UserId, model.Users[1].UserId);
            Assert.Equal(model.Users[1].UserName, model.Users[1].UserName);
        }

        [Fact]
        public void Object_Class_Dictionary_Test()
        {
            ObjectDictionaryModel model = new ObjectDictionaryModel();
            model.Users = new Dictionary<int, UserModel>();
            model.Users[2] = new UserModel
            {
                UserId = "a",
                UserName = "a"
            };
            model.Users[1] = new UserModel
            {
                UserId = "b",
                UserName = "b"
            };
            //model.Test = new Dictionary<int, int>();
            //model.Test[0] = 100;
            //model.Test[1] = 110;

            MemoryStream stream = new MemoryStream();
            Message.Serialize(stream, model);

            stream.Position = 0;
            var model2 = Message<ObjectDictionaryModel>.Deserialize(stream);

            Assert.NotNull(model2.Users);
            Assert.Equal(model.Users.Count, model2.Users.Count);
            Assert.Equal(model.Users[2].UserId, model2.Users[2].UserId);
            Assert.Equal(model.Users[2].UserName, model2.Users[2].UserName);
            Assert.Equal(model.Users[1].UserId, model2.Users[1].UserId);
            Assert.Equal(model.Users[1].UserName, model2.Users[1].UserName);
        }


        [Fact]
        public void Custom_Struct_Test()
        {
            PointModel model = new PointModel();
            model.X = 1.1;
            model.Y = 34.1;

            MemoryStream stream = new MemoryStream();
            Message.Serialize(stream, model);

            stream.Position = 0;
            var model2 = Message<PointModel>.Deserialize(stream);

            Assert.Equal(model.X, model2.X);
            Assert.Equal(model.Y, model2.Y);
        }

        [Fact]
        public void Primitives_Test()
        {
            Assert.Equal(0, ((IMessage)new StringMessage()).CalculateSize());
            Assert.Throws<TypeInitializationException>(() => new StructureMessage<PointModel>());
            MemoryStream stream = new MemoryStream();
            {
                var value = "test";
                Message.Serialize(stream, "test");
                stream.Position = 0;
                Assert.Equal(stream.Length, ((IMessage)(Message<string>)value).CalculateSize());
                var result = Message<string>.Deserialize(stream);
                Assert.Equal(value, result);
            }
            {
                stream.Position = 0;
                Message.Serialize(stream, true);
                stream.Position = 0;
                var result = Message<bool>.Deserialize(stream);
                Assert.True(result);
            }

            {
                byte value = 155;
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                var result = Message<byte>.Deserialize(stream);
                Assert.Equal(value, result);
            }
            {
                var value = new DateTime(2021, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                Assert.Equal(stream.Length, ((IMessage)(Message<DateTime>)value).CalculateSize());
                var result = Message<DateTime>.Deserialize(stream);
                Assert.Equal(value, result.ToLocalTime());
            }

            {
                var value = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.FromHours(8));
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                var result = Message<DateTimeOffset>.Deserialize(stream);
                Assert.Equal(value, result);
            }

            {
                var value = 3.5d;
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                var result = Message<double>.Deserialize(stream);
                Assert.Equal(value, result);
            }

            {
                var value = Guid.NewGuid();
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                var result = Message<Guid>.Deserialize(stream);
                Assert.Equal(value, result);
            }

            {
                short value = 32000;
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                var result = Message<short>.Deserialize(stream);
                Assert.Equal(value, result);
            }

            {
                int value = 1231245141;
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                var result = Message<int>.Deserialize(stream);
                Assert.Equal(value, result);
            }

            {
                long value = 123124511231241241;
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                var result = Message<long>.Deserialize(stream);
                Assert.Equal(value, result);
            }

            {
                sbyte value = -12;
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                var result = Message<sbyte>.Deserialize(stream);
                Assert.Equal(value, result);
            }

            {
                float value = -123.4f;
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                var result = Message<float>.Deserialize(stream);
                Assert.Equal(value, result);
            }

            {
                TimeSpan value = TimeSpan.FromSeconds(1234);
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                var result = Message<TimeSpan>.Deserialize(stream);
                Assert.Equal(value, result);
            }

            {
                ushort value = 52000;
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                var result = Message<ushort>.Deserialize(stream);
                Assert.Equal(value, result);
            }

            {
                uint value = 4231245141;
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                var result = Message<uint>.Deserialize(stream);
                Assert.Equal(value, result);
            }

            {
                ulong value = 12321124511231241241;
                stream.Position = 0;
                Message.Serialize(stream, value);
                stream.Position = 0;
                var result = Message<ulong>.Deserialize(stream);
                Assert.Equal(value, result);
            }
        }

        [Fact]
        public void Primitives_Collection_Test()
        {
            {
                MemoryStream stream = new MemoryStream();
                List<string> list = new List<string>
                {
                    "A","B","C"
                };
                Message.Serialize(stream, list);
                stream.Position = 0;
                Assert.Equal(stream.Length, ((IMessage)(Message<List<string>>)list).CalculateSize());
                var result = Message<List<string>>.Deserialize(stream);
                Assert.Equal(list, result);
            }
            {
                MemoryStream stream = new MemoryStream();
                List<int> list = new List<int>
                {
                    3,1345,34534
                };
                Message.Serialize(stream, list);
                stream.Position = 0;
                var result = Message<List<int>>.Deserialize(stream);
                Assert.Equal(list, result);
            }
            {
                MemoryStream stream = new MemoryStream();
                List<double> list = new List<double>
                {
                    1.2,3453412.123,564212.233
                };
                Message.Serialize(stream, list);
                stream.Position = 0;
                var result = Message<List<double>>.Deserialize(stream);
                Assert.Equal(list, result);
                stream.Position = 0;
                ((IMessage)(Message<List<double>>)result).MergeFrom(stream);
                Assert.Equal(list, result);
            }
            {
                Assert.Equal(0, ((IMessage)new CollectionMessage<List<SimplyModel>, SimplyModel>()).CalculateSize());
            }
        }

        [Fact]
        public void Primitives_Array_Test()
        {
            {
                MemoryStream stream = new MemoryStream();
                string[] list = new string[]
                {
                    "A","B","C"
                };
                Message.Serialize(stream, list);
                stream.Position = 0;
                Assert.Equal(stream.Length, ((IMessage)((Message<string[]>)list)).CalculateSize());
                var result = Message<string[]>.Deserialize(stream);
                Assert.Equal(list, result);
            }
            {
                MemoryStream stream = new MemoryStream();
                int[] list = new int[]
                {
                    3,1345,34534
                };
                Message.Serialize(stream, list);
                stream.Position = 0;
                var result = Message<int[]>.Deserialize(stream);
                Assert.Equal(list, result);
            }
            {
                MemoryStream stream = new MemoryStream();
                double[] list = new double[]
                {
                    1.2,3453412.123,564212.233
                };
                Message.Serialize(stream, list);
                stream.Position = 0;
                var result = Message<double[]>.Deserialize(stream);
                Assert.Equal(list, result);
            }
            {
                MemoryStream stream = new MemoryStream();
                UserModel[] list = new UserModel[]
                {
                    new UserModel
                    {
                         UserId = "123"
                    }
                };
                Message.Serialize(stream, list);
                stream.Position = 0;
                var result = Message<UserModel[]>.Deserialize(stream);
                Assert.Equal(list[0].UserId, result[0].UserId);
            }
            {
                MemoryStream stream = new MemoryStream();
                List<SimplyModel2> list = new List<SimplyModel2>
                {
                    new SimplyModel2
                    {
                        GuidValue = Guid.NewGuid()
                    }
                };
                Message.Serialize(stream, list);
                stream.Position = 0;
                var result = Message<SimplyModel2[]>.Deserialize(stream);
                Assert.Equal(list[0].GuidValue, result[0].GuidValue);
            }
            {
                Assert.Equal(0, ((IMessage)new ArrayMessage<SimplyModel>()).CalculateSize());
            }
        }

        [Fact]
        public void Loop_Test()
        {
            //LoopModel model = new LoopModel
            //{
            //    Name = "Test",
            //    Inner = new LoopModel
            //    {
            //        Name = "2"
            //    }
            //};
            MessageBuilder.GetMessageType<LoopModel>();
        }

        [Fact]
        public void ValueConstructorModel_Test()
        {
            MessageBuilder.SetTypeInitializer(() => new ValueConstructorModel(string.Empty));
            {
                MemoryStream stream = new MemoryStream();
                ValueConstructorModel model = new ValueConstructorModel("Test");
                Message.Serialize(stream, model);
                stream.Position = 0;
                var result = Message<ValueConstructorModel>.Deserialize(stream);
                Assert.Equal(model.Value, result.Value);
            }
        }
    }
}
