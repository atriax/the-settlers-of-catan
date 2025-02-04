﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Graph : MonoBehaviour
{

    private const int ROAD_COUNT = 71;

    private List<GraphNode> nodes = new List<GraphNode>();

    public void CreateGraphNode(WorldPath road)
    {
        GraphNode graphNode = new GraphNode
        {
            Road = road,
            Visited = false,
            RoadNeighbours = road.GetNeighbouringPaths(),
            Neighbours = new List<GraphNode>()
        };

        nodes.Add(graphNode);

        if (nodes.Count == ROAD_COUNT)
        {
            CalculateNodeNeighbours();
        }
    }

    // Initialise the neighbour lists for all nodes in the graph.
    private void CalculateNodeNeighbours()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue;

                if (nodes[i].RoadNeighbours.Contains(nodes[j].Road))
                {
                    nodes[i].Neighbours.Add(nodes[j]);
                }
            }
        }
    }
    public int CalculatePlayerRoadLength(int playerId)
    {
        
        // Perform DFS for this player.

        int length = 0;

        List<GraphNode> startNodes = new List<GraphNode>();

        // Find length starting from each road and grab the maximum one.
        foreach (GraphNode node in nodes)
        {
            if (node.Road.OwnerId == playerId)
            {
                startNodes.Add(node);
            }
        }

        foreach (GraphNode startNode in startNodes)
        {
            ResetVisitedFlags();
            foreach (GraphNode node in nodes)
            {
                if (node == startNode)
                {
                    Stack<GraphNode> trail = new Stack<GraphNode>();
                    trail.Push(node);
                    int dfsLength = DFS(playerId, null, node, 1, trail);

                    trail.Pop();
                    if (dfsLength > length)
                    {
                        length = dfsLength;
                    }
                }
            }
        }
       
        return length;
    }

    private void ResetVisitedFlags()
    {
        foreach (GraphNode node in nodes)
        {
            node.Visited = false;
        }
    }

    private void ResetChildrenVisitedFlags(Stack<GraphNode> trail)
    {
        foreach (GraphNode node in nodes)
        {
            if (!trail.Contains(node))
            {
                node.Visited = false;
            }
        }
    }

    private int DFS(int playerId, GraphNode parent, GraphNode node, int parentLength, Stack<GraphNode> trail)
    {
        int length = parentLength;
        node.Visited = true;

        trail.Push(node);

        foreach (GraphNode neighbour in node.Neighbours)
        {
            if (!neighbour.Visited && neighbour.Road.OwnerId == playerId)
            {

                // If the intersection between me and the parent is occupied by another player, ignore this road.
                //if (parent != null)
                //{
                //    Intersection foundIntersection = null;

                //    foreach (Intersection nodeIntersection in node.Road.GetIntersections())
                //    {
                //        foreach (Intersection parentIntersection in parent.Road.GetIntersections())
                //        {
                //            if (nodeIntersection == parentIntersection)
                //            {
                //                foundIntersection = nodeIntersection;
                //                break;
                //            }
                //        }
                //    }
                    
                //    if (foundIntersection.GetOwnerId() != playerId && foundIntersection.GetOwnerId() != 0)
                //    {
                //        continue;
                //    }
                //}
                //else
                //{
                    // Parent is null, but don't continue to neighbours if the mutual settlement is now occupied by another player.
                    
                    Intersection mutualIntersection = null;

                    foreach (Intersection nodeIntersection in node.Road.GetIntersections())
                    {
                        foreach (Intersection neighbourIntersection in neighbour.Road.GetIntersections())
                        {
                            if (nodeIntersection == neighbourIntersection)
                            {
                                mutualIntersection = nodeIntersection;
                                break;
                            }
                        }
                    }

                    if (mutualIntersection.GetOwnerId() != playerId && mutualIntersection.GetOwnerId() != 0)
                    {
                        continue;
                    }
                //}
                

                // Also ignore neighbours coming from parent's side.
                if (parent != null && parent.Neighbours.Contains(neighbour))
                {
                    continue;
                }

                // Reset children visited flags.
                ResetChildrenVisitedFlags(trail);

                int neighbourLength = DFS(playerId, node, neighbour, parentLength + 1, trail);
                if (neighbourLength > length)
                {
                    length = neighbourLength;
                }
            }
        }

        trail.Pop();

        return length;
    }
}