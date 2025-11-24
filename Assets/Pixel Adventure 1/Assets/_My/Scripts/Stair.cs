using UnityEngine;

public enum StairKind
{
    Normal,          // 일반 계단
    MemoryDisappear, // 서서히 사라졌다 다시 나타나는 계단
    ConfuseControl   // 밟으면 좌우 반전(대마왕)
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

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();

        // 플레이어는 Rigidbody2D, 계단은 트리거 충돌로 쓰는 걸 전제로 함
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

        // 스프라이트/알파 초기화
        if (sr != null)
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);

            // 계단 타입에 따라 스프라이트 결정
            if (stairKind == StairKind.ConfuseControl && confuseSprite != null)
                sr.sprite = confuseSprite;
            else if (normalSprite != null)
                sr.sprite = normalSprite;
        }
    }

    // GameManager에서 타입을 바꿀 때 호출
    public void SetKind(StairKind kind)
    {
        stairKind = kind;

        // 대마왕 타입이면 플래그도 자동 true
        isConfuseStair = (kind == StairKind.ConfuseControl);

        ResetStair();
    }

    private void Update()
    {
        // 투명 계단만 페이드 처리
        if (stairKind == StairKind.MemoryDisappear)
        {
            UpdateFadeCycle();
        }
    }

    /// <summary>
    /// 서서히 사라졌다가, 안 보였다가, 다시 서서히 나타나는 사이클
    /// </summary>
    private void UpdateFadeCycle()
    {
        if (sr == null) return;

        cycleTimer += Time.deltaTime;
        float t = cycleTimer;

        float phase1 = visibleDuration;
        float phase2 = phase1 + fadeOutDuration;
        float phase3 = phase2 + hiddenDuration;
        float phase4 = phase3 + fadeInDuration;

        // 한 사이클 끝나면 다시 0부터
        if (t > phase4)
        {
            cycleTimer = 0f;
            t = 0f;
        }

        Color c = sr.color;

        if (t <= phase1)
        {
            // 완전히 보이는 구간
            c.a = 1f;
        }
        else if (t <= phase2)
        {
            // 서서히 사라지는 구간
            float f = (t - phase1) / fadeOutDuration;   // 0 → 1
            c.a = Mathf.Lerp(1f, 0f, f);
        }
        else if (t <= phase3)
        {
            // 완전히 안 보이는 구간
            c.a = 0f;
        }
        else
        {
            // 다시 서서히 나타나는 구간
            float f = (t - phase3) / fadeInDuration;   // 0 → 1
            c.a = Mathf.Lerp(0f, 1f, f);
        }

        sr.color = c;
    }

    /// <summary>
    /// 플레이어가 "위에서" 밟았을 때만 대마왕 발동
    /// </summary>
    /// <summary>
    /// 플레이어가 "위에서" 밟았을 때만 대마왕 발동
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1) Player 아니면 무시
        if (!other.CompareTag("Player"))
            return;

        // 2) 이 계단이 혼란 계단이 아니면 무시
        //    (stairKind 또는 isConfuseStair 둘 중 하나라도 true면 대마왕 계단으로 인정)
        if (!isConfuseStair && stairKind != StairKind.ConfuseControl)
            return;

        // 3) "위에서 밟았는지" 체크 (너무 빡세면 이 조건은 나중에 지워도 됨)
        //    → 플레이어 중심이 계단보다 조금이라도 위에 있을 때만 인정
        if (other.transform.position.y <= transform.position.y)
            return;

        // 4) 실제 발동
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.ActivateConfuseControl(0f); // 0이면 Player 쪽 기본값(예: 15초) 사용
            Debug.Log("[Stair] Confuse stair triggered!");
        }
    }
}