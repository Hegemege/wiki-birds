using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Hardcoded token to cut away bots crawling semi-sensitive information
    // Does not secure anything, just makes sure that "most" of the traffic to the public API
    // is originated from the right source (the game). The other option is to authenticate users, overkill.
    [HideInInspector]
    public string Token = "9QfdXsTwmOPySh1zaB8A";

    public ServerConfiguration Server;

    [HideInInspector]
    public int ConnectionStatus;

    [HideInInspector]
    public string ErrorMessage;

    //Singleton
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }
    }

    // Identification data being sent to server
    private string _playerId;
    private string _playerName;

    [HideInInspector]
    public bool IsHost;

    [HideInInspector]
    public string RoomCode;

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        DontDestroyOnLoad(gameObject);

        _playerId = System.Guid.NewGuid().ToString();

        // Initialize connection
        StartCoroutine(CheckConnection());
    }

    public void ResetToMenu()
    {
        IsHost = false;
        ConnectionStatus = 0;
        StartCoroutine(CheckConnection());
    }

    public void JoinGame()
    {
        Debug.Log("Join a game");
        SceneManager.LoadScene("join");
    }

    public void NewGame()
    {
        Debug.Log("Request a new game");
        StartCoroutine(RequestNewRoom());
    }

    public void RetryConnection()
    {
        ResetToMenu();
    }

    /// <summary>
    /// Perform initialization steps after connection to server was established.
    /// </summary>
    private void InitializeConnected()
    {
        //StartCoroutine(GetQuestionData());
    }
    
    // Network methods

    /// <summary>
    /// Performs a connection check to the server
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckConnection()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(Server.ApiURL))
        {
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError) // Error
            {
                ConnectionStatus = 2;
            }
            else // Success
            {
                ConnectionStatus = 1;
            }
        }
    }
    
    private IEnumerator RequestNewRoom()
    {
        object body = new
        {
            Token = Token,
            PlayerID = _playerId
        };

        using (UnityWebRequest request = UnityWebRequest.Put(Server.ApiURL + "/new-room", JsonConvert.SerializeObject(body)))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError) // Error
            {
                HandleRequestError(request);
            }
            else // Success
            {
                var responseBody = JObject.Parse(request.downloadHandler.text);

                IsHost = true;
                RoomCode = responseBody["roomCode"].ToString();

                SceneManager.LoadScene("room");
            }
        }
    }

    private void HandleRequestError(UnityWebRequest request)
    {
        ConnectionStatus = 3;
        try
        {
            // Error after initial connetion check, go to menu and show error
            if (request.isHttpError)
            {
                var responseBody = JObject.Parse(request.downloadHandler.text);

                ErrorMessage = "Error " + request.responseCode + ": " + responseBody["error"];
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Unknown error.";
        }


        if (request.isNetworkError)
        {
            ErrorMessage = "Disconnected.";
        }

        SceneManager.LoadScene("main");
    }
}
