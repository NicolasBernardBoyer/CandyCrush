using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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
        if (CheckBoard())
        {
            Debug.Log("Matches found, recreating board");
            InitializeBoard();
        } else
        {
            Debug.Log("No matches found, starting game");
        }
    }

    public bool CheckBoard()
    {
        Debug.Log("Checking Board");
        bool hasMatched = false;

        List<Candy> candyToRemove = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // check to see if node is usable
                if (board[x, y].isUsable)
                {
                    //proceed to get class in node
                    Candy candy = board[x,y].candy.GetComponent<Candy>();

                    //ensure there is no match
                    if (!candy.isMatched)
                    {
                        //run matching logic

                        MatchResult matchedCandy = IsConnected(candy);

                        if(matchedCandy.connected.Count >= 3)
                        {
                            // complex matching

                            candyToRemove.AddRange(matchedCandy.connected);

                            foreach (Candy cand in matchedCandy.connected)
                                cand.isMatched = true;

                            hasMatched = true;
                        }
                    }
                }
            }

        }

        return hasMatched;
    }

    //IsConnected
    MatchResult IsConnected(Candy candy)
    {
        List<Candy> connected = new();
        CandyType CandyType = candy.candyType;

        connected.Add(candy);

        //check right
        CheckDirection(candy, new Vector2Int(1, 0), connected);
        //check left
        CheckDirection(candy, new Vector2Int(-1, 0), connected);

        //have we made a match (horizontal)
        if(connected.Count == 3)
        {
            Debug.Log("I have a normal horizontal match, the color is: " + connected[0].candyType);

            return new MatchResult
            {
                connected = connected,
                direction = MatchDirection.Horizontal
            };
        }

        //checking for more than 3 (long horizontal)
        else if (connected.Count > 3)
        {
            Debug.Log("I have a long horizontal match, the color is: " + connected[0].candyType);

            return new MatchResult
            {
                connected = connected,
                direction = MatchDirection.LongHorizontal
            };
        }

        //clear out connected
        connected.Clear();
        //read our initial candy
        connected.Add(candy);

        //check up
        CheckDirection(candy, new Vector2Int(0, 1), connected);

        //check down
        CheckDirection(candy, new Vector2Int(0, -1), connected);

        //have we made a a 3 match (vertical)
        if (connected.Count == 3)
        {
            Debug.Log("I have a normal vertical match, the color is: " + connected[0].candyType);

            return new MatchResult
            {
                connected = connected,
                direction = MatchDirection.Vertical
            };
        }

        //check for more than 3 (long vertical match)
        //have we made a a 3 match (vertical)
        if (connected.Count > 3)
        {
            Debug.Log("I have a long vertical match, the color is: " + connected[0].candyType);

            return new MatchResult
            {
                connected = connected,
                direction = MatchDirection.LongVertical
            };
        } else
        {
            return new MatchResult
            {
                connected = connected,
                direction = MatchDirection.None
            };
        }
    }


    //CheckDir
    //Could do diagonal matches if we wanted to
    void CheckDirection(Candy can, Vector2Int direction, List<Candy> connected)
    {
        CandyType type = can.candyType;
        int x = can.xIndex + direction.x;
        int y = can.yIndex + direction.y;

        //check that we're in board boundaries
        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (board[x, y].isUsable)
            {
                Candy neighbourCandy = board[x, y].candy.GetComponent<Candy>();

                //does it match? must not be
                if (!neighbourCandy.isMatched && neighbourCandy.candyType == type)
                {
                    connected.Add(neighbourCandy);

                    x += direction.x;
                    y += direction.y;
                }
                else
                {
                    break;
                }

            }
            else
            {
                break;
            }
        }
    }
}

public class MatchResult
{
    public List<Candy> connected;
    public  MatchDirection direction;
}

public enum MatchDirection
{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None
}


