using Photon.Pun;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region ���� ����
    public PhotonView pv { get; private set; }
    PlayManager playManager;
    BoxCollider boxCol;
    Camera cam;

    [Header("�̵�")]
    [SerializeField] float mov = 0;
    Vector3 dir = Vector3.zero;
    public Rigidbody rb;
    public float speed;
    public float jumpPower;
    public float curve;
    float jumpingMove;

    [Header("�ִϸ��̼�")]
    [SerializeField] Animator ani;
    [SerializeField] bool aniSwitch = false;
    bool aniTrigger = false;
    bool isLanding;

    [Header("�̺�Ʈ/ȿ��")]
    [SerializeField] GameObject stunEffect;
    [SerializeField] LayerMask lm;
    [SerializeField] float fovSpeed = 0.01f;
    Transform preObject = null;
    float targetFov;
    bool isStun = false;

    [Header("�̺�Ʈ/ȿ��")]
    AudioSource audioSource;
    #endregion

    #region �̺�Ʈ �Լ�
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
        // ������ or ���� �浹���� �� �����ֱ�
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

            Debug.Log("��");
        }
    }
    #endregion

    #region �Լ�
    /// <summary>
    /// ���� �ִϸ��̼�
    /// </summary>
    /// <returns></returns>
    IEnumerator LandingAni()
    
    {
        aniSwitch = false;  // �ڷ�ƾ ���� ������ �������� false
        yield return null;
        ani.SetBool("isLand", !ani.GetBool("isLand"));
    }
    /// <summary>
    /// �̵� �Է¹ޱ�
    /// </summary>
    void Move()
    {
        mov = Input.GetAxis("Vertical");

        transform.position += transform.forward * mov * Time.deltaTime * speed;
        dir.y = Input.GetAxis("Horizontal") * Time.deltaTime * curve;
        jumpingMove = dir.y;    // �����߿��� �ణ�� x�� �̵��� ���� ���� ����
        transform.eulerAngles += dir;
        transform.position += transform.right * jumpingMove * Time.deltaTime * 5f;
    }
   /// <summary>
   /// �ڵ� ���� / ������
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
   /// �ӵ� ��ȭ�� ����
   /// FOV ��ȭ
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
                // ���� �������� �ִٴ� ���
                targetFov += fovSpeed;
            }
            else if(currentMov - preMov < 0 || currentMov == 0)
            {
                // ���� �������� �ִٴ� ���
                targetFov -= fovSpeed * 1.5f;
            }
            cam.fieldOfView = Mathf.Clamp(targetFov, 60, 80);
            preMov = currentMov;
            yield return null;
        }

        //// �ڷΰ����� fov�� �����δ�.
        //// �ڷΰ����� fov�� ���̱⸸ ���� �� �־�� ��
        //float distance;
        //Vector3 currentVelocity;
        //while (true)
        //{
        //    //������ �ٵ� ���� ���� �ӵ� �˾Ƴ���
        //    currentVelocity = rb.velocity;
        //    //�ӵ����� y���� ����
        //    //������ ���� ������ �� �Ÿ��� �þ�°��� �ƴϴ�!
        //    currentVelocity.y = 0;
        //    //�ӵ��� �ӷ����� ��ȯ
        //    Debug.Log(currentVelocity);
        //    if (currentVelocity.z > 0)
        //    {
        //        // currentVelocity.z�� ����϶���(������ ����) ��ġ�� ���� fov��ȯ�� ���ְ�
        //        // �����϶��� 60���� �پ��⸸ �ϰԲ�
        //        distance = currentVelocity.magnitude;

        //        //�ӷ��� 0�̸� 60
        //        //�ְ� �ӷ��̸� 80
        //        //���� ��ǥ fov ����
        //        targetFov = Mathf.Lerp(60, 80, distance / maxDistance);

        //        //ī�޶� �������� fov ��������
        //        currentFov = cam.fieldOfView;

        //        //���� �������� fov �� ���� Ÿ���� fov�� �� Ŭ��
        //        //�����ų fov�� fov������ ��ŭ ���� Ȥ�� ����
        //        currentFov += currentFov < targetFov ? fovSpeed : -fovSpeed;

        //        //�����ų fov�� 60���� ���ų� ũ�� 80���� ���ų� ���� ������ ����
        //        //���� 59.99, 80.111 �̷� �� �� �� ���Բ�
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
   /// ���� ȿ��
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
