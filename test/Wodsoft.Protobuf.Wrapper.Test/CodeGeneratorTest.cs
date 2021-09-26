using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wodsoft.Protobuf.Generators;
using Xunit;

namespace Wodsoft.Protobuf.Wrapper.Test
{
    public class CodeGeneratorTest
    {
        [Fact]
        public void Boolean_Test()
        {
            var codeGenerator = new BooleanCodeGenerator();
            var value = true;
            Assert.Equal(CodedOutputStream.ComputeBoolSize(true), codeGenerator.CalculateSize(value));
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }

        [Fact]
        public void ByteArray_Test()
        {
            var codeGenerator = new ByteArrayCodeGenerator();
            var value = new byte[] { 1, 45, 72, 231 };
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }

        [Fact]
        public void Byte_Test()
        {
            var codeGenerator = new ByteCodeGenerator();
            byte value = 245;
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }

        [Fact]
        public void ByteString_Test()
        {
            var codeGenerator = new ByteStringCodeGenerator();
            var value = ByteString.CopyFrom(new byte[] { 23, 142, 53, 231, 123 });
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }

        [Fact]
        public void DateTime_Test()
        {
            var codeGenerator = new DateTimeCodeGenerator();
            var value = DateTime.Now;
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value.ToUniversalTime(), fieldCodec.Read(input).ToUniversalTime());
        }

        [Fact]
        public void DateTimeOffset_Test()
        {
            var codeGenerator = new DateTimeOffsetCodeGenerator();
            var value = DateTimeOffset.Now;
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }

        [Fact]
        public void Double_Test()
        {
            var codeGenerator = new DoubleCodeGenerator();
            var value = 123.123d;
            Assert.Equal(CodedOutputStream.ComputeDoubleSize(value), codeGenerator.CalculateSize(value));
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }

        [Fact]
        public void Guid_Test()
        {
            var codeGenerator = new GuidCodeGenerator();
            var value = Guid.NewGuid();
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }

        [Fact]
        public void Int16_Test()
        {
            var codeGenerator = new Int16CodeGenerator();
            short value = 12412;
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }

        [Fact]
        public void Int32_Test()
        {
            var codeGenerator = new Int32CodeGenerator();
            var value = 1123124352;
            Assert.Equal(CodedOutputStream.ComputeInt32Size(value), codeGenerator.CalculateSize(value));
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }

        [Fact]
        public void Int64_Test()
        {
            var codeGenerator = new Int64CodeGenerator();
            var value = 12413451231241356;
            Assert.Equal(CodedOutputStream.ComputeInt64Size(value), codeGenerator.CalculateSize(value));
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }

        [Fact]
        public void SByte_Test()
        {
            var codeGenerator = new SByteCodeGenerator();
            sbyte value = -87;
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }

        [Fact]
        public void Single_Test()
        {
            var codeGenerator = new SingleCodeGenerator();
            var value = 123f;
            Assert.Equal(CodedOutputStream.ComputeFloatSize(value), codeGenerator.CalculateSize(value));
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }

        [Fact]
        public void String_Test()
        {
            var codeGenerator = new StringCodeGenerator();
            var value = "123aw,";
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }

        [Fact]
        public void TimeSpan_Test()
        {
            var codeGenerator = new TimeSpanCodeGenerator();
            var value = TimeSpan.FromHours(1.23);
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }

        [Fact]
        public void UInt16_Test()
        {
            var codeGenerator = new UInt16CodeGenerator();
            ushort value = 32000;
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }

        [Fact]
        public void UInt32_Test()
        {
            var codeGenerator = new UInt32CodeGenerator();
            uint value = 123142;
            Assert.Equal(CodedOutputStream.ComputeUInt32Size(value), codeGenerator.CalculateSize(value));
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }

        [Fact]
        public void UInt64_Test()
        {
            var codeGenerator = new UInt64CodeGenerator();
            ulong value = 12413451231241356;
            Assert.Equal(CodedOutputStream.ComputeUInt64Size(value), codeGenerator.CalculateSize(value));
            var fieldCodec = codeGenerator.CreateFieldCodec(1);
            var stream = new MemoryStream();
            var output = new CodedOutputStream(stream, true);
            fieldCodec.WriteTagAndValue(output, value);
            output.Flush();
            stream.Position = 0;
            var input = new CodedInputStream(stream);
            Assert.Equal(WireFormat.MakeTag(1, codeGenerator.WireType), input.ReadTag());
            Assert.Equal(value, fieldCodec.Read(input));
        }
    }
}
