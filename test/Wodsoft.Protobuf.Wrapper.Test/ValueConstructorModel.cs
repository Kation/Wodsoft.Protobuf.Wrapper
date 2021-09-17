using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Wrapper.Test
{
    public class ValueConstructorModel
    {
        public ValueConstructorModel(string value)
        {
            Value = value;
        }

        public string Value { get; set; }
    }
}
