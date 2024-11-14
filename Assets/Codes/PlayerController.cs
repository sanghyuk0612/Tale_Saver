using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    public float moveSpeed = 5f;
    public float jumpForce = 9.6f;
    private bool isGrounded;
    private SpriteRenderer spriteRenderer;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.1f;  // 레이캐스트 거리
    public LayerMask groundLayer;
    public Vector2 groundCheckSize = new Vector2(0.4f, 0.1f);     // 박스캐스트 크기
    
    [Header("Jump Settings")]
    public int maxJumpCount = 2;  // 최대 점프 횟수
    private int remainingJumps;    // 남은 점프 횟수
    private bool hasJumped;  // 점프로 올라갔는지 여부를 체크하는 변수 추가

    [Header("Dash Settings")]
    public float dashForce = 15f;        // 돌진 힘
    public float dashCooldown = 5f;      // 쿨다운 시간
    private bool canDash = true;         // 돌진 가능 여부
    private float dashCooldownTimer = 0f; // 쿨다운 타이머
    
    private bool isDashing = false;

    [Header("Dash Effect")]
    public GameObject afterImagePrefab;  // 잔 리팅
    private float afterImageDelay = 0.1f;  // 잔상 생성 간격
    private float lastAfterImageTime;

    [Header("Platform Drop")]
    public LayerMask playerPassthroughLayer;  // Inspector에서 설정할 수 있도록 추가
    private float dropCheckRadius = 0.1f;  // 플랫폼 체크 범위
    public float dropSpeed = 1f;  // Inspector에서 조절 가능한 낙하 속도

    private Coroutine currentDashCoroutine;  // 현재 실행 중인 대시 코루틴 참조

    private Collider2D playerCollider;  // 플레이어의 콜라이더 참조 추가

    private bool canDropDown = true;  // 아래 방향키 입력 가능 여부

    public bool IsGrounded
    {
        get { return isGrounded; }
        private set { isGrounded = value; }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 2.5f;  // 중력 스케일을 1로 복구
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.drag = 0f;
        spriteRenderer = GetComponent<SpriteRenderer>();
        remainingJumps = maxJumpCount;
        playerCollider = GetComponent<Collider2D>();
    }
    // void OnTriggerEnter2D(Collider2D collision) {
    // //Tag가 item일 때
	// if (collision.gameObject.tag == "Item") {
	// 	//Deactive Item
	// 	collision.gameObject.SetActive(false);
    //     GameManager.instance.Score++;
	// }
    //}

    void FixedUpdate()
    {
        CheckGround();
        
        if (!isDashing)
        {
            float moveInput = Input.GetAxisRaw("Horizontal");
            if (moveInput != 0)
            {
                float targetVelocityX = moveInput * moveSpeed;
                rb.velocity = new Vector2(targetVelocityX, rb.velocity.y);
            }
            else
            {
                float currentVelocityX = rb.velocity.x;
                rb.velocity = new Vector2(currentVelocityX * 0.9f, rb.velocity.y);
            }
        }
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput != 0)
        {
            spriteRenderer.flipX = moveInput < 0;
        }

        if (!GameManager.instance.isPlayerInRange&&Input.GetButtonDown("Jump") && remainingJumps > 0)
        {
            Debug.Log($"Jump executed. Remaining jumps: {remainingJumps-1}");
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            remainingJumps--;
            
            if (IsGrounded)
            {
                hasJumped = true;
            }
        }

        // 대시 쿨다운 체크
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0)
            {
                canDash = true;
                Debug.Log("Dash is ready!");
            }
        }

        // 대시 실행
        if (Input.GetKeyDown(KeyCode.Q) && canDash)
        {
            Dash();
        }

        // 아래 방향키를 누르면 플랫폼 통과
        if (Input.GetAxisRaw("Vertical") < 0 && IsGrounded && canDropDown)
        {
            Debug.Log("아래 방향키 감지됨");
            
            // 감지 위를 더 크게 설정하고 플레이어 발 위치에서 체크
            Vector2 feetPosition = new Vector2(transform.position.x, 
                transform.position.y - GetComponent<Collider2D>().bounds.extents.y);
            
            Collider2D[] colliders = Physics2D.OverlapCircleAll(feetPosition, 0.3f, groundLayer);
            
            foreach (Collider2D col in colliders)
            {
                Debug.Log($"감지된 오브젝트: {col.gameObject.name}, 태그: {col.tag}");
                
                if (col.CompareTag("OneWayPlatform"))
                {
                    Debug.Log("OneWayPlatform 감지됨 - 통과 시도");
                    canDropDown = false;  // 아래키 입력 비활성화
                    StartCoroutine(DisableCollisionCoroutine(col));
                    break;
                }
            }
        }
    }

    void CheckGround()
    {
        // BoxCast의 시작점을 플레이어의 발 위치로 조정
        Vector2 boxCastOrigin = new Vector2(
            transform.position.x,
            transform.position.y - (GetComponent<Collider2D>().bounds.extents.y - groundCheckSize.y/2)
        );

        RaycastHit2D hit = Physics2D.BoxCast(
            boxCastOrigin,          // 시작점
            groundCheckSize,        // 크기
            0f,                     // 회전 각도
            Vector2.down,           // 방향
            groundCheckDistance,    // 거리
            groundLayer            
        );

        bool wasGrounded = IsGrounded;  // 이전 상태 저장
        IsGrounded = hit.collider != null;

        // 땅에 착지했을 때 점프 횟수 초기화
        if (!wasGrounded && IsGrounded)
        {
            remainingJumps = maxJumpCount;
            hasJumped = false;
        }

        // 상태 변경 시 로그 출력
        if (wasGrounded != IsGrounded)
        {
            Debug.Log($"Ground state changed: {IsGrounded}");
            if (hit.collider != null)
            {
                Debug.Log($"Detected ground: {hit.collider.gameObject.name}");
            }
        }
    }

    void Dash()
    {
        // 현재 바라보는 방향 확인 (spriteRenderer.flipX 기준)
        float dashDirection = spriteRenderer.flipX ? -1f : 1f;
        
        // 현재 속도를 초기화하고 대시 방향으로 힘을 가함
        rb.velocity = Vector2.zero;
        rb.AddForce(new Vector2(dashDirection * dashForce, 0f), ForceMode2D.Impulse);

        // 대시 코루틴 시작
        StartCoroutine(DashCoroutine());

        // 쿨다운 시작
        canDash = false;
        dashCooldownTimer = dashCooldown;
        Debug.Log($"Dash used! Cooldown started: {dashCooldown} seconds");
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        
        // 대시 지속 시간
        yield return new WaitForSeconds(0.35f);
        
        isDashing = false;
    }

    // UI에 쿨다운 표시를 한 public 메서드
    public float GetDashCooldownRemaining()
    {
        return canDash ? 0f : dashCooldownTimer;
    }

    public bool IsDashReady()
    {
        return canDash;
    }

    // 디버그용 시화 (선택사항)
    void OnDrawGizmos()
    {
        if (GetComponent<Collider2D>() != null)
        {
            // BoxCast 시작 위치 계산
            Vector2 boxCastOrigin = new Vector2(
                transform.position.x,
                transform.position.y - (GetComponent<Collider2D>().bounds.extents.y - groundCheckSize.y/2)
            );

            // BoxCast 영역 시각
            Gizmos.color = Color.green;
            
            // 시작 위치의 박스
            Gizmos.DrawWireCube(boxCastOrigin, groundCheckSize);
            
            // 끝 위치의 박스
            Vector2 endPosition = boxCastOrigin + Vector2.down * groundCheckDistance;
            Gizmos.DrawWireCube(endPosition, groundCheckSize);
            
            // BoxCast 경로
            Vector2 leftStart = boxCastOrigin + new Vector2(-groundCheckSize.x/2, 0);
            Vector2 leftEnd = leftStart + Vector2.down * groundCheckDistance;
            Vector2 rightStart = boxCastOrigin + new Vector2(groundCheckSize.x/2, 0);
            Vector2 rightEnd = rightStart + Vector2.down * groundCheckDistance;
            
            Gizmos.DrawLine(leftStart, leftEnd);
            Gizmos.DrawLine(rightStart, rightEnd);
        }

        // OverlapCircle 범위 시각화
        if (GetComponent<Collider2D>() != null)
        {
            Vector2 feetPosition = new Vector2(transform.position.x, 
                transform.position.y - GetComponent<Collider2D>().bounds.extents.y);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(feetPosition, 0.3f);
        }
    }

    private bool isWallBouncing = false;

    private IEnumerator BriefInputDelay()
    {
        isWallBouncing = true;
        yield return new WaitForSeconds(0.1f);  // 0.1 동안 입력 무시
        isWallBouncing = false;
    }

    // OnDisable 추가
    void OnDisable()
    {
        // 스크립트가 비활성화될 때 코루틴 정리
        if (currentDashCoroutine != null)
        {
            StopCoroutine(currentDashCoroutine);
            isDashing = false;
            currentDashCoroutine = null;
        }
    }

    private IEnumerator DisableCollisionCoroutine(Collider2D platformCollider)
    {
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        rb.velocity = new Vector2(rb.velocity.x, -2f);
        
        // 다른 플랫폼에 착지하거나 0.5초가 지날 때까지 대기
        float timer = 0;
        bool hasLanded = false;
        
        while (timer < 0.5f && !hasLanded)
        {
            timer += Time.deltaTime;
            
            // 새로운 플��폼에 착지했는지 확인
            if (IsGrounded && !Physics2D.GetIgnoreCollision(playerCollider, platformCollider))
            {
                hasLanded = true;
                Debug.Log("새로운 플랫폼에 착지");
            }
            
            yield return null;
        }
        
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
        canDropDown = true;  // 아래키 입력 다시 활성화
        Debug.Log("플랫폼 통과 완료 - 아래키 입력 가능");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Enemy 태그로 수정
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 충돌 지점이 몬스터의 위쪽인지 확인
            float monsterTop = collision.collider.bounds.max.y;
            float playerBottom = playerCollider.bounds.min.y;

            if (playerBottom >= monsterTop - 0.1f)
            {
                // 몬스터 머리 위에서 충돌했을 때 점프 횟수만 초기화
                remainingJumps = maxJumpCount;
                Debug.Log("Monster head hit - jumps reset!");
            }
        }
    }
}

