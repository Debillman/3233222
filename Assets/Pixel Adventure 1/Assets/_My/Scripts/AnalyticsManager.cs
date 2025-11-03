using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        await InitializeUnityAnalytics();
    }

    // ------------------------------------------------------------
    // Unity Analytics 초기화
    // ------------------------------------------------------------
    private async Task InitializeUnityAnalytics()
    {
        try
        {
            await UnityServices.InitializeAsync();
#pragma warning disable 618
            AnalyticsService.Instance.StartDataCollection();
#pragma warning restore 618
            Debug.Log("[Unity Analytics] 초기화 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[Unity Analytics] 초기화 실패: " + ex.Message);
        }
    }

    // ------------------------------------------------------------
    // 커스텀 이벤트
    // ------------------------------------------------------------

    // (1) 게임 시작 시 (앱 최초 실행 후 첫 플레이)
    public void LogGameStart()
    {
        AnalyticsService.Instance.RecordEvent(new CustomEvent("game_start"));
        AnalyticsService.Instance.Flush();
        Debug.Log("[Unity Analytics] game_start 이벤트 전송됨");
    }

    // (2) 게임 오버 시 (사망 시)
    public void LogGameOver(string playerName, int score)
    {
        var gameOverEvent = new CustomEvent("game_over_v2")
        {
            { "player_name", playerName },
            { "userScore", score  }
        };
        AnalyticsService.Instance.RecordEvent(gameOverEvent);
        AnalyticsService.Instance.Flush();
        Debug.Log($"[Unity Analytics] game_over_v2 이벤트 전송됨 ({playerName} / {score})");
    }

    // (3) 재시작 시 (Restart 버튼 클릭)
    public void LogRestartGame()
    {
        AnalyticsService.Instance.RecordEvent(new CustomEvent("restart_game"));
        AnalyticsService.Instance.Flush();
        Debug.Log("[Unity Analytics] restart_game 이벤트 전송됨");
    }

    // (4) 랭킹 창 열 때
    public void LogRankingOpened()
    {
        AnalyticsService.Instance.RecordEvent(new CustomEvent("ranking_opened"));
        AnalyticsService.Instance.Flush();
        Debug.Log("[Unity Analytics] ranking_opened 이벤트 전송됨");
    }
}
