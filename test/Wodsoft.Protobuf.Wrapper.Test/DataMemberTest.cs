using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Xunit;

namespace Wodsoft.Protobuf.Wrapper.Test
{
    public class DataMemberTest
    {
        [Fact]
        public void OrderTest()
        {
            var fields = GeneralMessageFieldProvider.Instance.GetFields(typeof(DataMemberModel));
            Assert.Equal(nameof(DataMemberModel.Argument2), fields.Last().FieldName);
        }

        [Fact]
        public void ConflictTest()
        {
            Assert.Throws<InvalidDataContractException>(() => GeneralMessageFieldProvider.Instance.GetFields(typeof(BadDataMemberModel)));
        }
    }
}
