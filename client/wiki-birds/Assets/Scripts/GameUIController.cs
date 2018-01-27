using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class GameUIController : MonoBehaviour
{
    public List<Transform> SpawnAnchorsVertical;
    public List<Transform> SpawnAnchorsHorizontal;

    public GameObject RedBirdPrefab;
    public GameObject BlueBirdPrefab;
    public GameObject YellowBirdPrefab;
    public GameObject GreenBirdPrefab;

    private GameObject _myBird;
    private List<GameObject> _others;

    private BirdController _myBirdController;
    private List<BirdController> _otherBirdControllers;

    private bool _disabledPlayers;

    void Awake()
    {
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

        // Generate other birds
        foreach (var other in otherPrefabs)
        {
            var otherBird = Instantiate(other);
            var otherIndex = GetHorizontalIndex(other);

            otherBird.transform.position = SpawnAnchorsHorizontal[otherIndex].position;
            otherBird.transform.position = new Vector3(otherBird.transform.position.x, -1000f, otherBird.transform.position.z);

            _otherBirdControllers.Add(otherBird.GetComponent<BirdController>());
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
        if (!_disabledPlayers)
        {
            _disabledPlayers = true;

            var players = data["players"].ToObject<List<JObject>>().Select(player => player["color"].ToString());

            Debug.Log(players);

            foreach (var inactivePlayer in _otherBirdControllers.Where(other => !players.Contains(other.Color)))
            {
                Debug.Log("Remove " + inactivePlayer.Color);
            }

            // Also move players to the correct height immediately
            //var playerPositions = data["players"].ToObject<List<dynamic>>();

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
}
