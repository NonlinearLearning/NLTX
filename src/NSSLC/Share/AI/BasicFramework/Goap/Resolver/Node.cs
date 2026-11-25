// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
using System;
using System.Collections.Generic;
using Terraria.Share.Ai.Goap.Core;

namespace Terraria.Share.Ai.Goap.Resolver
{
    public class Node : INode
    {
        public Guid Guid { get; } = Guid.NewGuid();

        public IConnectable Action { get; set; }

        public List<INodeEffect> Effects { get; set; } = new();
        public List<INodeCondition> Conditions { get; set; } = new();

        public bool IsRootNode => this.Action is IGoal;

        public void GetActions(List<IGoapAction> actions)
        {
            if (actions.Contains(this.Action as IGoapAction))
                return;

            if (this.Action is IGoapAction goapAction)
                actions.Add(goapAction);

            foreach (var condition in this.Conditions)
            {
                foreach (var connection in condition.Connections)
                {
                    connection.GetActions(actions);
                }
            }
        }
    }
}
