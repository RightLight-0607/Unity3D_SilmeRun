using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json;
using Unity.VisualScripting;
using System.Collections;

#region ����ü �� Ŭ����
/// <summary>
/// �÷��̾��� �⺻ ����
/// </summary>
public struct PlayerInfo
{
    public int characterIndex;    // ĳ���� ����
    public string name;         // �г���
    public float rapTime;       // ��Ÿ��
    public int course;
}
/// <summary>
/// �÷��̾��� ���
/// </summary>
public class Record
{
    public string name;
    public float rapTime;
}
/// <summary>
/// ��� ǥ�⸦ ����
/// </summary>
[Serializable]
public struct SoloRecordObject
{
    public TextMeshProUGUI name;
    public TextMeshProUGUI rapTime;
    public GameObject rankingImage;
}
/// <summary>
/// ��Ƽ�� ����� ����� �� ����
/// </summary>
[Serializable]
public struct MultiInfoPos
{
    public Image characterPos;
    public TextMeshProUGUI nameText;
}

#endregion

public class GameManager : MonoBehaviourPunCallbacks
{

    #region ���� ����
    public static GameManager instance;
    public bool isSolo;

    [Header("�α���")]
    [SerializeField] InputField nameField;
    [SerializeField] GameObject nameErrorImage;

    [Header("������")]
    [SerializeField] GameObject playerInfo;
    [SerializeField] Image selectedCharacterImage;
    public TextMeshProUGUI nameText;
    public PlayerInfo me;

    [Header("��Ƽ")]
    [SerializeField]
    MultiInfoPos[] multiPos = new MultiInfoPos[2];
    [SerializeField] TextMeshProUGUI roomNameText;
    [HideInInspector] public PhotonView pv;
    List<RoomInfo> myRoomList;
    PlayerInfo[] playerList = new PlayerInfo[3];
    GameObject roomTemp;

    [Header("��� / ��ũ")]
    [SerializeField] SoloRecordObject[] soloRecordObject;
    [HideInInspector] public string[] COURSE_KEY = new string[3];
    public Record[] records = new Record[5];

    [Header("ĵ���� â")]
    [SerializeField] GameObject lobbyImage;
    [SerializeField] GameObject loginImage;
    [SerializeField] GameObject room;
    [SerializeField] GameObject soloRecordList;
    [SerializeField] GameObject multiPlayerListObject;
    [SerializeField] GameObject errorImage;
    [SerializeField] GameObject storyScroll;
    [SerializeField] GameObject closeButton;
    [SerializeField] TextMeshProUGUI InRoomName;
    [SerializeField] Transform roomContent;
    [HideInInspector] public GameObject canvas;
    public GameObject lodingImage;

    [Header("�ڽ�����")]
    [SerializeField] Image courseChoiceImage;
    [SerializeField] Image courseInfoImage;
    [SerializeField] Sprite[] courseImage = new Sprite[3];
    public Sprite[] characterImage = new Sprite[4];

    [Header("ĵ���� ��ư")]
    [SerializeField] GameObject roomButtonOrigin;
    [SerializeField] Button courseChoiceButton;
    [SerializeField] Button gameStartButton;

    [Header("����")]
    public AudioSource lobbyBGM;
    #endregion

    #region �̺�Ʈ�Լ�
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        FixedResolution();

        instance = this;

