using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Unity.VisualScripting;
using TMPro;
using System;

public class TestStruct
{
    public TestStruct(string _name, int _num, float _hp, Player _player)
    {
        name = _name;
        num = _num;
        hp = _hp;
        player = _player;
        isYes = false;
        lits = new SoloSave[3];
        numlist = new int[3];
        numlist2 = new List<string>();
    }
    public string name;
    public int num;
    public float hp;
    public bool isYes;
    public Player player;
    public SoloSave[] lits;
    public int[] numlist;
    public List<string> numlist2;
}
public class SoloSave
{
    public int course;
}

public class Test : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        Debug.Log(transform.position + (transform.right * transform.localScale.x / 2));
        Debug.Log(transform.position - (transform.right * transform.localScale.x / 2));
    }










    //float t = 0;
    //int a = 5;
    //[SerializeField] TextMeshProUGUI text;
    //private void Update()
    //{
    //    //text.text = ((int)(t % 60)).ToString("D2");
    //    //text.text = string.Format("{0:F2}", t%60-(int)t);
    //    //text.text = string.Format("{0:D2} : {1:D2}", (int)t / 60, (int)t % 60);
    //    //text.text = string.Format("0:mm\\:ss\\.##", t);
    //    t += Time.deltaTime;
    //    TimeSpan timeSpan = TimeSpan.FromSeconds(t);
    //    text.text = string.Format("{0:00}:{1:00}.{2:##}", (int)timeSpan.TotalMinutes, timeSpan.TotalSeconds % 60, timeSpan.Milliseconds / 10);
    //    //Debug.Log(temp);

    //    //text.text = string.Format("{0:D2} : {1:D2} . {3:F2}", t/60, t%60, t%60 - (int)t);
    //}

















    //List<int> testlist = new List<int>();
    //List<int> testlist1 = new List<int>();
    //[SerializeField] Player testPlayer;
    //List<TestStruct> tests = new List<TestStruct>();
    //private void Start()
    //{
    //    // 리스트 덮어씌우기 가능한지 알아보기 위함
    //    testlist.Add(0);
    //    // testlist[0]

    //    Debug.Log(testlist[0]);

    //    testlist1.Add(3);
    //    testlist1.Add(4);
    //    testlist1.Add(5);
    //    // testlist1[3, 4, 5]
    //    testlist = testlist1;

    //    Debug.Log(testlist[0]);
    //    Debug.Log(testlist[1]);
    //    Debug.Log(testlist[2]);

    //    // LIst가 json으로 변환 가능한지 여부 알아보기 위함
    //    // 불가능
    //    TestStruct test = new TestStruct("test", 3, 5.33f, testPlayer);
    //    SoloSave save = new SoloSave();
    //    save.course = 2;

    //    test.numlist2.Add(JsonUtility.ToJson(save));

    //    test.numlist[0] = 10;
    //    test.numlist[1] = 20;
    //    test.numlist[2] = 30;
    //    test.isYes = true;
    //    test.lits[0] = save;

    //    tests.Add(test);
    //    Debug.Log(tests[0].name);

    //    string json = JsonUtility.ToJson(tests); // 배열을 json으로 변환 불가능
    //    Debug.Log(json);
    //    //tests = JsonUtility.FromJson<List<TestStruct>>(json);
    //    //Debug.Log(tests[0].name);

    //    // 리스트를 가지는 클래스는 json화 가능
    //    json = JsonUtility.ToJson(test); // 구조체는 json으로 변환 가능
    //    Debug.Log(json);
    //    Debug.Log(JsonUtility.ToJson(save));
    //    Debug.Log(JsonUtility.FromJson<SoloSave>(test.numlist2[0]).course);


    //    for (int i = 0; i < KEY_LIST.Length; i++)
    //    {
    //        KEY_LIST[i] = KEY + i.ToString();
    //    }

    //    testList.Add(testNum);
    //    testList.Add(testNum1);
    //    testList.Add(testNum2);
    //    ShowValue();
    //    string json = JsonUtility.ToJson(testList);

    //    resultList = JsonUtility.FromJson<List<int>>(json);

    //    Debug.Log(resultList[0]);
    //    PlayerPrefs.SetString(KEY_LIST[0], json);

    //    testList[0] = testNum3;
    //    testList[1] = testNum4;
    //    testList[2] = testNum5;
    //    ShowValue();
    //    json = JsonUtility.ToJson(testList);
    //    Debug.Log(json);
    //    PlayerPrefs.SetString(KEY_LIST[1], json);

    //    testList[0] = testNum6;
    //    testList[1] = testNum7;
    //    testList[2] = testNum8;
    //    ShowValue();
    //    json = JsonUtility.ToJson(testList);
    //    Debug.Log(json);
    //    PlayerPrefs.SetString(KEY_LIST[2], json);

    //    Debug.Log(JsonUtility.FromJson<List<int>>(PlayerPrefs.GetString(KEY_LIST[0]))[0]);
    //    Debug.Log(JsonUtility.FromJson<List<int>>(PlayerPrefs.GetString(KEY_LIST[1]))[0]);
    //    Debug.Log(JsonUtility.FromJson<List<int>>(PlayerPrefs.GetString(KEY_LIST[2]))[0]);

    //}

    //void ShowValue()
    //{
    //    for (int i = 0; i < testList.Count; i++)
    //    {
    //        Debug.Log(testList[i]);
    //    }
    //}

    //string[] KEY_LIST = new string[3];

    //const string KEY = "testKey";

    //int testNum = 0;
    //int testNum1 = 1;
    //int testNum2 = 2; 
    //int testNum3 = 3;
    //int testNum4 = 4;
    //int testNum5 = 5;
    //int testNum6 = 6;
    //int testNum7 = 7;
    //int testNum8 = 8;


    //List<int> testList = new List<int>();
    //List<int> resultList;


    //List<PlayerInfo> list = new List<PlayerInfo>();
    //PhotonView pv;
    //private void Awake()
    //{
    //    PhotonNetwork.ConnectUsingSettings();
    //    pv = GetComponent<PhotonView>();
    //}

    //private void Start()
    //{
    //    // room에 들어 갔을 때만 보낼 수 있다

    //}

    //[PunRPC]
    //void Send(List<PlayerInfo> l)
    //{

    //    Debug.Log(l[0].character);
    //    Debug.Log(l[0].name);
    //    Debug.Log(l[0].rapTime);
    //    Debug.Log(l[0].course);
    //    Debug.Log(l[1].character);
    //    Debug.Log(l[1].name);
    //    Debug.Log(l[1].rapTime);
    //    Debug.Log(l[1].course);

    //    //list.Add()
    //    //Debug.Log(list);

    //    //Debug.Log(list[0].character);
    //    //Debug.Log(list[0].name);
    //    //Debug.Log(list[0].rapTime);
    //    //Debug.Log(list[0].course);
    //    //Debug.Log(list[1].character);
    //    //Debug.Log(list[1].name);
    //    //Debug.Log(list[1].rapTime);
    //    //Debug.Log(list[1].course);

    //}
    //[PunRPC]
    //void Testasd()
    //{
    //    Debug.Log("aaa");
    //}
    //public override void OnConnectedToMaster()
    //{
    //    Debug.Log("서버 연결");
    //    PhotonNetwork.JoinLobby();
    //}

    //public override void OnJoinedLobby()
    //{
    //    Debug.Log("로비 연결");
    //    PhotonNetwork.JoinRandomOrCreateRoom();
    //}

    //public override void OnRoomListUpdate(List<RoomInfo> roomList)
    //{
    //    // 룸 리스트 콜백은 로비에 접속했을때 자동으로 호출된다.
    //    // 로비에서만 호출할 수 있음...
    //    Debug.Log($"룸 리스트 업데이트 ::::::: 현재 방 갯수 : {roomList.Count}");
    //    // list를 매개변수로 보내고 받을 수 없다
    //}

    //public override void OnJoinedRoom()
    //{
    //    if (!PhotonNetwork.IsMasterClient)
    //    {
    //        PlayerInfo info;
    //        info.character = null;
    //        info.name = "abcd";
    //        info.rapTime = 33333;
    //        info.course = 33333;

    //        list.Add(info);
    //        list.Add(info);

    //        pv.RPC("Send", RpcTarget.Others, list);
    //    }

    //    Debug.Log("방 연결");
    //}
}
