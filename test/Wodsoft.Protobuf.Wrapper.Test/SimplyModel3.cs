using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Wrapper.Test
{
#if NET8_0_OR_GREATER
    public class SimplyModel3
    {
        public DateTimeOffset? DateTimeOffsetValue { get; set; }

        public DateOnly? DateOnlyValue { get; set; }

        public PointModel? PointValue { get; set; }
    }
#endif
}
