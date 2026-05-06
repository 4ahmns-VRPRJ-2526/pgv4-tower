using UnityEngine;

public enum PlayMode
{
    Koopterative,
    VROnly,
    PepperOnly,
    None
}

public class Session : MonoBehaviour
{
    public string PlayerName { set; get; }
    public string zeitstempel;
    public PlayMode mode;

    [SerializeField] private PepperOnlyTask[] tasks;
    private int maxRounds = 4;
    [SerializeField] private int currentRound = -1;

    public PepperOnlyTask GetRoundTask(int round)
    {
        return tasks[round];
    }

    // Nur get, kein set -> Wert kann nur gelesen werden
    public int MaxRounds
    {
        get { return maxRounds; }
    }

    public void SetInfos(string name, string zeit, PlayMode modus)
    {
        gameObject.name = "session";
        PlayerName = name;
        zeitstempel = zeit;
        mode = modus;

        tasks = GenerateTaskArray(maxRounds);
    }

    public int SumTaskResults()
    {
        int sum = 0;
        foreach(PepperOnlyTask a in tasks)
        {
            if (a.points == -1) break;
            sum += a.points;
        }
        return sum;
    }

    public void SetFirstRound()
    {
        currentRound = 0;
    }

    private PepperOnlyTask CreateNewTask()
    {
        GameObject obj = new GameObject();
        obj.AddComponent<PepperOnlyTask>();
        return obj.GetComponent<PepperOnlyTask>();
    }

    public void SetTask(TaskToDo task)
    {
        tasks[currentRound].SetTask((int)task);
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }

    public void IncreaseRound()
    {
        currentRound++;
    }

    private PepperOnlyTask[] GenerateTaskArray(int maxRounds)
    {
        PepperOnlyTask[] tasks = new PepperOnlyTask[maxRounds];
        for(int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = CreateNewTask();
            tasks[i].name = "task" + i;
        }

        return tasks;
    }
}
