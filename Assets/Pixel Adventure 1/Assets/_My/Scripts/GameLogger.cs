using UnityEngine;
using Firebase.Analytics;

public class GameLogger : MonoBehaviour
{
    // 게임 시작할 때 기록
    public static void LogGameStart(string playerName)
    {
        FirebaseAnalytics.LogEvent("game_start", new Parameter("player", playerName));
        Debug.Log("[Firebase Analytics] 게임 시작 로그 전송됨");
    }

    // 게임 오버 시 기록
    public static void LogGameOver(string playerName, int score)
    {
        FirebaseAnalytics.LogEvent("game_over",
            new Parameter("player", playerName),
            new Parameter("score", score)
        );
        Debug.Log("[Firebase Analytics] 게임 종료 로그 전송됨");
    }

    // 랭킹 창 열 때 기록
    public static void LogRankingOpened()
    {
        FirebaseAnalytics.LogEvent("open_ranking");
        Debug.Log("[Firebase Analytics] 랭킹 열기 로그 전송됨");
    }
}
