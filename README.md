﻿# Wodsoft Protobuf Wrapper

## Contents

- [About](#About)
- [Requirements](#Requirements)
- [Installation](#Installation)
- [Usage](#Usage)
  - [Serialize](#Serialize)
  - [Deserialize](#Deserialize)
  - [Field Definition](#Field-Definition)
  - [Field Order](#Field-Order)
  - [Non empty parameter constructor object](#Non-empty-parameter-constructor-object)
  - [Get Protobuf Wrapper](#Get-Protobuf-Wrapper)
- [Advanced](#Advanced)
  - [Supported property types and relationships](#Supported-property-types-and-relationships)
  - [How it works](#How-it-works)
  - [Performance](#Performance)
- [License](#License)

## About

This library is a extension that help your use Google Protobuf without `.proto` files.

In general `.proto` file will generate model that inherit `IMessage`.
Protobuf using it for serialization.

Sometimes we already have some models in .NET projects and doesn't need to share them for other languages.
And we also need to serialize or deserialize them with Protobuf.

In this case, this library will help you to use Protobuf for serialization.

## Requirements

Wodsoft.Protobuf.Wrapper requires NETStandard 2.0 or above.

The library works on platform that allow dynamic complie. For example, **IOS is not allow to work.**

## Installation

Library is available in NuGet package [Wodsoft.Protobuf.Wrapper](https://www.nuget.org/packages/Wodsoft.Protobuf.Wrapper).

```bash
dotnet add package Wodsoft.Protobuf.Wrapper
```

## Usage

### Serialize

You can use method `Serialize` of `Wodsoft.Protobuf.Message`.
You need a `System.IO.Stream` to store bytes.

```csharp
YourModel model = new ();
MemoryStream stream = new MemoryStream();
Message.Serialize(stream, model);
```

Also, there is a overloaded method.
You can pass a `Google.Protobuf.CodedInputStream` from your context.

```csharp
YourModel model = new ();
CodedInputStream input = ...;
Message.Serialize(input, model);
```

Or you want to serialize to bytes directly.

```csharp
YourModel model = new ();
var bytes = Message.SerializeToBytes(model);
```

### Deserialize

You can use method `Deserialize` of `Wodsoft.Protobuf.Message`.
You need a `System.IO.Stream` where contains serialized bytes.
Then it will return your model of generic argument `T`.

```csharp
Stream stream = ...;
YourType model = Message.Deserialize<YourType>(stream);
```

Also, there is a overloaded method too.
You can pass a `Google.Protobuf.CodedOutputStream` from your context.

```csharp
CodedOutputStream output = ...;
YourType model = Message.Deserialize<YourType>(output);
```

Or you want to deserialize from bytes directly.

```csharp
YourType model = Message.DeserializeFromBytes<YourType>(bytes);
```

### Field Definition

`IMessageFieldProvider.GetFields(Type type)` will return message fields that map from a object type.

The default implementation is `GeneralMessageFieldProvider.Intance`.
It's only map readable and writeable propertie to message field.

You can create your own `IMessageFieldProvider` to map message fields.
And set `Message<T>.FieldProvider` to your field provider instance value.

### Field Order

Use `System.Runtime.Serialization.DataMemberAttribute` for your properties and set the `Order` property to attribute.  
Otherwise it will order properties with name.

> **⚠️** If there is a `DataMemberAttribute` on any property, it will **ONLY** serialize or deserialize with properties which has `DataMemberAttribute`.

> **⚠️** If there is no `DataMemberAttribute` exists, communication with difference version model program maybe **FAULT** because of difference property **ORDER**.

### Non empty parameter constructor object

Use `MessageBuilder.SetTypeInitializer<T>(Func<T> initializer)` to set the initializer for non empty parameter constructor object.

### Get Protobuf Wrapper

We can set a model value to `Message<>` variable directly.

```csharp
SimplyModel model;
Message<SimplyModel> message = model;
```

Then `message` can be used in Protobuf serialization directly.


## Advanced

### Supported property types and relationships

| C# Types | Protobuf Types | Message Structure |
| - | - | - |
| bool(?) | bool | Varint |
| sbyte(?) | int32 | Varint |
| byte(?) | int32 | Varint |
| short(?) | int32 | Varint |
| ushort(?) | int32 | Varint |
| int(?) | int32 | Varint |
| long(?) | int64 | Varint |
| uint(?) | uint32 | Varint |
| ulong(?) | uint64 | Varint |
| float(?) | float | Varint |
| double(?) | double | Varint |
| decimal(?) | ByteString | Length-delimited |
| string | string | Length-delimited |
| byte[] | ByteString | Length-delimited |
| Guid(?) | ByteString | Length-delimited |
| DateTime(?) | google.protobuf.Timestamp | Length-delimited |
| DateTimeOffset(?) | google.protobuf.Timestamp | Length-delimited |
| TimeSpan(?) | google.protobuf.Duration | Length-delimited |
| IMessage | | Length-delimited |
| T[] | RepeatedField\<T\> | Length-delimited |
| ICollection\<T\> | RepeatedField\<T\> | Length-delimited |
| Collection\<T\> | RepeatedField\<T\> | Length-delimited |
| IList\<T\> | RepeatedField\<T\> | Length-delimited |
| List\<T\> | RepeatedField\<T\> | Length-delimited |
| IDictionary\<TKey, TValue\> | MapField\<TKey, TValue\> | Length-delimited |
| Dictionary\<TKey, TValue\> | MapField\<TKey, TValue\> | Length-delimited |

- **(?)** means work with `Nullable<>` types.
- It's fine to use Protobuf object that inherit `Google.Protobuf.IMessage` as property type.
- All `RepeatedField` and `MapField` object **CAN NOT CONTAINS** `null` values.
- We support use `byte`, `sbyte`, `short` and `ushort` as property type.
It will work as type `int` with serialization.
Deserialize from other library serialized message, `int` field maybe lost its data.

### How it works

Mainly, Protobuf do serialization through `Google.Protobuf.IMessage` and `Google.Protobuf.IBufferMessage` interfaces.

So we define a abstract `Wodsoft.Protobuf.Message` class.
And define protected abstract `Read`, `Write`, `CalculateSize` methods.
Explicit implement there interfaces and call the methods.

Then define a abstract `Wodsoft.Protobuf.Message<T>` generic class.
There is a property to reach origin model. And we can make some implicit operator here.
```csharp
public T Source { get; }
```

Finally, we create dynamic class inherit `Message<T>` for those models when they need to do serialization in runtime.
Emit codes for `Read`, `Write`, `CalculateSize`.

### Performance

- **RECOMMEND USE** `RepeatedField<>`, `IList<>` or `ICollection<>` as collection property type.
Use `RepeatedField<>` will be the **fastest performance**.
- Use `IList<>` or `ICollection<>` will convert it to `RepeatedField<>` when serialize model.
- Use `List<>` or `Collection<>` will convert it to `RepeatedField<>` when serialize model.
And convert it back when deserialize model.
- **RECOMMEND USE** `MapField<,>` or `IDictionary<,>` as dictionary property type.
Use `MapField<,>` will be the **fastest performance**.
- Use `IDictionary<,>` will convert it to `MapField<,>` when serialize model.
- Use `Dictionary<,>` will convert it to `MapField<,>` when serialize model.
And convert it back when deserialize model.

## License

This library is under the MIT License.