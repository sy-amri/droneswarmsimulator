using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class DroneCommunicationManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField searchInputField;
    [SerializeField] private Button searchButton;
    [SerializeField] private Button terminateButton;
    [SerializeField] private TextMeshProUGUI resultsText;
    [SerializeField] private TextMeshProUGUI performanceText; // New UI element for performance metrics

    private GameObject highlightMarker;
    private float highlightDuration = 1f;
    private Coroutine currentHighlightCoroutine;
    private Transform trackedDroneTransform;

    private DroneCommunication highFECNetwork;
    private DroneCommunication lowFECNetwork;
    private Dictionary<string, DroneCommunication> droneNetworkMap;
    private HashSet<string> terminatedDrones;
    private Stopwatch operationStopwatch; // For measuring operation time

    private void Start()
    {
        droneNetworkMap = new Dictionary<string, DroneCommunication>();
        terminatedDrones = new HashSet<string>();
        operationStopwatch = new Stopwatch();

        searchButton.onClick.AddListener(SearchDrone);
        terminateButton.onClick.AddListener(TerminateDrone);

        if (resultsText != null)
            resultsText.text = "Results will appear here";

        if (performanceText != null)
            performanceText.text = "Operation time: -- ms";

        CreateHighlightMarker();

        UnityEngine.Debug.Log("DroneCommunicationManager started");
    }

    public void InitializeNetworks(List<Drone> highFECDrones, List<Drone> lowFECDrones)
    {
        UnityEngine.Debug.Log($"Initializing networks with {highFECDrones.Count} high FEC drones and {lowFECDrones.Count} low FEC drones");

        droneNetworkMap.Clear();
        terminatedDrones.Clear();

        // Create high FEC network
        if (highFECDrones.Count > 0)
        {
            highFECNetwork = new DroneCommunication(highFECDrones[0]);
            droneNetworkMap[highFECDrones[0].name] = highFECNetwork;
            DroneCommunication current = highFECNetwork;

            for (int i = 1; i < highFECDrones.Count; i++)
            {
                current.Next = new DroneCommunication(highFECDrones[i]);
                current = current.Next;
                droneNetworkMap[highFECDrones[i].name] = current;
            }
        }

        // Create low FEC network
        if (lowFECDrones.Count > 0)
        {
            lowFECNetwork = new DroneCommunication(lowFECDrones[0]);
            droneNetworkMap[lowFECDrones[0].name] = lowFECNetwork;
            DroneCommunication current = lowFECNetwork;

            for (int i = 1; i < lowFECDrones.Count; i++)
            {
                current.Next = new DroneCommunication(lowFECDrones[i]);
                current = current.Next;
                droneNetworkMap[lowFECDrones[i].name] = current;
            }
        }

        UnityEngine.Debug.Log($"Network initialization complete. Map contains {droneNetworkMap.Count} drones");
    }

    private bool IsValidDroneId(string droneName)
    {
        if (string.IsNullOrEmpty(droneName))
            return false;

        // Check if drone exists in either network
        return droneNetworkMap.ContainsKey(droneName) && !terminatedDrones.Contains(droneName);
    }

    private void CreateHighlightMarker()
    {
        highlightMarker = new GameObject("DroneHighlightMarker");
        SpriteRenderer renderer = highlightMarker.AddComponent<SpriteRenderer>();

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        renderer.sprite = sprite;
        renderer.color = new Color(1f, 1f, 0f, 0.5f);
        highlightMarker.transform.localScale = new Vector3(80f, 80f, 80f);
        highlightMarker.SetActive(false);
    }

    private void Update()
    {
        // Update highlight position if we're tracking a drone
        if (trackedDroneTransform != null && highlightMarker.activeSelf)
        {
            highlightMarker.transform.position = new Vector3(
                trackedDroneTransform.position.x,
                trackedDroneTransform.position.y,
                trackedDroneTransform.position.z + 0.1f
            );
        }
    }

    private void SearchDrone()
    {
        operationStopwatch.Reset();
        operationStopwatch.Start();

        string droneName = searchInputField.text.Trim();

        if (string.IsNullOrEmpty(droneName))
        {
            resultsText.text = "Please enter a drone name";
            UpdatePerformanceMetrics("Invalid Input");
            return;
        }

        UnityEngine.Debug.Log($"Searching for drone: {droneName}");
        UnityEngine.Debug.Log($"Terminated drones count: {terminatedDrones.Count}");

        if (terminatedDrones.Contains(droneName))
        {
            UnityEngine.Debug.Log($"Drone {droneName} is in terminated set");
            resultsText.text = $"Drone {droneName} has been terminated and is no longer active";
            trackedDroneTransform = null;
            UpdatePerformanceMetrics("Search (Terminated)");
            return;
        }

        if (!droneNetworkMap.ContainsKey(droneName))
        {
            resultsText.text = $"Drone {droneName} not found in any network";
            trackedDroneTransform = null;
            UpdatePerformanceMetrics("Search (Not Found)");
            return;
        }

        DroneCommunication droneComm = droneNetworkMap[droneName];
        if (droneComm != null && droneComm.CurrentDrone != null)
        {
            Vector3 pos = droneComm.CurrentDrone.transform.position;
            resultsText.text = $"Drone {droneName} found at position: ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})";

            trackedDroneTransform = droneComm.CurrentDrone.transform;
            ShowHighlight();
        }
        else
        {
            resultsText.text = $"Error: Drone {droneName} exists in map but reference is invalid";
            trackedDroneTransform = null;
        }

        UpdatePerformanceMetrics("Search");
    }

    private void ShowHighlight()
    {
        if (currentHighlightCoroutine != null)
        {
            StopCoroutine(currentHighlightCoroutine);
        }

        currentHighlightCoroutine = StartCoroutine(ShowHighlightCoroutine());
    }

    private IEnumerator ShowHighlightCoroutine()
    {
        // Get the SpriteRenderer component
        SpriteRenderer highlightRenderer = highlightMarker.GetComponent<SpriteRenderer>();

        // Set initial color and enable highlight
        Color startColor = new Color(1f, 1f, 0f, 0.5f); // Yellow with 50% alpha
        highlightRenderer.color = startColor;
        highlightMarker.SetActive(true);

        // Fade out over duration
        float elapsedTime = 0f;
        while (elapsedTime < highlightDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0.5f, 0f, elapsedTime / highlightDuration);
            highlightRenderer.color = new Color(1f, 1f, 0f, alpha);
            yield return null;
        }

        // Hide the highlight and clear tracked drone
        highlightMarker.SetActive(false);
        trackedDroneTransform = null;
        currentHighlightCoroutine = null;
    }


    private void TerminateDrone()
    {
        operationStopwatch.Reset();
        operationStopwatch.Start();

        string droneName = searchInputField.text.Trim();

        if (string.IsNullOrEmpty(droneName))
        {
            resultsText.text = "Please enter a drone name";
            UpdatePerformanceMetrics("Invalid Input");
            return;
        }

        if (terminatedDrones.Contains(droneName))
        {
            UnityEngine.Debug.Log($"Drone {droneName} was already terminated");
            resultsText.text = $"Drone {droneName} was already terminated";
            UpdatePerformanceMetrics("Terminate (Already Terminated)");
            return;
        }

        if (!droneNetworkMap.ContainsKey(droneName))
        {
            resultsText.text = $"Drone {droneName} not found in any network";
            UpdatePerformanceMetrics("Terminate (Not Found)");
            return;
        }

        DroneCommunication droneToTerminate = droneNetworkMap[droneName];
        if (droneToTerminate == null || droneToTerminate.CurrentDrone == null)
        {
            resultsText.text = "Error: Invalid drone reference";
            UpdatePerformanceMetrics("Terminate (Invalid Reference)");
            return;
        }

        if (trackedDroneTransform == droneToTerminate.CurrentDrone.transform)
        {
            trackedDroneTransform = null;
            highlightMarker.SetActive(false);
            if (currentHighlightCoroutine != null)
            {
                StopCoroutine(currentHighlightCoroutine);
                currentHighlightCoroutine = null;
            }
        }

        bool isHighFEC = RemoveFromNetwork(ref highFECNetwork, droneName);
        if (!isHighFEC)
        {
            RemoveFromNetwork(ref lowFECNetwork, droneName);
        }

        droneToTerminate.CurrentDrone.gameObject.SetActive(false);
        droneNetworkMap.Remove(droneName);
        terminatedDrones.Add(droneName);

        UnityEngine.Debug.Log($"Drone {droneName} terminated successfully. Terminated count: {terminatedDrones.Count}");
        resultsText.text = $"Drone {droneName} terminated successfully";

        UpdatePerformanceMetrics("Terminate");
    }

    private void UpdatePerformanceMetrics(string operation)
    {
        operationStopwatch.Stop();
        float milliseconds = operationStopwatch.ElapsedTicks / (float)System.TimeSpan.TicksPerMillisecond;
        float fps = milliseconds > 0 ? 1000f / milliseconds : 0;

        performanceText.text = $"Operation: {operation}\n" +
                             $"Time: {milliseconds:F2} ms\n" +
                             $"Equivalent FPS: {fps:F1}";
    }

    private bool RemoveFromNetwork(ref DroneCommunication networkHead, string droneName)
    {
        if (networkHead == null) return false;

        // If head is the target
        if (networkHead.CurrentDrone.name == droneName)
        {
            networkHead = networkHead.Next;
            return true;
        }

        // Search through the network
        DroneCommunication current = networkHead;
        while (current.Next != null)
        {
            if (current.Next.CurrentDrone.name == droneName)
            {
                current.Next = current.Next.Next;
                return true;
            }
            current = current.Next;
        }

        return false;
    }

    // Debug method to print network state
    private void PrintNetworkState()
    {
        UnityEngine.Debug.Log("=== Network State ===");
        UnityEngine.Debug.Log($"DroneNetworkMap Count: {droneNetworkMap.Count}");
        UnityEngine.Debug.Log($"Terminated Drones Count: {terminatedDrones.Count}");
        UnityEngine.Debug.Log("Terminated Drones:");
        foreach (var drone in terminatedDrones)
        {
            UnityEngine.Debug.Log($"- {drone}");
        }
        UnityEngine.Debug.Log("==================");
    }
}
