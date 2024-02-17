using Newtonsoft.Json;
using Photon.Pun;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayManager : MonoBehaviour
{
    #region ���� ����
    [SerializeField] PhotonView pv;
    GameManager gm;
    public bool isSolo { get; private set; }

    [Header ("���ȭ��")]
    [SerializeField] GameObject resultImage;

    [Header("������")]
    [SerializeField] TextMeshProUGUI myRapTimeText;
    [SerializeField] TextMeshProUGUI myInfoNameText;
    [SerializeField] Image myInfoCharacterImage;
    [HideInInspector]public GameObject my;

    [Header("�ð� üũ ����")]
    [SerializeField] TextMeshProUGUI countDownText;
    [SerializeField] TextMeshProUGUI rapTimeText;
    [SerializeField] TextMeshProUGUI multiExitText;
    [SerializeField] AudioSource startCountDownAudioSource;
    [SerializeField] AudioSource goalCountDownAudioSource;
    float RapTime = 0;
    float CountDown = 0;
    public float countDown
    {
        get { return CountDown; }
        set
        {
            CountDown = value;

            countDownText.text = (((int)value % 60) + 1).ToString();
            // ������ ǥ��
        }
    }
    float T;
    public float t
    {
        get { return T; }
        set
        {
            T = value;
            multiExitText.text = ((int)value % 60).ToString();
        }
    }

    [Header("������ ��������")]
    [SerializeField] string[] myCharacter;
    [SerializeField] GameObject[] myCharacterObject;
    [SerializeField] GameObject[] courseOrigin;
    GameObject courseTemp;
    Transform startPos;
    int courseIndex;
    int characterIndex;

    [Header("��� üũ")]
    [SerializeField] SoloRecordObject[] soloRecordObject;
    [SerializeField] SoloRecordObject[] multiRankObject;
    [SerializeField] Sprite[] medalImage;
    [SerializeField] TextMeshProUGUI ranking;
    [SerializeField] GameObject soloRecordList;
    [SerializeField] GameObject multiRank;
    [SerializeField] GameObject breakRecord;
    [SerializeField] GameObject outOfRecord;
    [SerializeField] Image medalPos;
    [HideInInspector] public bool isRetire;
    [HideInInspector] public bool onFirst;
    Record[] rankInfo;
    Record record;
    public float rapTime
    {
        get { return RapTime; }
        set
        {
            RapTime = value;
            TimeSpan timeSpan = TimeSpan.FromSeconds(value);
            rapTimeText.text = string.Format("{0:00}:{1:00}.{2:##}", (int)timeSpan.TotalMinutes, timeSpan.TotalSeconds % 60, timeSpan.Milliseconds / 10);
            //rapTimeText.text = ((int)(value / 60)).ToString() + " : " + ((int)(value % 60)).ToString();
            // 00:00 �������� ǥ��
        }
    }

    [Header("���")]
    [SerializeField] GameObject exitButton;
    [SerializeField] GameObject rankUpdatingImage;
    [SerializeField] TextMeshProUGUI rankUpdatingText;
    [HideInInspector] public bool isPlaying;
    [SerializeField] AudioSource[] courseBGM;

    #endregion

    #region �̺�Ʈ �Լ�
    private void Awake()
    {
        gm = GameManager.instance;

        resultImage.gameObject.SetActive(false);
        isSolo = gm.isSolo;
        isRetire = true;
        onFirst = false;
        courseIndex = gm.me.course;
        characterIndex = gm.me.characterIndex;
        courseTemp = GameObject.Instantiate(courseOrigin[courseIndex]);
        startPos = GameObject.FindWithTag("StartPos").transform;

        if (isSolo)
        {
            my = GameObject.Instantiate(myCharacterObject[characterIndex], startPos.position, Quaternion.identity);
        }
        else
        {
            Vector3 startPosA = startPos.position + (startPos.right * startPos.localScale.x);
            Vector3 startPosZ = startPos.position - (startPos.right * startPos.localScale.x);
            Vector3 myPos = Vector3.Lerp(startPosA, startPosZ, UnityEngine.Random.Range(0, 2) * 0.3f);
            my = PhotonNetwork.Instantiate(myCharacter[characterIndex], myPos, Quaternion.identity);
            rankInfo = new Record[PhotonNetwork.PlayerList.Length];
        }
        courseBGM[courseIndex].Play();
        myInfoNameText.text = gm.me.name;
        myInfoCharacterImage.sprite = gm.characterImage[gm.me.characterIndex];
        rapTime = 0;
        StartGame();
    }
    void Update()
    {
        if (isPlaying)
            rapTime += Time.deltaTime;
    }
    #endregion

    #region �Լ�

    /// <summary>
    /// ���� ���� ��
    /// </summary>
    void StartGame()
    {
        StartCoroutine(CountDownTextEffect(false));
    }

    /// <summary>
    /// ���ֽ�
    /// </summary>
    /// <returns></returns>
    public IEnumerator GoalGame()
    {
        isPlaying = false;
        isRetire = false;
        myRapTimeText.text = rapTimeText.text;
        yield return null;

        record = new Record();
        record.name = gm.me.name;
        record.rapTime = rapTime;

        if (isSolo)
        {
            ShowSoloRank();
            StartCoroutine(CountDownTextEffect(true));
        }
        else
        {
            pv.RPC("MultiRankListAdd", RpcTarget.MasterClient, JsonConvert.SerializeObject(record));
            if (!onFirst)
                pv.RPC("SendCountDown", RpcTarget.All);
        }
    }
    [PunRPC]
    public void SendCountDown()
    {
        StartCoroutine(MultiGoalCountDown());
    }

    /// <summary>
    /// ������ �÷��̾� ������ �°� �迭�� �߰�
    /// </summary>
    /// <param name="goalInfo"></param>
    [PunRPC]
    void MultiRankListAdd(string goalInfo)
    {
        for (int i = 0; i < rankInfo.Length; i++)
        {
            if (rankInfo[i] == null) 
            {
                rankInfo[i] = JsonConvert.DeserializeObject<Record>(goalInfo);
                break;
            }
        }
        pv.RPC("RankUpdate", RpcTarget.Others, JsonConvert.SerializeObject(rankInfo));
    }
    /// <summary>
    /// ������ ������Ʈ �� �迭 ����
    /// </summary>
    /// <param name="rank"></param>
    [PunRPC]
    void RankUpdate(string rank)
    {   
        rankInfo = JsonConvert.DeserializeObject<Record[]>(rank);
    }
    /// <summary>
    /// ��Ƽ���� ���� �Ǵ�
    /// </summary>
    void ShowMultiRank()
    {
        for (int i = 0; i < rankInfo.Length; i++)
        {
            multiRankObject[i].name.text = rankInfo[i].name;

            if (rankInfo[i].rapTime == 0)
                multiRankObject[i].rapTime.text = "��Ÿ�̾�";
            else
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(rankInfo[i].rapTime);
                multiRankObject[i].rapTime.text = string.Format("{0:00}:{1:00}.{2:##}", (int)timeSpan.TotalMinutes, timeSpan.TotalSeconds % 60, timeSpan.Milliseconds / 10);
                if (i > 0)
                    multiRankObject[i].rankingImage.SetActive(true);
            }

        }
        StartCoroutine(MultiResultCountDown());
        resultImage.gameObject.SetActive(true);
        multiExitText.gameObject.SetActive(true);
        multiRank.SetActive(true);
    }
    /// <summary>
    /// �ַο��� ���� �Ǵ� �� ����
    /// </summary>
    void ShowSoloRank()
    {
        for (int i = 0; i < 5; i++)
        {
            if (gm.records[i] == null)
            {
                breakRecord.SetActive(true);
                ranking.text = (i + 1) + "��";
                gm.records[i] = record;

                if (i < 3)
                {
                    medalPos.sprite = medalImage[i];
                    break;
                }
                else
                {
                    medalPos.gameObject.SetActive(false);
                    break;
                }
            }
            // ����� Ŀ�� ������ ������
            if (gm.records[i].rapTime > record.rapTime)
            {
                breakRecord.SetActive(true);
                ranking.text = (i + 1) + "��";
                for (int j = 4; j >= i; j--)
                {
                    if (j == 0)
                        break;
                    gm.records[j] = gm.records[j - 1];
                }
                gm.records[i] = record;

                if (i < 3)
                {
                    medalPos.sprite = medalImage[i];
                    break;
                }
                else
                {
                    medalPos.gameObject.SetActive(false);
                    break;
                }
            }
            // 4�� �ε���(5��)���� �����غôµ� break���� ���ߴٸ� ������ ���̴�
            if (i == 4)
                outOfRecord.SetActive(true);
        }

        string jsonRecord = JsonConvert.SerializeObject(gm.records);

        PlayerPrefs.SetString(gm.COURSE_KEY[courseIndex], jsonRecord);

        for (int i = 0; i < 5; i++)
        {
            if (gm.records[i] != null)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(gm.records[i].rapTime);
                soloRecordObject[i].name.text = gm.records[i].name;
                soloRecordObject[i].rapTime.text = string.Format("{0:00}:{1:00}.{2:##}", (int)timeSpan.TotalMinutes, timeSpan.TotalSeconds % 60, timeSpan.Milliseconds / 10);
                //soloRecordObject[i].rapTime.text = (gm.records[i].rapTime / 60).ToString() + " : " + (gm.records[i].rapTime % 60).ToString();
            }
            else
            {
                soloRecordObject[i].name.text = "-";
                soloRecordObject[i].rapTime.text = "-";
            }
        }
    }
    
    /// <summary>
    /// �ַ�/��Ƽ ���� �� �ַ� ���� ��
    /// ī��Ʈ�ٿ�
    /// </summary>
    /// <param name="isGoal"></param>
    /// <returns></returns>
    IEnumerator CountDownTextEffect(bool isGoal)
    {
        countDownText.gameObject.SetActive(true);
        // ���� �� ���� ī��Ʈ�ٿ�
        if (!isGoal)
        {
            startCountDownAudioSource.Play();
            countDown = 3f;
            while (countDown >= 0)
            {
                countDown -= Time.deltaTime;
                yield return null;
            }
            isPlaying = true;
        }
        // ���� ���� ī��Ʈ�ٿ�
        else
        {
            countDown = 3; // ȥ���ϱ� �ÿ� ���� �� 3�� ���
            goalCountDownAudioSource.Play();
            while (countDown >= 0)
            {
                countDown -= Time.deltaTime;
                yield return null;
            }
            goalCountDownAudioSource.Stop();
            resultImage.gameObject.SetActive(true);
            soloRecordList.SetActive(true);
            exitButton.SetActive(true);
        }
        countDownText.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// ��Ƽ ���� �� 10�� ���ѽð� ��ٸ���
    /// 1���� ���� �� ������ �� ����
    /// </summary>
    /// <returns></returns>
    IEnumerator MultiGoalCountDown()
    {
        onFirst = true;
        countDownText.gameObject.SetActive(true);
        goalCountDownAudioSource.Play();
        // 10�� ���ð��� ���� ��
        countDown = 10;
        while (countDown >= 0)
        {
            countDown -= Time.deltaTime;
            yield return null;
        }
        goalCountDownAudioSource.Stop();
        yield return new WaitForSeconds(1f);
        if (isRetire)
        {
            isPlaying = false;
            record = new Record();
            record.name = gm.me.name;
            record.rapTime = 0;
            pv.RPC("MultiRankListAdd", RpcTarget.MasterClient, JsonConvert.SerializeObject(record));
        }
        yield return null;
        StartCoroutine(MultiRankUpdating());
        countDownText.gameObject.SetActive(false);
    }
    /// <summary>
    /// ��Ƽ �������� ��
    /// ��� �÷��̾� ��� ���� ���
    /// </summary>
    /// <returns></returns>
    IEnumerator MultiRankUpdating()
    {
        int a = 0;
        rankUpdatingImage.SetActive(true);
        while (a <= 10)
        {
            rankUpdatingText.text = "��� ������" + PointDot((a++ % 3) + 1);

            yield return new WaitForSeconds(0.5f);
        }
        rankUpdatingImage.SetActive(false);
        ShowMultiRank();
    }
    string PointDot(int num)
    {
        string a = "";
        for (int i = 0; i < num; i++)
        {
            a += ".";
        }
        return a;
    }
   
    /// <summary>
    /// ��Ƽ ���ȭ�� 10�� �����ֱ�
    /// </summary>
    IEnumerator MultiResultCountDown()
    {
        t = 10;
        while (t >= 0)
        {
            t -= Time.deltaTime;
            yield return null;
        }
        Exit();
    }

    /// <summary>
    /// ȥ���ϱ� ���� �� '������'��ư ������ ��
    /// </summary>
    public void ExitButton()
    {
        gm.RecordUpdate(courseIndex);
        Exit();
    }

    void Exit()
    {
        if (!isSolo)
            PhotonNetwork.CurrentRoom.IsOpen = true;
        Destroy(my.gameObject);
        gm.canvas.SetActive(true);
        courseBGM[courseIndex].Stop();
        gm.lobbyBGM.Play();
        SceneManager.LoadScene(0);
    }
    #endregion
}
