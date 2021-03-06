﻿using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Wodsoft.Protobuf.Wrapper.Test
{
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
    }
}
