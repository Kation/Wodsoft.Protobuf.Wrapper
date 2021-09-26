using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Wodsoft.Protobuf.Wrapper.Test
{
    public class DataMemberTest
    {
        [Fact]
        public void Test()
        {
            var field = GeneralMessageFieldProvider.Instance.GetFields(typeof(DataMemberModel));
        }
    }
}
