﻿using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wodsoft.Protobuf.Primitives
{
    /// <summary>
    /// Enum message.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class EnumMessage<TEnum, TValue> : Message<TEnum>
        where TEnum : Enum
        where TValue : struct
    {
        private static readonly IStructureCodeGenerator<TValue> _CodeGenerator = (IStructureCodeGenerator<TValue>)MessageBuilder.GetCodeGenerator<TValue>();

        /// <summary>
        /// Initialize nullable message wrapper.
        /// </summary>
        public EnumMessage() : base(default) { }

        /// <summary>
        /// Initialize nullable message wrapper with a value.
        /// </summary>
        /// <param name="value">Value.</param>
        public EnumMessage(TEnum value) : base(value) { }

        /// <inheritdoc/>
        protected override int CalculateSize()
        {
            return _CodeGenerator.CalculateSize((TValue)Convert.ChangeType(SourceValue, typeof(TValue)));
        }

        /// <inheritdoc/>
        protected override void Read(ref ParseContext parser)
        {
            SourceValue = (TEnum)Enum.ToObject(typeof(TEnum), _CodeGenerator.ReadValue(ref parser));
        }

        /// <inheritdoc/>
        protected override void Write(ref WriteContext writer)
        {
            _CodeGenerator.WriteValue(ref writer, (TValue)Convert.ChangeType(SourceValue, typeof(TValue)));
        }

        /// <summary>
        /// Compute value size.
        /// </summary>
        /// <param name="value">value.</param>
        /// <returns></returns>
        public static int ComputeSize(TEnum value)
        {
            return _CodeGenerator.CalculateSize((TValue)Convert.ChangeType(value, typeof(TValue)));
        }
    }
}