        pv = this.AddComponent<PhotonView>();
        pv.ViewID = 1;
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        for (int i = 0; i < COURSE_KEY.Length; i++)
        {
            COURSE_KEY[i] = "Course" + i.ToString();
        }
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("���� ����");
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("�κ� ����");
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"�� ����Ʈ ������Ʈ ::::::: ���� �� ���� : {roomList.Count}");
        myRoomList = roomList;

        RoomListUpdate();
    }
    public override void OnCreatedRoom()
    {
        Debug.Log("���� �߰�");
        playerList[0] = me;
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorImage.GetComponentInChildren<TextMeshProUGUI>().text = "�� ������ �Ұ��մϴ�.";
        errorImage.SetActive(true);
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("�� ����");
        isSolo = false;
        EnterRoom();

        if (!PhotonNetwork.IsMasterClient)
        {
            pv.RPC("AddPlayerInfo", RpcTarget.MasterClient, JsonConvert.SerializeObject(me));
            courseChoiceButton.interactable = false;
            gameStartButton.interactable = false;
            return;
        }

    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("���� ����");
        errorImage.GetComponentInChildren<TextMeshProUGUI>().text = "�� ������ �Ұ��մϴ�.";
        errorImage.SetActive(true);
        // ���� ȭ���� ����ִ°� �� ������
        // ���н� ������(�ε�ȭ��) �ȶߵ���
    }
    public override void OnMasterClientSwitched(Photon.Realtime.Player p)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            courseChoiceButton.interactable = true;
            gameStartButton.interactable = true;
        }
    }
    public override void OnLeftRoom()
    {
        // �� ����Ʈ �ʱ�ȭ
        for (int i = 0; i < playerList.Length; i++)
        {
            playerList[i].name = null;
            playerList[i].characterIndex = 0;
        }
    }
    #endregion

    #region �Լ�


    /// <summary>
    /// �ػ� ����
    /// </summary>
    void FixedResolution()
    {
        int w = 1920;
        int h = 1080;

        Screen.SetResolution(w, h, false);
    }

    /// <summary>
    /// �г��� �Է� �� ���� ��ư ���� �� ����
    /// </summary>
    public void OnClickLogin()
    {
        if (SetNickname())
            return;
        storyScroll.SetActive(true);
        PhotonNetwork.ConnectUsingSettings();
        loginImage.SetActive(false);
        lobbyImage.SetActive(true);
        playerInfo.SetActive(true);
        CharacterChoice(0);
        me.name = PhotonNetwork.NickName;
        nameText.text = me.name;
    }
    bool SetNickname()
    {
        if (CheckNickName(nameField.text))
        {
            nameErrorImage.SetActive(true);
            return true;
        }
        PhotonNetwork.NickName = nameField.text;
        return false;
    }

    /// <summary>
    /// �г��� �ߺ� üũ
    /// PhotonNetwork.PlayerList�� �� �ȿ� �÷��̾���� ���� �迭
    /// ������ ���ִ� �÷��̾���� �г����� �˱⿡�� ������ ����
    /// </summary>
    /// <param name="nickname"></param>
    /// <returns></returns>
    bool CheckNickName(string nickname)
    {
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (nickname == player.NickName)
                return true;
        }
        return false;
    }

    /// <summary>
    /// �����ϱ� ��ư Ŭ�� ��
    /// </summary>
    public void OnMulti()
    {
        PhotonNetwork.JoinLobby();
    }
    public void RoomLlstUpdateButton()
    {
        StartCoroutine(MultiUpdateButton());
    }
    IEnumerator MultiUpdateButton()
    {
        yield return PhotonNetwork.LeaveLobby();
        yield return PhotonNetwork.JoinLobby();
        RoomListUpdate();
    }
    /// <summary>
    /// �� ��� ������Ʈ
    /// ������Ʈ �� ������ �ִ� ���� ���� ���� �� ��
    /// �� List�� Ȯ���ؼ� ���Ӱ� ����
    /// </summary>
    public void RoomListUpdate()
    {
        for (int i = 0; i < roomContent.childCount; i++)
        {
            Destroy(roomContent.GetChild(i).gameObject);
        }

        for (int i = 0; i < myRoomList.Count; i++)
        {
            roomTemp = GameObject.Instantiate(roomButtonOrigin, roomContent);
            roomTemp.GetComponentInChildren<TextMeshProUGUI>().text = myRoomList[i].Name;
            roomTemp.transform.Find("RoomNameText").GetComponent<TextMeshProUGUI>().text = myRoomList[i].Name;
            roomTemp.transform.Find("PersonnelText").GetComponent<TextMeshProUGUI>().text = myRoomList[i].PlayerCount + " / " + myRoomList[i].MaxPlayers;
        }
        Debug.Log($"�� ����Ʈ ������Ʈ ::::::: ���� �� ���� : {myRoomList.Count}");
    }

    /// <summary>
    /// �� ������
    /// </summary>
    public void CreatedRoom()
    {
        CourseChoice(0);
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 3;
        options.IsOpen = true;

        PhotonNetwork.CreateRoom(roomNameText.text, options, null);
    }

    /// <summary>
    /// ��Ƽ���� ���ο� �÷��̾� ���� ��
    /// ������ playerList�� �߰�
    /// </summary>
    /// <param name="info"></param>
    [PunRPC]
    void AddPlayerInfo(string info)
    {
        // ������ �� �� > ����Ʈ�� �߰�
        PlayerInfo temp = JsonConvert.DeserializeObject<PlayerInfo>(info);
        for (int i = 0; i < playerList.Length; i++)
        {
            if (playerList[i].name == null)
            {
                playerList[i] = temp;
                break;
            }
        }
        pv.RPC("PlayerListUpdate", RpcTarget.Others, JsonConvert.SerializeObject(playerList));
        MultiRoomInfoUpdate(playerList);
    }

    /// <summary>
    /// ��Ƽ���� ������ �÷��̾� ���� ��
    /// ������ playerList���� ����
    /// </summary>
    /// <param name="info"></param>
    [PunRPC]
    void LeftPlayerInfo(string info)
    {
        PlayerInfo temp = JsonConvert.DeserializeObject<PlayerInfo>(info);
        for (int i = 0; i < playerList.Length; i++)
        {
            if (playerList[i].name == temp.name)
            {
                playerList[i].name = null;
                break;
            }
        }
        pv.RPC("PlayerListUpdate", RpcTarget.Others, JsonConvert.SerializeObject(playerList));
        MultiRoomInfoUpdate(playerList);
    }

    /// <summary>
    /// ������ ������ playerList��
    /// �����ڵ鿡�� ������ �Լ�
    /// </summary>
    /// <param name="jsonKey"></param>
    [PunRPC]
    void PlayerListUpdate(string jsonKey)
    {
        playerList = JsonConvert.DeserializeObject<PlayerInfo[]>(jsonKey);
        MultiRoomInfoUpdate(playerList);
    }

    /// <summary>
    /// ��Ƽ�濡�� �ð������� �� ���� ������Ʈ
    /// �÷��̾� ��� �� �� ������ ����
    /// </summary>
    /// <param name="playerList"></param>
    void MultiRoomInfoUpdate(PlayerInfo[] playerList)
    {
        int j = 0;
        //// ���ŵ� ����Ʈ�� �̿��Ͽ� �� �÷��̾ �ش� ��ġ�� �̵�
        for (int i = 0; i < playerList.Length; i++)
        {
            // ���� ������ ������� ��ġ�� ����
            // �̸����� ���Ϸ��� �̸� ���� �� �ߺ��� �ȵǰԲ� �ؾ���
            if (playerList[i].name == null)
            {
                multiPos[j].nameText.text = "";
                multiPos[j].characterPos.sprite = null;
            }
            else if (me.name != playerList[i].name)
            {
                multiPos[j].nameText.text = playerList[i].name;
                multiPos[j].characterPos.sprite = characterImage[playerList[i].characterIndex];
                j++;
            }
        }
        MultiCourseChange(playerList[0].course);
    }
   

    /// <summary>
    /// ȥ���ϱ� ��ư Ŭ�� ��
    /// </summary>
    public void OnSoloRoom()
    {
        isSolo = true;
        EnterRoom();
    }

    /// <summary>
    /// ���濡�� ���� ���� ��ư Ŭ�� ��
    /// </summary>
    public void GameStartButton()
    {
        if (isSolo)
        {
            GameStart(me.course);
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                pv.RPC("GameStart", RpcTarget.All, me.course);
            }
        }
    }    
    [PunRPC]
    void GameStart(int num)
    {
        if (!isSolo)
            PhotonNetwork.CurrentRoom.IsOpen = false;
        lobbyBGM.Stop();
        canvas = FindObjectOfType<CanvasManager>().gameObject;
        canvas.SetActive(false);
        me.course = num;
        SceneManager.LoadScene(1);
    }
    
    /// <summary>
    /// �ڽ� ���� �� ����� �Լ�
    /// ȥ���ϱ� / �����ϱ� ��� ���
    /// </summary>
    /// <param name="index"></param>
    public void CourseChoice(int index)
    {
        me.course = index;
        courseChoiceImage.sprite = courseImage[index];
        // �̹��� �ٲٱ� (��Ƽ������ �����ڵ鿡�Ե� ���� �ٲ���ٴ°� �˷������)
        // PunRPC�� ������ �� ����
        if (isSolo)
            RecordUpdate(index);
        else if (!isSolo)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                playerList[0].course = index;
                pv.RPC("MultiCourseChange", RpcTarget.Others, index);
            }
        }
        else
            return;
    }
    [PunRPC]
    void MultiCourseChange(int index)
    {
        me.course = index;
        courseChoiceImage.sprite = courseImage[index];
    }

    /// <summary>
    /// �ڽ����ý� ���콺 �÷��� �� �̹��� ������ �Լ�
    /// </summary>
    /// <param name="index"></param>
    public void CoruseInfo(int index)
    {
        courseInfoImage.sprite = courseImage[index];
    }

    /// <summary>
    /// ĳ���� ���� �� �� ĳ���� ����
    /// </summary>
    /// <param name="index"></param>
    public void CharacterChoice(int index)
    {
        me.characterIndex = index;
        selectedCharacterImage.sprite = characterImage[index];
    }

    /// <summary>
    /// ���濡�� ���� ��
    /// </summary>
    public void OnBackRoom()
    {
        if (isSolo)
        {
            soloRecordList.SetActive(false);
        }
        else if (!isSolo)
        {
            multiPlayerListObject.SetActive(false);
            pv.RPC("LeftPlayerInfo", RpcTarget.MasterClient, JsonConvert.SerializeObject(me));
            PhotonNetwork.LeaveRoom(); // ��(����) ���� ����
        }        
    }
    

    /// <summary>
    /// ���濡 �� �� �ؾ��� �ൿ
    /// </summary>
    public void EnterRoom()
    {
        room.SetActive(true);
        closeButton.SetActive(false);
        if (isSolo)
        {
            InRoomName.text = "ȥ���ϱ�";
            soloRecordList.SetActive(true);
            courseChoiceButton.interactable = true;
            gameStartButton.interactable = true;
            CourseChoice(0);
            RecordUpdate(0);
        }
        else
        {
            InRoomName.text = PhotonNetwork.CurrentRoom.Name;
            multiPlayerListObject.SetActive(true);
        }
    }

    /// <summary>
    /// ȥ���ϱ� �� �ʿ� ���� ��� �ҷ����� ǥ���ϱ�
    /// </summary>
    /// <param name="keyIndex"></param>
    public void RecordUpdate(int keyIndex)
    {
        // ��� �� ���â �ҷ�����
        // ���濡�� �� �ٲܶ� ���� �ҷ��;� ��
        records = JsonConvert.DeserializeObject<Record[]>(PlayerPrefs.GetString(COURSE_KEY[keyIndex]));
        if (records == null)
        {
            records = new Record[5];
            for (int i = 0; i < 5; i++)
            {
                records[i] = null;
            }
        }
        for (int i = 0; i < 5; i++)
        {
            if (records[i] != null)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(records[i].rapTime);
                soloRecordObject[i].name.text = records[i].name;
                soloRecordObject[i].rapTime.text = string.Format("{0:00}:{1:00}.{2:##}", (int)timeSpan.TotalMinutes, timeSpan.TotalSeconds % 60, timeSpan.Milliseconds / 10);
            }
            else
            {
                soloRecordObject[i].name.text = "-";
                soloRecordObject[i].rapTime.text = "-";
            }
        }
    }
    /// <summary>
    /// ���� ���� ��
    /// </summary>
    public void CloseGame()
    {
        PhotonNetwork.Disconnect(); // ���� ���� ����

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion

    // �ϼ� �� ����
    // �׽�Ʈ�� �Լ�
    public void PlayerCheck()
    {
        Debug.Log(playerList[0].name);
        Debug.Log(playerList[0].course);
        Debug.Log(me.course);
        //Debug.Log(PhotonNetwork.PlayerList.Count() + "��");
        //Debug.Log(PhotonNetwork.IsMasterClient);
        //Debug.Log(me.name);
        //Debug.Log(instance);
        //Debug.Log(PhotonNetwork.CountOfRooms + "�� ���");
        //Debug.Log(me.characterIndex);
        //Debug.Log(me.course);
        //Debug.Log(pv.Controller.NickName); -> �÷����߿��� Ȯ�� ����
        for (int i = 0; i < playerList.Length; i++)
        {
            Debug.Log(playerList[i].name);
        }
        PlayerPrefs.DeleteAll();
        GetPlayersInServer();
    }
    void GetPlayersInServer()
    {
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            string nickname = player.NickName;
            Debug.Log("Player nickname: " + nickname);
        }
    }
}


