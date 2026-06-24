using System;
using System.Globalization;
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

    private static string sessionFilePath;
    private string filePath;

    private void Awake()
    {
        currentUserID = PlayerPrefs.GetInt("SavedUserID", 1111);
        EnsureLogFile();
    }

    public void WriteNewDataToCSV()
    {
        EnsureLogFile();

        string timePart = DateTime.Now.ToString("HHmm");
        string fullID = currentUserID.ToString("D4") + timePart;
        string score = currentUserScore.ToString("0.##", CultureInfo.InvariantCulture);
        string csvLine = fullID + ",\"" + currentUserName + "\"," + score;

        try
        {
            File.AppendAllText(filePath, csvLine + Environment.NewLine);

            currentUserID++;
            PlayerPrefs.SetInt("SavedUserID", currentUserID);
            PlayerPrefs.Save();

            Debug.Log("Line written to file: " + filePath + ". Next User ID saved as: " + currentUserID);
        }
        catch (Exception e)
        {
            Debug.LogError("Error writing to CSV: " + e.Message);
        }
    }

    public void WriteData(int score)
    {
        currentUserScore = score;
        WriteNewDataToCSV();
    }

    public void Update()
    {
        if (Input.GetKeyDown(manualLogKey))
        {
            WriteNewDataToCSV();
        }
    }

    private void EnsureLogFile()
    {
        if (string.IsNullOrEmpty(sessionFilePath))
        {
            string fileTimeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = fileTimeStamp + "_DataLog.csv";
            sessionFilePath = Path.Combine(Application.persistentDataPath, fileName);
        }

        filePath = sessionFilePath;

        if (!File.Exists(filePath))
        {
            string header = "sep=," + Environment.NewLine + "ID,Name,Score" + Environment.NewLine;
            File.WriteAllText(filePath, header);
            Debug.Log("Log File created at: " + filePath);
        }
    }
}
