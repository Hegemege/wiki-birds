using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    public float MovementSpeed;

    public Camera CameraRef;
    public Canvas CanvasRef;

    public RectTransform UpButton;
    public RectTransform DownButton;

    public Image Timer1;
    public Image Timer2;

    public GameObject ReadyBG;
    public GameObject ReadyImage;
    public GameObject Round1;
    public GameObject Round2;
    public GameObject Round3;

    public List<AudioClip> RoundMusics;
    public AudioSource RoundMusicPlayer;

    public List<Sprite> Numbers;

    public List<Transform> SpawnAnchorsVertical;
    public List<Transform> SpawnAnchorsHorizontal;

    public GameObject RedBirdPrefab;
    public GameObject BlueBirdPrefab;
    public GameObject YellowBirdPrefab;
    public GameObject GreenBirdPrefab;

    public Text WordText;
    public AudioSource LineSound;

    [HideInInspector]
    public List<string> CorrectLines;

    public List<string> Words;
    public List<AudioClip> Clips;

    private GameObject _myBird;
    private List<GameObject> _others;

    private BirdController _myBirdController;
    private List<BirdController> _otherBirdControllers;

    private bool _initialData;
    private Coroutine _gameLoop;

    private int _roundNumber;
    private bool _roundEnded;
    private int _roundIndex;

    void Awake()
    {
        WordText.text = "";

        _others = new List<GameObject>();
        _otherBirdControllers = new List<BirdController>();
        GameObject colorPrefab = null;
        List<GameObject> otherPrefabs = new List<GameObject>();
        // Spawn player, move to place only when the data is given
        switch (GameManager.Instance.PlayerColor)
        {
            case "Red":
                colorPrefab = RedBirdPrefab;
                otherPrefabs.Add(YellowBirdPrefab);
                otherPrefabs.Add(GreenBirdPrefab);
                otherPrefabs.Add(BlueBirdPrefab);
                break;
            case "Yellow":
                colorPrefab = YellowBirdPrefab;
                otherPrefabs.Add(RedBirdPrefab);
                otherPrefabs.Add(GreenBirdPrefab);
                otherPrefabs.Add(BlueBirdPrefab);
                break;
            case "Green":
                colorPrefab = GreenBirdPrefab;
                otherPrefabs.Add(RedBirdPrefab);
                otherPrefabs.Add(YellowBirdPrefab);
                otherPrefabs.Add(BlueBirdPrefab);
                break;
            case "Blue":
                colorPrefab = BlueBirdPrefab;
                otherPrefabs.Add(RedBirdPrefab);
                otherPrefabs.Add(YellowBirdPrefab);
                otherPrefabs.Add(GreenBirdPrefab);
                break;
        }

        _myBird = Instantiate(colorPrefab);
        var myIndex = GetHorizontalIndex(colorPrefab);
        _myBird.transform.position = SpawnAnchorsHorizontal[myIndex].position;
        _myBird.transform.position = new Vector3(_myBird.transform.position.x, -1000f, _myBird.transform.position.z);

        _myBird.transform.localScale = Vector3.one * 0.37f;

        _myBirdController = _myBird.GetComponent<BirdController>();
        _myBirdController.Color = GameManager.Instance.PlayerColor;
        _myBirdController.HorizontalIndex = myIndex;

        GameManager.Instance.MyBirdController = _myBirdController;

        // Generate other birds
        foreach (var other in otherPrefabs)
        {
            var otherBird = Instantiate(other);
            var otherIndex = GetHorizontalIndex(other);

            otherBird.transform.position = SpawnAnchorsHorizontal[otherIndex].position;
            otherBird.transform.position = new Vector3(otherBird.transform.position.x, -1000f, otherBird.transform.position.z);

            otherBird.transform.localScale = Vector3.one * 0.37f;

            var otherController = otherBird.GetComponent<BirdController>();

            otherController.Color = GetColorFromPrefab(other);
            otherController.HorizontalIndex = otherIndex;

            _otherBirdControllers.Add(otherController);
            _others.Add(otherBird);
        }


        _gameLoop = StartCoroutine(GetGameInfo());
    }

    private IEnumerator GetGameInfo()
    {
        while (true)
        {
            GameManager.Instance.GetGameInfo(HandleGameInfo);

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void HandleGameInfo(JObject data)
    {
        try
        {
            var players = data["players"].ToObject<List<JObject>>();

            // Also move players to the correct height immediately
            var playerPositions = players.Select(player => new
            {
                Color = player["color"].ToObject<string>(),
                LineIndex = player["lineIndex"].ToObject<int>()
            }).ToList();


            // Disable the players that are not in the game
            if (!_initialData)
            {
                _initialData = true;

                // Resolve colors, delete those who are not active
                var playerColors = players.Select(player => player["color"].ToString());

                foreach (var inactivePlayer in _otherBirdControllers.Where(other => !playerColors.Contains(other.Color))
                )
                {
                    inactivePlayer.Inactive = true;
                    Destroy(inactivePlayer.gameObject);
                }

                var myBirdLineIndex = playerPositions.Single(pos => pos.Color == _myBirdController.Color).LineIndex;
                var myBirdHeight =
                    SpawnAnchorsVertical[myBirdLineIndex].position
                        .y; // Potential bug if multiple players end to have the same color
                _myBird.transform.position = new Vector3(_myBird.transform.position.x, myBirdHeight,
                    _myBird.transform.position.z);
                _myBirdController.CurrentLine = myBirdLineIndex;
                _myBirdController.TargetLine = myBirdLineIndex;

                foreach (var other in _otherBirdControllers)
                {
                    if (other.Inactive) continue;

                    var birdLineIndex = playerPositions.Single(pos => pos.Color == other.Color).LineIndex;
                    var birdHeight = SpawnAnchorsVertical[birdLineIndex].position.y;
                    other.transform.position =
                        new Vector3(other.transform.position.x, birdHeight, other.transform.position.z);

                    other.CurrentLine = birdLineIndex;
                    other.TargetLine = birdLineIndex;
                }

                // Resolve lines
                CorrectLines = data["correctLines"].ToObject<List<string>>();

                // Resolve words
                var playerWords = players.Select(player => new
                {
                    Color = player["color"].ToObject<string>(),
                    Word = player["word"].ToObject<string>()
                });

                _myBirdController.Word = playerWords.Single(word => word.Color == _myBirdController.Color).Word;
                WordText.text = _myBirdController.Word;

                // We dont care about the words of others right now
                // TODO: score screen should show everyone's words in the end?

                // Get round number
                _roundNumber = data["round"].ToObject<int>();
                GameManager.Instance.RoundNumber = _roundNumber;

                _roundIndex = _roundNumber - 1;

                RoundMusicPlayer.clip = RoundMusics[_roundIndex];
                RoundMusicPlayer.Play();

                ReadyImage.SetActive(true);

                return;
            }

            // Set other bird targets

            foreach (var other in _otherBirdControllers)
            {
                if (other.Inactive) continue;

                var birdLineIndex = playerPositions.Single(pos => pos.Color == other.Color).LineIndex;

                other.TargetLine = birdLineIndex;
            }

        }
        catch (Exception ex)
        {

        }

    }

    private void TimeOut()
    {
        if (_roundEnded) return;

        _roundEnded = true;
        //StopCoroutine(_gameLoop);

            // Show scores
            // TODO
        StartCoroutine(GoToScore());
    }

    private IEnumerator GoToScore()
    {
        yield return new WaitForSeconds(2f);

        GameManager.Instance.EndRound();
    }

    void Update()
    {
        var now = DateTime.UtcNow;
        if (now > GameManager.Instance.NextRoundStart)
        {
            Timer1.gameObject.SetActive(true);
            Timer2.gameObject.SetActive(true);

            ReadyBG.SetActive(false);

            if (now < GameManager.Instance.NextRoundEnd)
            {
                UpButton.gameObject.SetActive(true);
                DownButton.gameObject.SetActive(true);

                if (_myBirdController.CurrentLine == _myBirdController.TargetLine)
                {
                    var currentWord = CorrectLines[_myBirdController.CurrentLine];
                    var clipIndex = Words.IndexOf(currentWord);
                    if (clipIndex != -1)
                    {
                        if (!LineSound.isPlaying)
                        {
                            LineSound.clip = Clips[clipIndex];
                            LineSound.Play();
                        }
                    }
                    else
                    {
                        LineSound.Stop();
                        // Shouldnt happen
                    }
                }
                else
                {
                    LineSound.Stop();
                }
                
                
            }
            else
            {
                UpButton.gameObject.SetActive(false);
                DownButton.gameObject.SetActive(false);

                LineSound.Stop();
            }

            var timeleft = GameManager.Instance.NextRoundEnd - now;

            if (timeleft.TotalMilliseconds < 100f)
            {
                Timer1.sprite = Numbers[0];
                Timer2.sprite = Numbers[0];

                TimeOut();
            }
            else
            {
                var leftSeconds = timeleft.TotalSeconds;
                var leftFullSeconds = Mathf.CeilToInt((float)leftSeconds);
                var leftTens = Mathf.FloorToInt(leftFullSeconds / 10f);
                var leftOnes = leftFullSeconds % 10;

                Timer1.sprite = Numbers[leftTens];
                Timer2.sprite = Numbers[leftOnes];
            }
        }
        else
        {
            Timer1.gameObject.SetActive(false);
            Timer2.gameObject.SetActive(false);

            ReadyBG.SetActive(true);

            UpButton.gameObject.SetActive(false);
            DownButton.gameObject.SetActive(false);

            if ((GameManager.Instance.NextRoundStart - now).TotalSeconds > 5f)
            {
                ReadyImage.SetActive(true);
            }
            else
            {
                ReadyImage.SetActive(false);
                if (_roundIndex == 0)
                {
                    Round1.SetActive(true);
                }

                if (_roundIndex == 1)
                {
                    Round2.SetActive(true);
                }

                if (_roundIndex == 2)
                {
                    Round3.SetActive(true);
                }
            }
        }


    }

    void FixedUpdate()
    {
        var dt = Time.fixedDeltaTime;

        var birdScreenPosition = CameraRef.WorldToScreenPoint(_myBird.transform.position);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRef.transform as RectTransform, birdScreenPosition, CameraRef, out localPoint);

        UpButton.anchoredPosition = localPoint + new Vector2(0f, 200f);
        DownButton.anchoredPosition = localPoint - new Vector2(0f, 200f);

        // Bird movement
        MoveBird(_myBird, _myBirdController, dt);
        foreach (var bird in _otherBirdControllers)
        {
            if (bird.Inactive) continue;

            MoveBird(bird.gameObject, bird, dt);
        }

        // Do my bird audio stuff if current == target
    }

    private void MoveBird(GameObject bird, BirdController birdController, float dt)
    {
        if (birdController.TargetLine == birdController.CurrentLine) return;

        // Get target height
        var targetHeight = SpawnAnchorsVertical[birdController.TargetLine].position.y;

        if (targetHeight > bird.transform.position.y)
        {
            bird.transform.position += new Vector3(0f, 1f, 0f) * dt * MovementSpeed;

            if (targetHeight <= bird.transform.position.y)
            {
                bird.transform.position = new Vector3(bird.transform.position.x, targetHeight, bird.transform.position.z);
                birdController.CurrentLine = birdController.TargetLine;
            }
        }
        else if (targetHeight < bird.transform.position.y)
        {
            bird.transform.position -= new Vector3(0f, 1f, 0f) * dt * MovementSpeed;

            if (targetHeight >= bird.transform.position.y)
            {
                bird.transform.position = new Vector3(bird.transform.position.x, targetHeight, bird.transform.position.z);
                birdController.CurrentLine = birdController.TargetLine;
            }
        }
    }

    public void GoUp()
    {
        if (_myBirdController.TargetLine > 0)
        {
            _myBirdController.TargetLine -= 1;
        }
    }

    public void GoDown()
    {
        if (_myBirdController.TargetLine < 3)
        {
            _myBirdController.TargetLine += 1;
        }
    }

    private int GetHorizontalIndex(GameObject prefab)
    {
        if (prefab == RedBirdPrefab) return 0;
        if (prefab == YellowBirdPrefab) return 1;
        if (prefab == GreenBirdPrefab) return 2;
        if (prefab == BlueBirdPrefab) return 3;

        return 0; // Will not run
    }

    private string GetColorFromPrefab(GameObject prefab)
    {
        if (prefab == RedBirdPrefab) return "Red";
        if (prefab == BlueBirdPrefab) return "Blue";
        if (prefab == YellowBirdPrefab) return "Yellow";
        if (prefab == GreenBirdPrefab) return "Green";

        return "Red";
    }
}
