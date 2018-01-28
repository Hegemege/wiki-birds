using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsUIController : MonoBehaviour
{
	public void BackButtonPressed()
	{
		GameManager.Instance.ResetToMenu();
		SceneManager.LoadScene("main");
	}
}
