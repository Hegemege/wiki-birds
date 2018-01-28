using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
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

        _myBirdController = _myBird.GetComponent<BirdController>();
        _myBirdController.Color = GameManager.Instance.PlayerColor;
        _myBirdController.HorizontalIndex = myIndex;

        // Generate other birds
        foreach (var other in otherPrefabs)
        {
            var otherBird = Instantiate(other);
            var otherIndex = GetHorizontalIndex(other);

            otherBird.transform.position = SpawnAnchorsHorizontal[otherIndex].position;
            otherBird.transform.position = new Vector3(otherBird.transform.position.x, -1000f, otherBird.transform.position.z);
            var otherController = otherBird.GetComponent<BirdController>();

            otherController.Color = GetColorFromPrefab(other);
            otherController.HorizontalIndex = otherIndex;

            _otherBirdControllers.Add(otherController);
            _others.Add(otherBird);
        }


        StartCoroutine(GetGameInfo());
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
        // Disable the players that are not in the game
        if (!_initialData)
        {
            _initialData = true;

            var players = data["players"].ToObject<List<JObject>>();

            // Resolve colors, delete those who are not active
            var playerColors = players.Select(player => player["color"].ToString());

            foreach (var inactivePlayer in _otherBirdControllers.Where(other => !playerColors.Contains(other.Color)))
            {
                inactivePlayer.Inactive = true;
                Destroy(inactivePlayer.gameObject);
            }

            // Also move players to the correct height immediately
            var playerPositions = players.Select(player => new {
                Color = player["color"].ToObject<string>(),
                LineIndex = player["lineIndex"].ToObject<int>()
            });

            var myBirdLineIndex = playerPositions.Single(pos => pos.Color == _myBirdController.Color).LineIndex;
            var myBirdHeight = SpawnAnchorsVertical[myBirdLineIndex].position.y; // Potential bug if multiple players end to have the same color
            _myBird.transform.position = new Vector3(_myBird.transform.position.x, myBirdHeight, _myBird.transform.position.z);
            _myBirdController.CurrentLine = myBirdLineIndex;
            _myBirdController.TargetLine = myBirdLineIndex;

            foreach (var other in _otherBirdControllers)
            {
                if (other.Inactive) continue;

                var birdLineIndex = playerPositions.Single(pos => pos.Color == other.Color).LineIndex;
                var birdHeight = SpawnAnchorsVertical[birdLineIndex].position.y;
                other.transform.position = new Vector3(other.transform.position.x, birdHeight, other.transform.position.z);

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
            // TODO: score screen show everyone's words
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
