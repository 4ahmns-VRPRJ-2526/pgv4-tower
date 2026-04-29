using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class WindmillShowHide : MonoBehaviour
{
    public List<Windmill> allWindmills;
    

    public void ShowOnly(Windmill selected)
    {
        foreach (var windmill in allWindmills)
        {
            if (windmill == selected)
            {
                windmill.HighlightLamp(); // ausgewählte Windmühle farbig leuchten lassen
            }
            else
            {
                windmill.DimLamp(); // andere abdämpfen
            }
        }
    }

    public void ShowAll()
    {
        
    }

}
