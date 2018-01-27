using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public Text StartText;

    public GameObject LeaveButton;

    public GameObject LoadingBG;

    private bool _gotRoomData;
    private Coroutine _infoCoroutine;
    private Coroutine _innerCoroutine;

    void Awake()
    {
        LoadingBG.SetActive(true);

        // Start room info loop
        _infoCoroutine = StartCoroutine(GetRoomInfo());
    }

    private IEnumerator GetRoomInfo()
    {
        while (true)
        {
            _innerCoroutine = GameManager.Instance.GetRoomInfo(UpdateRoomInfo);

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void UpdateRoomInfo(List<string> players)
    {
        PlayerListHeader.text = "Players (" + players.Count + " / 4)";
        PlayerList.text = String.Join("\n", players.ToArray());

        StartButton.SetActive(GameManager.Instance.IsHost);
        if (players.Count < 2 || players.Count > 4)
        {
            StartText.text = players.Count < 2 ? "Waiting for players..." : "Too many players!";
            StartButton.SetActive(false);
        }
        else
        {
            StartText.text = GameManager.Instance.IsHost ? "Tap START when ready." : "Waiting for host to start the game";
        }

        PlayerName.text = "You:\n" + GameManager.Instance.PlayerName;

        Code.text = GameManager.Instance.RoomCode;

        LoadingBG.SetActive(false);
    }

    void Update()
    {

    }

    public void LeaveButtonPressed()
    {
        StopCoroutine(_infoCoroutine);
        StopCoroutine(_innerCoroutine);
        GameManager.Instance.LeaveRoom();
        LeaveButton.SetActive(false);
    }

    public void StartButtonPressed()
    {
        GameManager.Instance.StartRoom();
        StartButton.SetActive(false);
    }

}
