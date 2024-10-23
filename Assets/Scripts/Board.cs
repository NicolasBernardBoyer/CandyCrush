using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Board : MonoBehaviour
{
    //define the size of the board
    public int width = 6;
    public int height = 8;
    //define some spacing for the board
    public float spacingX;
    public float spacingY;
    //get a reference to our candy prefabs
    public GameObject[] prefabs;
    //get a reference to the collection nodes board + GO
    private Node[,] board;
    public GameObject boardGO;

    //layoutArray
    public ArrayLayout arrayLayout;

    //public static of board
    public static Board Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Start()
    {
        InitializeBoard();
    }



    void InitializeBoard()
    {
        board = new Node[width, height];

        spacingX = (float)(width - 1) / 2;
        spacingY = (float)((height - 1) / 2) + 1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 position = new Vector2(x - spacingX, y - spacingY);
                if (arrayLayout.rows[y].row[x])
                {
                    board[x, y] = new Node(false, null);
                } else
                {
                    int randomIndex = Random.Range(0, prefabs.Length);

                    GameObject candy = Instantiate(prefabs[randomIndex], position, Quaternion.identity);
                    candy.GetComponent<Candy>().SetIndices(x, y);
                    board[x, y] = new Node(true, candy);
                }
                
            }
        }
    }
}
