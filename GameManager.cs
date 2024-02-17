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

#region 구조체 및 클래스
/// <summary>
/// 플레이어의 기본 정보
/// </summary>
public struct PlayerInfo
{
    public int characterIndex;    // 캐릭터 종류
    public string name;         // 닉네임
    public float rapTime;       // 랩타임
    public int course;
}
/// <summary>
/// 플레이어의 기록
/// </summary>
public class Record
{
    public string name;
    public float rapTime;
}
/// <summary>
/// 기록 표기를 위해
/// </summary>
[Serializable]
public struct SoloRecordObject
{
    public TextMeshProUGUI name;
    public TextMeshProUGUI rapTime;
    public GameObject rankingImage;
}
/// <summary>
/// 멀티방 입장시 띄워야 할 정보
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

    #region 변수 선언
    public static GameManager instance;
    public bool isSolo;

    [Header("로그인")]
    [SerializeField] InputField nameField;
    [SerializeField] GameObject nameErrorImage;

    [Header("내정보")]
    [SerializeField] GameObject playerInfo;
    [SerializeField] Image selectedCharacterImage;
    public TextMeshProUGUI nameText;
    public PlayerInfo me;

    [Header("멀티")]
    [SerializeField]
    MultiInfoPos[] multiPos = new MultiInfoPos[2];
    [SerializeField] TextMeshProUGUI roomNameText;
    [HideInInspector] public PhotonView pv;
    List<RoomInfo> myRoomList;
    PlayerInfo[] playerList = new PlayerInfo[3];
    GameObject roomTemp;

    [Header("기록 / 랭크")]
    [SerializeField] SoloRecordObject[] soloRecordObject;
    [HideInInspector] public string[] COURSE_KEY = new string[3];
    public Record[] records = new Record[5];

    [Header("캔버스 창")]
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

    [Header("코스선택")]
    [SerializeField] Image courseChoiceImage;
    [SerializeField] Image courseInfoImage;
    [SerializeField] Sprite[] courseImage = new Sprite[3];
    public Sprite[] characterImage = new Sprite[4];

    [Header("캔버스 버튼")]
    [SerializeField] GameObject roomButtonOrigin;
    [SerializeField] Button courseChoiceButton;
    [SerializeField] Button gameStartButton;

    [Header("사운드")]
    public AudioSource lobbyBGM;
    #endregion

    #region 이벤트함수
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
        Debug.Log("서버 연결");
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("로비 연결");
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"룸 리스트 업데이트 ::::::: 현재 방 갯수 : {roomList.Count}");
        myRoomList = roomList;

        RoomListUpdate();
    }
    public override void OnCreatedRoom()
    {
        Debug.Log("방장 추가");
        playerList[0] = me;
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorImage.GetComponentInChildren<TextMeshProUGUI>().text = "방 생성이 불가합니다.";
        errorImage.SetActive(true);
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("방 연결");
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
        Debug.Log("입장 실패");
        errorImage.GetComponentInChildren<TextMeshProUGUI>().text = "방 입장이 불가합니다.";
        errorImage.SetActive(true);
        // 실패 화면을 띄워주는게 더 좋을듯
        // 실패시 입장모션(로딩화면) 안뜨도록
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
        // 내 리스트 초기화
        for (int i = 0; i < playerList.Length; i++)
        {
            playerList[i].name = null;
            playerList[i].characterIndex = 0;
        }
    }
    #endregion

    #region 함수


    /// <summary>
    /// 해상도 고정
    /// </summary>
    void FixedResolution()
    {
        int w = 1920;
        int h = 1080;

        Screen.SetResolution(w, h, false);
    }

    /// <summary>
    /// 닉네임 입력 후 접속 버튼 누를 때 실행
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
    /// 닉네임 중복 체크
    /// PhotonNetwork.PlayerList는 룸 안에 플레이어들이 들어가는 배열
    /// 서버에 들어가있는 플레이어들의 닉네임을 알기에는 무리가 있음
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
    /// 같이하기 버튼 클릭 시
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
    /// 방 목록 업데이트
    /// 업데이트 시 기존에 있던 방을 전부 삭제 한 후
    /// 방 List를 확인해서 새롭게 생성
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
        Debug.Log($"룸 리스트 업데이트 ::::::: 현재 방 갯수 : {myRoomList.Count}");
    }

    /// <summary>
    /// 방 생성시
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
    /// 멀티에서 새로운 플레이어 입장 시
    /// 방장이 playerList에 추가
    /// </summary>
    /// <param name="info"></param>
    [PunRPC]
    void AddPlayerInfo(string info)
    {
        // 방장이 할 일 > 리스트에 추가
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
    /// 멀티에서 기존의 플레이어 떠날 시
    /// 방장이 playerList에서 삭제
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
    /// 방장이 갱신한 playerList를
    /// 참가자들에게 보내는 함수
    /// </summary>
    /// <param name="jsonKey"></param>
    [PunRPC]
    void PlayerListUpdate(string jsonKey)
    {
        playerList = JsonConvert.DeserializeObject<PlayerInfo[]>(jsonKey);
        MultiRoomInfoUpdate(playerList);
    }

    /// <summary>
    /// 멀티방에서 시각적으로 방 정보 업데이트
    /// 플레이어 목록 및 맵 정보도 포함
    /// </summary>
    /// <param name="playerList"></param>
    void MultiRoomInfoUpdate(PlayerInfo[] playerList)
    {
        int j = 0;
        //// 갱신된 리스트를 이용하여 각 플레이어를 해당 위치로 이동
        for (int i = 0; i < playerList.Length; i++)
        {
            // 나를 제외한 순서대로 위치에 삽입
            // 이름으로 비교하려면 이름 설정 시 중복이 안되게끔 해야함
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
    /// 혼자하기 버튼 클릭 시
    /// </summary>
    public void OnSoloRoom()
    {
        isSolo = true;
        EnterRoom();
    }

    /// <summary>
    /// 대기방에서 게임 시작 버튼 클릭 시
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
    /// 코스 선택 시 실행될 함수
    /// 혼자하기 / 같이하기 모두 사용
    /// </summary>
    /// <param name="index"></param>
    public void CourseChoice(int index)
    {
        me.course = index;
        courseChoiceImage.sprite = courseImage[index];
        // 이미지 바꾸기 (멀티에서는 참가자들에게도 맵이 바뀌었다는걸 알려줘야함)
        // PunRPC로 보내야 할 정보
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
    /// 코스선택시 마우스 올렸을 때 이미지 변경할 함수
    /// </summary>
    /// <param name="index"></param>
    public void CoruseInfo(int index)
    {
        courseInfoImage.sprite = courseImage[index];
    }

    /// <summary>
    /// 캐릭터 변경 시 내 캐릭터 변경
    /// </summary>
    /// <param name="index"></param>
    public void CharacterChoice(int index)
    {
        me.characterIndex = index;
        selectedCharacterImage.sprite = characterImage[index];
    }

    /// <summary>
    /// 대기방에서 나갈 시
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
            PhotonNetwork.LeaveRoom(); // 룸(대기방) 접속 종료
        }        
    }
    

    /// <summary>
    /// 대기방에 들어갈 때 해야할 행동
    /// </summary>
    public void EnterRoom()
    {
        room.SetActive(true);
        closeButton.SetActive(false);
        if (isSolo)
        {
            InRoomName.text = "혼자하기";
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
    /// 혼자하기 시 맵에 따른 기록 불러오고 표시하기
    /// </summary>
    /// <param name="keyIndex"></param>
    public void RecordUpdate(int keyIndex)
    {
        // 기록 및 기록창 불러오기
        // 대기방에서 맵 바꿀때 또한 불러와야 함
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
    /// 게임 종료 시
    /// </summary>
    public void CloseGame()
    {
        PhotonNetwork.Disconnect(); // 서버 접속 종료

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion

    // 완성 후 삭제
    // 테스트용 함수
    public void PlayerCheck()
    {
        Debug.Log(playerList[0].name);
        Debug.Log(playerList[0].course);
        Debug.Log(me.course);
        //Debug.Log(PhotonNetwork.PlayerList.Count() + "명");
        //Debug.Log(PhotonNetwork.IsMasterClient);
        //Debug.Log(me.name);
        //Debug.Log(instance);
        //Debug.Log(PhotonNetwork.CountOfRooms + "개 방수");
        //Debug.Log(me.characterIndex);
        //Debug.Log(me.course);
        //Debug.Log(pv.Controller.NickName); -> 플레이중에만 확인 가능
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


