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
        // 게임이 시작된 후 첫 클릭이 발생해야 타이머가 작동합니다.
        if (!isStarted) return;

        // 현재 시간을 1초(TIMEOUT_DURATION)에 맞춰 증가시킵니다.
        currentTime += Time.deltaTime;

        // 경과 시간이 타임아웃 시간을 초과하면 더 이상 증가시키지 않습니다.
        if (currentTime > TIMEOUT_DURATION)
        {
            currentTime = TIMEOUT_DURATION;
        }

        // 게이지 바의 fillAmount를 계산합니다.
        // 현재 시간 / 전체 시간(1.0초)으로 0.0 ~ 1.0 사이의 비율을 구하고,
        // 1에서 이 비율을 빼서 시간이 지날수록 줄어들게 만듭니다.
        float fillRatio = 1f - (currentTime / TIMEOUT_DURATION);

        // 이미지의 fillAmount에 적용합니다.
        fillImage.fillAmount = fillRatio;
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