using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ConfuseUIController
/// - Player의 혼란 상태를 폴링하여 게이지를 표시하고, 플레이어의 머리 위에 UI를 배치합니다.
/// </summary>
public class ConfuseUIController : MonoBehaviour
{
    [Header("UI - 드래그로 연결")]
    public GameObject confusedBar;    // 배경(부모) 오브젝트
    public Image fillGauge;          // 보라색 채움 이미지 (Image 컴포넌트)
    public RectTransform canvasRect;  // Canvas RectTransform (드래그)

    [Header("Player")]
    public Player player;            // Player 스크립트 (수동 드래그 권장)
    public bool autoFindPlayer = true;

    [Header("옵션")]
    [Tooltip("플레이어 위치로부터의 월드 오프셋 (머리 위 위치 조정)")]
    public Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    public bool followPlayer = true;
    public bool debugLogs = false;

    // 내부 캐시
    private RectTransform barRect;
    private Canvas parentCanvas;

    // 월드 좌표를 스크린 좌표로 변환할 때 사용할 카메라
    private Camera worldViewCamera = null;

    void Awake()
    {
        enabled = true;
    }

    void Start()
    {
        // 1. Player 찾기
        if (player == null && autoFindPlayer)
            player = FindFirstObjectByType<Player>();

        // 2. Canvas 및 CanvasRect 찾기
        if (canvasRect == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                canvasRect = parentCanvas.GetComponent<RectTransform>();
            }
        }
        else
        {
            parentCanvas = canvasRect.GetComponent<Canvas>();
        }

        // 3. UI RectTransform 및 초기 상태 설정
        if (confusedBar != null)
        {
            barRect = confusedBar.GetComponent<RectTransform>();
            confusedBar.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[ConfuseUI] confusedBar 미할당!");
            return;
        }

        if (fillGauge != null)
        {
            // 기존 초기화 로직 유지
            if (fillGauge.sprite == null)
            {
                Sprite builtin = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                if (builtin != null) fillGauge.sprite = builtin;
            }
            fillGauge.type = Image.Type.Filled;
            fillGauge.fillMethod = Image.FillMethod.Horizontal;
            fillGauge.fillAmount = 0f;
            fillGauge.color = new Color(0.7f, 0.2f, 1f, 1f);
        }
        else
        {
            Debug.LogWarning("[ConfuseUI] fillGauge 미할당!");
        }

        // 4. 월드 뷰 카메라 결정 (가장 중요한 부분!)
        // 씬을 렌더링하는 메인 카메라를 사용하거나, Screen Space - Camera 모드일 경우 Canvas에 지정된 카메라를 사용합니다.
        worldViewCamera = Camera.main;

        if (parentCanvas != null && parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            if (parentCanvas.worldCamera != null)
            {
                worldViewCamera = parentCanvas.worldCamera;
            }
            else
            {
                Debug.LogError("[ConfuseUI] Canvas가 Screen Space - Camera인데 Render Camera가 할당되지 않았습니다! Camera.main을 사용합니다.");
            }
        }

        if (worldViewCamera == null && debugLogs) Debug.LogWarning("[ConfuseUI] World View Camera가 설정되지 않았습니다! UI 위치 계산이 실패할 수 있습니다.");
    }

    void Update()
    {
        // 널 체크
        if (player == null || confusedBar == null || fillGauge == null || canvasRect == null || worldViewCamera == null)
            return;

        bool isConfused = player.IsControlConfused;

        if (isConfused)
        {
            float remaining = player.GetConfuseTimeRemaining();
            float duration = Mathf.Max(0.0001f, player.GetConfuseDuration());

            if (!confusedBar.activeSelf) confusedBar.SetActive(true);

            // fill amount (남은/전체)
            float ratio = Mathf.Clamp01(remaining / duration);
            fillGauge.fillAmount = ratio;

            // 위치 업데이트
            if (followPlayer) UpdatePositionAbovePlayer();

            // UI가 다른 요소 위에 보이도록 보장
            confusedBar.transform.SetAsLastSibling();
        }
        else
        {
            if (confusedBar.activeSelf) confusedBar.SetActive(false);
        }
    }

    /// <summary>
    /// 플레이어 월드 좌표를 Canvas 로컬 좌표로 변환하여 barRect의 anchoredPosition에 할당
    /// </summary>
    void UpdatePositionAbovePlayer()
    {
        if (barRect == null || canvasRect == null || player == null || worldViewCamera == null) return;

        // 1. 월드 좌표를 스크린 좌표로 변환: 씬을 렌더링하는 카메라(worldViewCamera) 사용
        Vector3 worldPos = player.transform.position + worldOffset;
        Vector2 screenPoint = worldViewCamera.WorldToScreenPoint(worldPos);
        Vector2 localPoint;

        // 2. 스크린 좌표를 Canvas의 로컬 좌표로 변환: Canvas Render Mode에 따라 카메라 인수를 결정
        // ScreenSpaceOverlay 모드일 때만 카메라 인수가 null입니다.
        Camera cameraForLocalConversion = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : worldViewCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cameraForLocalConversion, out localPoint))
        {
            // Canvas 경계에 바가 잘리지 않도록 크기를 고려하여 Clamp
            float halfCanvasW = canvasRect.rect.width * 0.5f;
            float halfCanvasH = canvasRect.rect.height * 0.5f;
            float halfBarW = barRect.rect.width * 0.5f;
            float halfBarH = barRect.rect.height * 0.5f;

            localPoint.x = Mathf.Clamp(localPoint.x, -halfCanvasW + halfBarW, halfCanvasW - halfBarW);
            localPoint.y = Mathf.Clamp(localPoint.y, -halfCanvasH + halfBarH, halfCanvasH - halfBarH);

            barRect.anchoredPosition = localPoint;

            if (debugLogs)
            {
                Debug.Log($"[ConfuseUI] WorldPos: {worldPos} -> ScreenPoint: {screenPoint} -> LocalPoint: {localPoint}");
            }
        }
    }

    [ContextMenu("DebugForceShow")]
    public void DebugForceShow()
    {
        StartCoroutine(DebugShowCoroutine(2f));
    }

    IEnumerator DebugShowCoroutine(float seconds)
    {
        if (confusedBar != null) confusedBar.SetActive(true);
        if (fillGauge != null) fillGauge.fillAmount = 1f;
        yield return new WaitForSeconds(seconds);
        if (confusedBar != null) confusedBar.SetActive(false);
        if (fillGauge != null) fillGauge.fillAmount = 0f;
    }
}