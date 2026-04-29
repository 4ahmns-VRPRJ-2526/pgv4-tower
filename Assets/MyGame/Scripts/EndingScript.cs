using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndingScript : MonoBehaviour
{
    public WindmillManager wma;
    public GameManagerScript cgsa;
    [SerializeField] GameObject goalSphere;
    [SerializeField] GameObject achievedSphere;
    [SerializeField] TMP_Text procentageText;


    void Start()
    {

    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Destroy(GameObject.Find("GameManager"));
            Destroy(GameObject.Find("Manager"));
            SceneManager.LoadScene(0);
        }
    }

    float GetColorSimilarityPercentage(Color a, Color b)
    {
        wma = FindObjectOfType<WindmillManager>();
        cgsa = FindObjectOfType<GameManagerScript>();
        float similarity = GetColorSimilarityPercentage(cgsa._goalColour, wma.windmillColor);
        goalSphere.GetComponent<Renderer>().material.color = cgsa._goalColour;
        achievedSphere.GetComponent<Renderer>().material.color = wma.windmillColor;
        procentageText.text = similarity + "%";
        float rDiff = a.r - b.r;
        float gDiff = a.g - b.g;
        float bDiff = a.b - b.b;

        float distance = Mathf.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
        float knappheit = 1f - (distance / Mathf.Sqrt(3f));

        return Mathf.Clamp((float)System.Math.Round(knappheit * 100f, 2), 0f, 100f);

    }
}
