using UnityEngine;
using TMPro;

public class PlayerNameManager : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject playerNamePanel;     // 이름 입력 패널
    public GameObject blurBackground;      // 블러 배경
    public TMP_InputField nameInputField;  // 입력 필드

    private const string playerNameKey = "PlayerName";

    void Start()
    {
        // 항상 이름 입력창 표시 (기존 이름이 있으면 불러오기)
        blurBackground.SetActive(true);
        playerNamePanel.SetActive(true);
        Time.timeScale = 0f; // 게임 일시정지

        string oldName = PlayerPrefs.GetString(playerNameKey, "");
        if (!string.IsNullOrEmpty(oldName))
        {
            nameInputField.text = oldName;
        }
    }

    // 버튼 클릭 시 실행
    public void OnConfirmName()
    {
        string playerName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("[PlayerNameManager] 이름이 비어 있음");
            return;
        }

        // 이름 저장
        PlayerPrefs.SetString(playerNameKey, playerName);
        PlayerPrefs.Save();

        // 입력창 및 블러 배경 끄기
        if (blurBackground != null) blurBackground.SetActive(false);
        if (playerNamePanel != null) playerNamePanel.SetActive(false);

        // 게임 재개
        Time.timeScale = 1f;

        Debug.Log($"[PlayerNameManager] 이름 저장 완료: {playerName}, 게임 시작됨");
    }

    public static string GetPlayerName()
    {
        return PlayerPrefs.GetString(playerNameKey, "Guest");
    }
}
