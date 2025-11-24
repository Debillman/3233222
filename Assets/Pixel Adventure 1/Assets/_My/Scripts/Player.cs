using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    private int retryCount = 0;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isTurn = false;
    private Vector3 startPosition;
    private Vector3 oldPosition;

    private int moveCnt = 0;   // 전체 이동 횟수
    private int TurnCnt = 0;
    private int SpawnCnt = 0;
    private bool isDie = false;

    private AudioSource sound;
    private MonoBehaviour currentStair;

    // ----------------------------------------------------
    // 타이머 관련
    // ----------------------------------------------------
    [Header("타이머 관련")]
    public TimerBarController timerBarController;
    private float lastClickTime;
    private bool isFirstClick = true;
    private const float TIMEOUT_DURATION = 1.0f;

    // ----------------------------------------------------
    // 대마왕 효과
    // ----------------------------------------------------
    [Header("조작 혼란 효과(대마왕)")]
    [Tooltip("기본 조작 혼란 지속 시간(초)")]
    public float defaultConfuseDuration = 15f;

    // 대마왕 상태일 때 적용할 색
    public Color confuseTintColor = new Color(0.7f, 0.2f, 1f, 1f); // 보라빛

    [Tooltip("대마왕 상태일 때 사용할 스프라이트 (비워두면 색만 바뀜)")]
    public Sprite confuseSprite;

    private bool isControlConfused = false; // 조작 반전 여부
    private float confuseEndTime = 0f;      // 조작 혼란이 끝나는 시각

    // 원래 캐릭터 색/스프라이트 저장용
    private Color originalColor;
    private Sprite originalSprite;

    // ----------------------------------------------------
    // 잔상 효과
    // ----------------------------------------------------
    [Header("잔상 효과")]
    public AfterimageManager afterimageManager;

    [Header("잔상 조건")]
    [Tooltip("연속 몇 번 이동했을 때 잔상을 켤지 설정합니다.")]
    public int afterimageStartStep = 3;

    [Tooltip("연속 이동으로 인정할 최대 간격 (초)")]
    public float afterimageMoveInterval = 0.3f;

    [Tooltip("잔상이 켜진 후, 마지막 이동 후 이 시간이 지나면 잔상이 꺼집니다.")]
    public float afterimageDurationAfterLastMove = 1.0f;

    private int continuousMoveCount = 0;          // 연속 이동 횟수
    private float lastMoveTime = -999f;           // 마지막 이동 시각(연속 판단용)
    private float lastAfterimageMoveTime = -999f; // 잔상 켜진 상태에서의 마지막 이동 시각
    private bool afterimageActive = false;        // 현재 잔상 on/off

    public bool IsDead => isDie;

    // ====================================================
    // Unity 콜백
    // ====================================================
    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        sound = GetComponent<AudioSource>();
        startPosition = transform.position;

        // 원래 비주얼 저장
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            originalSprite = spriteRenderer.sprite;
        }

        Init();
    }

    void Update()
    {
        // 0) 대마왕 효과 끝났는지 확인
        if (isControlConfused && Time.time >= confuseEndTime)
        {
            isControlConfused = false;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
                spriteRenderer.sprite = originalSprite;
            }

            Debug.Log("[Player] 조작 혼란 종료");
        }

        // ① 이름 입력 패널 열려 있으면 조작 막기
        GameObject playerNamePanel = GameObject.Find("PlayerNamePanel");
        if (playerNamePanel != null && playerNamePanel.activeSelf)
            return;

        // ② 인풋필드에 포커스가 있으면 조작 막기
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected.GetComponent<TMP_InputField>() != null)
                return;
        }

        // ③ 좌/우 입력을 먼저 받음
        bool leftInput = Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
        bool rightInput = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);

        // ★ 대마왕 상태면 좌/우 입력을 서로 바꿔치기
        if (isControlConfused)
        {
            bool tmp = leftInput;
            leftInput = rightInput;
            rightInput = tmp;
        }

        // ④ 좌/우 입력 처리 (여기서는 leftInput/rightInput만 사용!)
        if (leftInput)
        {
            isTurn = true;
            spriteRenderer.flipX = isTurn;
            CharMove();
            timerBarController?.ResetTimer();
            timerBarController?.EnableDeathCheck();
        }

        if (rightInput)
        {
            isTurn = false;
            spriteRenderer.flipX = isTurn;
            CharMove();
            timerBarController?.ResetTimer();
            timerBarController?.EnableDeathCheck();
        }


        // 잔상 자동 종료 체크
        if (afterimageActive)
        {
            // 마지막 잔상 이동 이후 일정 시간이 지나면 잔상 끄기
            if (Time.time - lastAfterimageMoveTime >= afterimageDurationAfterLastMove)
            {
                afterimageActive = false;
                afterimageManager?.StopAfterimage();
                continuousMoveCount = 0;
            }
        }

        CheckTimeout();
    }

    // ====================================================
    // 타임아웃 관련
    // ====================================================
    private void CheckTimeout()
    {
        if (isDie || isFirstClick)
            return;

        if (Time.time > lastClickTime + TIMEOUT_DURATION)
        {
            CharDie();
        }
    }

    // ====================================================
    // 초기화 / 이동 / 사망 / 재시작
    // ====================================================
    private void Init()
    {
        anim.SetBool("Die", false);
        transform.position = startPosition;
        oldPosition = startPosition;

        moveCnt = 0;
        TurnCnt = 0;
        SpawnCnt = 0;
        isTurn = false;
        spriteRenderer.flipX = isTurn;
        isDie = false;

        isFirstClick = true;
        lastClickTime = 0f;

        // 잔상 관련 변수 초기화
        afterimageActive = false;
        continuousMoveCount = 0;
        lastMoveTime = -999f;
        lastAfterimageMoveTime = -999f;

        // 대마왕 상태 리셋
        isControlConfused = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            spriteRenderer.sprite = originalSprite;
        }

        // 시작 시 타이머를 강제로 리셋하지 않음
        // timerBarController?.ResetTimer();

        afterimageManager?.StopAfterimage();
    }

    public void CharMove()
    {
        if (isDie) return;

        // 첫 이동 이후부터 타임아웃 활성화
        isFirstClick = false;
        lastClickTime = Time.time;

        if (sound != null) sound.Play();

        moveCnt++;
        MoveDirection();

        // ─────────────────────────────
        // 잔상 발동용 연속 이동 카운트 계산
        // ─────────────────────────────
        float now = Time.time;

        if (now - lastMoveTime <= afterimageMoveInterval)
        {
            // 연속 이동
            continuousMoveCount++;
        }
        else
        {
            // 연속이 끊겼으니 다시 1부터
            continuousMoveCount = 1;
        }

        lastMoveTime = now;

        // 잔상이 아직 꺼져 있고, 연속 이동 횟수가 기준 이상이면 ON
        if (!afterimageActive && continuousMoveCount >= afterimageStartStep)
        {
            afterimageActive = true;
            lastAfterimageMoveTime = now;
            afterimageManager?.StartAfterimage();
        }

        // 잔상이 켜져 있는 동안 이동했다면 마지막 잔상 이동 시각 갱신
        if (afterimageActive)
        {
            lastAfterimageMoveTime = now;
        }

        // ─────────────────────────────
        // 계단 실패/스폰/점수 처리
        // ─────────────────────────────
        if (isFailTurn())
        {
            CharDie();
            return;
        }

        if (moveCnt > 5)
            RespawnStair();

        GameManager.Instance.AddScore();
    }

    private void MoveDirection()
    {
        if (isTurn)
            oldPosition += new Vector3(-0.75f, 0.5f, 0);
        else
            oldPosition += new Vector3(0.75f, 0.5f, 0);

        transform.position = oldPosition;
        anim.SetTrigger("Move");
    }

    private bool isFailTurn()
    {
        bool result = false;

        if (GameManager.Instance.isTurn[TurnCnt] != isTurn)
            result = true;

        TurnCnt++;

        if (TurnCnt > GameManager.Instance.Stairs.Length - 1)
            TurnCnt = 0;

        return result;
    }

    private void RespawnStair()
    {
        GameManager.Instance.SpawnStair(SpawnCnt);
        SpawnCnt++;

        if (SpawnCnt > GameManager.Instance.Stairs.Length - 1)
            SpawnCnt = 0;
    }

    public void CharDie()
    {
        if (isDie) return;

        GameManager.Instance.GameOver();
        anim.SetBool("Die", true);
        isDie = true;

        // 사망 시 잔상 중지 및 상태 리셋
        afterimageActive = false;
        continuousMoveCount = 0;
        afterimageManager?.StopAfterimage();

        // 대마왕 상태 정리
        isControlConfused = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            spriteRenderer.sprite = originalSprite;
        }
    }

    // Stair 에서 호출하는 대마왕 발동 함수
    public void ActivateConfuseControl(float duration)
    {
        if (duration <= 0f)
            duration = defaultConfuseDuration;   // 예: 15초

        isControlConfused = true;
        confuseEndTime = Time.time + duration;

        // 비주얼 변경 (보라색 + 스프라이트 교체)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = confuseTintColor;   // Inspector에서 보라색으로 설정된 값
            if (confuseSprite != null)
                spriteRenderer.sprite = confuseSprite;
        }

        Debug.Log($"[Player] 조작 혼란 시작! {duration}초 동안 좌우 반전");
    }


    public void ButtonRestart()
    {
        // 1) 재시작 횟수 증가
        retryCount++;

        // 2) Analytics 전송
        if (AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.LogRestartGame(retryCount);
        }

        // 3) 실제 게임 리셋
        Init();
        GameManager.Instance.Init();
        GameManager.Instance.InitStairs();
    }

    public void SetCurrentStair(MonoBehaviour stair)
    {
        currentStair = stair;
    }
}
