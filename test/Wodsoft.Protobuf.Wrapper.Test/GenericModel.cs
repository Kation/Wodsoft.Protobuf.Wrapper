using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Wrapper.Test
{
    public class GenericModel<T1>
    {
        public T1 Items1 { get; set; }
    }

    public class GenericModel<T1, T2> : GenericModel<T1>
    {
        public T2 Item2 { get; set; }
    }
}
