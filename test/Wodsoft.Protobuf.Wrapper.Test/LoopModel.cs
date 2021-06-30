using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Wrapper.Test
{
    public class LoopModel
    {
        public string Name { get; set; }

        public LoopModel Inner { get; set; }
    }
}
