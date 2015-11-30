using System;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Zhenway.YamlSerializations
{
    internal sealed class ScalarYamlNodeTypeResolver : INodeTypeResolver
    {
        bool INodeTypeResolver.Resolve(NodeEvent nodeEvent, ref Type currentType)
        {
            if (currentType == typeof(string) || currentType == typeof(object))
            {
                var scalar = nodeEvent as Scalar;
                if (scalar != null && scalar.IsPlainImplicit)
                {
                    if (Regexes.BooleanLike.IsMatch(scalar.Value))
                    {
                        currentType = typeof(bool);
                        return true;
                    }

                    if (Regexes.IntegerLike.IsMatch(scalar.Value))
                    {
                        currentType = typeof(int);
                        return true;
                    }

                    if (Regexes.DoubleLike.IsMatch(scalar.Value))
                    {
                        currentType = typeof(double);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
