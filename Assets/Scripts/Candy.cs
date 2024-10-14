using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candy : MonoBehaviour
{
    public CandyType candyType;

    public int xIndex;
    public int yIndex;

    public bool isMatched;
    public Vector2 currentPos;
    public Vector2 targetPos;

    public bool isMoving;

    public Candy(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }
}

public enum CandyType
{
    Red,
    Blue,
    Purple,
    Green,
    White
}
