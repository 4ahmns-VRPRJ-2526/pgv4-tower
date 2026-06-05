using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "ScriptableObjects/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    public Sprite[] taskSprites;
    public Department[] departments;
    public Color32[] taskColors = new Color32[]
    {
        new Color32(253, 196, 4, 255), //fdch04 gelb bear Grafik und Medien
        new Color32(90, 160, 216, 255), //5aa0d8 blau cat Bautechnik
        new Color32(0, 167, 102, 255), //00a766 grün owl Maschinenbau
        new Color32(217, 75, 50, 255), //d94b32 rot one Elektronik
        new Color32(186, 198, 52, 255), //bac634 lime two
        new Color32(147, 114, 177, 255), //fdch04 lila three Elektro
        new Color32(0, 0, 0, 255), //000000 black None
        new Color32(0, 0, 0, 255), //000000 black None
    };

    public string[] feedback = new string[]
    {
        "Maximal unterschiedlich!",
        "Ziemlich unterschiedlich!",
        "Ähnlich, aber erkennbar unterschiedlich!",
        "Ähnlich, fast nicht zu unterscheiden!",
        "Die Farben sind komplett identisch!"
    };

    public Color32 GetTargetColor(TaskToDo task)
    {
        switch (task)
        {
            case TaskToDo.GrafikMedien:
                return taskColors[(int)TaskToDo.GrafikMedien];
            case TaskToDo.Bautechnik:
                return taskColors[(int)TaskToDo.Bautechnik];
            case TaskToDo.Maschinenbau:
                return taskColors[(int)TaskToDo.Maschinenbau];
            case TaskToDo.Elektrotechnik:
                return taskColors[(int)TaskToDo.Elektrotechnik];
            case TaskToDo.Abendschule:
                return taskColors[(int)TaskToDo.Abendschule];
            case TaskToDo.ElektronikTechInfo:
                return taskColors[(int)TaskToDo.ElektronikTechInfo];
            case TaskToDo.Informationstechnologie:
                return taskColors[(int)TaskToDo.ElektronikTechInfo];
            default:
                return Color.black;
        }
    }

    public Sprite GetTargetSprite(TaskToDo targetTask)
    {
        switch (targetTask)
        {
            case TaskToDo.GrafikMedien:
                return taskSprites[(int)TaskToDo.GrafikMedien];
            case TaskToDo.Bautechnik:
                return taskSprites[(int)TaskToDo.Bautechnik];
            case TaskToDo.Maschinenbau:
                return taskSprites[(int)TaskToDo.Maschinenbau];
            case TaskToDo.Elektrotechnik:
                return taskSprites[(int)TaskToDo.Elektrotechnik];
            case TaskToDo.Abendschule:
                return taskSprites[(int)TaskToDo.Abendschule];
            case TaskToDo.ElektronikTechInfo:
                return taskSprites[(int)TaskToDo.ElektronikTechInfo];
            default:
                return taskSprites[(int)TaskToDo.ElektronikTechInfo];
        }
    }
}
