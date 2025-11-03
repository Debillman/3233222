using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;
using System.IO;
using Newtonsoft.Json.Linq;

public class FirebaseInit : MonoBehaviour
{
    void Awake()
    {
        var existing = FindFirstObjectByType<FirebaseInit>();
        if (existing != null && existing != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        string path = Path.Combine(Application.streamingAssetsPath, "google-services-desktop.json");
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                JObject root = JObject.Parse(json);
                string apiKey = (string)root["client"][0]["api_key"][0]["current_key"];
                string appId = (string)root["client"][0]["client_info"]["mobilesdk_app_id"];
                string projectId = (string)root["project_info"]["project_id"];
                string senderId = (string)root["project_info"]["project_number"];
                string bucket = (string)root["project_info"]["storage_bucket"];

                var options = new AppOptions()
                {
                    ApiKey = apiKey,
                    AppId = appId,
                    ProjectId = projectId,
                    MessageSenderId = senderId,
                    StorageBucket = bucket
                };

                FirebaseApp.Create(options);
                Debug.Log("[Firebase] AppOptions 직접 로드 완료");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[Firebase] JSON 파싱 중 오류: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("[Firebase] google-services-desktop.json 파일을 찾을 수 없습니다: " + path);
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                Debug.Log("[Firebase] 초기화 성공");

                // Firebase Analytics는 비활성화 (Unity Analytics와 충돌 방지)
                // FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);  ← 주석 처리됨

                var app = FirebaseApp.DefaultInstance;
                var db = FirebaseDatabase.GetInstance(
                    app,
                    "https://endless-3497f-default-rtdb.firebaseio.com/"
                );

                // Firebase Database 정상 접근 테스트
                db.GetReference("system/check")
                  .SetValueAsync("ready")
                  .ContinueWithOnMainThread(t =>
                  {
                      if (t.IsCompleted)
                          Debug.Log("[Firebase] Database 연결 확인 완료");
                      else
                          Debug.LogError("[Firebase] Database 연결 실패: " + t.Exception);
                  });
            }
            else
            {
                Debug.LogError("[Firebase] 초기화 실패: " + status);
            }
        });
    }
}
