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

    public void SetIndices(int _x, int _y)
    {
        xIndex = _x;
        yIndex = _y;
    }

    //MoveToTarget
    public void MoveToTarget(Vector2 _targetPos)
    {
        StartCoroutine(MoveCoroutine(_targetPos));
    }

    //MoveCoroutine
    private IEnumerator MoveCoroutine(Vector2 _targetPos)
    {
        isMoving = true;
        float duration = 0.2f;

        Vector2 startPosition = transform.position;
        float elaspedTime = 0f;

        while (elaspedTime < duration)
        {
            float t = elaspedTime / duration;

            transform.position = Vector2.Lerp(startPosition, _targetPos, t);

            elaspedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = _targetPos;
        isMoving = false;
    }
}

public enum CandyType
{
    Red,
    Blue,
    Purple,
    Green,
    Yellow
}
