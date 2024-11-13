using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class Flock : MonoBehaviour
{
    private DroneCommunicationManager linkedListManager;
    private DroneBTCommunicationManager binaryTreeManager;
    private List<Drone> highFECDrones = new List<Drone>();
    private List<Drone> lowFECDrones = new List<Drone>();
    private FPSDisplay fpsDisplay;

    public Drone agentPrefab;
    List<Drone> agents = new List<Drone>();
    public FlockBehavior behavior;

    public Sprite blueDrone;
    public Sprite redDrone;

    [Range(10, 5000)]
    public int startingCount = 100;
    const float AgentDensity = 0.08f;

    [Range(1f, 100f)]
    public float driveFactor = 10f;
    [Range(1f, 100f)]
    public float maxSpeed = 5f;
    [Range(1f, 10f)]
    public float neighborRadius = 2f;
    [Range(0f, 1f)]
    public float avoidanceRadiusMultiplier = 0.5f;

    float squareMaxSpeed;
    float squareNeighborRadius;
    float squareAvoidanceRadius;

    public float SquareAvoidanceRadius { get { return squareAvoidanceRadius; } }

    // Start is called before the first frame update
    void Start()
    {
        fpsDisplay = FindObjectOfType<FPSDisplay>();

        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = squareNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;

        for (int i = 0; i < startingCount; i++)
        {
            Drone newAgent = Instantiate(
                agentPrefab,
                UnityEngine.Random.insideUnitCircle * startingCount * AgentDensity,
                Quaternion.Euler(Vector3.forward * UnityEngine.Random.Range(0f, 360f)),
                transform
            );
            newAgent.name = "Agent " + i;
            newAgent.Initialize(this);
            agents.Add(newAgent);
        }

        // Find both communication managers in the scene
        linkedListManager = FindObjectOfType<DroneCommunicationManager>();
        binaryTreeManager = FindObjectOfType<DroneBTCommunicationManager>();

        if (linkedListManager == null)
            UnityEngine.Debug.LogError("DroneCommunicationManager not found in scene!");

        if (binaryTreeManager == null)
            UnityEngine.Debug.LogError("DroneBTCommunicationManager not found in scene!");

        // Partition the drones and initialize both communication networks
        Partition();
    }

    void BubbleSort(Drone[] arr, int n) // O(N^2)
    {
        int i, j;
        Drone temp;
        bool swapped;               // let n =10
        for (i = 0; i < n - 1; i++)  // i=0..9
        {
            swapped = false;
            for (j = 0; j < n - i - 1; j++)   // i=0: j=0..9
                                              // i=1; j=0..8
                                              // i=2; j=0..7
                                              // i
            {
                if (arr[j].temperature > arr[j + 1].temperature) // check whether to swap
                {

                    // Swap arr[j] and arr[j+1]
                    temp = arr[j];
                    arr[j] = arr[j + 1];
                    arr[j + 1] = temp;
                    swapped = true;
                }
            }

            // If no two elements were
            // swapped by inner loop, then break
            if (swapped == false)
                break;
        }
    }

    //Partition method
    void Partition()
    {
        highFECDrones.Clear();
        lowFECDrones.Clear();

        Drone firstDrone = agents[0];
        float pivot = firstDrone.fireExtinguisherCapacity;

        foreach (Drone drone in agents)
        {
            SpriteRenderer droneRenderer = drone.gameObject.GetComponent<SpriteRenderer>();
            if (droneRenderer != null)
            {
                if (drone.fireExtinguisherCapacity >= pivot)
                {
                    highFECDrones.Add(drone);
                    droneRenderer.sprite = blueDrone;
                }
                else
                {
                    lowFECDrones.Add(drone);
                    droneRenderer.sprite = redDrone;
                }
            }
            else
            {
                UnityEngine.Debug.LogError("Drone " + drone.name + " is missing a SpriteRenderer component!");
            }
        }

        // Initialize both communication networks
        if (linkedListManager != null)
        {
            linkedListManager.InitializeNetworks(highFECDrones, lowFECDrones);
        }

        if (binaryTreeManager != null)
        {
            binaryTreeManager.InitializeNetworks(highFECDrones, lowFECDrones);
        }

        UnityEngine.Debug.Log($"Partition complete: {highFECDrones.Count} high FEC drones, {lowFECDrones.Count} low FEC drones");
    }
    private DroneCommunication CreateCommunicationNetwork(List<Drone> drones)
    {
        if (drones.Count == 0) return null;

        DroneCommunication head = new DroneCommunication(drones[0]);
        DroneCommunication current = head;

        for (int i = 1; i < drones.Count; i++)
        {
            current.Next = new DroneCommunication(drones[i]);
            current = current.Next;
        }

        return head;
    }

    // Update is called once per frame
    void Update()
    {
        Drone[] drones = agents.ToArray();
        //BubbleSort(drones, drones.Length);
        //Array.Sort(tempArray);

        Stopwatch sw = new Stopwatch();
        sw.Start();
        Partition();
        sw.Stop();
        TimeSpan timePartition = sw.Elapsed;

        // Use the FPSDisplay instance to get the current FPS
        float currentFPS = fpsDisplay != null ? fpsDisplay.Fps : 0;

        using (StreamWriter file = new StreamWriter("results.csv", true))
        {
            if (file.BaseStream.Length == 0)
            {
                file.WriteLine("PartitionTime,FPS");
            }

            int numdrones = drones.Length;
            file.WriteLine($"{timePartition.Milliseconds},{currentFPS}");
        }

        foreach (Drone agent in agents)
        {
            // decide on next movement direction
            List<Transform> context = GetNearbyObjects(agent);

            Vector2 move = behavior.CalculateMove(agent, context, this);
            move *= driveFactor;
            if (move.sqrMagnitude > squareMaxSpeed)
            {
                move = move.normalized * maxSpeed;
            }
            agent.Move(move);
        }
    }

    List<Transform> GetNearbyObjects(Drone agent)
    {
        List<Transform> context = new List<Transform>();
        Collider2D[] contextColliders = Physics2D.OverlapCircleAll(agent.transform.position, neighborRadius);
        foreach (Collider2D c in contextColliders)
        {
            if (c != agent.AgentCollider)
            {
                context.Add(c.transform);
            }
        }
        return context;
    }

}