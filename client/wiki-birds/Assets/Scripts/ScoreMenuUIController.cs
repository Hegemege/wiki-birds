using System.Collections;
using System.Collections.Generic;
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

    void Awake()
    {
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
