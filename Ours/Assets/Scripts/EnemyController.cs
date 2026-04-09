using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyController : MonoBehaviour
{
    [Header("Wander Area")]
    public float moveRadius = 2.0f;     // 이 몹이 활동할 수 있는 '자기 구역' (originPos 기준)
    public float moveSpeed = 0.8f;

    [Header("Chase")]
    public float detectRadius = 2.5f;   // 플레이어 감지 반경 (enemy 기준)
    public float loseRadius = 3.0f;     // 추적 해제 반경 (hysteresis: detect보다 살짝 크게)
    public float chaseSpeed = 1.1f;     // 추적 속도

    [Header("Wander Timing")]
    public float walkTimeMin = 1.0f;
    public float walkTimeMax = 2.5f;
    public float idleTimeMin = 0.5f;
    public float idleTimeMax = 1.5f;

    [Header("Animation")]
    public float animInterval = 0.25f;

    [Header("Sprites (Dog)")]
    public Sprite frontIdle;  // 아래로(정면) 프레임(1)
    public Sprite backIdle;   // 위로(뒷면) 프레임(2)
    public Sprite sideA;      // 옆 프레임 A(3)
    public Sprite sideB;      // 옆 프레임 B(4)
    bool battleStarted = false;
    BattleTransitionEffect battleEffect;
    enum State { Wander, Chase }
    State state = State.Wander;

    Vector3 originPos;
    Vector2 moveDir;
    float stateTimer;
    bool isMoving;

    float animTimer;
    bool animToggle;

    SpriteRenderer sr;
    Transform player;

    void Start()
    {
        originPos = transform.position;
        sr = GetComponent<SpriteRenderer>();

        // Player 찾기 (Player 오브젝트에 Tag를 "Player"로 설정해줘)
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        GameObject b = GameObject.FindGameObjectWithTag("BattleEffect");
        if (b != null) battleEffect = b.GetComponent<BattleTransitionEffect>();
        SetIdle();
    }

    void Update()
    {
        UpdateAnimClock();

        if (player != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);

            if (state == State.Wander && distToPlayer <= detectRadius)
                state = State.Chase;

            else if (state == State.Chase && distToPlayer >= loseRadius)
            {
                state = State.Wander;
                SetIdle(); // 바로 배회 로직으로 복귀
            }
        }

        if (state == State.Chase)
        {
            ChaseMove();
            UpdateSprite(); // moveDir 기반 애니메이션 유지
            return;
        }

        // --- Wander 상태 ---
        stateTimer -= Time.deltaTime;

        if (isMoving)
        {
            WanderMove();
            UpdateSprite();

            if (stateTimer <= 0f)
                SetIdle();
        }
        else
        {
            // 정지 중엔 스프라이트 고정(요구사항)
            if (stateTimer <= 0f)
                SetMove();
        }
    }

    void UpdateAnimClock()
    {
        animTimer += Time.deltaTime;
        if (animTimer >= animInterval)
        {
            animTimer = 0f;
            animToggle = !animToggle;
        }
    }

    // ---------------- Wander ----------------
    void SetMove()
    {
        isMoving = true;
        stateTimer = Random.Range(walkTimeMin, walkTimeMax);

        // 8방향 랜덤(원하면 y범위 줄여서 더 땅 위 느낌 가능)
        moveDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        if (moveDir == Vector2.zero) moveDir = Vector2.right;
    }

    void SetIdle()
    {
        isMoving = false;
        stateTimer = Random.Range(idleTimeMin, idleTimeMax);
    }

    void WanderMove()
    {
        Vector3 nextPos = transform.position + (Vector3)moveDir * moveSpeed * Time.deltaTime;

        // originPos 기준 moveRadius 밖으로 나가려 하면 방향 반전
        if (Vector3.Distance(originPos, nextPos) > moveRadius)
            moveDir = -moveDir;
        else
            transform.position = nextPos;
    }

    // ---------------- Chase ----------------
    void ChaseMove()
    {
        if (player == null) return;

        isMoving = true;

        Vector3 toPlayer = (player.position - transform.position);
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        moveDir = ((Vector2)toPlayer).normalized;
        originPos = transform.position;
        Vector3 nextPos = transform.position + (Vector3)moveDir * chaseSpeed * Time.deltaTime;

        // 추적하더라도 자기 활동 구역(moveRadius)은 넘지 않게 '클램프'
        float distFromOrigin = Vector3.Distance(originPos, nextPos);
        if (distFromOrigin > moveRadius)
        {
            Vector3 dirFromOrigin = (nextPos - originPos).normalized;
            nextPos = originPos + dirFromOrigin * moveRadius;
        }

        transform.position = nextPos;
    }

    // ---------------- Sprite ----------------
    void UpdateSprite()
    {
        if (!isMoving) return;

        // 상/하 우선
        if (Mathf.Abs(moveDir.y) > Mathf.Abs(moveDir.x))
        {
            if (moveDir.y < 0)
            {
                // 아래
                sr.sprite = frontIdle;
                sr.flipX = animToggle;
            }
            else
            {
                // 위
                sr.sprite = backIdle;
                sr.flipX = animToggle;
            }
        }
        else
        {
            // 좌/우
            sr.sprite = animToggle ? sideA : sideB;
            sr.flipX = moveDir.x > 0;
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (battleStarted) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            battleStarted = true;
            StartBattle();
        }
    }
    void StartBattle()
    {
        Debug.Log("StartBattle 호출됨");


        if (battleEffect != null)
            battleEffect.Play();
        else
            Debug.Log("battleEffect가 null");
    }
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // 디버그용: 씬에서 반경 보기
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? originPos : transform.position, moveRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, loseRadius);
    }
#endif
}