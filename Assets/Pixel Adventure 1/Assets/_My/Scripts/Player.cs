using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isTurn = false;
    private Vector3 startPosition;
    private Vector3 oldPosition;

    private int moveCnt = 0;
    private int TurnCnt = 0;
    private int SpawnCnt = 0;
    private bool isDie = false;

    private AudioSource sound;

    [Header("타이머 관련")]
    public TimerBarController timerBarController; // Inspector 연결 필요
    private float lastClickTime;
    private bool isFirstClick = true;
    private const float TIMEOUT_DURATION = 1.0f; // 1초 타임아웃

    [Header("잔상 효과")]
    public AfterimageManager afterimageManager; // Inspector에서 연결

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        sound = GetComponent<AudioSource>();
        startPosition = transform.position;
        Init();
    }

    void Update()
    {
        // ① PlayerNamePanel이 열려 있다면 조작 금지
        GameObject playerNamePanel = GameObject.Find("PlayerNamePanel");
        if (playerNamePanel != null && playerNamePanel.activeSelf)
        {
            return; // 아직 이름 입력 중이거나 시작 버튼 안 누름
        }

        // ② InputField에 포커스가 가 있으면 조작 금지
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected.GetComponent<TMPro.TMP_InputField>() != null)
            {
                return;
            }
        }

        // ③ 좌/우 입력 처리 (A, D, 방향키)
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            isTurn = true; // 왼쪽
            spriteRenderer.flipX = isTurn;
            CharMove();

            if (timerBarController != null)
                timerBarController.ResetTimer();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            isTurn = false; // 오른쪽
            spriteRenderer.flipX = isTurn;
            CharMove();

            if (timerBarController != null)
                timerBarController.ResetTimer();
        }

        // 타임아웃 체크
        CheckTimeout();
    }


    // ===============================================
    // [타임아웃 관련 로직]
    // ===============================================
    private void UpdateClickTime()
    {
        if (isDie) return;
        isFirstClick = false;
        lastClickTime = Time.time;
    }

    private void CheckTimeout()
    {
        if (isDie || isFirstClick)
            return;

        if (Time.time > lastClickTime + TIMEOUT_DURATION)
        {
            Debug.Log("타임아웃! 1초 동안 입력이 없어 사망 처리됩니다.");
            CharDie();
        }
    }

    // ===============================================
    // [초기화 로직]
    // ===============================================
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

        if (timerBarController != null)
            timerBarController.ResetTimer();

        // 캐릭터 부활 시 잔상 중지
        if (afterimageManager != null)
            afterimageManager.StopAfterimage();
    }

    // ===============================================
    // [캐릭터 조작]
    // ===============================================
    public void CharMove()
    {
        if (isDie) return;

        sound.Play();
        moveCnt++;
        MoveDirection();

        // 이동 시 잔상 시작
        if (afterimageManager != null)
            afterimageManager.StartAfterimage();

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

    // ===============================================
    // [사망 처리]
    // ===============================================
    public void CharDie()
    {
        GameManager.Instance.GameOver();
        anim.SetBool("Die", true);
        isDie = true;

        // 사망 시 잔상 중지
        if (afterimageManager != null)
            afterimageManager.StopAfterimage();
    }

    public void ButtonRestart()
    {
        Init();
        GameManager.Instance.Init();
        GameManager.Instance.InitStairs();

        // 재시작 이벤트 전송
        AnalyticsManager.Instance?.LogRestartGame();
    }
}
