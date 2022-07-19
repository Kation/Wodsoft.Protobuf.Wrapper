using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Wrapper.Test
{
    public class DictionaryModel
    {
        public IDictionary<string, string> StringMap { get; set; }

        public Dictionary<string, string> StringMap2 { get; set; }

        public Dictionary<byte, string> ByteMap { get; set; }

        public Dictionary<string, short?> ShortMap { get; set; }

        public Dictionary<string, string[]> StringArrayMap { get; set; }

        public Dictionary<string, List<string>> StringListMap { get; set; }
    }
}
