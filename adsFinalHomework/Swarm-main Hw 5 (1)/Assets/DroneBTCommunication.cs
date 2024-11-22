
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneBTCommunication : MonoBehaviour
{
    public class BSTNode
    {
        public Drone Drone { get; set; }
        public BSTNode Left { get; set; }
        public BSTNode Right { get; set; }

        public BSTNode(Drone drone)
        {
            Drone = drone;
            Left = Right = null;
        }
    }

    private BSTNode root;
    private float communicationSpeed = 10f; // Units per second

    public DroneBTCommunication()
    {
        root = null;
    }

    // Insert drone into BST using temperature as key
    public void Insert(Drone drone)
    {
        root = InsertRec(root, drone);
    }

    private BSTNode InsertRec(BSTNode root, Drone drone)
    {
        if (root == null)
        {
            return new BSTNode(drone);
        }

        if (drone.temperature < root.Drone.temperature)
        {
            root.Left = InsertRec(root.Left, drone);
        }
        else if (drone.temperature > root.Drone.temperature)
        {
            root.Right = InsertRec(root.Right, drone);
        }

        return root;
    }

    // Search by temperature (BST search)
    public (Drone foundDrone, float timeTaken, List<Vector3> path) SearchByTemperature(float temperature)
    {
        List<Vector3> communicationPath = new List<Vector3>();
        float totalTime = 0f;
        Vector3? lastPosition = null;

        var result = SearchByTemperatureRec(root, temperature, ref totalTime, communicationPath, lastPosition);
        return (result, totalTime, communicationPath);
    }

    private Drone SearchByTemperatureRec(BSTNode node, float temperature, ref float totalTime, List<Vector3> path, Vector3? lastPos)
    {
        if (node == null) return null;

        // Add current node position to path
        path.Add(node.Drone.transform.position);

        // Calculate communication time if this isn't the first node
        if (lastPos.HasValue)
        {
            totalTime += Vector3.Distance(lastPos.Value, node.Drone.transform.position) / communicationSpeed;
        }

        if (Mathf.Approximately(node.Drone.temperature, temperature))
            return node.Drone;

        if (temperature < node.Drone.temperature)
            return SearchByTemperatureRec(node.Left, temperature, ref totalTime, path, node.Drone.transform.position);

        return SearchByTemperatureRec(node.Right, temperature, ref totalTime, path, node.Drone.transform.position);
    }

    // Exhaustive search by any attribute
    public (Drone foundDrone, float timeTaken, List<Vector3> path) ExhaustiveSearch(System.Predicate<Drone> condition)
    {
        List<Vector3> communicationPath = new List<Vector3>();
        float totalTime = 0f;
        Vector3? lastPosition = null;

        var result = ExhaustiveSearchRec(root, condition, ref totalTime, communicationPath, lastPosition);
        return (result, totalTime, communicationPath);
    }

    private Drone ExhaustiveSearchRec(BSTNode node, System.Predicate<Drone> condition, ref float totalTime,
        List<Vector3> path, Vector3? lastPos)
    {
        if (node == null) return null;

        // Add current node to path
        path.Add(node.Drone.transform.position);

        // Calculate communication time
        if (lastPos.HasValue)
        {
            totalTime += Vector3.Distance(lastPos.Value, node.Drone.transform.position) / communicationSpeed;
        }

        if (condition(node.Drone))
            return node.Drone;

        Drone leftResult = ExhaustiveSearchRec(node.Left, condition, ref totalTime, path, node.Drone.transform.position);
        if (leftResult != null)
            return leftResult;

        return ExhaustiveSearchRec(node.Right, condition, ref totalTime, path, node.Drone.transform.position);
    }


    // Send self-destruct message
    public (bool success, float timeTaken, List<Vector3> path) SendSelfDestruct(string droneId)
    {
        List<Vector3> communicationPath = new List<Vector3>();
        float totalTime = 0f;
        Vector3? lastPosition = null;

        bool success = SendSelfDestructRec(root, droneId, ref totalTime, communicationPath, lastPosition);
        return (success, totalTime, communicationPath);
    }

    private bool SendSelfDestructRec(BSTNode node, string droneId, ref float totalTime, List<Vector3> path, Vector3? lastPos)
    {
        if (node == null) return false;

        // Add current node to path
        path.Add(node.Drone.transform.position);

        // Calculate communication time
        if (lastPos.HasValue)
        {
            totalTime += Vector3.Distance(lastPos.Value, node.Drone.transform.position) / communicationSpeed;
        }

        if (node.Drone.Id == droneId)
        {
            node.Drone.gameObject.SetActive(false);
            return true;
        }

        bool leftResult = SendSelfDestructRec(node.Left, droneId, ref totalTime, path, node.Drone.transform.position);
        if (leftResult) return true;

        return SendSelfDestructRec(node.Right, droneId, ref totalTime, path, node.Drone.transform.position);
    }

    // Send self-destruct message based on temperature (or other criteria)
    public (bool success, float timeTaken, List<Vector3> path) SendSelfDestructByTemperature(float temperature)
    {
        List<Vector3> communicationPath = new List<Vector3>();
        float totalTime = 0f;
        Vector3? lastPosition = null;

        bool success = SendSelfDestructByTemperatureRec(root, temperature, ref totalTime, communicationPath, lastPosition);
        return (success, totalTime, communicationPath);
    }

    private bool SendSelfDestructByTemperatureRec(BSTNode node, float temperature, ref float totalTime, List<Vector3> path, Vector3? lastPos)
    {
        if (node == null) return false;

        path.Add(node.Drone.transform.position);

        if (lastPos.HasValue)
        {
            totalTime += Vector3.Distance(lastPos.Value, node.Drone.transform.position) / communicationSpeed;
        }

        if (Mathf.Approximately(node.Drone.temperature, temperature))
        {
            node.Drone.gameObject.SetActive(false);
            return true;
        }

        bool leftResult = SendSelfDestructByTemperatureRec(node.Left, temperature, ref totalTime, path, node.Drone.transform.position);
        if (leftResult) return true;

        return SendSelfDestructByTemperatureRec(node.Right, temperature, ref totalTime, path, node.Drone.transform.position);
    }

    // Send self-destruct message based on FE Capacity
    public (bool success, float timeTaken, List<Vector3> path) SendSelfDestructByFECapacity(float feCapacity)
    {
        List<Vector3> communicationPath = new List<Vector3>();
        float totalTime = 0f;
        Vector3? lastPosition = null;

        bool success = SendSelfDestructByFECapacityRec(root, feCapacity, ref totalTime, communicationPath, lastPosition);
        return (success, totalTime, communicationPath);
    }

    private bool SendSelfDestructByFECapacityRec(BSTNode node, float feCapacity, ref float totalTime, List<Vector3> path, Vector3? lastPos)
    {
        if (node == null) return false;

        // Add current node to path
        path.Add(node.Drone.transform.position);

        // Calculate communication time
        if (lastPos.HasValue)
        {
            totalTime += Vector3.Distance(lastPos.Value, node.Drone.transform.position) / communicationSpeed;
        }

        if (Mathf.Approximately(node.Drone.fireExtinguisherCapacity, feCapacity))
        {
            node.Drone.gameObject.SetActive(false); // Self-destruct
            return true;
        }

        bool leftResult = SendSelfDestructByFECapacityRec(node.Left, feCapacity, ref totalTime, path, node.Drone.transform.position);
        if (leftResult) return true;

        return SendSelfDestructByFECapacityRec(node.Right, feCapacity, ref totalTime, path, node.Drone.transform.position);
    }



    // Visualize communication path
    public void VisualizeCommPath(List<Vector3> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            Debug.DrawLine(path[i], path[i + 1], Color.yellow, 2f);
        }
    }
}