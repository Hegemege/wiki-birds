using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIController : MonoBehaviour
{
    public GameObject UnableToConnectBG;
    private Text _connectionInfoText;

    void Awake()
    {
        _connectionInfoText = UnableToConnectBG.GetComponentInChildren<Text>();
        UnableToConnectBG.SetActive(true);
    }

    void Update()
    {
        if (!GameManager.Instance) return;

        switch (GameManager.Instance.ConnectionStatus)
        {
            case 0:
                // Initial state
                break;
            case 1:
                // Connected
                UnableToConnectBG.SetActive(false);
                break;
            case 2:
                // Unable to connect to server
                UnableToConnectBG.SetActive(true);
                _connectionInfoText.text = 
@"Could not connect to the server.
Try again";
                break;
        }
    }

    public void NewGameButtonClicked()
    {
        GameManager.Instance.NewGame();
    }

    public void JoinGameButtonClick()
    {
        GameManager.Instance.JoinGame();
    }
}
