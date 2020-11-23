using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Extensions.Test
{
    public class DictionaryModel
    {
        public IDictionary<string,string> StringMap { get; set; }

        public Dictionary<string,string> StringMap2 { get; set; }

        public Dictionary<byte, string> ByteMap { get; set; }

        public Dictionary<string, short?> ShortMap { get; set; }
    }
}
