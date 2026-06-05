using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VRButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private MixObjectColor objColor;
    private ColorRGB myColor;

    private bool isHolding = false;

    private void Start()
    {
        objColor = GameObject.FindObjectOfType<MixObjectColor>();

        if (gameObject.name.Contains("Red"))
        {
            myColor = ColorRGB.Red;
        }
        else if (gameObject.name.Contains("Blue"))
        {
            myColor = ColorRGB.Blue;
        }
        else if (gameObject.name.Contains("Green"))
        {
            myColor = ColorRGB.Green;
        }
    }


    private void Update()
    {
        if (isHolding)
        {
            objColor.IncreaseFillState(myColor);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        Debug.Log("Button is being held down.");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        Debug.Log("Button is released.");
    }
}
