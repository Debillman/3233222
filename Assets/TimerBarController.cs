using UnityEngine;
using UnityEngine.UI;

public class TimerBarController : MonoBehaviour
{
    public Image fillImage;
    private const float TIMEOUT_DURATION = 0.7f; // ← 1.0f를 0.7f로 변경
    private float currentTime = 0f;

    // 타이머는 시작부터 돌려서 게이지는 줄어들게 함
    private bool isStarted = true;

    // 첫 이동 이전에는 사망 판정 비활성
    private bool hasPlayerMoved = false;

    void Start()
    {
        if (fillImage != null)
            fillImage.fillAmount = 1f;
    }

    void Update()
    {
        if (!isStarted) return;

        currentTime += Time.deltaTime;
        if (currentTime > TIMEOUT_DURATION)
            currentTime = TIMEOUT_DURATION;

        float fillRatio = 1f - (currentTime / TIMEOUT_DURATION);
        if (fillImage != null)
            fillImage.fillAmount = fillRatio;

        // 게이지가 0이고, 최소 한 번이라도 이동한 이후에만 사망 판정
        if (fillImage != null && fillImage.fillAmount <= 0f && hasPlayerMoved)
        {
            var player = FindFirstObjectByType<Player>();
            if (player != null && !player.IsDead)
                player.CharDie();

            // 중복 방지
            isStarted = false;
        }
    }

    // 매 이동 때 호출: 게이지를 리셋(시각적으로도 1로)하고 타이머 계속 작동
    public void ResetTimer()
    {
        currentTime = 0f;
        isStarted = true;
        if (fillImage != null)
            fillImage.fillAmount = 1f;
        // 여기서 hasPlayerMoved를 건드리지 않음
    }

    // 첫 이동이 발생한 시점에 호출하여 사망 판정 활성화
    public void EnableDeathCheck()
    {
        hasPlayerMoved = true;
    }


}
