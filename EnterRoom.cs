using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class EnterRoom : MonoBehaviour
{
    Button button;
    GameManager gameManager;
    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(RoomClick);
        gameManager = GameManager.instance;
    }
    void RoomClick()
    {
        gameManager.lodingImage.SetActive(true);
        string roomName = GetComponentInChildren<TextMeshProUGUI>().text;
        PhotonNetwork.JoinRoom(roomName);
    }
}
