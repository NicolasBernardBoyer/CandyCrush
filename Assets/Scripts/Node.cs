using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    //Determine if space can be filled
    public bool isUsable;

    public GameObject candy;

    public Node(bool _isUsable, GameObject _candy)
    {
        isUsable = _isUsable;
        candy = _candy;
    }
}
