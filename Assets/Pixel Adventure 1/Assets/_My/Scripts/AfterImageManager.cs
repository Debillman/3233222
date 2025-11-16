using UnityEngine;
using System.Collections;

public class AfterimageManager : MonoBehaviour
{
    [Header("기본 설정")]
    public GameObject player;                // 플레이어 오브젝트
    public float spawnInterval = 0.04f;      // 잔상 생성 간격 (초)
    public float afterimageLifetime = 0.35f; // 잔상이 유지되는 시간

    // 기본 잔상 색상 (부드러운 청록 계열, 알파로 투명도 조절)
    public Color baseColor = new Color(0.6f, 0.9f, 0.9f, 0.55f);

    [Header("속도 기반 밝기 조절")]
    public float colorBoostThreshold = 4f;        // 이 속도 이상이면 조금 더 밝게
    public float maxSpeedBrightnessMultiplier = 1.4f; // 최대 속도 시 밝기 배율

    private SpriteRenderer playerRenderer;
    private Rigidbody2D rb;
    private bool isActive = false;

    void Start()
    {
        if (player != null)
        {
            playerRenderer = player.GetComponent<SpriteRenderer>();
            rb = player.GetComponent<Rigidbody2D>(); // 없으면 null 그대로 둠
        }
    }

    // 잔상 생성 시작 (Player에서 호출)
    public void StartAfterimage()
    {
        if (!isActive && playerRenderer != null)
        {
            StartCoroutine(SpawnAfterimages());
        }
    }

    // 잔상 생성 중단 (Player에서 호출)
    public void StopAfterimage()
    {
        isActive = false;
    }

    // 잔상 반복 생성 코루틴
    IEnumerator SpawnAfterimages()
    {
        isActive = true;

        while (isActive)
        {
            CreateAfterimage();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // 잔상 1개 생성
    void CreateAfterimage()
    {
        if (player == null || playerRenderer == null)
            return;

        GameObject after = new GameObject("Afterimage");
        SpriteRenderer sr = after.AddComponent<SpriteRenderer>();

        // 스프라이트/방향 복사
        sr.sprite = playerRenderer.sprite;
        sr.flipX = playerRenderer.flipX;

        // 속도에 따른 "밝기"만 살짝 조정
        float speed = 0f;
        if (rb != null)
        {
            speed = rb.linearVelocity.magnitude; // linearVelocity 말고 velocity 사용
        }

        float t = Mathf.Clamp01(speed / colorBoostThreshold);
        float brightness = Mathf.Lerp(1f, maxSpeedBrightnessMultiplier, t);

        // 기본 색에 밝기만 곱해 줌 (색상 톤은 유지)
        Color c = baseColor;
        c.r = Mathf.Clamp01(c.r * brightness);
        c.g = Mathf.Clamp01(c.g * brightness);
        c.b = Mathf.Clamp01(c.b * brightness);

        sr.color = c;

        // 크기/정렬/위치 맞추기
        after.transform.localScale = player.transform.localScale * 1.05f;
        sr.sortingLayerID = playerRenderer.sortingLayerID;
        sr.sortingOrder = playerRenderer.sortingOrder - 1;
        after.transform.position = player.transform.position;

        StartCoroutine(FadeAndDestroy(after, sr));
    }

    // 잔상 페이드아웃 및 삭제
    IEnumerator FadeAndDestroy(GameObject obj, SpriteRenderer sr)
    {
        float t = 0f;
        Color originalColor = sr.color;

        while (t < afterimageLifetime)
        {
            float alpha = Mathf.Lerp(originalColor.a, 0f, t / afterimageLifetime);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            t += Time.deltaTime;
            yield return null;
        }

        Destroy(obj);
    }
}
