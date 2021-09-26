using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Wodsoft.Protobuf.Wrapper.Test
{
    public class DataMemberModel
    {
        [DataMember(Order = 2)]
        public Guid Argument2 { get; set; }

        [DataMember(Order = 1)]
        public Guid Argument1 { get; set; }
    }
}
