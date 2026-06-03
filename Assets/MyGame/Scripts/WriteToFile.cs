using UnityEngine;
using System.IO;

public class WriteToFile : MonoBehaviour
{
    public string parameterToWrite = "Dies ist der zu schreibende Parameter";

    private void Start()
    {
        // Der Pfad zur TXT-Datei, die du erstellen oder überschreiben möchtest
        string filePath = Application.dataPath + "/meineDatei.txt";

        try
        {
            // Versuche, die Datei zu öffnen und zu schreiben
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Schreibe den Parameter in die Datei
                writer.WriteLine(parameterToWrite);
            }

            Debug.Log("Parameter erfolgreich in die Datei geschrieben.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Fehler beim Schreiben in die Datei: " + e.Message);
        }
    }
}