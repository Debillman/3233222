using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("계단")]
    public GameObject[] Stairs;
    public bool[] isTurn;

    private enum State { Start, Left, Right };
    private State state;
    private Vector3 oldPosition;

    [Header("특수 계단 등장 확률")]
    [Range(0f, 1f)]
    public float confuseStairChance = 0.05f;      // 5% 정도
    [Range(0f, 1f)]
    public float memoryDisappearChance = 0.10f;   // 10% 정도

    [Header("UI")]
    public GameObject UI_GameOver;
    public TextMeshProUGUI textMaxScore;
    public TextMeshProUGUI textNowScore;
    public TextMeshProUGUI textShowScore;
    private int maxScore = 0;
    private int nowScore = 0;

    [Header("Audio")]
    private AudioSource sound;
    public AudioClip bgmSound;
    public AudioClip dieSound;

    private bool hasGameStarted = false; // 게임 시작 여부 추적용

    void Start()
    {
        Instance = this;
        sound = GetComponent<AudioSource>();

        Init();
        InitStairs();

        // 최초 한 번만 game_start 이벤트 전송
        if (!hasGameStarted)
        {
            AnalyticsManager.Instance?.LogGameStart();
            hasGameStarted = true;
        }
    }

    // 하나의 계단 GameObject에 대해, 어떤 타입으로 쓸지 랜덤 결정
    private void RandomizeStairKind(GameObject stairGO)
    {
        var stair = stairGO.GetComponent<Stair>();
        if (stair == null) return;

        float r = Random.value; // 0 ~ 1 사이 랜덤

        if (r < confuseStairChance)
        {
            // 대마왕 계단
            stair.SetKind(StairKind.ConfuseControl);
        }
        else if (r < confuseStairChance + memoryDisappearChance)
        {
            // 투명(암기) 계단
            stair.SetKind(StairKind.MemoryDisappear);
        }
        else
        {
            // 일반 계단
            stair.SetKind(StairKind.Normal);
        }
    }

    public void Init()
    {
        state = State.Start;
        oldPosition = Vector3.zero;

        isTurn = new bool[Stairs.Length];

        for (int i = 0; i < Stairs.Length; i++)
        {
            Stairs[i].transform.position = Vector3.zero;
            isTurn[i] = false;
        }

        nowScore = 0;
        textShowScore.text = nowScore.ToString();
        UI_GameOver.SetActive(false);

        sound.clip = bgmSound;
        sound.Play();
        sound.loop = true;
        sound.volume = 0.2f;
    }

    public void InitStairs()
    {
        for (int i = 0; i < Stairs.Length; i++)
        {
            switch (state)
            {
                case State.Start:
                    Stairs[i].transform.position = new Vector3(0.75f, -0.1f, 0);
                    state = State.Right;
                    break;

                case State.Left:
                    Stairs[i].transform.position = oldPosition + new Vector3(-0.75f, 0.5f, 0);
                    isTurn[i] = true;
                    break;

                case State.Right:
                    Stairs[i].transform.position = oldPosition + new Vector3(0.75f, 0.5f, 0);
                    isTurn[i] = false;
                    break;
            }

            oldPosition = Stairs[i].transform.position;

            if (i != 0)
            {
                int ran = Random.Range(0, 5);
                if (ran < 2 && i < Stairs.Length - 1)
                {
                    state = state == State.Left ? State.Right : State.Left;
                }
            }

            // 위치 정해진 후, 이 계단 타입(일반/투명/대마왕)을 랜덤으로 결정 + 초기화
            RandomizeStairKind(Stairs[i]);
        }
    }

    public void SpawnStair(int cnt)
    {
        int ran = Random.Range(0, 5);

        if (ran < 2)
        {
            state = state == State.Left ? State.Right : State.Left;
        }

        switch (state)
        {
            case State.Left:
                Stairs[cnt].transform.position = oldPosition + new Vector3(-0.75f, 0.5f, 0);
                isTurn[cnt] = true;
                break;

            case State.Right:
                Stairs[cnt].transform.position = oldPosition + new Vector3(0.75f, 0.5f, 0);
                isTurn[cnt] = false;
                break;
        }

        oldPosition = Stairs[cnt].transform.position;

        // 새로 스폰된 계단도 타입 랜덤 설정 + 초기화
        RandomizeStairKind(Stairs[cnt]);
    }

    public void GameOver()
    {
        sound.loop = false;
        sound.Stop();
        sound.clip = dieSound;
        sound.volume = 1;
        sound.Play();

        string playerName = PlayerNameManager.GetPlayerName();

        // Firebase 랭킹 저장
        var rr = FindFirstObjectByType<RealtimeRankingManager>();
        rr?.SaveScore(playerName, nowScore);

        // Analytics 이벤트 전송 (game_over_v2)
        AnalyticsManager.Instance?.LogGameOver(playerName, nowScore);

        StartCoroutine(ShowGameOver());
    }

    IEnumerator ShowGameOver()
    {
        yield return new WaitForSeconds(1f);
        UI_GameOver.SetActive(true);

        if (nowScore > maxScore)
            maxScore = nowScore;

        textMaxScore.text = maxScore.ToString();
        textNowScore.text = nowScore.ToString();
    }

    public void AddScore()
    {
        nowScore++;
        textShowScore.text = nowScore.ToString();
    }
}
