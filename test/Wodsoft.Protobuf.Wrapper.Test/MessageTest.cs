using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Wodsoft.Protobuf.Wrapper.Test
{
    [Collection("MessageTest")]
    public class MessageTest
    {
        [Fact]
        public void Convert_Test()
        {
            SimplyModel model = new SimplyModel
            {
                IntValue = 100,
                DoubleValue = 222.222,
                StringValue = "This is a simply model"
            };

            Message<SimplyModel> message = model;

            Assert.Equal(model, message.Source);

            SimplyModel model2 = message;

            Assert.Equal(model, model2);

            ((IMessage)message).CalculateSize();
        }

        [Fact]
        public void New_Test()
        {
            Message.New<string>();
            Message.New<int>();
            Message.New<UserModel>();
            Assert.Throws<NotSupportedException>(() => Message.New<ValueConstructorModel>());
            MessageBuilder.SetTypeInitializer(() => new ValueConstructorModel(string.Empty));
            Message.New<ValueConstructorModel>();
            Message.New<string[]>();
            Message.New<List<string>>();
            Message.New<List<ValueConstructorModel>>();
        }
    }
}