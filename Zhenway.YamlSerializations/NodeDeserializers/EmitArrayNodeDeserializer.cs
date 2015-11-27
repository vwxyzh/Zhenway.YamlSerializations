using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Zhenway.YamlSerializations
{
    public class EmitArrayNodeDeserializer : INodeDeserializer
    {
        private static MethodInfo DeserializeHelperMethod =
            typeof(EmitArrayNodeDeserializer).GetMethod(nameof(DeserializeHelper));
        private static readonly Dictionary<Type, Func<EventReader, Type, Func<EventReader, Type, object>, object>> _funcCache =
            new Dictionary<Type, Func<EventReader, Type, Func<EventReader, Type, object>, object>>();

        bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
        {
            if (!expectedType.IsArray)
            {
                value = false;
                return false;
            }

            Func<EventReader, Type, Func<EventReader, Type, object>, object> func;
            if (!_funcCache.TryGetValue(expectedType, out func))
            {
                var dm = new DynamicMethod(string.Empty, typeof(object), new[] { typeof(EventReader), typeof(Type), typeof(Func<EventReader, Type, object>) });
                var il = dm.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Call, DeserializeHelperMethod.MakeGenericMethod(expectedType.GetElementType()));
                il.Emit(OpCodes.Ret);
                func = (Func<EventReader, Type, Func<EventReader, Type, object>, object>)dm.CreateDelegate(typeof(Func<EventReader, Type, Func<EventReader, Type, object>, object>));
            }
            value = func(reader, expectedType, nestedObjectDeserializer);
            return true;
        }

        public static TItem[] DeserializeHelper<TItem>(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer)
        {
            var items = new List<TItem>();
            EmitGenericCollectionNodeDeserializer.DeserializeHelper(reader, expectedType, nestedObjectDeserializer, items);
            return items.ToArray();
        }

    }
}
