using System.Collections;
using UnityEngine;

public enum StairKind
{
    Normal,
    MemoryDisappear,
    ConfuseControl
}

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Stair : MonoBehaviour
{
    [Header("기본 설정")]
    public StairKind stairKind = StairKind.Normal;

    [Header("암기용(투명화) 계단 설정")]
    [Tooltip("처음 완전히 보이는 시간(초)")]
    public float visibleDuration = 0.7f;
    [Tooltip("서서히 사라지는 시간(초)")]
    public float fadeOutDuration = 0.7f;
    [Tooltip("완전히 안 보이는 상태로 유지되는 시간(초)")]
    public float hiddenDuration = 1.5f;
    [Tooltip("다시 서서히 나타나는 시간(초)")]
    public float fadeInDuration = 0.7f;

    [Header("대마왕(혼란) 계단 설정")]
    [Tooltip("이 계단이 대마왕 계단인지 여부(랜덤 말고 직접 지정하고 싶을 때만 체크)")]
    public bool isConfuseStair = false;
    [Tooltip("일반 계단 스프라이트")]
    public Sprite normalSprite;
    [Tooltip("대마왕 계단 스프라이트")]
    public Sprite confuseSprite;

    private SpriteRenderer sr;
    private BoxCollider2D col;

    // 투명 계단용 타이머
    private float cycleTimer = 0f;

    [Header("착지(안착) 판정 설정")]
    [Tooltip("플레이어 발(콜라이더 하단)이 계단 윗면에 얼마나 근접해야 하는지 (유닛)")]
    public float standEps = 0.05f;
    [Tooltip("플레이어와 계단의 가로 겹침 비율(플레이어 폭 대비). 0~1")]
    public float requiredOverlapRatio = 0.5f;
    [Tooltip("플레이어가 '거의 정지'로 간주되는 최소 고정시간(초) — 이 시간 동안 위치 변화가 작아야 안착으로 봄")]
    public float settledTime = 0.06f;
    [Tooltip("감시 최대 시간(초). 이 시간 지나면 감시 중단(안착 못함)")]
    public float monitorTimeout = 1.0f;
    [Tooltip("플레이어가 위로 튀는 속도를 무시할 임계값")]
    public float upwardIgnoreVelocity = 1.0f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();

        if (col != null)
            col.isTrigger = true;
    }

    private void OnEnable()
    {
        ResetStair();
    }

    public void ResetStair()
    {
        cycleTimer = 0f;
        if (sr != null)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
            if (stairKind == StairKind.ConfuseControl && confuseSprite != null)
                sr.sprite = confuseSprite;
            else if (normalSprite != null)
                sr.sprite = normalSprite;
        }
    }

    public void SetKind(StairKind kind)
    {
        stairKind = kind;
        isConfuseStair = (kind == StairKind.ConfuseControl);
        ResetStair();
    }

    private void Update()
    {
        if (stairKind == StairKind.MemoryDisappear)
            UpdateFadeCycle();
    }

    private void UpdateFadeCycle()
    {
        if (sr == null) return;

        cycleTimer += Time.deltaTime;
        float t = cycleTimer;

        float phase1 = visibleDuration;
        float phase2 = phase1 + fadeOutDuration;
        float phase3 = phase2 + hiddenDuration;
        float phase4 = phase3 + fadeInDuration;

        if (t > phase4)
        {
            cycleTimer = 0f;
            t = 0f;
        }

        Color c = sr.color;

        if (t <= phase1) c.a = 1f;
        else if (t <= phase2) c.a = Mathf.Lerp(1f, 0f, (t - phase1) / fadeOutDuration);
        else if (t <= phase3) c.a = 0f;
        else c.a = Mathf.Lerp(0f, 1f, (t - phase3) / fadeInDuration);

        sr.color = c;
    }

    // ---------------------------
    // 트리거 진입: 플레이어가 들어오면 '착지 모니터' 코루틴 시작
    // ---------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!isConfuseStair && stairKind != StairKind.ConfuseControl)
            return;

        // 시작: 코루틴으로 "완전 착지" 검사 (한 플레이어에 대해 동시에 여러 시작 방지)
        StartCoroutine(MonitorPlayerLandingAndActivate(other));
    }

    private IEnumerator MonitorPlayerLandingAndActivate(Collider2D playerCollider)
    {
        if (playerCollider == null) yield break;

        float elapsed = 0f;
        float stableTimer = 0f;

        // 이전 프레임 위치(안정 판정용)
        Vector3 prevPos = playerCollider.transform.position;

        // 반복 검사: 타임아웃 또는 발동 성공 시 종료
        while (elapsed < monitorTimeout)
        {
            // 콜라이더가 유효하고 트리거 범위 안에 있는지(대략) 확인 — 만약 플레이어가 이미 나갔으면 종료
            if (playerCollider == null)
                yield break;

            // Bounds 가져오기
            Bounds playerBounds = playerCollider.bounds;
            Bounds stairBounds = col.bounds;

            // 1) 플레이어 바닥이 계단 윗면에 거의 닿아있는지
            bool verticalOK = playerBounds.min.y >= stairBounds.max.y - standEps;

            // 2) 가로 겹침 비율 확인
            float overlapX = Mathf.Min(playerBounds.max.x, stairBounds.max.x) - Mathf.Max(playerBounds.min.x, stairBounds.min.x);
            float overlapRatio = (playerBounds.size.x > 0f) ? (overlapX / playerBounds.size.x) : 0f;
            bool horizontalOK = (overlapX > 0f) && (overlapRatio >= requiredOverlapRatio);

            // 3) 위로 튀는 중인지 검사 (있으면 대기)
            Rigidbody2D rb = playerCollider.attachedRigidbody;
            bool notJumping = true;
            if (rb != null)
            {
                if (rb.linearVelocity.y > upwardIgnoreVelocity)
                    notJumping = false;
            }

            // 4) 플레이어가 "거의 정지" 상태인지: transform 위치 변화가 작아야 함
            Vector3 curPos = playerCollider.transform.position;
            float posDelta = Vector3.Distance(curPos, prevPos);
            prevPos = curPos;

            if (posDelta < 0.001f) // 매우 작게 움직이면 안정으로 간주하여 stableTimer 누적
            {
                stableTimer += Time.deltaTime;
            }
            else
            {
                stableTimer = 0f;
            }

            bool settled = stableTimer >= settledTime;

            // 조건 모두 충족하면 발동
            if (verticalOK && horizontalOK && notJumping && settled)
            {
                Player player = playerCollider.GetComponent<Player>();
                if (player != null)
                {
                    player.ActivateConfuseControl(0f); // 0 => Player의 default duration 사용
                    Debug.Log("[Stair] Confuse stair triggered after landing.");
                }
                yield break;
            }

            // 플레이어가 트리거 밖으로 나갔는지 확인: 간단히 y 기준으로 많이 벗어나면 중단
            // (정확하게는 OnTriggerExit2D가 호출되며, 그쪽에서 중단할 수도 있음)
            if (!playerCollider.bounds.Intersects(col.bounds))
            {
                // 나갔다고 판단하면 더 이상 감시하지 않음
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 타임아웃으로 종료 (착지 못함)
        yield break;
    }
}
