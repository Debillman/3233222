using UnityEngine;
using System.Collections;

public class AfterimageManager : MonoBehaviour
{
    [Header("기본 설정")]
    public GameObject player;                // 플레이어 오브젝트
    public float spawnInterval = 0.04f;      // 잔상 생성 간격 (초)
    public float afterimageLifetime = 0.35f; // 잔상이 유지되는 시간
    public Color baseColor = new Color(1f, 0.2f, 0.1f, 0.6f); // 기본 잔상 색상 (붉은 계열)

    [Header("속도 기반 색상 변화")]
    public float colorBoostThreshold = 4f;   // 이 속도 이상이면 색상 강화
    public float maxSpeedColorMultiplier = 2.0f; // 최대 속도 시 색상 배율

    private SpriteRenderer playerRenderer;
    private Rigidbody2D rb;                  // 속도 계산용
    private bool isActive = false;

    void Start()
    {
        playerRenderer = player.GetComponent<SpriteRenderer>();
        rb = player.GetComponent<Rigidbody2D>();
    }

    // 잔상 생성 시작
    public void StartAfterimage()
    {
        if (!isActive)
            StartCoroutine(SpawnAfterimages());
    }

    // 잔상 생성 중단
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
        GameObject after = new GameObject("Afterimage");
        SpriteRenderer sr = after.AddComponent<SpriteRenderer>();

        sr.sprite = playerRenderer.sprite;
        sr.flipX = playerRenderer.flipX;

        // 속도에 따른 색상 변화
        float speed = rb != null ? rb.linearVelocity.magnitude : 0f;
        float intensity = Mathf.Clamp01(speed / colorBoostThreshold);
        float multiplier = Mathf.Lerp(1f, maxSpeedColorMultiplier, intensity);

        Color c = baseColor;
        c.g = Random.Range(0.0f, 0.3f); // 불빛 잔상 느낌의 노란톤 섞기
        c.r = Mathf.Clamp01(c.r * multiplier);
        c.b = Mathf.Clamp01(c.b * (1f - intensity * 0.5f)); // 속도 높을수록 파란빛 줄이기

        sr.color = c;

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
