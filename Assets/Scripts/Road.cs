﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Class <c>Road</c> handles level updates.
/// </summary>
public class Road : MonoBehaviour
{   
    [System.NonSerialized]
    public int std = 10;

    [System.NonSerialized]
    public List<float> carSpeeds = new List<float>();

    [System.NonSerialized]
    public int carsFinishedTrack = 0;

    public GameObject carPrefab;
    public int carCount = 10;
    public int maxSpeed = 100;
    private float roadDensity = 30;

    private System.Random rnd = new System.Random();
    private List<Vector3> spawnLocations = new List<Vector3>();
    private int currentCarCount = 0;
    
    void Start()
    {
        Application.runInBackground = true;        
        setSpawnLocations();
        
        // !NOTE: Create a variable currentCarCount so we can change it without the unity options GUI
        this.currentCarCount = this.carCount;

        // !NOTE: Spawn cars as a coroutine (a side-process so we can run the level in the meantime)
        StartCoroutine(spawnCars());
    }

    /// <summary> This method spawns the car at a spawnpoint defined in the "setSpawnLocations()" function </summary>
    IEnumerator spawnCars()
    {
        for (int i = 0; i < this.currentCarCount; i++)
        {
            int r = rnd.Next(this.spawnLocations.Count);
            Vector3 spawnLocation = this.spawnLocations[r];

            if (Physics.OverlapSphere(spawnLocation, 20F).Length <= 1) {
                GameObject car = Instantiate(carPrefab, spawnLocation, Quaternion.identity);
                car.GetComponent<Car>().lane = r;
            } else {
                // Run once every frame
                yield return new WaitForSeconds(1/this.roadDensity);
                this.currentCarCount += 1;
            }
        }
    }

    /// <summary> Append the current run data to a .csv file </summary>
    public void saveData() {
        Debug.Log("Saving data");

        StringBuilder csv = new StringBuilder();

        string newLine = string.Format("{0},{1}", maxSpeed.ToString(), this.carSpeeds.Average().ToString());
        csv.AppendLine(newLine);

        File.AppendAllText(Directory.GetCurrentDirectory() + "/Assets/Data/data_" + this.carCount + ".csv", csv.ToString());
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary> Set the available spawn locations for the level </summary>
    void setSpawnLocations()
    {
        float groundLevel = 0.5F;
        float startOfLevel = transform.position.x + (transform.localScale.x * 5);

        // Spawn location for the cars
        Vector3 midLane = new Vector3(startOfLevel, groundLevel, 0);
        this.spawnLocations.Add(midLane);
    }
}
