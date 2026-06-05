using UnityEngine;

[CreateAssetMenu(fileName = "NewDepartmentData", menuName = "ScriptableObjects/DepartmentData", order = 1)]
public class Department : ScriptableObject
{
    public string departmentName;
    public string hexColor;
    public Color32 rgbColor;
    public string colorName;
    public int x;
    public int y;

    // Start is called before the first frame update
    void Start()
    {
        name = "not set";
    }
}
