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
        float similarity = ColorMatchUtility.CalculatePerceptualMatchPercent(
            wma.windmillColor,
            cgsa._goalColour
        );

        goalSphere.GetComponent<Renderer>().material.color = cgsa._goalColour;
        achievedSphere.GetComponent<Renderer>().material.color = wma.windmillColor;
        procentageText.text = similarity + "%";

        return similarity;
    }
}
