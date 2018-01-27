using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomMenuUIController : MonoBehaviour
{
    public Text PlayerListHeader;
    public Text PlayerList;
    public Text PlayerName;
    public Text Code;

    public GameObject StartButton;
    public GameObject StartText;

    public GameObject LeaveButton;

    public GameObject LoadingBG;

    private bool _gotRoomData;

    void Awake()
    {
        LoadingBG.SetActive(true);
    }

    void Update()
    {

    }

    public void LeaveButtonPressed()
    {
        GameManager.Instance.LeaveRoom();
        LeaveButton.SetActive(false);
    }

    public void StartButtonPressed()
    {
        GameManager.Instance.StartRoom();
        StartButton.SetActive(false);
    }

}
