using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(VerticalLayoutGroup))]
[RequireComponent(typeof(RectTransform))]
public class DropdownUI : MonoBehaviour
{
    [SerializeField] private bool dynamicHeight;
    private const float transitionTime = 0.05f;

    private bool isInitialized;
    private bool isShown;
    private float height;
    private RectTransform rectTransform;
    private VerticalLayoutGroup verticalLayoutGroup;
    private ContentSizeFitter contentSizeFitter;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (!isInitialized) gameObject.SetActive(false);
    }

    private void Initialize()
    {
        if (isInitialized) return;

        rectTransform = GetComponent<RectTransform>();
        verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
        contentSizeFitter = GetComponent<ContentSizeFitter>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (!dynamicHeight)
        {
            contentSizeFitter.enabled = false;
            height = CalculateHeight();
        }

        isInitialized = true;
    }

    public void ToggleVisibility()
    {
        if (isShown)
            Hide();
        else
            Show();
    }

    public void Show()
    {
        Initialize();
        isShown = true;
        canvasGroup.interactable = true;
        EventSystem.current.SetSelectedGameObject(gameObject);

        if (dynamicHeight) height = CalculateHeight();

        gameObject.SetActive(true);

        rectTransform.DOKill();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 0);
        rectTransform.DOSizeDelta(new Vector2(rectTransform.sizeDelta.x, height), transitionTime);
    }

    public void HideInstant()
    {
        isShown = false;
        canvasGroup.interactable = false;

        rectTransform.DOKill();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 0);
        gameObject.SetActive(false);
    }

    public void Hide()
    {
        isShown = false;
        canvasGroup.interactable = false;

        rectTransform.DOKill();
        rectTransform.DOSizeDelta(new Vector2(rectTransform.sizeDelta.x, 0), transitionTime).onComplete += () =>
        {
            gameObject.SetActive(false);
        };
    }

    private float CalculateHeight()
    {
        float height = 0;
        foreach (RectTransform t in transform)
        {
            height += t.sizeDelta.y + verticalLayoutGroup.spacing;
        }
        height -= verticalLayoutGroup.spacing;
        return height;
    }
}
