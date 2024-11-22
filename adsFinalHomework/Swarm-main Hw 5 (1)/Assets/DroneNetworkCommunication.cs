// Safiqur Rahman bin Rowther Neine, 22008929

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class DroneNetworkCommunication
{
    public Dictionary<Drone, List<Drone>> adjacencyList;

    public DroneNetworkCommunication()
    {
        adjacencyList = new Dictionary<Drone, List<Drone>>();
    }

    public void AddDrone(Drone drone)
    {
        if (!adjacencyList.ContainsKey(drone))
        {
            adjacencyList[drone] = new List<Drone>();
        }
    }

    public void AddLink(Drone from, Drone to)
    {
        if (!adjacencyList.ContainsKey(from))
        {
            AddDrone(from);
        }
        if (!adjacencyList.ContainsKey(to))
        {
            AddDrone(to);
        }

        adjacencyList[from].Add(to);
        adjacencyList[to].Add(from); // For undirected graph
    }

    public List<Drone> GetNeighbors(Drone drone)
    {
        return adjacencyList.ContainsKey(drone) ? adjacencyList[drone] : null;
    }

    public Drone SearchDroneById(string id)
    {
        foreach (var drone in adjacencyList.Keys)
        {
            if (drone.Id.Trim().Equals(id.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return drone;
            }
        }
        return null;
    }


    // Danish Harith bin Ahmad Nizam, 22009489

    public Drone GetDroneById(string id)
    {
        foreach (var drone in adjacencyList.Keys)
        {
            if (drone.name.Equals(id, System.StringComparison.OrdinalIgnoreCase))
            {
                return drone;
            }
        }
        return null;
    }

    public List<Drone> FindShortestPath(Drone startDrone, Drone endDrone)
    {
        if (startDrone == null || endDrone == null) return null;

        Queue<Drone> queue = new Queue<Drone>();
        Dictionary<Drone, Drone> previous = new Dictionary<Drone, Drone>();

        queue.Enqueue(startDrone);
        previous[startDrone] = null;

        while (queue.Count > 0)
        {
            Drone current = queue.Dequeue();

            if (current == endDrone)
            {
                // Reconstruct path
                List<Drone> path = new List<Drone>();
                while (current != null)
                {
                    path.Add(current);
                    current = previous[current];
                }
                path.Reverse();
                return path;
            }

            foreach (Drone neighbor in GetNeighbors(current))
            {
                if (!previous.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor);
                    previous[neighbor] = current;
                }
            }
        }

        return null; // No path found
    }

    private List<Drone> ReconstructPath(Dictionary<Drone, Drone> previous, Drone start, Drone end)
    {
        List<Drone> path = new List<Drone>();
        for (Drone at = end; at != null; at = previous.ContainsKey(at) ? previous[at] : null)
        {
            path.Add(at);
        }
        path.Reverse();
        return path;
    }

    public float CalculateDistance(Drone drone1, Drone drone2)
    {
        return Vector3.Distance(drone1.transform.position, drone2.transform.position);
    }

    public float GetDistance(Drone drone1, Drone drone2)
    {
        return Vector3.Distance(drone1.transform.position, drone2.transform.position);
    }
}