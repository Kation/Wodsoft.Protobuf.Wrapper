using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Wodsoft.Protobuf.Extensions.Test
{
    public class SerializationTest
    {
        [Fact]
        public void Simply_Class_Test()
        {
            var type = MessageBuilder.GetMessageType<SimplyModel>();

            //var generator = new Lokad.ILPack.AssemblyGenerator();
            //// for ad-hoc serialization
            //var bytes = generator.GenerateAssemblyBytes(DomainGrpcTypeBuilder.GetAssembly());
            //File.WriteAllBytes("dynamic.dll", bytes);

            SimplyModel model = new SimplyModel
            {
                IntValue = 100,
                DoubleValue = 222.222,
                StringValue = "This is a simply object"
            };

            MemoryStream stream = new MemoryStream();
            Message<SimplyModel>.Serialize(stream, model);

            stream.Position = 0;
            var model2 = Message<SimplyModel>.Deserialize(stream);

            Assert.Equal(model2.IntValue, model.IntValue);
            Assert.Equal(model2.DoubleValue, model.DoubleValue);
            Assert.Equal(model2.StringValue, model.StringValue);
        }
    }
}
