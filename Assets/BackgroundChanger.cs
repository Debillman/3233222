using UnityEngine;

public class BackgroundChanger : MonoBehaviour
{
    // === 1. 오브젝트 연결 변수 ===

    // 메인 카메라 (색상 변경 대상)
    public Camera mainCamera;

    // 별 파티클 오브젝트 (활성화/비활성화 대상)
    public GameObject starField;

    // === 2. 높이 설정 변수 ===

    public float startHeight = 10f; // 배경 변화가 시작되는 Y 높이
    public float maxDarkHeight = 100f; // 배경이 완전히 어두워지는 Y 높이
    public float starActivationHeight = 50f; // 별 파티클이 켜지는 Y 높이

    // === 3. 색상 설정 변수 ===

    public Color startColor = Color.white; // 시작 색상 (밝은 색)
    public Color endColor = new Color(0.1f, 0.1f, 0.2f); // 끝 색상 (어두운 우주 색)

    void Update()
    {
        float currentHeight = transform.position.y;

        // ----------------------------------------------------
        // 1. 카메라 배경색 변경 로직
        // ----------------------------------------------------

        float heightRange = maxDarkHeight - startHeight;
        float heightProgress = currentHeight - startHeight;

        // 진행률(t)을 0.0에서 1.0 사이로 제한
        float t = Mathf.Clamp01(heightProgress / heightRange);

        // 색상 선형 보간 (Lerp)
        Color targetColor = Color.Lerp(startColor, endColor, t);

        if (mainCamera != null)
        {
            mainCamera.backgroundColor = targetColor;
        }

        // ----------------------------------------------------
        // 2. 별 파티클 시스템 활성화 로직
        // ----------------------------------------------------

        if (starField != null)
        {
            // 현재 높이가 활성화 높이보다 크거나 같은지 확인
            bool shouldBeActive = currentHeight >= starActivationHeight;

            // 현재 활성화 상태와 다를 때만 변경
            if (starField.activeSelf != shouldBeActive)
            {
                starField.SetActive(shouldBeActive);
            }
        }
    }
}