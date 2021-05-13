using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Wodsoft.Protobuf.Wrapper.Test
{
    public class GenericTest
    {
        [Fact]
        public void Convert_Test()
        {
            GenericModel<string> model1 = new GenericModel<string> { Items1 = "test1" };
            GenericModel<string, string> model2 = new GenericModel<string, string> { Items1 = "test1", Item2 = "test2" };
            GenericModel<int> model3 = new GenericModel<int> { Items1 = 1 };

            Message<GenericModel<string>> message1 = model1;
            Message<GenericModel<string, string>> message2 = model2;
            Message<GenericModel<int>> message3 = model3;
        }
    }
}
