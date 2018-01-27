using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsUIController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void Credits()
	{
		SceneManager.LoadScene ("credits");
	}

	public void BackButtonPressed()
	{
		GameManager.Instance.ResetToMenu();
		SceneManager.LoadScene("main");
	}
}
