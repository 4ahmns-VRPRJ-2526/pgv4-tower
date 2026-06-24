using UnityEngine;

public class GlobalGameData : MonoBehaviour
{
    public static GlobalGameData Instance;

    public string currentUserIdString;
    public string currentUserName;
    public string currentPercentScore;

    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }
}
