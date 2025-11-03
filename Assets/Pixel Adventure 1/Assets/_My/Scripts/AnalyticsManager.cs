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

    private async Task InitializeUnityAnalytics()
    {
        try
        {
            await UnityServices.InitializeAsync();

            //  Obsolete 경고만 끄고 사용 (버전 호환 최우선)
#pragma warning disable 618
            AnalyticsService.Instance.StartDataCollection();
#pragma warning restore 618

            Debug.Log("[Unity Analytics] 초기화 완료 (StartDataCollection 사용)");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[Unity Analytics] 초기화 실패: " + ex.Message);
        }
    }

    // 자동(세션) 이벤트
    public void LogGameStart()
    {
        AnalyticsService.Instance.RecordEvent(new CustomEvent("gameStarted"));
        Debug.Log("[Unity Analytics] gameStarted 이벤트 전송됨");
    }

    public void LogGameEnd()
    {
        AnalyticsService.Instance.RecordEvent(new CustomEvent("gameEnded"));
        Debug.Log("[Unity Analytics] gameEnded 이벤트 전송됨");
    }

    // 커스텀 이벤트
    public void LogGameOver(string playerName, int score)
    {
        var gameOverEvent = new CustomEvent("game_over")
        {
            { "player_name", playerName },
            { "score", score }
        };
        AnalyticsService.Instance.RecordEvent(gameOverEvent);
        AnalyticsService.Instance.Flush();

        Debug.Log("[Unity Analytics] game_over 이벤트 전송됨");
    }

    public void LogRankingOpened()
    {
        AnalyticsService.Instance.RecordEvent(new CustomEvent("ranking_opened"));
        AnalyticsService.Instance.Flush();

        Debug.Log("[Unity Analytics] ranking_opened 이벤트 전송됨");
    }
}
