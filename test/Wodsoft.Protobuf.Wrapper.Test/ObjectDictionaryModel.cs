using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Wrapper.Test
{
    public class ObjectDictionaryModel
    {
        public IDictionary<int, UserModel> Users { get; set; }

        public IDictionary<int, int> Test { get; set; }
    }
}
