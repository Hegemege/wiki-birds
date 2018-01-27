using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIController : MonoBehaviour
{
    public GameObject UnableToConnectBG;
    public GameObject RetryButton;
    private Text _retryButtonText;
    private Text _connectionInfoText;

    void Awake()
    {
        _connectionInfoText = UnableToConnectBG.GetComponentInChildren<Text>();
        UnableToConnectBG.SetActive(true);

        _retryButtonText = RetryButton.GetComponentInChildren<Text>();
        RetryButton.SetActive(false);
    }

    void Update()
    {
        if (!GameManager.Instance) return;

        switch (GameManager.Instance.ConnectionStatus)
        {
            case 0:
                // Initial state
                UnableToConnectBG.SetActive(true);
                _connectionInfoText.text = "Connecting...";
                RetryButton.SetActive(false);
                break;
            case 1:
                // Connected
                UnableToConnectBG.SetActive(false);
                break;
            case 2:
                // Unable to connect to server
                UnableToConnectBG.SetActive(true);
                RetryButton.SetActive(true);
                _retryButtonText.text = "Retry";
                _connectionInfoText.text = "Could not connect to the server. Try again";
                break;
            case 3:
                UnableToConnectBG.SetActive(true);
                RetryButton.SetActive(true);
                _retryButtonText.text = "Back to menu";
                _connectionInfoText.text = GameManager.Instance.ErrorMessage;
                break;
        }
    }

    public void NewGameButtonClicked()
    {
        GameManager.Instance.NewGame();
    }

    public void JoinGameButtonClick()
    {
        GameManager.Instance.GotoJoinRoom();
    }

    public void RetryButtonClick()
    {
        GameManager.Instance.RetryConnection();
    }
}
