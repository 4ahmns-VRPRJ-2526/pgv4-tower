using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class GameExitManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private float holdColorDelay = 2;
    [SerializeField] private float holdThreshold = 5f;
    [SerializeField] private float autoCloseDelay = 3.0f;
    [SerializeField] private Color colorOfImageComponent;

    [SerializeField] private bool useScaleAnimation;

    private bool isPointerDown = false;
    private float pointerDownTimer = 0f;
    private bool holdTriggered = false;
    private Coroutine autoCloseCoroutine;

    public void OnPointerDown(PointerEventData eventData) { isPointerDown = true; holdTriggered = false; pointerDownTimer = 0f; }
    public void OnPointerUp(PointerEventData eventData) { isPointerDown = false; }

    private void Start()
    {
        targetObject.SetActive(false);
    }

    private void Update()
    {
        float alpha = isPointerDown ? Mathf.Clamp01(pointerDownTimer / holdThreshold) : 0;

        if (pointerDownTimer >= holdColorDelay && !targetObject.active)
        {
            GetComponent<Image>().color = new Color(colorOfImageComponent.r, colorOfImageComponent.g, colorOfImageComponent.b, alpha);
            if (useScaleAnimation) { GetComponent<Transform>().localScale = new Vector3(alpha, alpha, 1f); }
        }

        if (isPointerDown && !holdTriggered && !targetObject.active)
        {
            pointerDownTimer += Time.deltaTime;
            if (pointerDownTimer >= holdThreshold + holdColorDelay)
            {
                holdTriggered = true;
                ActivateTarget();

                pointerDownTimer = 0f;

                GetComponent<Image>().color = new Color(colorOfImageComponent.r, colorOfImageComponent.g, colorOfImageComponent.b, 0);
                if (useScaleAnimation) { GetComponent<Transform>().localScale = new Vector3(1f, 1f, 1f); }
            }
        }
    }

    private void ActivateTarget()
    {
        if (targetObject == null) return;
        targetObject.SetActive(true);
        if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);
        autoCloseCoroutine = StartCoroutine(DeactivateAfterDelay());
    }

    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        targetObject.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();

        Debug.LogWarning("Game should have ended by now");
    }
}