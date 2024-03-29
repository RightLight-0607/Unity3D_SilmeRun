using Photon.Pun;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region 변수 선언
    public PhotonView pv { get; private set; }
    PlayManager playManager;
    BoxCollider boxCol;
    Camera cam;

    [Header("이동")]
    [SerializeField] float mov = 0;
    Vector3 dir = Vector3.zero;
    public Rigidbody rb;
    public float speed;
    public float jumpPower;
    public float curve;
    float jumpingMove;

    [Header("애니메이션")]
    [SerializeField] Animator ani;
    [SerializeField] bool aniSwitch = false;
    bool aniTrigger = false;
    bool isLanding;

    [Header("이벤트/효과")]
    [SerializeField] GameObject stunEffect;
    [SerializeField] LayerMask lm;
    [SerializeField] float fovSpeed = 0.01f;
    Transform preObject = null;
    float targetFov;
    bool isStun = false;

    [Header("이벤트/효과")]
    AudioSource audioSource;
    #endregion

    #region 이벤트 함수
    private void Awake()
    {
        cam = Camera.main;
        audioSource = GetComponent<AudioSource>();
        if (GameManager.instance.isSolo)
            return;
        pv = GetComponent<PhotonView>();
    }
    IEnumerator Start()
    {
        GameObject temp;
        while (true)
        {
            temp = GameObject.Find("PlayManager");

            if (temp != null)
            {
                playManager = temp.GetComponent<PlayManager>();
                break;
            }
            yield return null;
        }
        if (!playManager.isSolo)
        {
            if (pv.IsMine)
            {
                rb = this.AddComponent<Rigidbody>();
                rb.freezeRotation = true;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                boxCol = GetComponent<BoxCollider>();
                boxCol.isTrigger = false;
            }
        }
        StartCoroutine(FovEffect());
    }


    void Update()
    {
        if (!GameManager.instance.isSolo && !pv.IsMine)
            return;

        Jump();

        if (isStun || !playManager.isPlaying)
            return;

        Move();

        ani.SetFloat("HighPoint", rb.velocity.magnitude);
        if (rb.velocity.magnitude < 0.5 && aniTrigger)
        {
            aniSwitch = true;   
            aniTrigger = false;
        }
        if (aniSwitch)
            StartCoroutine(LandingAni());
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 3)
        {
            isLanding = true;
            preObject = collision.transform;
        }
        // 데드존 or 벽과 충돌했을 때 스턴주기
        else if (collision.gameObject.CompareTag("Wall"))
        {
            if (isStun)
                return;

            StartCoroutine(Stun(collision));
        }
        else if (collision.gameObject.CompareTag("Water"))
        {
            if (isStun)
                return;

            StartCoroutine(Stun(collision));
            transform.position = preObject.position + preObject.up * 3;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            if (playManager.isPlaying)
                StartCoroutine(playManager.GoalGame());

            Debug.Log("골");
        }
    }
    #endregion

    #region 함수
    /// <summary>
    /// 착지 애니메이션
    /// </summary>
    /// <returns></returns>
    IEnumerator LandingAni()
    
    {
        aniSwitch = false;  // 코루틴 연속 실행을 막기위한 false
        yield return null;
        ani.SetBool("isLand", !ani.GetBool("isLand"));
    }
    /// <summary>
    /// 이동 입력받기
    /// </summary>
    void Move()
    {
        mov = Input.GetAxis("Vertical");

        transform.position += transform.forward * mov * Time.deltaTime * speed;
        dir.y = Input.GetAxis("Horizontal") * Time.deltaTime * curve;
        jumpingMove = dir.y;    // 점프중에는 약간의 x축 이동을 위해 따로 저장
        transform.eulerAngles += dir;
        transform.position += transform.right * jumpingMove * Time.deltaTime * 5f;
    }
   /// <summary>
   /// 자동 점프 / 강점프
   /// </summary>
    void Jump()
    {
        if (isStun)
            return;
        if (isLanding)
        {
            isLanding = false;
            rb.velocity = Vector3.zero;
            
            rb.AddForce(((transform.up + transform.forward * mov) * speed) * jumpPower);
            aniTrigger = true;

            audioSource.Play();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (Physics.Raycast(transform.position, -Vector3.up, 0.5f, lm))
            {
                rb.AddForce(transform.up * 1.5f * jumpPower);
                aniTrigger = true;
                audioSource.Play();
            }
        }
    }
   /// <summary>
   /// 속도 변화에 따른
   /// FOV 변화
   /// </summary>
   /// <returns></returns>
    IEnumerator FovEffect()
    {
        float currentMov;
        float preMov = 0;
        while (true)
        {
            currentMov = mov;
            targetFov = cam.fieldOfView;
            if(mov > 0 && (currentMov - preMov > 0 || currentMov == 1))
            {
                // 점점 빨라지고 있다는 얘기
                targetFov += fovSpeed;
            }
            else if(currentMov - preMov < 0 || currentMov == 0)
            {
                // 점점 느려지고 있다는 얘기
                targetFov -= fovSpeed * 1.5f;
            }
            cam.fieldOfView = Mathf.Clamp(targetFov, 60, 80);
            preMov = currentMov;
            yield return null;
        }

        //// 뒤로갈때도 fov가 움직인다.
        //// 뒤로갈때는 fov를 줄이기만 해줄 수 있어야 함
        //float distance;
        //Vector3 currentVelocity;
        //while (true)
        //{
        //    //리지드 바디 에서 현재 속도 알아내기
        //    currentVelocity = rb.velocity;
        //    //속도에서 y값은 제외
        //    //점프로 인해 앞으로 간 거리가 늘어나는것이 아니다!
        //    currentVelocity.y = 0;
        //    //속도를 속력으로 변환
        //    Debug.Log(currentVelocity);
        //    if (currentVelocity.z > 0)
        //    {
        //        // currentVelocity.z가 양수일때만(앞으로 전진) 위치에 따른 fov변환을 해주고
        //        // 음수일때는 60까지 줄어들기만 하게끔
        //        distance = currentVelocity.magnitude;

        //        //속력이 0이면 60
        //        //최고 속력이면 80
        //        //으로 목표 fov 설정
        //        targetFov = Mathf.Lerp(60, 80, distance / maxDistance);

        //        //카메라에 적용중인 fov 가져오기
        //        currentFov = cam.fieldOfView;

        //        //현재 적용중인 fov 값 보다 타겟의 fov가 더 클때
        //        //적용시킬 fov에 fov증가량 만큼 증가 혹은 감소
        //        currentFov += currentFov < targetFov ? fovSpeed : -fovSpeed;

        //        //적용시킬 fov가 60보다 같거나 크고 80보다 같거나 작은 값으로 설정
        //        //이유 59.99, 80.111 이런 값 할 수 없게끔
        //        cam.fieldOfView = Mathf.Clamp(currentFov, 60, 80);
        //    }

        //    else
        //    {
        //        currentFov -= fovSpeed;
        //        cam.fieldOfView = Mathf.Clamp(currentFov, 60, 80);
        //    }

        //    yield return null;
        //}
    }

   /// <summary>
   /// 스턴 효과
   /// </summary>
   /// <param name="wall"></param>
   /// <returns></returns>
    IEnumerator Stun(Collision wall)
    {
        rb.velocity = Vector3.zero;
        rb.AddForce((transform.up - transform.forward) * 50f);
        isStun = true;
        stunEffect.SetActive(true);
        mov = 0;
        yield return new WaitForSeconds(3f);
        isStun = false;
        stunEffect.SetActive(false);
    }
    #endregion
}
