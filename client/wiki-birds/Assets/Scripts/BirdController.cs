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

    private Animator anim;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        anim.SetBool("Fly", CurrentLine != TargetLine);
        anim.SetBool("Land", CurrentLine == TargetLine);
    }
}
