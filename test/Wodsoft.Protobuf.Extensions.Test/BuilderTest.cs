using System;
using System.Collections.Generic;
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
            MessageBuilder.GetMessageType<DictionaryModel>();
            MessageBuilder.GetMessageType<ContentModel>();

            var generator = new Lokad.ILPack.AssemblyGenerator();
            var bytes = generator.GenerateAssemblyBytes(MessageBuilder.GetAssembly());
            File.WriteAllBytes("dynamic.dll", bytes);
        }

        public void Test(ref Google.Protobuf.ParseContext ctx)
        {
            Message<ContentModel> message = new ContentModel();
            ctx.ReadMessage(message);
        }
    }


}
