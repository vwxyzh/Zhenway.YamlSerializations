using System;
using System.Collections.Generic;
using System.IO;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.NodeTypeResolvers;
using YamlDotNet.Serialization.ObjectFactories;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;
using YamlDotNet.Serialization.Utilities;
using YamlDotNet.Serialization.ValueDeserializers;

namespace Zhenway.YamlSerializations
{
    /// <summary>
    /// A fa�ade for the YAML library with the standard configuration.
    /// </summary>
    public sealed class YamlDeserializer
    {
        private static readonly Dictionary<string, Type> predefinedTagMappings = new Dictionary<string, Type>
        {
            { "tag:yaml.org,2002:map", typeof(Dictionary<object, object>) },
            { "tag:yaml.org,2002:bool", typeof(bool) },
            { "tag:yaml.org,2002:float", typeof(double) },
            { "tag:yaml.org,2002:int", typeof(int) },
            { "tag:yaml.org,2002:str", typeof(string) },
            { "tag:yaml.org,2002:timestamp", typeof(DateTime) },
        };

        private readonly Dictionary<string, Type> tagMappings;
        private readonly List<IYamlTypeConverter> converters;
        private TypeDescriptorProxy typeDescriptor = new TypeDescriptorProxy();
        private IValueDeserializer valueDeserializer;

        public IList<INodeDeserializer> NodeDeserializers { get; private set; }
        public IList<INodeTypeResolver> TypeResolvers { get; private set; }

        private class TypeDescriptorProxy : ITypeInspector
        {
            public ITypeInspector TypeDescriptor;

            public IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
            {
                return TypeDescriptor.GetProperties(type, container);
            }

            public IPropertyDescriptor GetProperty(Type type, object container, string name, bool ignoreUnmatched)
            {
                return TypeDescriptor.GetProperty(type, container, name, ignoreUnmatched);
            }
        }

        public YamlDeserializer(
            IObjectFactory objectFactory = null,
            INamingConvention namingConvention = null,
            bool ignoreUnmatched = false)
        {
            objectFactory = objectFactory ?? new DefaultEmitObjectFactory();
            namingConvention = namingConvention ?? new NullNamingConvention();

            typeDescriptor.TypeDescriptor =
                new CachedTypeInspector(
                    new YamlAttributesTypeInspector(
                        new NamingConventionTypeInspector(
                            new ReadableAndWritablePropertiesTypeInspector(
                                new EmitTypeInspector(
                                    new StaticTypeResolver()
                                )
                            ),
                            namingConvention
                        )
                    )
                );

            converters = new List<IYamlTypeConverter>();
            foreach (IYamlTypeConverter yamlTypeConverter in YamlTypeConverters.BuiltInConverters)
            {
                converters.Add(yamlTypeConverter);
            }

            NodeDeserializers = new List<INodeDeserializer>();
            NodeDeserializers.Add(new TypeConverterNodeDeserializer(converters));
            NodeDeserializers.Add(new NullNodeDeserializer());
            NodeDeserializers.Add(new ScalarNodeDeserializer());
            NodeDeserializers.Add(new EmitArrayNodeDeserializer());
            NodeDeserializers.Add(new GenericDictionaryNodeDeserializer(objectFactory));
            NodeDeserializers.Add(new NonGenericDictionaryNodeDeserializer(objectFactory));
            NodeDeserializers.Add(new EmitGenericCollectionNodeDeserializer(objectFactory));
            NodeDeserializers.Add(new NonGenericListNodeDeserializer(objectFactory));
            NodeDeserializers.Add(new EnumerableNodeDeserializer());
            NodeDeserializers.Add(new ObjectNodeDeserializer(objectFactory, typeDescriptor, ignoreUnmatched));

            tagMappings = new Dictionary<string, Type>(predefinedTagMappings);
            TypeResolvers = new List<INodeTypeResolver>();
            TypeResolvers.Add(new TagNodeTypeResolver(tagMappings));
            TypeResolvers.Add(new TypeNameInTagNodeTypeResolver());
            TypeResolvers.Add(new DefaultContainersNodeTypeResolver());
            TypeResolvers.Add(new ScalarYamlNodeTypeResolver());

            valueDeserializer =
                new AliasValueDeserializer(
                    new NodeValueDeserializer(
                        NodeDeserializers,
                        TypeResolvers
                    )
                );
        }

        public void RegisterTagMapping(string tag, Type type)
        {
            tagMappings.Add(tag, type);
        }

        public void RegisterTypeConverter(IYamlTypeConverter typeConverter)
        {
            converters.Add(typeConverter);
        }

        public T Deserialize<T>(TextReader input)
        {
            return (T)Deserialize(input, typeof(T));
        }

        public object Deserialize(TextReader input)
        {
            return Deserialize(input, typeof(object));
        }

        public object Deserialize(TextReader input, Type type)
        {
            return Deserialize(new EventReader(new Parser(input)), type);
        }

        public T Deserialize<T>(EventReader reader)
        {
            return (T)Deserialize(reader, typeof(T));
        }

        public object Deserialize(EventReader reader)
        {
            return Deserialize(reader, typeof(object));
        }

        /// <summary>
        /// Deserializes an object of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="EventReader" /> where to deserialize the object.</param>
        /// <param name="type">The static type of the object to deserialize.</param>
        /// <returns>Returns the deserialized object.</returns>
        public object Deserialize(EventReader reader, Type type)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var hasStreamStart = reader.Allow<StreamStart>() != null;

            var hasDocumentStart = reader.Allow<DocumentStart>() != null;

            object result = null;
            if (!reader.Accept<DocumentEnd>() && !reader.Accept<StreamEnd>())
            {
                using (var state = new SerializerState())
                {
                    result = valueDeserializer.DeserializeValue(reader, type, state, valueDeserializer);
                    state.OnDeserialization();
                }
            }

            if (hasDocumentStart)
            {
                reader.Expect<DocumentEnd>();
            }

            if (hasStreamStart)
            {
                reader.Expect<StreamEnd>();
            }

            return result;
        }
    }
}