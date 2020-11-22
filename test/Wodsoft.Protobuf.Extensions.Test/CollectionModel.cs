using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Wodsoft.Protobuf.Extensions.Test
{
    public class CollectionModel
    {
        public IList<string> StringList { get; set; }

        public List<string> StringList2 { get; set; }

        public IList<int> IntList { get; set; }

        public IList<int?> NullableIntList { get; set; }

        public IList<byte> ByteList { get; set; }

        public IList<byte?> NullableByteList { get; set; }

        public ICollection<sbyte> SByteCollection { get; set; }

        public Collection<sbyte> SByteCollection2 { get; set; }

        public ICollection<short?> NullableShortCollection { get; set; }

        public Collection<short?> NullableShortCollection2 { get; set; }

        public ushort[] UShortArray { get; set; }

        public ushort?[] NullableUShortArray { get; set; }

        public long[] LongArray { get; set; }

        public long?[] NullableLongArray { get; set; }
    }
}
