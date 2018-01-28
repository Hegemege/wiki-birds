using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreMenuUIController : MonoBehaviour
{
    public GameObject NextButton;
    public GameObject QuitButton;

    public Text PlayerText;
    public Text ScoreText;

    private Coroutine outer;
    private Coroutine inner;

    private bool _gotData;

    void Awake()
    {
        PlayerText.text = "";
        ScoreText.text = "";

        if (GameManager.Instance.RoundNumber < 3)
        {
            NextButton.SetActive(GameManager.Instance.IsHost);
            QuitButton.SetActive(false);
        }
        else
        {
            QuitButton.SetActive(true);
        }

        outer = StartCoroutine(PollInfo());
    }

    private IEnumerator PollInfo()
    {
        while (true)
        {
            inner = GameManager.Instance.ScoreInfo(HandleScoreInfo);
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void HandleScoreInfo(JObject data)
    {
        try
        {
            if (!_gotData)
            {
                _gotData = true;

                var players = data["players"].ToObject<List<JObject>>();

                var colorNames = players.Select(player => new
                {
                    Color = player["color"].ToObject<string>(),
                    Name = player["name"].ToObject<string>()
                }).ToList();

                var colorScores = players.Select(player => new
                {
                    Color = player["color"].ToObject<string>(),
                    Score = player["points"].ToObject<string>()
                }).ToList();

                foreach (var name in colorNames)
                {
                    if (name.Color == GameManager.Instance.PlayerColor)
                    {
                        PlayerText.text += "(YOU) ";
                    }

                    PlayerText.text += name.Name + "\n";

                    // Score text
                    var rowScore = colorScores.Single(score => score.Color == name.Color);
                    ScoreText.text += rowScore.Score + "\n";
                }
            }
        }
        catch (Exception ex)
        {

        }
    }

    public void NextPress()
    {
        GameManager.Instance.StartRoom();
    }

    public void QuitPress()
    {
        GameManager.Instance.ResetToMenu();
        SceneManager.LoadScene("main");
    }
}
