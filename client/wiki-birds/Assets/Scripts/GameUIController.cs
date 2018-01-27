using System.Collections;
using System.Collections.Generic;
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

    void Awake()
    {
        // Spawn player, move to place only when the data is given

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
        // TODO:
    }
}
