using UnityEngine;

/// <summary>
/// 계단 종류
/// </summary>
public enum StairKind
{
    Normal,          // 일반 계단
    MemoryDisappear  // 서서히 사라졌다가 서서히 나타나는 암기용 계단
}

/// <summary>
/// 각 계단(발판)에 붙는 스크립트
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Stair : MonoBehaviour
{
    [Header("기본 설정")]
    public StairKind stairKind = StairKind.Normal;  // 이 계단이 어떤 타입인지

    [Header("암기용(투명/재등장) 계단 설정")]
    [Tooltip("완전히 보이는 상태에서 완전히 안 보이는 상태까지 걸리는 시간(초)")]
    public float fadeOutDuration = 2.0f;   // 사라지는 속도: 2초 동안 서서히

    [Tooltip("완전히 안 보이는 상태에서 다시 완전히 보이는 상태까지 걸리는 시간(초)")]
    public float fadeInDuration = 0.8f;    // 나타나는 속도: 0.8초 정도로 비교적 빠르게

    private SpriteRenderer sr;
    private float spawnTime;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // 콜라이더는 "트리거"로 사용하는 걸 추천 (Player가 올라가면 OnTriggerEnter2D 호출)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnEnable()
    {
        // 계단이 새로 활성화될 때마다 시간/알파 초기화
        spawnTime = Time.time;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f;      // 시작은 항상 완전 보이는 상태
            sr.color = c;
        }
    }

    private void Update()
    {
        // 암기용 계단이면 페이드 인/아웃 루프 실행
        if (stairKind == StairKind.MemoryDisappear)
        {
            UpdateFadeLoop();
        }

        // (필요하면 여기 아래에 다른 Update 로직 추가)
    }

    /// <summary>
    /// 서서히 사라졌다가(1→0), 서서히 다시 나타나는(0→1) 루프
    /// </summary>
    private void UpdateFadeLoop()
    {
        if (sr == null) return;

        float cycle = fadeOutDuration + fadeInDuration;
        if (cycle <= 0f) return;

        float elapsed = Time.time - spawnTime;
        float t = elapsed % cycle;   // 현재 1사이클 내 위치

        Color c = sr.color;

        if (t <= fadeOutDuration)
        {
            // 페이드 아웃 구간: 1 → 0
            float outT = fadeOutDuration > 0f ? t / fadeOutDuration : 1f;
            outT = Mathf.Clamp01(outT);
            c.a = 1f - outT;   // 1에서 0으로 감소
        }
        else
        {
            // 페이드 인 구간: 0 → 1
            float inTime = t - fadeOutDuration;                 // 페이드 인 진행 시간
            float inT = fadeInDuration > 0f ? inTime / fadeInDuration : 1f;
            inT = Mathf.Clamp01(inT);
            c.a = inT;          // 0에서 1로 증가
        }

        sr.color = c;
    }

    /// <summary>
    /// Player가 이 계단 위로 올라왔을 때 호출
    /// - Player에게 "지금 너는 이 계단 밟고 있다" 라고 알려줌
    /// - Player 쪽에는 SetCurrentStair(MonoBehaviour stair) 가 있어야 함
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.SetCurrentStair(this);
        }
    }

    /// <summary>
    /// GameManager에서 계단을 재사용할 때(위치 다시 잡을 때) 호출해 주면
    /// 타이밍이 다시 처음부터 시작됨.
    /// </summary>
    public void ResetStair()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        // 타이머 리셋
        spawnTime = Time.time;

        // 알파를 다시 1로
        Color c = sr.color;
        c.a = 1f;
        sr.color = c;

        enabled = true;
    }
}
