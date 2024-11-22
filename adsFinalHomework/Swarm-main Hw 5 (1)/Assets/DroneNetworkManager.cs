// Adam Marwan bin Husin Albasri, 22009203

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class DroneNetworkManager : MonoBehaviour
{
    public DroneNetworkCommunication highFECNetwork;
    public DroneNetworkCommunication lowFECNetwork;

    [SerializeField] private TMP_InputField startInputField; // Input field for start drone
    [SerializeField] private TMP_InputField endInputField;   // Input field for end drone
    [SerializeField] private TextMeshProUGUI resultsText; // Text for displaying results
    [SerializeField] private Button findShortestPathButton;

    private void Start()
    {
        // Initialize the two communication networks
        highFECNetwork = new DroneNetworkCommunication();
        lowFECNetwork = new DroneNetworkCommunication();



        // Add drones to the networks
        for (int i = 0; i < 10; i++)
        {
            Drone drone = new Drone { Id = "Agent " + i };
            if (i % 2 == 0)
            {
                highFECNetwork.AddDrone(drone);
            }
            else
            {
                lowFECNetwork.AddDrone(drone);
            }
        }

        // Connect the drones within each partition
        ConnectDrones(highFECNetwork);
        ConnectDrones(lowFECNetwork);
    }

    private void Awake()
    {
        // Attach the button's click event to the method
        findShortestPathButton.onClick.AddListener(FindShortestPath);
    }
    private void ConnectDrones(DroneNetworkCommunication network)
    {
        List<Drone> drones = new List<Drone>(network.adjacencyList.Keys);
        for (int i = 0; i < drones.Count; i++)
        {
            for (int j = i + 1; j < drones.Count; j++)
            {
                network.AddLink(drones[i], drones[j]);
            }
        }
    }

    private Drone SearchDrone(string id)
    {
        // Search in high FEC network
        Drone drone = highFECNetwork.GetDroneById(id);
        if (drone != null) return drone;

        // Search in low FEC network
        return lowFECNetwork.GetDroneById(id);
    }

    private void FindShortestPath()
    {
        string startId = startInputField.text.Trim(); // Get the Start Drone ID
        string endId = endInputField.text.Trim();     // Get the End Drone ID

        if (string.IsNullOrEmpty(startId) || string.IsNullOrEmpty(endId))
        {
            resultsText.text = "Please enter valid drone IDs in both fields.";
            return;
        }

        // Dynamically find drones by name
        GameObject startDrone = GameObject.Find(startId);
        GameObject endDrone = GameObject.Find(endId);

        if (startDrone == null || endDrone == null)
        {
            resultsText.text = $"Drone not found. Start: {startId}, End: {endId}";
            return;
        }

        // Calculate the shortest path distance
        float distance = Vector3.Distance(startDrone.transform.position, endDrone.transform.position);

        // Display the result
        resultsText.text = $"Shortest distance between {startId} and {endId}: {distance:F2} units";
    }

}