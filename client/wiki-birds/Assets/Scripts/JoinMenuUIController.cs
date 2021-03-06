﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class JoinMenuUIController : MonoBehaviour
{
    private int[] code;

    public List<Image> codeImages;
    public List<Sprite> AllNumbers;

    void Awake()
    {
        code = new int[] {0, 0, 0, 0};
    }

    public void DialPressedUp(int index)
    {
        code[index] = ((1 + code[index]) % 10 + 10) % 10;
    }

    public void DialPressedDown(int index)
    {
        code[index] = ((-1 + code[index]) % 10 + 10) % 10;
    }

    public void BackButtonPressed()
    {
        GameManager.Instance.ResetToMenu();
        SceneManager.LoadScene("main");
    }

    public void JoinButtonPressed()
    {
        var stringCode = "";
        for (var i = 0; i < 4; i++)
        {
            stringCode += code[i].ToString();
        }

        GameManager.Instance.JoinRoom(stringCode);
    }

    void Update()
    {
        for (var i = 0; i < 4; i++)
        {
            codeImages[i].sprite = AllNumbers[code[i]];
        }
    }
}
