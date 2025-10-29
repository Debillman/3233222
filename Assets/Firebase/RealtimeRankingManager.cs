using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;

public class RealtimeRankingManager : MonoBehaviour
{
    [Header("UI refs")]
    public GameObject rankingPanel;          // RankingPanel (Canvas child, default inactive)
    public TextMeshProUGUI rankingText;      // Text to display top 10

    private DatabaseReference dbRef;

    void Start()
    {
        // Get Firebase app and database reference
        var app = Firebase.FirebaseApp.DefaultInstance;

        // Replace with your own DB URL if different
        FirebaseDatabase db = FirebaseDatabase.GetInstance(
            app,
            "https://endless-3497f-default-rtdb.firebaseio.com/"
        );

        dbRef = db.RootReference;

        if (rankingPanel != null)
            rankingPanel.SetActive(false);

        ListenRankings();
    }

    // Called by Ranking button (OnClick)
    public void OpenRankingPanel()
    {
        if (rankingPanel == null)
        {
            Debug.LogError("rankingPanel is not assigned.");
            return;
        }

        rankingPanel.SetActive(true);
        RefreshRankings();
    }

    // Called by Close button (OnClick)
    public void CloseRankingPanel()
    {
        if (rankingPanel != null)
            rankingPanel.SetActive(false);
    }

    // Called from GameManager.GameOver() to store the score
    public void SaveScore(string playerName, int score)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Guest";

        string key = dbRef.Child("rankings").Push().Key;

        var data = new Dictionary<string, object>
        {
            { "name", playerName },
            { "score", score },
            { "ts", ServerValue.Timestamp } // optional
        };

        dbRef.Child("rankings").Child(key).SetValueAsync(data)
            .ContinueWithOnMainThread(t =>
            {
                if (t.IsFaulted)
                {
                    Debug.LogError("SaveScore failed: " + t.Exception);
                }
                else
                {
                    Debug.Log("SaveScore ok: " + playerName + " " + score);
                }
            });
    }

    // Realtime listener (fires when data under /rankings changes)
    private void ListenRankings()
    {
        dbRef.Child("rankings")
            .OrderByChild("score")
            .LimitToLast(10)
            .ValueChanged += HandleRankingChanged;
    }

    // Apply updates only when the panel is open
    private void HandleRankingChanged(object sender, ValueChangedEventArgs args)
    {
        if (rankingPanel != null && rankingPanel.activeSelf)
        {
            RefreshRankings();
        }
    }

    // Fetch top 10 immediately and render
    private void RefreshRankings()
    {
        dbRef.Child("rankings")
            .OrderByChild("score")
            .LimitToLast(10)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    if (rankingText != null)
                        rankingText.text = "Failed to load rankings.";
                    return;
                }

                var snapshot = task.Result;
                if (snapshot == null || !snapshot.HasChildren)
                {
                    if (rankingText != null)
                        rankingText.text = "No scores yet.";
                    return;
                }

                var list = new List<(string name, int score)>();
                foreach (var child in snapshot.Children)
                {
                    string name = SafeString(child.Child("name")?.Value, "Guest");
                    int score = SafeInt(child.Child("score")?.Value, 0);
                    list.Add((name, score));
                }

                // Firebase returns ascending. Sort to descending.
                list.Sort((a, b) => b.score.CompareTo(a.score));

                if (rankingText != null)
                    rankingText.text = BuildRankingBoard(list);
            });
    }

    // Build plain ASCII table for top 10
    private string BuildRankingBoard(List<(string name, int score)> list)
    {
        var sb = new StringBuilder();
        sb.AppendLine("RANKING BOARD");
        sb.AppendLine("--------------------------------");
        sb.AppendLine(" Rank    Name            Score");
        sb.AppendLine("--------------------------------");

        int count = Mathf.Min(list.Count, 10);
        for (int i = 0; i < count; i++)
        {
            var row = list[i];
            // Simple alignment: rank right, name left, score right
            sb.AppendFormat("{0,4}.   {1,-12}   {2,6}\n", i + 1, row.name, row.score);
        }

        sb.AppendLine("--------------------------------");
        return sb.ToString();
    }

    // Safe parsers
    private static string SafeString(object v, string fallback)
    {
        if (v == null) return fallback;
        return v.ToString();
    }

    private static int SafeInt(object v, int fallback)
    {
        try
        {
            if (v == null) return fallback;
            if (v is int i) return i;
            if (v is long l) return (int)l;
            if (v is double d) return Convert.ToInt32(d);
            return Convert.ToInt32(v.ToString());
        }
        catch
        {
            return fallback;
        }
    }
}
