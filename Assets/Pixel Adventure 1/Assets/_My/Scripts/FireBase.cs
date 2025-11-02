using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Analytics;
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

        // 직접 파일 읽어서 AppOptions 생성
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

        // Firebase 정상 초기화 확인
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                Debug.Log("[Firebase] 초기화 성공");
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

                // 같은 DB 인스턴스를 명시적으로 사용
                var app = FirebaseApp.DefaultInstance;
                var db = FirebaseDatabase.GetInstance(
                    app,
                    "https://endless-3497f-default-rtdb.firebaseio.com/" 
                );

                db.GetReference("analytics/events/_init/status")
                  .SetValueAsync("ready")
                  .ContinueWithOnMainThread(t =>
                  {
                      if (t.IsCompleted)
                          Debug.Log("[Firebase] analytics 폴더 자동 생성 완료");
                      else
                          Debug.LogError("[Firebase] analytics 생성 실패: " + t.Exception);
                  });
            }
            else
            {
                Debug.LogError("[Firebase] 초기화 실패: " + status);
            }
        });
    }
}
