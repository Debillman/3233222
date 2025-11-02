using UnityEngine;
using Firebase.Database;
using System.Collections.Generic;

public static class AnalyticsLogger
{
    private static DatabaseReference db => FirebaseDatabase.DefaultInstance.RootReference;

    private static string PlayerName =>
        PlayerNameManager.GetPlayerName(); // 이미 있는 함수

    private static Dictionary<string, object> BuildEvent(string type, Dictionary<string, object> param = null)
    {
        var e = new Dictionary<string, object>
        {
            ["type"] = type,
            ["name"] = PlayerName,
            ["ts"] = ServerValue.Timestamp,
            ["platform"] = Application.platform.ToString(),
            ["app_ver"] = Application.version
        };
        if (param != null)
            foreach (var kv in param) e[kv.Key] = kv.Value;
        return e;
    }

    public static void LogGameStart()
    {
        db.Child("analytics").Child("events").Push()
          .SetValueAsync(BuildEvent("game_start"));
    }

    public static void LogGameOver(int score)
    {
        var p = new Dictionary<string, object> { ["score"] = score };
        db.Child("analytics").Child("events").Push()
          .SetValueAsync(BuildEvent("game_over", p));
    }
}
