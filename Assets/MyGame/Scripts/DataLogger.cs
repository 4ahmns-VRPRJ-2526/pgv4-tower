using System;
using UnityEngine;
using System.IO;

public class DataLogger : MonoBehaviour
{
    [Header("User Data (Set by External Scripts)")]
    public int currentUserID;
    public string currentUserName = "Wilder Hase";
    public float currentUserScore = 55;

    [Header("Manual Logging")]
    [SerializeField] private KeyCode manualLogKey;

    private string filePath;

    private void Awake()
    {
        // Load the next ID from device memory, or start at 1111 on first launch.
        currentUserID = PlayerPrefs.GetInt("SavedUserID", 1111);

        string fileTimeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = fileTimeStamp + "_DataLog.csv";

        filePath = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(filePath))
        {
            string header = "sep=," + Environment.NewLine + "ID,Name,Score" + Environment.NewLine;
            File.WriteAllText(filePath, header);
            Debug.Log("Log File created at: " + filePath);
        }
    }

    public void WriteNewDataToCSV()
    {
        string timePart = DateTime.Now.ToString("HHmm");
        string fullID = currentUserID.ToString("D4") + timePart;
        string csvLine = fullID + ",\"" + currentUserName + "\"," + currentUserScore;

        try
        {
            File.AppendAllText(filePath, csvLine + Environment.NewLine);

            currentUserID++;
            PlayerPrefs.SetInt("SavedUserID", currentUserID);
            PlayerPrefs.Save();

            Debug.Log("Line written to file. Next User ID saved as: " + currentUserID);
        }
        catch (Exception e)
        {
            Debug.LogError("Error writing to CSV: " + e.Message);
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(manualLogKey))
        {
            WriteNewDataToCSV();
        }
    }
}
