using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트를 사용하기 위해 필요합니다!

public class TimerBarController : MonoBehaviour
{
    // Inspector 창에서 FillBar Image 컴포넌트를 연결할 변수
    public Image fillImage;

    // Player 스크립트와 동일한 타임아웃 시간 (1.0f)
    private const float TIMEOUT_DURATION = 1.0f;

    // 현재 경과 시간 (0.0f에서 1.0f까지 증가)
    private float currentTime = 0f;

    // 게임이 시작된 후 첫 클릭 전인지 확인 (Player 스크립트의 isFirstClick과 연동)
    private bool isStarted = false;

    void Start()
    {
        // 시작 시 게이지를 완전히 채워둡니다.
        fillImage.fillAmount = 1f;
    }

    void Update()
    {
        if (!isStarted) return;

        currentTime += Time.deltaTime;
        if (currentTime > TIMEOUT_DURATION)
        {
            currentTime = TIMEOUT_DURATION;
        }

        float fillRatio = 1f - (currentTime / TIMEOUT_DURATION);
        fillImage.fillAmount = fillRatio;

        // [추가 부분] 게이지가 0이 되면 GameOver 호출
        if (fillImage.fillAmount <= 0f)
        {
            var player = FindFirstObjectByType<Player>();
            if (player != null)
                player.CharDie();        // ← 여기서 죽음 애니메이션/이미지 전환까지 모두 처리
            isStarted = false;           // 중복 호출 방지
        }
    }


    // 외부(Player 스크립트)에서 호출하여 타이머를 초기화하는 함수
    public void ResetTimer()
    {
        // 시간이 멈춰있더라도 다시 0에서 시작하도록 초기화합니다.
        currentTime = 0f;

        // 타이머가 작동 중임을 표시합니다.
        isStarted = true;
    }
}