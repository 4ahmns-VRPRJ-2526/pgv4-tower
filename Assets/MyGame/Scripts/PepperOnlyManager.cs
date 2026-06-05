using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class PepperOnlyManager : MonoBehaviour
{
    [SerializeField] private Canvas welcome, auftrag, endscreen;
    [SerializeField] private TMP_Text playerNameWelcome, playerNameAuftrag, playerNameEnd;
    [SerializeField] private TMP_Text taskPoints;
    [SerializeField] private TMP_Text[] roundPoints;
    [SerializeField] private Image[] roundImgsAuftrag;
    [SerializeField] private Image[] endSceneTaskImages;
    [SerializeField] private TMP_Text[] rgbTarget0, rgbTarget1, rgbTarget2, rgbTarget3;
    [SerializeField] private TMP_Text[] rgbPlayer0, rgbPlayer1, rgbPlayer2, rgbPlayer3;
    [SerializeField] private TMP_Text[] playerPoints;
    [SerializeField] private MixObjectColor mixColorObj;
    [SerializeField] private Sprite[] spritesAuftrag;
    [SerializeField] private TMP_Text sumPoints;
    [SerializeField] private Button[] colorMixButtons;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private ParticleSystem colorOnModel;
    [SerializeField] private Material materialColorizeObj;
    [SerializeField] private AudioSource asPaint;

    private GameObject current3DBalloon;
    

    private Session currentSession;

    private void Start()
    {
        if(auftrag != null)
        {
            if (auftrag.gameObject.activeSelf)
            {
                auftrag.gameObject.SetActive(false);
            }
        }

        welcome.gameObject.SetActive(true);
    }


    //WelcomeSene BTN Start. 
    public void StartSession()
    {
        DateTime currentDateTime = DateTime.Now;
        string customFormat = currentDateTime.ToString("yyyy-MM-dd-HH:mm:ss");

        currentSession = new GameObject().AddComponent<Session>();
        currentSession.SetInfos(playerNameWelcome.text, customFormat, PlayMode.PepperOnly);
        currentSession.SetFirstRound();

        SetUIRoundsUnplayed();
        EnableTaskCanvas();

        //UpdateUI in Welcome-Canvas
        playerNameAuftrag.text = currentSession.PlayerName;
    }

    //Wir von Card in AuftragCanvas aufgerufen. 
    public void GenerateTask(int task)
    {
        currentSession.SetTask((TaskToDo)task);
        auftrag.gameObject.SetActive(false);
        SetMixColorButtonsInteractable(true);
    }

    public void SetCurrentBalloon(GameObject balloon)
    {
        current3DBalloon = balloon;
    }

    public void SetColorToBalloon()
    {
        colorOnModel.GetComponent<ParticleSystem>().Play();
        current3DBalloon.GetComponent<Renderer>().material.color = mixColorObj.GetPlayerMixedColor();
        //colorOnModel.Stop();
    }

    public void SetMixColorButtonsInteractable(bool active)
    {
        for(int i = 0; i < colorMixButtons.Length; i++)
        {
            colorMixButtons[i].interactable = active;
        }
    }

    public bool IsInTaskMode()
    {
        return auftrag.gameObject.activeSelf;
    }

    public void EnableTaskCanvas()
    {
        EventSystem.current.SetSelectedGameObject(null);
        playerNameAuftrag.text = currentSession.PlayerName;
        
        int currentRound = currentSession.GetCurrentRound();
        roundImgsAuftrag[currentRound].color = Color.white;

        if(currentRound >= currentSession.MaxRounds-1)
        {
            SetEndScreenInfos();
            ActivateScreen(endscreen.gameObject);
        }
        else
        {
            for (int i = 0; i < currentSession.MaxRounds; i++)
            {
                if (currentSession.GetRoundTask(i).points == -1) break;
                roundPoints[i].text = currentSession.GetRoundTask(i).points.ToString();
            }

            ActivateScreen(auftrag.gameObject);
        }
    }

    public void SetEndScreenInfos()
    {
        for (int i = 0; i < currentSession.MaxRounds; i++)
        {
            PepperOnlyTask tmpTask = currentSession.GetRoundTask(i);

            playerNameEnd.text = currentSession.PlayerName;
            
            Sprite taskSprite = tmpTask.targetSprite;
            endSceneTaskImages[i].sprite = taskSprite;

            playerPoints[i].text = tmpTask.points.ToString();

            TMP_Text[] tmpRGBTarget = new TMP_Text[3];
            TMP_Text[] tmpRGBPlayer = new TMP_Text[3];

            switch (i)
            {
                case 0:
                    tmpRGBTarget = rgbTarget0;
                    tmpRGBPlayer = rgbPlayer0;
                    break;
                case 1:
                    tmpRGBTarget = rgbTarget1;
                    tmpRGBPlayer = rgbPlayer1;
                    break;
                case 2:
                    tmpRGBTarget = rgbTarget2;
                    tmpRGBPlayer = rgbPlayer2;
                    break;
                case 3:
                    tmpRGBTarget = rgbTarget3;
                    tmpRGBPlayer = rgbPlayer3;
                    break;
            }

            tmpRGBTarget[0].text = tmpTask.targetColor.r.ToString();
            tmpRGBTarget[1].text = tmpTask.targetColor.g.ToString();
            tmpRGBTarget[2].text = tmpTask.targetColor.b.ToString();

            tmpRGBPlayer[0].text = tmpTask.playerColor.r.ToString();
            tmpRGBPlayer[1].text = tmpTask.playerColor.g.ToString();
            tmpRGBPlayer[2].text = tmpTask.playerColor.b.ToString();
        }
        sumPoints.text = currentSession.SumTaskResults().ToString();
    }

    private void SetUIRoundsUnplayed()
    {
        for(int i = 0; i < roundImgsAuftrag.Length; i++)
        {
            roundImgsAuftrag[i].color = Color.gray;
            roundPoints[i].text = "--";
        }
    }

    private void ActivateScreen(GameObject objToActivate)
    {
        Canvas[] allCanvases = { welcome, auftrag, endscreen };

        if (objToActivate.gameObject.GetComponent<Canvas>() == null) return;

        foreach (Canvas canvas in allCanvases)
        {
            canvas.gameObject.SetActive(canvas.gameObject == objToActivate);
        }

    }

    public void SwitchCanvas()
    {
        if (IsInTaskMode()) return;

        if (currentSession.GetRoundTask(currentSession.GetCurrentRound()).points == -1)
        {
            SetMixColorButtonsInteractable(false);

            //SetTaskInfos for Task, before Task Canvas access it. 
            PepperOnlyTask cptask = currentSession.GetRoundTask(currentSession.GetCurrentRound());
            cptask.playerColor = mixColorObj.GetPlayerMixedColor();
            cptask.CalcPoints();
            mixColorObj.ResetPipeStation();

            EnableTaskCanvas();
            currentSession.IncreaseRound();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //SetColorToBalloon();
            materialColorizeObj.color = mixColorObj.GetPlayerMixedColor();
            StartEffectAndChangeColor();
        }
    }

    public void StartEffectAndChangeColor()
    {
        // Coroutine starten
        StartCoroutine(PlayEffectAndChangeColor());
    }

    private IEnumerator PlayEffectAndChangeColor()
    {
        if (colorOnModel.GetComponent<ParticleSystem>().isPlaying)
        {
            yield break;
        }
        else
        {
            if (asPaint.isPlaying)
            {
                yield break;
            }
            asPaint.Play();
            // Particle Effect abspielen
            colorOnModel.GetComponent<ParticleSystem>().Play();

            // 2 Sekunden warten
            yield return new WaitForSeconds(2);

            current3DBalloon.GetComponent<Renderer>().material.color = mixColorObj.GetPlayerMixedColor();
            colorOnModel.GetComponent<ParticleSystem>().Stop();
            asPaint.Stop();
        }
       
    }
}
