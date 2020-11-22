using System;
using System.IO;
using Xunit;

namespace Wodsoft.Protobuf.Extensions.Test
{
    public class BuilderTest
    {
        [Fact]
        public void Create_Simply_Type_Test()
        {
            MessageBuilder.GetMessageType<SimplyModel>();
            MessageBuilder.GetMessageType<EnumModel>();
            MessageBuilder.GetMessageType<CollectionModel>();

            var generator = new Lokad.ILPack.AssemblyGenerator();
            var bytes = generator.GenerateAssemblyBytes(MessageBuilder.GetAssembly());
            File.WriteAllBytes("dynamic.dll", bytes);
        }
    }
}
