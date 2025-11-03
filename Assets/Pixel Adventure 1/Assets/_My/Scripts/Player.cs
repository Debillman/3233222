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
    public TimerBarController timerBarController;
    private float lastClickTime;
    private bool isFirstClick = true;
    private const float TIMEOUT_DURATION = 1.0f;

    [Header("잔상 효과")]
    public AfterimageManager afterimageManager;

    public bool IsDead => isDie;

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
        GameObject playerNamePanel = GameObject.Find("PlayerNamePanel");
        if (playerNamePanel != null && playerNamePanel.activeSelf)
            return;

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected.GetComponent<TMPro.TMP_InputField>() != null)
                return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            isTurn = true;
            spriteRenderer.flipX = isTurn;
            CharMove();

            if (timerBarController != null)
            {
                timerBarController.ResetTimer();      // 게이지 리셋
                timerBarController.EnableDeathCheck(); // 첫 이동 후부터 사망 판정 활성
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            isTurn = false;
            spriteRenderer.flipX = isTurn;
            CharMove();

            if (timerBarController != null)
            {
                timerBarController.ResetTimer();
                timerBarController.EnableDeathCheck();
            }
        }

        CheckTimeout();
    }

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
            CharDie();
        }
    }

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

        // 시작 시 타이머를 강제로 리셋하지 않음
        // timerBarController?.ResetTimer();  <- 제거

        afterimageManager?.StopAfterimage();
    }

    public void CharMove()
    {
        if (isDie) return;

        if (sound != null) sound.Play();
        moveCnt++;
        MoveDirection();

        afterimageManager?.StartAfterimage();

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

        afterimageManager?.StopAfterimage();
    }

    public void ButtonRestart()
    {
        Init();
        GameManager.Instance.Init();
        GameManager.Instance.InitStairs();
    }
}
