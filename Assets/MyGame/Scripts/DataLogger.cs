using System;
using UnityEngine;
using System.IO;

public class DataLogger : MonoBehaviour
{
    [Header("User Data (Set by External Scripts)")]
    public int currentUserID = 1111; // This is the starting counter
    public string currentUserName = "Wilder Hase";
    public float currentUserScore = 55;

    [Header("Manusl Logging")]
    [SerializeField] private KeyCode manualLogKey;

    private string filePath;

    private void Awake()
    {
        // 1. Create filename with date and time
        string fileTimeStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = fileTimeStamp + "_DataLog.csv";

        // Path logic
        filePath = Path.Combine(Application.persistentDataPath, fileName);

        // 2. Create the file and write the header with the Excel "sep=" fix
        if (!File.Exists(filePath))
        {
            // The first line "sep=," tells Excel specifically to use commas for columns
            string header = "sep=," + Environment.NewLine + "ID,Name,Score" + Environment.NewLine;
            File.WriteAllText(filePath, header);
            Debug.Log("Log File created at: " + filePath);
        }
    }

    public void WriteNewDataToCSV()
    {
        // 3. Format the Time part (HHMM in 24h format)
        string timePart = DateTime.Now.ToString("HHmm");

        // 4. Create the Full ID: 4-digit counter + HHMM
        // "D4" ensures it stays 4 digits (e.g., 1111, 1112...)
        string fullID = currentUserID.ToString("D4") + timePart;

        // 5. Create the CSV line (Directly using commas like your first code)
        // We wrap the name in quotes just in case the name contains a comma
        string csvLine = fullID + ",\"" + currentUserName + "\"," + currentUserScore;

        try
        {
            File.AppendAllText(filePath, csvLine + Environment.NewLine);

            currentUserID++;

            Debug.Log("Line written to file");
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
