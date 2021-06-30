using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Wodsoft.Protobuf.Generators
{
    /// <summary>
    /// Generally object code generator.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectCodeGenerator<T> : NonstandardPrimitiveCodeGenerator<T>
        where T : class, new()
    {
        private static Func<T, int> _ComputeSizeDelegate;

        static ObjectCodeGenerator()
        {
            var messageType = MessageBuilder.GetMessageType<T>();
            var computeSizeMethod = messageType.GetMethod("ComputeSize", BindingFlags.Public | BindingFlags.Static);

            DynamicMethod method = new DynamicMethod("ComputeSize", typeof(int), new Type[] { typeof(T) });
            var ilGenerator = method.GetILGenerator();
            var lengthVariable = ilGenerator.DeclareLocal(typeof(int));
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Call, computeSizeMethod);
            ilGenerator.Emit(OpCodes.Stloc, lengthVariable);
            ilGenerator.Emit(OpCodes.Ldloc, lengthVariable);
            ilGenerator.Emit(OpCodes.Ldloc, lengthVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeLengthSize), BindingFlags.Public | BindingFlags.Static));
            ilGenerator.Emit(OpCodes.Add_Ovf);
            ilGenerator.Emit(OpCodes.Ret);

            _ComputeSizeDelegate = (Func<T, int>)method.CreateDelegate(typeof(Func<T, int>));
        }

        public override WireFormat.WireType WireType => WireFormat.WireType.LengthDelimited;

        public override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable, int fieldNumber)
        {
            var messageType = MessageBuilder.GetMessageType<T>();
            var computeSizeMethod = messageType.GetMethod("ComputeSize", BindingFlags.Public | BindingFlags.Static);

            //var lengthVariable = ilGenerator.DeclareLocal(typeof(int));

            var next = ilGenerator.DefineLabel();
            var end = ilGenerator.DefineLabel();
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Brtrue, next);
            ilGenerator.Emit(OpCodes.Ldc_I4_0);
            ilGenerator.Emit(OpCodes.Br, end);
            ilGenerator.MarkLabel(next);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, computeSizeMethod);
            ilGenerator.Emit(OpCodes.Dup);
            //ilGenerator.Emit(OpCodes.Stloc, lengthVariable);
            //ilGenerator.Emit(OpCodes.Ldloc, lengthVariable);
            //ilGenerator.Emit(OpCodes.Ldloc, lengthVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(CodedOutputStream).GetMethod(nameof(CodedOutputStream.ComputeLengthSize), BindingFlags.Public | BindingFlags.Static));
            ilGenerator.Emit(OpCodes.Add_Ovf);
            ilGenerator.Emit(OpCodes.Ldc_I4, CodedOutputStream.ComputeTagSize(fieldNumber));
            ilGenerator.Emit(OpCodes.Add_Ovf);
            ilGenerator.MarkLabel(end);
        }

        protected override void GenerateCalculateSizeCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
        }

        public override void GenerateReadCode(ILGenerator ilGenerator)
        {
            var messageType = MessageBuilder.GetMessageType<T>();
            var valueVariable = ilGenerator.DeclareLocal(messageType);
            ilGenerator.Emit(OpCodes.Newobj, messageType.GetConstructor(Array.Empty<Type>()));
            ilGenerator.Emit(OpCodes.Stloc, valueVariable);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, typeof(ParseContext).GetMethod(nameof(ParseContext.ReadMessage)));
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Call, messageType.GetProperty("Source").GetMethod);
        }

        protected override int CalculateSize(T value)
        {
            return _ComputeSizeDelegate(value);
        }

        //public override void GenerateWriteCode(ILGenerator ilGenerator, LocalBuilder valueVariable, int fieldNumber)
        //{
        //    var end = ilGenerator.DefineLabel();
        //    ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
        //    ilGenerator.Emit(OpCodes.Brfalse, end);
        //    base.GenerateWriteCode(ilGenerator, valueVariable, fieldNumber);
        //    ilGenerator.MarkLabel(end);
        //}

        protected override void GenerateWriteValueCode(ILGenerator ilGenerator, LocalBuilder valueVariable)
        {
            var messageType = MessageBuilder.GetMessageType<T>();
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldloc, valueVariable);
            ilGenerator.Emit(OpCodes.Newobj, messageType.GetConstructor(new Type[] { typeof(T) }));
            ilGenerator.Emit(OpCodes.Call, typeof(WriteContext).GetMethod(nameof(WriteContext.WriteMessage)));
        }

        protected override T ReadValue(ref ParseContext parser)
        {
            Message<T> message = (Message<T>)Activator.CreateInstance(MessageBuilder.GetMessageType<T>());
            parser.ReadMessage(message);
            return message.Source;
        }

        protected override void WriteValue(ref WriteContext writer, T value)
        {
            Message<T> message = value;
            writer.WriteMessage(message);
        }
    }
}
