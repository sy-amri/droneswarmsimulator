// Manas Ismail Abdylas, 22008600

using UnityEngine;

public class DroneCommunication
{
    public Drone CurrentDrone { get; private set; }
    public DroneCommunication Next { get; set; }

    public DroneCommunication(Drone drone)
    {
        CurrentDrone = drone;
        Next = null;
    }

    // Send a message through the linked list
    public void SendMessage(string message)
    {
        Debug.Log($"Message to {CurrentDrone.name}: {message}");
        if (Next != null)
        {
            Next.SendMessage(message); // Forward the message to the next drone
        }
    }

    // Search for a drone in the list by ID
    public void SearchDrone(string id)
    {
        if (CurrentDrone.Id == id)
        {
            Debug.Log($"Drone {id} found at position: {CurrentDrone.transform.position}");
            return;
        }
        if (Next != null)
        {
            Next.SearchDrone(id);
        }
        else
        {
            Debug.Log($"Drone {id} not found.");
        }
    }

    // Self-destruct a drone by ID
    public void SelfDestructDrone(string id)
    {
        if (CurrentDrone.Id == id)
        {
            Debug.Log($"Drone {id} self-destructing.");
            CurrentDrone.gameObject.SetActive(false);
            return;
        }
        if (Next != null)
        {
            Next.SelfDestructDrone(id);
        }
        else
        {
            Debug.Log($"Drone {id} not found.");
        }
    }

    // Calculate time for message based on distance
    public float CalculateTime(Vector3 startPos, Vector3 endPos)
    {
        float distance = Vector3.Distance(startPos, endPos);
        float speed = 1.0f; // Adjust as needed
        return distance / speed;
    }
}
