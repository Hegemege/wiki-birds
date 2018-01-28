using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdController : MonoBehaviour
{
    [HideInInspector]
    public string Color;

    [HideInInspector]
    public int HorizontalIndex;

    [HideInInspector]
    public string Word;

    [HideInInspector]
    public bool Inactive;

    [HideInInspector]
    public int CurrentLine;

    [HideInInspector]
    public int TargetLine;
}
