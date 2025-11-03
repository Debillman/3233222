using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    private bool isInitialized = false;
    private bool isFlushing = false;
    private readonly Queue<Action> eventBuffer = new();

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
#pragma warning disable 618
            AnalyticsService.Instance.StartDataCollection();
#pragma warning restore 618

            isInitialized = true;

            while (eventBuffer.Count > 0) eventBuffer.Dequeue().Invoke();
            Debug.Log("[Analytics] 초기화 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError("[Analytics] 초기화 실패: " + ex.Message);
        }
    }

    private void SendEvent(Action sendAction, bool flush = true, float delay = 0.25f)
    {
        if (!isInitialized)
        {
            eventBuffer.Enqueue(() => SendEvent(sendAction, flush, delay));
            return;
        }

        sendAction.Invoke();

        if (flush && !isFlushing)
            StartCoroutine(FlushAfterDelay(delay));
    }

    private IEnumerator FlushAfterDelay(float wait)
    {
        isFlushing = true;
        yield return new WaitForSeconds(wait);
        AnalyticsService.Instance.Flush();
        isFlushing = false;
    }

    // -----------------------
    // 커스텀 이벤트 구간
    // -----------------------
    public void LogGameStart()
    {
        SendEvent(() => AnalyticsService.Instance.RecordEvent(new CustomEvent("session_start")));
        Debug.Log("[Analytics] session_start 전송됨");
    }

    public void LogGameEnd()
    {
        SendEvent(() => AnalyticsService.Instance.RecordEvent(new CustomEvent("session_end")));
        Debug.Log("[Analytics] session_end 전송됨");
    }

    public void LogGameOver(string playerName, int score)
    {
        SendEvent(() =>
        {
            var e = new CustomEvent("game_over_v2") // 새 이벤트 이름 사용
            {
                { "player_name", playerName ?? "Guest" },
                { "score", score }
            };
            AnalyticsService.Instance.RecordEvent(e);
        }, true, 0.5f);

        Debug.Log("[Analytics] game_over_v2 이벤트 전송됨");
    }

    public void LogRankingOpened()
    {
        SendEvent(() => AnalyticsService.Instance.RecordEvent(new CustomEvent("ranking_opened")), true, 0.1f);
        Debug.Log("[Analytics] ranking_opened 이벤트 전송됨");
    }

    public void LogRestartGame()
    {
        SendEvent(() => AnalyticsService.Instance.RecordEvent(new CustomEvent("restart_game")), true, 0.2f);
        Debug.Log("[Analytics] restart_game 이벤트 전송됨");
    }
}
