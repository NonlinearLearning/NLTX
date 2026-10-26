// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: synchronous .NET collections and System.Numerics; see SOURCES.md.
#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Terraria.Ai.Goap.Resolver
{
    public struct GraphResolverJob
    {
        // Graph specific
// Dictionary<ActionIndex, ConditionIndex[]>
        public ILookup<int, int> NodeConditions;

        // Dictionary<ConditionIndex, NodeIndex[]>
        public ILookup<int, int> ConditionConnections;


        // Resolve specific
        public RunData RunData;

        // Results
        public List<NodeData> Result;
        public List<NodeData> PickedGoal;

        public static readonly Vector3 InvalidPosition = new(float.MaxValue, float.MaxValue, float.MaxValue);
        public void Execute()
        {
            var nodeCount = this.NodeConditions.Count;
            var runData = this.RunData;

            var openSet = new Dictionary<int, NodeData>(nodeCount);
            var closedSet = new Dictionary<int, NodeData>(nodeCount);

            // Add each start node's (goal) connections to the open set
            foreach (var i in runData.StartIndex)
            {
                var nodeData = new NodeData
                {
                    Index = i,
                    G = this.RunData.Costs[i],
                    P = this.RunData.Costs[i],
                    H = int.MaxValue,
                    ParentIndex = -1,
                    Position = InvalidPosition,
                };

                // We're assuming the start node is always a goal, and as such not executable
                closedSet.TryAdd(nodeData.Index, nodeData);

                this.AddConnections(this.RunData, ref openSet, ref closedSet, nodeData);
            }

            while (openSet.Count != 0)
            {
                var openList = openSet.Values.ToList();
                openList.Sort(new NodeSorter());

                var currentNode = openList[0];

                if (runData.IsExecutable[currentNode.Index])
                {
                    this.RetracePath(currentNode, closedSet, this.Result);
                    break;
                }

                closedSet.TryAdd(currentNode.Index, currentNode);
                openSet.Remove(currentNode.Index);

                // If this node has a condition that is false and has no connections, it is unresolvable
                if (this.HasUnresolvableCondition(currentNode.Index))
                {
                    continue;
                }

                this.AddConnections(this.RunData, ref openSet, ref closedSet, currentNode);
            }
        }

        private void AddConnections(
            RunData runData, ref Dictionary<int, NodeData> openSet,
            ref Dictionary<int, NodeData> closedSet, NodeData currentNode
        )
        {
            foreach (var conditionIndex in this.NodeConditions[currentNode.Index])
            {
                if (runData.ConditionsMet[conditionIndex])
                {
                    continue;
                }

                foreach (var neighborIndex in this.ConditionConnections[conditionIndex])
                {
                    if (closedSet.ContainsKey(neighborIndex))
                    {
                        continue;
                    }

                    if (!runData.IsEnabled[neighborIndex])
                    {
                        continue;
                    }

                    var neighborPosition = this.GetPosition(currentNode, neighborIndex);

                    // The cost with distance from the current node to the neighbour node
                    var newParentG = this.GetNewCost(currentNode, neighborIndex, neighborPosition);
                    // The cost with distance from the agent to the neighbour node
                    var newG = newParentG + this.GetDistanceCost(runData.AgentPosition, neighborPosition);
                    NodeData neighbor;

                    // Current neighbour is not in the open set
                    if (!openSet.TryGetValue(neighborIndex, out neighbor))
                    {
                        neighbor = new NodeData
                        {
                            Index = neighborIndex,
                            P = newParentG,
                            G = newG,
                            H = this.GetHeuristic(neighborIndex),
                            ParentIndex = currentNode.Index,
                            Position = neighborPosition,
                        };
                        openSet.Add(neighborIndex, neighbor);
                        continue;
                    }

                    // This neighbour has a lower cost
                    if (newG < neighbor.G)
                    {
                        neighbor.G = newG;
                        neighbor.P = newParentG;
                        neighbor.ParentIndex = currentNode.Index;
                        neighbor.Position = neighborPosition;

                        openSet.Remove(neighborIndex);
                        openSet.Add(neighborIndex, neighbor);
                    }
                }
            }
        }

        private float GetNewCost(NodeData currentNode, int neighborIndex, Vector3 neighborPosition)
        {
            return currentNode.P + this.RunData.Costs[neighborIndex] +
                   this.GetDistanceCost(currentNode, neighborPosition);
        }

        private float GetHeuristic(int neighborIndex)
        {
            return this.UnmetConditionCost(neighborIndex);
        }

        private float GetDistanceCost(NodeData previousNode, Vector3 currentPosition)
        {
            return this.GetDistanceCost(previousNode.Position, currentPosition);
        }

        private float GetDistanceCost(Vector3 previousPosition, Vector3 currentPosition)
        {
            if (previousPosition.Equals(InvalidPosition) || currentPosition.Equals(InvalidPosition))
            {
                return 0f;
            }

            return Vector3.Distance(previousPosition, currentPosition) * this.RunData.DistanceMultiplier;
        }

        private Vector3 GetPosition(NodeData currentNode, int currentIndex)
        {
            var pos = this.RunData.Positions[currentIndex];

            if (pos.Equals(InvalidPosition))
                return currentNode.Position;

            return pos;
        }

        private void RetracePath(NodeData startNode, Dictionary<int, NodeData> closedSet, List<NodeData> path)
        {
            var currentNode = startNode;
            while (currentNode.ParentIndex != -1)
            {
                path.Add(currentNode);
                currentNode = closedSet[currentNode.ParentIndex];
            }

            this.PickedGoal.Add(currentNode);
        }

        private bool HasUnresolvableCondition(int currentIndex)
        {
            foreach (var conditionIndex in this.NodeConditions[currentIndex])
            {
                if (this.RunData.ConditionsMet[conditionIndex])
                {
                    continue;
                }

                if (!this.ConditionConnections[conditionIndex].Any())
                {
                    return true;
                }
            }

            return false;
        }

        private float UnmetConditionCost(int currentIndex)
        {
            var cost = 0f;
            foreach (var conditionIndex in this.NodeConditions[currentIndex])
            {
                if (!this.RunData.ConditionsMet[conditionIndex])
                {
                    cost += this.GetCheapestCostForCondition(conditionIndex);
                }
            }

            return cost;
        }

        private float GetCheapestCostForCondition(int conditionIndex)
        {
            var cost = float.MaxValue;
            foreach (var nodeIndex in this.ConditionConnections[conditionIndex])
            {
                if (this.RunData.Costs[nodeIndex] < cost)
                    cost = this.RunData.Costs[nodeIndex];
            }

            return cost;
        }
    }

}
