
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;

public class DroneBTCommunicationManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField searchInputField;
    [SerializeField] private TMP_InputField attributeValueField;
    [SerializeField] private TMP_Dropdown searchAttributeDropdown;
    [SerializeField] private Button searchTemperatureButton;
    [SerializeField] private Button searchAttributeButton;
    [SerializeField] private Button selfDestructButton;
    [SerializeField] private Button selfDestructByFECapacityButton;
    [SerializeField] private TextMeshProUGUI resultsText;
    [SerializeField] private TextMeshProUGUI performanceText;

    private DroneBTCommunication highFECNetwork;
    private DroneBTCommunication lowFECNetwork;
    private HashSet<string> destructedDrones;
    private Stopwatch operationStopwatch;

    private GameObject highlightMarker;
    private float highlightDuration = 1f;
    private Coroutine currentHighlightCoroutine;
    private Transform trackedDroneTransform;

    private void Start()
    {
        highFECNetwork = new DroneBTCommunication();
        lowFECNetwork = new DroneBTCommunication();
        destructedDrones = new HashSet<string>();
        operationStopwatch = new Stopwatch();

        CreateHighlightMarker();
        InitializeUI();
    }

    private void CreateHighlightMarker()
    {
        highlightMarker = new GameObject("DroneBTHighlightMarker");
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

    private void ShowHighlight(Transform droneTransform)
    {
        trackedDroneTransform = droneTransform;

        if (currentHighlightCoroutine != null)
        {
            StopCoroutine(currentHighlightCoroutine);
        }

        currentHighlightCoroutine = StartCoroutine(ShowHighlightCoroutine());
    }

    private IEnumerator ShowHighlightCoroutine()
    {
        SpriteRenderer highlightRenderer = highlightMarker.GetComponent<SpriteRenderer>();
        Color startColor = new Color(1f, 1f, 0f, 0.5f);
        highlightRenderer.color = startColor;
        highlightMarker.SetActive(true);

        float elapsedTime = 0f;
        while (elapsedTime < highlightDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0.5f, 0f, elapsedTime / highlightDuration);
            highlightRenderer.color = new Color(1f, 1f, 0f, alpha);
            yield return null;
        }

        highlightMarker.SetActive(false);
        trackedDroneTransform = null;
        currentHighlightCoroutine = null;
    }

    private void Update()
    {
        if (trackedDroneTransform != null && highlightMarker.activeSelf)
        {
            highlightMarker.transform.position = new Vector3(
                trackedDroneTransform.position.x,
                trackedDroneTransform.position.y,
                trackedDroneTransform.position.z + 0.1f
            );
        }
    }

    private void InitializeUI()
    {
        searchTemperatureButton.onClick.AddListener(SearchByTemperature);
        searchAttributeButton.onClick.AddListener(SearchByAttribute);
        selfDestructButton.onClick.AddListener(SelfDestruct);

        // Setup dropdown options
        searchAttributeDropdown.ClearOptions();
        searchAttributeDropdown.AddOptions(new List<string> { "Temperature", "Fire Extinguisher Capacity" });

        searchTemperatureButton.onClick.AddListener(SearchByTemperature);
        searchAttributeButton.onClick.AddListener(SearchByAttribute);
        selfDestructButton.onClick.AddListener(SelfDestruct);
        // Add event listener for the new Self-Destruct by FE Capacity button
        selfDestructByFECapacityButton.onClick.AddListener(SelfDestructByFECapacity); // Assuming you have this button reference

        // Setup dropdown options
        searchAttributeDropdown.ClearOptions();
        searchAttributeDropdown.AddOptions(new List<string> { "Temperature", "Fire Extinguisher Capacity" });
    }

    public void InitializeNetworks(List<Drone> highFECDrones, List<Drone> lowFECDrones)
    {
        // Create new BST networks
        highFECNetwork = new DroneBTCommunication();
        lowFECNetwork = new DroneBTCommunication();

        // Insert drones into respective BSTs
        foreach (var drone in highFECDrones)
        {
            highFECNetwork.Insert(drone);
        }

        foreach (var drone in lowFECDrones)
        {
            lowFECNetwork.Insert(drone);
        }

        UnityEngine.Debug.Log($"BST Networks initialized with {highFECDrones.Count} high FEC and {lowFECDrones.Count} low FEC drones");
    }

    private void SearchByTemperature()
    {
        if (!float.TryParse(searchInputField.text, out float temperature))
        {
            resultsText.text = "Please enter a valid temperature";
            return;
        }

        operationStopwatch.Reset();
        operationStopwatch.Start();

        var highFECResult = highFECNetwork.SearchByTemperature(temperature);
        var lowFECResult = lowFECNetwork.SearchByTemperature(temperature);

        if (highFECResult.foundDrone != null)
        {
            ShowSearchResults(highFECResult.foundDrone, highFECResult.timeTaken, highFECResult.path, "High FEC");
            highFECNetwork.VisualizeCommPath(highFECResult.path);
            ShowHighlight(highFECResult.foundDrone.transform);
        }
        else if (lowFECResult.foundDrone != null)
        {
            ShowSearchResults(lowFECResult.foundDrone, lowFECResult.timeTaken, lowFECResult.path, "Low FEC");
            lowFECNetwork.VisualizeCommPath(lowFECResult.path);
            ShowHighlight(lowFECResult.foundDrone.transform);
        }
        else
        {
            resultsText.text = $"No drone found with temperature {temperature}";
        }

        UpdatePerformanceMetrics("Temperature Search");
    }


    private void SearchByAttribute()
    {
        string attributeValue = attributeValueField.text;
        string selectedAttribute = searchAttributeDropdown.options[searchAttributeDropdown.value].text;

        operationStopwatch.Reset();
        operationStopwatch.Start();

        System.Predicate<Drone> searchCondition = drone =>
        {
            switch (selectedAttribute)
            {
                case "Temperature":
                    return drone.Id == attributeValue;
                case "Fire Extinguisher Capacity":
                    if (float.TryParse(attributeValue, out float capacity))
                        return Mathf.Approximately(drone.fireExtinguisherCapacity, capacity);
                    return false;
                default:
                    return false;
            }
        };

        var highFECResult = highFECNetwork.ExhaustiveSearch(searchCondition);
        var lowFECResult = lowFECNetwork.ExhaustiveSearch(searchCondition);

        if (highFECResult.foundDrone != null)
        {
            ShowSearchResults(highFECResult.foundDrone, highFECResult.timeTaken, highFECResult.path, "High FEC");
            highFECNetwork.VisualizeCommPath(highFECResult.path);
            ShowHighlight(highFECResult.foundDrone.transform);
        }
        else if (lowFECResult.foundDrone != null)
        {
            ShowSearchResults(lowFECResult.foundDrone, lowFECResult.timeTaken, lowFECResult.path, "Low FEC");
            lowFECNetwork.VisualizeCommPath(lowFECResult.path);
            ShowHighlight(lowFECResult.foundDrone.transform);
        }
        else
        {
            resultsText.text = $"No drone found with {selectedAttribute} = {attributeValue}";
        }

        UpdatePerformanceMetrics("Attribute Search");
    }


    private void SelfDestruct()
    {
        string droneId = searchInputField.text;

        if (string.IsNullOrEmpty(droneId))
        {
            resultsText.text = "Please enter a drone ID";
            return;
        }

        operationStopwatch.Reset();
        operationStopwatch.Start();

        // Assuming you want to search by temperature or fire extinguisher capacity
        var temperatureSearch = float.TryParse(searchInputField.text, out float temperature);
        bool selfDestructed = false;

        if (temperatureSearch)
        {
            // If searching by temperature, call SendSelfDestruct with temperature match
            var highFECResult = highFECNetwork.SendSelfDestructByTemperature(temperature);
            var lowFECResult = lowFECNetwork.SendSelfDestructByTemperature(temperature);

            if (highFECResult.success || lowFECResult.success)
            {
                selfDestructed = true;
                resultsText.text = $"Drone with temperature {temperature} self-destructed";
            }
        }

        if (!selfDestructed)
        {
            // If not self-destructed via temperature, check by ID or other criteria
            var highFECResult = highFECNetwork.SendSelfDestruct(droneId);
            var lowFECResult = lowFECNetwork.SendSelfDestruct(droneId);

            if (highFECResult.success || lowFECResult.success)
            {
                selfDestructed = true;
                resultsText.text = $"Drone {droneId} self-destructed\nTime taken: {highFECResult.timeTaken:F2} seconds";
            }
        }

        if (!selfDestructed)
        {
            resultsText.text = $"Drone {droneId} not found";
        }

        UpdatePerformanceMetrics("Self Destruct");
    }

    private void SelfDestructByFECapacity()
    {
        // Retrieve the FE capacity input from the UI
        string feCapacityText = attributeValueField.text;

        if (string.IsNullOrEmpty(feCapacityText) || !float.TryParse(feCapacityText, out float feCapacity))
        {
            resultsText.text = "Please enter a valid Fire Extinguisher Capacity";
            return;
        }

        operationStopwatch.Reset();
        operationStopwatch.Start();

        // Search both networks by FE Capacity
        var highFECResult = highFECNetwork.SendSelfDestructByFECapacity(feCapacity);
        var lowFECResult = lowFECNetwork.SendSelfDestructByFECapacity(feCapacity);

        if (highFECResult.success || lowFECResult.success)
        {
            var result = highFECResult.success ? highFECResult : lowFECResult;
            resultsText.text = $"Drone with FE Capacity {feCapacity} self-destructed\nTime taken: {result.timeTaken:F2} seconds";

            if (highFECResult.success)
                highFECNetwork.VisualizeCommPath(highFECResult.path);
            else
                lowFECNetwork.VisualizeCommPath(lowFECResult.path);
        }
        else
        {
            resultsText.text = $"No drone found with FE Capacity {feCapacity}";
        }

        UpdatePerformanceMetrics("Self Destruct by FE Capacity");
    }

    private void ShowSearchResults(Drone drone, float timeTaken, List<Vector3> path, string network)
    {
        Vector3 pos = drone.transform.position;
        resultsText.text = $"Found in {network} network:\n" +
                          $"Drone {drone.Id} at ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})\n" +
                          $"Temperature: {drone.temperature:F2}\n" +
                          $"FE Capacity: {drone.fireExtinguisherCapacity:F2}\n" +
                          $"Time taken: {timeTaken:F2} seconds\n" +
                          $"Hops: {path.Count - 1}";
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
}