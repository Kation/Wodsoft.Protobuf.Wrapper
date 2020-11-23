using Google.Protobuf;
using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wodsoft.Protobuf
{
    public static class MessageHelper
    {
        #region Collection

        public static IEnumerable<int> ConvertToInt(IEnumerable<byte> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Select(t => (int)t);
        }

        public static IEnumerable<int> ConvertToInt(IEnumerable<sbyte> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Select(t => (int)t);
        }

        public static IEnumerable<int> ConvertToInt(IEnumerable<short> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Select(t => (int)t);
        }

        public static IEnumerable<int> ConvertToInt(IEnumerable<ushort> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Select(t => (int)t);
        }

        public static IEnumerable<int?> ConvertToInt(IEnumerable<byte?> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Select(t => (int?)t);
        }

        public static IEnumerable<int?> ConvertToInt(IEnumerable<sbyte?> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Select(t => (int?)t);
        }

        public static IEnumerable<int?> ConvertToInt(IEnumerable<short?> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Select(t => (int?)t);
        }

        public static IEnumerable<int?> ConvertToInt(IEnumerable<ushort?> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Select(t => (int?)t);
        }

        public static IEnumerable<byte> ConvertToByte(RepeatedField<int> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Select(t => (byte)t);
        }

        public static IEnumerable<sbyte> ConvertToSByte(RepeatedField<int> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Select(t => (sbyte)t);
        }

        public static IEnumerable<short> ConvertToInt16(RepeatedField<int> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Select(t => (short)t);
        }

        public static IEnumerable<ushort> ConvertToUInt16(RepeatedField<int> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Select(t => (ushort)t);
        }

        public static IEnumerable<byte?> ConvertToByte(RepeatedField<int?> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Select(t => (byte?)t);
        }

        public static IEnumerable<sbyte?> ConvertToSByte(RepeatedField<int?> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Select(t => (sbyte?)t);
        }

        public static IEnumerable<short?> ConvertToInt16(RepeatedField<int?> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Select(t => (short?)t);
        }

        public static IEnumerable<ushort?> ConvertToUInt16(RepeatedField<int?> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return source.Select(t => (ushort?)t);
        }

        #endregion

        #region Dictionary

        public static Dictionary<TKey, TValue> ConvertDictionary<TKey, TValue>(IDictionary<int, TValue> source)
            where TKey : struct
        {
            return source.ToDictionary(t => (TKey)Convert.ChangeType(t.Key, typeof(TKey)), t => t.Value);
        }

        public static Dictionary<TKey, TValue> ConvertDictionary<TKey, TValue>(IDictionary<int, int> source)
            where TKey : struct
            where TValue : struct
        {
            return source.ToDictionary(t => (TKey)Convert.ChangeType(t.Key, typeof(TKey)), t => (TValue)Convert.ChangeType(t.Value, typeof(TValue)));
        }

        public static Dictionary<TKey, TValue> ConvertDictionary<TKey, TValue>(IDictionary<TKey, int> source)
            where TValue : struct
        {
            return source.ToDictionary(t => t.Key, t => (TValue)Convert.ChangeType(t.Value, typeof(TValue)));
        }

        public static Dictionary<TKey?, TValue> ConvertDictionary<TKey, TValue>(IDictionary<int?, TValue> source)
            where TKey : struct
        {
            return source.ToDictionary(t => new TKey?((TKey)Convert.ChangeType(t.Key, typeof(TKey))), t => t.Value);
        }

        public static Dictionary<TKey?, TValue?> ConvertDictionary<TKey, TValue>(IDictionary<int?, int?> source)
            where TKey : struct
            where TValue : struct
        {
            return source.ToDictionary(t => new TKey?((TKey)Convert.ChangeType(t.Key, typeof(TKey))), t => new TValue?((TValue)Convert.ChangeType(t.Value, typeof(TValue))));
        }

        public static Dictionary<TKey, TValue?> ConvertDictionary<TKey, TValue>(IDictionary<TKey, int?> source)
            where TValue : struct
        {
            return source.ToDictionary(t => t.Key, t => new TValue?((TValue)Convert.ChangeType(t.Value.GetValueOrDefault(), typeof(TValue))));
        }

        #endregion
    }
}
