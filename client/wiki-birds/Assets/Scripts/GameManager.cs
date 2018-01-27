using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        DontDestroyOnLoad(gameObject);

        // Initialize connection
        StartCoroutine(CheckConnection());
    }

    public void JoinGame()
    {

    }

    public void NewGame()
    {

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

    /*

    /// <summary>
    /// Send gameplay statistics to server.
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="sessionId"></param>
    /// <param name="questionId"></param>
    /// <param name="answerId"></param>
    /// <param name="groupId"></param>
    private void SendGameplayStatistics(string deviceId, string sessionId, int questionId, int answerId, int groupId)
    {
        StartCoroutine(SendStatistics(deviceId, sessionId, questionId, answerId, groupId));
    }

    // Coroutine helper for above
    private IEnumerator SendStatistics(string deviceId, string sessionId, int questionId, int answerId, int groupId)
    {
        object body = new
        {
            Token = Token,
            QuestionId = questionId,
            AnswerId = answerId,
            GroupId = groupId,
            DeviceId = deviceId,
            SessionId = sessionId
        };

        // Unity can't do POST with JSON, so we use PUT
        using (UnityWebRequest request = UnityWebRequest.Put(Server.ServerUrl + "/api/answers", JsonConvert.SerializeObject(body)))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            // We don't really need to handle the response.
            if (request.isNetworkError || request.isHttpError) // Error
            {
                // Debug.Log(request.error);
            }
            else // Success
            {
                // Debug.Log(request.downloadHandler.text);
            }
        }
    }

    /// <summary>
    /// Fetches the question data from the server
    /// </summary>
    /// <returns></returns>
    private IEnumerator GetQuestionData()
    {
        object body = new
        {
            Token = Token
        };

        // Unity can't do POST with JSON, so we use PUT
        using (UnityWebRequest request = UnityWebRequest.Put(Server.ServerUrl + "/api/questions", JsonConvert.SerializeObject(body)))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError) // Error
            {
                InitializeLocalQuestionData();
            }
            else // Success
            {
                // Parse the data from the body. The body is in JSON format, and the raw file data is in JSON key "data".
                var responseBody = JObject.Parse(request.downloadHandler.text);
                QuestionData = QuestionCSVReader.ParseData(responseBody["data"].ToString());
            }
        }
    }

    /// <summary>
    /// Fetches the background music AudioClip from the server
    /// </summary>
    /// <returns></returns>
    private IEnumerator GetMusic()
    {
        object body = new
        {
            Token = Token,
            GroupId = _groupId
        };

        // We have to first extract single-use token from the server that we use with HTTP GET, because UnityWebRequestMultimedia does not support PUT
        using (UnityWebRequest tokenRequest = UnityWebRequest.Put(Server.ServerUrl + "/api/music", JsonConvert.SerializeObject(body)))
        {
            tokenRequest.SetRequestHeader("Content-Type", "application/json");

            yield return tokenRequest.SendWebRequest();

            if (tokenRequest.isNetworkError || tokenRequest.isHttpError) // Error
            {
                // No need to handle, local AudioClip will be used
            }
            else // Success
            {
                var responseBody = JObject.Parse(tokenRequest.downloadHandler.text);
                var musicToken = responseBody["token"].ToString();

                // Unity can't do POST with JSON, so we use PUT
                using (UnityWebRequest musicRequest = UnityWebRequestMultimedia.GetAudioClip(Server.ServerUrl + "/api/music?token=" + musicToken, AudioType.WAV))
                {
                    yield return musicRequest.SendWebRequest();

                    if (musicRequest.isNetworkError || musicRequest.isHttpError) // Error
                    {
                        // No need to handle, local AudioClip will be used
                    }
                    else // Success
                    {
                        AudioClip clip = DownloadHandlerAudioClip.GetContent(musicRequest);
                        // TODO: Perform validation
                        // clip.length

                        BackgroundMusic = clip;
                    }
                }
            }
        }

        MusicResolved();
    }

    /// <summary>
    /// Background music has been resolved (either locally or from the server)
    /// </summary>
    private void MusicResolved()
    {
        _backgroundMusicAudioSource.clip = BackgroundMusic;

        StartLevel("player_select");
    }
    */
}
