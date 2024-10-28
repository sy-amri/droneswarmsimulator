// Syahir Amri bin Mohd Azha, 22007728

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartBtn : MonoBehaviour
{
    [SerializeField] private GameObject titleObject;
    [SerializeField] private Sprite startBtn1;
    [SerializeField] private Sprite startBtn2;
    [SerializeField] private GameObject flockParent;
    [SerializeField] private GameObject fpsCounter;

    // Additional UI elements
    [SerializeField] private GameObject searchDroneInput;
    [SerializeField] private GameObject searchBtn;
    [SerializeField] private GameObject terminateBtn;
    [SerializeField] private GameObject resultsText;
    [SerializeField] private GameObject fpsText;

    private GameObject startButtonObject;
    private Button startButton;
    private Image buttonImage;

    [SerializeField] private float fadeTime = 0.3f;
    [SerializeField] private float titleMoveDistance = 100f;

    [SerializeField] private float wobbleSpeed = 1.5f;
    [SerializeField] private float wobbleAngleMin = -3f;
    [SerializeField] private float wobbleAngleMax = 3f;

    private CanvasGroup titleCanvasGroup;
    private CanvasGroup buttonCanvasGroup;
    private RectTransform titleRectTransform;
    private Vector2 titleStartPosition;
    private Quaternion titleStartRotation;

    private bool isWobbling = true;
    private Coroutine wobbleCoroutine;

    void Start()
    {
        startButtonObject = gameObject;
        startButton = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        if (flockParent != null)
        {
            flockParent.SetActive(false);
        }
        if (fpsCounter != null)
        {
            fpsCounter.SetActive(false);
        }

        // Hide additional UI elements initially
        if (searchDroneInput != null) searchDroneInput.SetActive(false);
        if (searchBtn != null) searchBtn.SetActive(false);
        if (terminateBtn != null) terminateBtn.SetActive(false);
        if (resultsText != null) resultsText.SetActive(false);
        if (fpsText != null) fpsText.SetActive(false);

        if (buttonImage != null && startBtn1 != null)
        {
            buttonImage.sprite = startBtn1;
        }

        buttonCanvasGroup = GetComponent<CanvasGroup>();
        if (buttonCanvasGroup == null)
            buttonCanvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (titleObject != null)
        {
            titleCanvasGroup = titleObject.GetComponent<CanvasGroup>();
            if (titleCanvasGroup == null)
                titleCanvasGroup = titleObject.AddComponent<CanvasGroup>();

            titleRectTransform = titleObject.GetComponent<RectTransform>();
            titleStartPosition = titleRectTransform.anchoredPosition;
            titleStartRotation = titleRectTransform.rotation;

            wobbleCoroutine = StartCoroutine(WobbleTitle());
        }

        if (titleObject != null)
            titleObject.SetActive(true);
        startButtonObject.SetActive(true);

        buttonCanvasGroup.alpha = 1f;
        if (titleCanvasGroup != null)
            titleCanvasGroup.alpha = 1f;

        startButton.onClick.AddListener(OnStartButtonClick);
    }

//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
// Safiqur Rahman bin Rowther Neine, 22008929

    void OnStartButtonClick()
    {
        startButton.interactable = false;
        isWobbling = false;

        if (flockParent != null)
        {
            flockParent.SetActive(true);
        }

        // Show FPSCounter and additional UI elements
        if (fpsCounter != null) fpsCounter.SetActive(true);
        if (searchDroneInput != null) searchDroneInput.SetActive(true);
        if (searchBtn != null) searchBtn.SetActive(true);
        if (terminateBtn != null) terminateBtn.SetActive(true);
        if (resultsText != null) resultsText.SetActive(true);
        if (fpsText != null) fpsText.SetActive(true);

        if (buttonImage != null && startBtn2 != null)
        {
            buttonImage.sprite = startBtn2;
        }

        if (wobbleCoroutine != null)
            StopCoroutine(wobbleCoroutine);

        StartCoroutine(FadeOutElements());
    }

    IEnumerator WobbleTitle()
    {
        float time = 0f;

        while (isWobbling)
        {
            time += Time.deltaTime * wobbleSpeed;

            float t = (Mathf.Sin(time) + 1f) / 2f;
            t = t * t * (3f - 2f * t);

            float currentAngle = Mathf.Lerp(wobbleAngleMin, wobbleAngleMax, t);

            if (titleRectTransform != null)
            {
                titleRectTransform.rotation = titleStartRotation *
                    Quaternion.Euler(0f, 0f, currentAngle);
            }

            yield return null;
        }
    }


//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
// Adam Marwan bin Husin Albasri, 22009203
    IEnumerator FadeOutElements()
    {
        float elapsedTime = 0f;
        Quaternion startRotation = titleRectTransform.rotation;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeTime;

            buttonCanvasGroup.alpha = 1f - normalizedTime;

            if (titleCanvasGroup != null)
            {
                titleCanvasGroup.alpha = 1f - normalizedTime;
                Vector2 newPosition = titleStartPosition +
                    new Vector2(0f, titleMoveDistance * normalizedTime);
                titleRectTransform.anchoredPosition = newPosition;
                titleRectTransform.rotation = Quaternion.Lerp(startRotation,
                    titleStartRotation, normalizedTime);
            }

            yield return null;
        }

        buttonCanvasGroup.alpha = 0f;
        if (titleCanvasGroup != null)
        {
            titleCanvasGroup.alpha = 0f;
            titleRectTransform.anchoredPosition = titleStartPosition +
                new Vector2(0f, titleMoveDistance);
            titleRectTransform.rotation = titleStartRotation;
        }

        startButtonObject.SetActive(false);
        if (titleObject != null)
            titleObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartButtonClick);

        isWobbling = false;
    }
}