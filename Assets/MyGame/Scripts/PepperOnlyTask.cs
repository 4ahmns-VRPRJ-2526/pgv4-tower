using UnityEngine;

public enum TaskToDo
{
    GrafikMedien = 0,
    Bautechnik = 1,
    Maschinenbau = 2,
    Elektrotechnik = 3,
    Abendschule = 4,
    ElektronikTechInfo = 5,
    None = 6,
    Informationstechnologie = 7
}

public class PepperOnlyTask : MonoBehaviour
{
    private ItemData peppersGhostData;
    public Color32 targetColor = Color.black;
    public Sprite targetSprite;

    public TaskToDo targetTask = TaskToDo.None;
    public Color32 playerColor = Color.black;
    public int points=-1;

    private void Start()
    {
        peppersGhostData = Resources.Load<ItemData>("PeppersGhostData");
        points = -1;
        if (peppersGhostData != null)
        {
            Debug.Log("Scriptable Object erfolgreich geladen!");
        }
        else
        {
            Debug.LogWarning("Scriptable Object konnte nicht geladen werden.");
        }
    }

    public void SetTask(int task)
    {
        targetTask = (TaskToDo)task;
        targetColor = peppersGhostData.GetTargetColor(targetTask);
        targetSprite = peppersGhostData.GetTargetSprite(targetTask);
    }

    private int CalculatePoints(float euclidDistance)
    {
        int points = -1;

        //Die Farben sind komplett identisch.
        if (euclidDistance >= 0 && euclidDistance <= 10)
        {
            points = 4;
        }
        //Ähnlich, fast nicht zu unterscheiden.
        else if (euclidDistance >= 11 && euclidDistance <= 40)
        {
            points = 3;
        }
        //Ähnlich, aber erkennbar unterschiedlich
        else if (euclidDistance >= 41 && euclidDistance <= 100)
        {
            points = 2;
        }
        //Ziemlich unterschiedlich
        else if (euclidDistance >= 101 && euclidDistance <= 200)
        {
            points = 1;
        }
        //Maximal unterschiedlich, maximale Distanz von etwa 441.67
        else if (euclidDistance >= 201)
        {
            points = 0;
        }

        return points;
    }

    public void  CalcPoints()
    {
        points = EvaluatePoints(targetTask, playerColor);
    }

    public int EvaluatePoints(TaskToDo cTask, Color32 mixedColor)
    {
        int distance = (int)CalculateColorDistance(targetColor, mixedColor);
        int points = CalculatePoints(distance);
        return points;
    }

    public float CalculateColorDistance(Color32 color1, Color32 color2)
    {
        // Differenzen der Farbkanäle
        int rDifference = color2.r - color1.r;
        int gDifference = color2.g - color1.g;
        int bDifference = color2.b - color1.b;

        // Euklidische Distanz berechnen
        float euclidDistance = Mathf.Sqrt(rDifference * rDifference + gDifference * gDifference + bDifference * bDifference);

        return euclidDistance;
    }
}
