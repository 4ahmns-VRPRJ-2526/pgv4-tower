using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    [SerializeField] private Button[] assignmentCards;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Button btn in assignmentCards)
        {
            ChangeButtonColors(btn);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Methode zum Ändern der Button-Farben für Normal, Highlighted, Selected usw.
    public void ChangeButtonColors(Button btn)
    {
        ColorBlock colors = btn.colors;  // Hole den aktuellen ColorBlock des Buttons

        // Setze die Farben für die verschiedenen Zustände
        //colors.normalColor = Color.white;  // Normale Farbe des Buttons
        //colors.highlightedColor = Color.yellow;  // Farbe beim Hover-Effekt
        //colors.pressedColor = Color.green;  // Farbe beim Klicken
        colors.selectedColor = Color.cyan;  // Farbe wenn der Button ausgewählt ist
        //colors.disabledColor = Color.gray;  // Farbe wenn der Button deaktiviert ist

        // Wende die neuen Farben an
        btn.colors = colors;
    }
}
