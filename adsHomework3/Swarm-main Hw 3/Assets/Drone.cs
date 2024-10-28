

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Drone : MonoBehaviour
{
    public string Id;

    public string[] statuses = { "active", "returning", "recharging", "extinguishing" };

    public int temperature { set; get; } = 0;
    public int fireExtinguisherCapacity { get; set; } = 0;
    public int heatSensorRange { get; set; } = 0;
    public string status { get; set; }


    Flock agentFlock;
    Collider2D agentCollider;

    private const int maxTemperature = 100;
    private const int maxFireExtinguisherCapacity = 100;
    private const int maxHeatSensorRange = 100;

    private float fireExtinguisherCapacityRandomizerInterval = 2f;
    private float fireExtinguisherCapacityRandomizerTimer = 0f;

    public Flock AgentFlock { get { return agentFlock; } }
    public Collider2D AgentCollider { get { return agentCollider; } }

    // Start is called before the first frame update
    void Start()
    {
        temperature = (int)(Random.value * maxTemperature);
        fireExtinguisherCapacity = (int)(Random.value * maxFireExtinguisherCapacity);
        heatSensorRange = (int)(Random.value * maxHeatSensorRange);
        status = statuses[Random.Range(0, statuses.Length)];

        agentCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        UpdateFireExtinguisherCapacity();
    }

    public void Initialize(Flock flock)
    {
        agentFlock = flock;
    }

    public void Move(Vector2 velocity)
    {
        transform.up = velocity;
        transform.position += (Vector3)velocity * Time.deltaTime;
    }

    private void UpdateFireExtinguisherCapacity()
    {
        fireExtinguisherCapacityRandomizerTimer += Time.deltaTime;

        if (fireExtinguisherCapacityRandomizerTimer >= fireExtinguisherCapacityRandomizerInterval)
        {
            fireExtinguisherCapacityRandomizerTimer = 0f;
            fireExtinguisherCapacity = Random.Range(0, maxFireExtinguisherCapacity);
        }
    }
}