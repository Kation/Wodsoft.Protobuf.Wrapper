using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Extensions.Test
{
    public class EnumModel
    {
        public ByteEnum ByteEnum { get; set; }

        public SByteEnum SByteEnum { get; set; }

        public Int16Enum Int16Enum { get; set; }

        public Int32Enum Int32Enum { get; set; }

        public Int64Enum Int64Enum { get; set; }

        public UInt16Enum UInt16Enum { get; set; }

        public UInt32Enum UInt32Enum { get; set; }

        public UInt64Enum UInt64Enum { get; set; }
    }

    public enum ByteEnum : byte
    {
        Normal = 0,
        One = 1,
        Two = 2
    }

    public enum SByteEnum : sbyte
    {
        Disabled = -1,
        Normal = 0,
        One = 1,
        Two = 2
    }

    public enum Int16Enum : short
    {
        Disabled = -1,
        Normal = 0,
        One = 1,
        Two = 2
    }

    public enum UInt16Enum : ushort
    {
        Normal = 0,
        One = 1,
        Two = 2
    }

    public enum Int32Enum : int
    {
        Disabled = -1,
        Normal = 0,
        One = 1,
        Two = 2
    }

    public enum UInt32Enum : uint
    {
        Normal = 0,
        One = 1,
        Two = 2,
        Three = uint.MaxValue - 2
    }

    public enum Int64Enum : long
    {
        Disabled = -1,
        Normal = 0,
        One = 1,
        Two = 2,
        Three = long.MaxValue - 5
    }

    public enum UInt64Enum : ulong
    {
        Normal = 0,
        One = 1,
        Two = 2,
        Three = ulong.MaxValue - 10
    }
}
