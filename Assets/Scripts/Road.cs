using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

/// <summary>
/// Class <c>Road</c> handles level updates.
/// </summary>
public class Road : MonoBehaviour
{   
    [System.NonSerialized]
    public int std = 10;

    [System.NonSerialized]
    public int throughput = 0;

    public GameObject carPrefab;
    public int lanes = 1;
    public int carCount = 10;
    public int maxSpeed = 100;

    private System.Random rnd = new System.Random();
    private List<Vector3> spawnLocations = new List<Vector3>();
    private int currentCarCount = 0;
    
    void Start()
    {
        setSpawnLocations();
        
        // !NOTE: Create a variable currentCarCount so we can change it without the unity options GUI
        this.currentCarCount = this.carCount;

        // !NOTE: Spawn cars as a coroutine (a side-process so we can run the level in the meantime)
        StartCoroutine(spawnCars());

        Invoke("saveData", 40);
    }

    /// <summary> This method spawns the car at a spawnpoint defined in the "setSpawnLocations()" function </summary>
    IEnumerator spawnCars()
    {
        for (int i = 0; i < this.currentCarCount; i++)
        {
            int r = rnd.Next(this.spawnLocations.Count);
            Vector3 spawnLocation = this.spawnLocations[r];

            float spawnDistance = .1F;
            if (Physics.OverlapSphere(spawnLocation, spawnDistance).Length <= 1) {
                GameObject car = Instantiate(carPrefab, spawnLocation, Quaternion.identity);
                car.GetComponent<Car>().lane = r;
            } else {
                yield return new WaitForSeconds(1);
                this.currentCarCount += 1;
            }
        }
    }

    /// <summary> Save the current run data to a .csv file </summary>
    void saveData() {
        Debug.Log("Saving data");

        StringBuilder csv = new StringBuilder();

        string newLine = string.Format("{0},{1}", maxSpeed.ToString(), throughput.ToString());
        csv.AppendLine(newLine);

        File.AppendAllText(Directory.GetCurrentDirectory() + "./Assets/Data/data.csv", csv.ToString());
    }

    /// <summary> Set the available spawn locations for the level </summary>
    void setSpawnLocations()
    {
        float groundLevel = 0.5F;
        float lanePadding = 6.7F;
        float startOfLevel = 49F;

        // Spawn locations for the cars
        Vector3 midLane = new Vector3(startOfLevel, groundLevel, 0);
        Vector3 rightLane = new Vector3(startOfLevel, groundLevel, lanePadding);
        Vector3 leftLane = new Vector3(startOfLevel, groundLevel, -lanePadding);

        this.spawnLocations.Add(midLane);

        if (this.lanes == 2)
        {
            this.spawnLocations.Add(leftLane);
        }

        if (this.lanes == 3)
        {
            this.spawnLocations.Add(leftLane);
            this.spawnLocations.Add(rightLane);
        }
    }
}
