using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Main")]
    public Camera mainCamera;
    public Canvas uiCanvas;

    [Header("Popup")]
    public ModePopup modePopup;
    public ActPopup actPopup;
    public StatusPopup statusPopup1;
    public StatusPopup statusPopup2;
    public AttackPopup attackPopup;

    [Header("Point Mark")]
    [SerializeField] private RectTransform pointMark;
    public float yOffset = 70.0f;

    [Header("Tile Effect")]
    [SerializeField] private GameObject pointEffect;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        modePopup.gameObject.SetActive(true);
    }

    public void SetPointMark(bool isActive)
    {
        pointMark.gameObject.SetActive(isActive);
    }

    public void TrackPoint(Vector3 worldPos)
    {
        Vector2 localPos = WorldToCanvas(mainCamera, uiCanvas, worldPos);
        localPos.y += yOffset;

        pointMark.anchoredPosition = localPos;
        SetPointMark(true);
    }
    public void StopTracking()
    {
        SetPointMark(false);
    }

    public void SetPointEffect(Vector3Int gridPos)
    {
        Vector3 pos = new(gridPos.x, 0.01f, gridPos.z);
        pointEffect.transform.position = pos;

        if (!pointEffect.activeSelf)
            pointEffect.SetActive(true);
    }
    public void ClearPointEffect()
    {
        pointEffect.SetActive(false);
    }

    public Vector2 WorldToCanvas(Camera cam, Canvas canvas, Vector3 worldPos)
    {

        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        Camera uiCamera = canvas.worldCamera;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCamera, out Vector2 localPos);

        return localPos;
    }

    public void UpdateTileUI(Vector3Int? hitTilePos)
    {
        if (hitTilePos.HasValue)
        {
            if (InputManager.instance.lastTilePosition != hitTilePos.Value)
            {
                InputManager.instance.lastTilePosition = hitTilePos;
                SetPointEffect(hitTilePos.Value);
            }
            TrackPoint(hitTilePos.Value);
        }
        else
        {
            InputManager.instance.lastTilePosition = null;
            StopTracking();
            ClearPointEffect();
        }
    }
    public void UpdateUI()
    {
        if (InputManager.instance.lastUnit == null)
        {
            if (InputManager.instance.selectedCharacter == null)
                statusPopup1.Close();
            else
                statusPopup1.SetUp(InputManager.instance.selectedCharacter);

            statusPopup2.Close();
            InputManager.instance.currentTargetIndex = -1;

            return;
        }

        if (InputManager.instance.selectedCharacter == null)
        {
            statusPopup1.SetUp(InputManager.instance.lastUnit);
            statusPopup2.Close();
        }
        else
        {
            statusPopup1.SetUp(InputManager.instance.selectedCharacter);

            if (InputManager.instance.lastUnit == InputManager.instance.selectedCharacter)
                statusPopup2.Close();
            else
                statusPopup2.SetUp(InputManager.instance.lastUnit);
        }
    }
}
