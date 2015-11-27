﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Zhenway.YamlSerializations
{
    public class EmitGenericDictionaryNodeDeserializer : INodeDeserializer
    {
        private static readonly MethodInfo DeserializeHelperMethod = typeof(EmitGenericDictionaryNodeDeserializer).GetMethod(nameof(DeserializeHelper));
        private readonly IObjectFactory _objectFactory;
        private readonly Dictionary<Type, Type[]> _gpCache =
            new Dictionary<Type, Type[]>();
        private readonly Dictionary<Tuple<Type, Type>, Action<EventReader, Type, Func<EventReader, Type, object>, object>> _actionCache =
            new Dictionary<Tuple<Type, Type>, Action<EventReader, Type, Func<EventReader, Type, object>, object>>();

        public EmitGenericDictionaryNodeDeserializer(IObjectFactory objectFactory)
        {
            _objectFactory = objectFactory;
        }

        bool INodeDeserializer.Deserialize(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, out object value)
        {
            Type[] gp;
            if (!_gpCache.TryGetValue(expectedType, out gp))
            {
                var dictionaryType = ReflectionUtility.GetImplementedGenericInterface(expectedType, typeof(IDictionary<,>));
                if (dictionaryType != null)
                {
                    gp = dictionaryType.GetGenericArguments();
                }
                _gpCache[expectedType] = gp;
            }

            if (gp == null)
            {
                value = false;
                return false;
            }

            reader.Expect<MappingStart>();

            value = _objectFactory.Create(expectedType);
            Action<EventReader, Type, Func<EventReader, Type, object>, object> action;
            var cacheKey = Tuple.Create(gp[0], gp[1]);
            if (!_actionCache.TryGetValue(cacheKey, out action))
            {
                var dm = new DynamicMethod(string.Empty, typeof(void), new[] { typeof(EventReader), typeof(Type), typeof(Func<EventReader, Type, object>), typeof(object) });
                var il = dm.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldarg_3);
                il.Emit(OpCodes.Castclass, typeof(IDictionary<,>).MakeGenericType(gp));
                il.Emit(OpCodes.Call, DeserializeHelperMethod.MakeGenericMethod(gp));
                il.Emit(OpCodes.Ret);
                action = (Action<EventReader, Type, Func<EventReader, Type, object>, object>)dm.CreateDelegate(typeof(Action<EventReader, Type, Func<EventReader, Type, object>, object>));
                _actionCache[cacheKey] = action;
            }
            action(reader, expectedType, nestedObjectDeserializer, value);

            reader.Expect<MappingEnd>();

            return true;
        }

        public static void DeserializeHelper<TKey, TValue>(EventReader reader, Type expectedType, Func<EventReader, Type, object> nestedObjectDeserializer, IDictionary<TKey, TValue> result)
        {
            while (!reader.Accept<MappingEnd>())
            {
                var key = nestedObjectDeserializer(reader, typeof(TKey));
                var keyPromise = key as IValuePromise;

                var value = nestedObjectDeserializer(reader, typeof(TValue));
                var valuePromise = value as IValuePromise;

                if (keyPromise == null)
                {
                    if (valuePromise == null)
                    {
                        // Happy path: both key and value are known
                        result[(TKey)key] = (TValue)value;
                    }
                    else
                    {
                        // Key is known, value is pending
                        valuePromise.ValueAvailable += v => result[(TKey)key] = (TValue)v;
                    }
                }
                else
                {
                    if (valuePromise == null)
                    {
                        // Key is pending, value is known
                        keyPromise.ValueAvailable += v => result[(TKey)v] = (TValue)value;
                    }
                    else
                    {
                        // Both key and value are pending. We need to wait until both of them becom available.
                        var hasFirstPart = false;

                        keyPromise.ValueAvailable += v =>
                        {
                            if (hasFirstPart)
                            {
                                result[(TKey)v] = (TValue)value;
                            }
                            else
                            {
                                key = v;
                                hasFirstPart = true;
                            }
                        };

                        valuePromise.ValueAvailable += v =>
                        {
                            if (hasFirstPart)
                            {
                                result[(TKey)key] = (TValue)v;
                            }
                            else
                            {
                                value = v;
                                hasFirstPart = true;
                            }
                        };
                    }
                }
            }
        }
    }
}
