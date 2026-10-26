// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Goap.Resolver
{
    internal static class Extensions
    {
        public static (INode[] RootNodes, INode[] ChildNodes) ToNodes(this IEnumerable<IConnectable> actions)
        {
            var mappedNodes = actions.Select(ToNode).ToArray();

            return (
                mappedNodes.Where(x => x.IsRootNode).ToArray(),
                mappedNodes.Where(x => !x.IsRootNode).ToArray()
            );
        }

        private static INode ToNode(IConnectable action)
        {
            return new Node
            {
                Action = action,
                Conditions = action.Conditions?.Select(y => new NodeCondition
                {
                    Condition = y,
                }).Cast<INodeCondition>().ToList() ?? new List<INodeCondition>(),
                Effects = action.Effects?.Select(y => new NodeEffect
                {
                    Effect = y,
                }).Cast<INodeEffect>().ToList() ?? new List<INodeEffect>(),
            };
        }

        public static string GetGenericTypeName(this Type type)
        {
            var typeName = type.Name;

            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments();
                var genericTypeName = typeName.Substring(0, typeName.IndexOf('`'));
                var typeArgumentNames = string.Join(",", genericArguments.Select(a => a.GetGenericTypeName()));
                typeName = $"{genericTypeName}<{typeArgumentNames}>";
            }

            return typeName;
        }
    }
}
