using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Wodsoft.Protobuf.Wrapper.Test
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
            MessageBuilder.GetMessageType<MessageModel>();
            
            var generator = new Lokad.ILPack.AssemblyGenerator();
            var bytes = generator.GenerateAssemblyBytes(MessageBuilder.GetAssembly());
            File.WriteAllBytes("dynamic.dll", bytes);
        }
    }
}
