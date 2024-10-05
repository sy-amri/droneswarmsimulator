using System.Security.AccessControl;

public class Flock
{
    Drone[] agents;
    int num;
    
    public Flock(int maxnum)
    {
        agents = new Drone[maxnum];
    }
    
    // actually add the drones
    public void Init(int num)
    {
        this.num = num;
        for (int i=0; i<num; i++)
        {
            agents[i] = new Drone(i);
        }
    }
    
    public void Update()
    {
        for (int i=0; i<num; i++)
        {
            agents[i].Update();
        }
    }
    
    // Manas Ismail Abdylas, 22008600
    public float average() 
    {
        float totalTemp = 0;
        for (int i = 0; i < num; i++)
         {
            totalTemp += agents[i].Temperature;
        }
        return totalTemp / num;
    }

    // Danish Harith bin Ahmad Nizam, 22009489
    public int max()
    {
        float maxTemp = agents[0].Temperature;
        int maxID = agents[0].ID;

        for (int i = 1; i < num; i++)
        {
            if (agents[i].Temperature > maxTemp)
            {
                maxTemp = agents[i].Temperature;
                maxID = agents[i].ID;
            }
        }
        return maxID;
    }

    // Safiqur Rahman bin Rowther Neine, 22008929
    public int min()
    {
        if (num == 0) return 0;

        float minTemp = agents[0].Temperature;
        int minID = agents[0].ID;

        for (int i = 1; i < num; i++)
        {
            if (agents[i].Temperature < minTemp)
            {
                minTemp = agents[i].Temperature;
                minID = agents[i].ID;
            }
        }
        return minID;
    }

    // Adam Marwan bin Husin Albasri, 22009203
    public void print()
    {

    }

    public void append(Drone val)
    {
    }

    public void appendFront(Drone val)
    {
    }

    public void insert(Drone val, int index)
    {
    }

    public void deleteFront(int index)
    {
    }

    public void deleteBack(int index)
    {
    }

    public void delete(int index)
    {
    } 
    
    // Syahir Amri bin Mohd Azha, 22007728
    public void bubblesort()
    {
        for (int i = 0; i < num-1; i++) {
            bool swapped = false;
            for (int j = 0; j < num-i-1; j++) {
                if (agents[j].Temperature > agents[j + 1].Temperature) {
                    Drone temp = agents[j];
                    agents[j] = agents[j + 1];
                    agents[j + 1] = temp;
                    swapped = true;
                }
            }

            if (swapped == false)
                break;
        }
    }

    

    public void insertionsort()
    {
        
    }
}