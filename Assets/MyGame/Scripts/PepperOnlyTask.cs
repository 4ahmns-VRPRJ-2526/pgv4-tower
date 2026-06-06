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

    private int CalculatePoints(float matchPercent)
    {
        if (matchPercent >= 90f)
        {
            return 4;
        }

        if (matchPercent >= 75f)
        {
            return 3;
        }

        if (matchPercent >= 50f)
        {
            return 2;
        }

        if (matchPercent >= 20f)
        {
            return 1;
        }

        return 0;
    }

    public void  CalcPoints()
    {
        points = EvaluatePoints(targetTask, playerColor);
    }

    public int EvaluatePoints(TaskToDo cTask, Color32 mixedColor)
    {
        float matchPercent = CalculateColorMatchPercent(targetColor, mixedColor);
        int points = CalculatePoints(matchPercent);
        return points;
    }

    public float CalculateColorMatchPercent(Color32 target, Color32 current)
    {
        return ColorMatchUtility.CalculatePerceptualMatchPercent(current, target);
    }
}

