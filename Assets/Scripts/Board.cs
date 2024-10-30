using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class Board : MonoBehaviour
{
    //define the size of the board
    public int width = 8;
    public int height = 8;
    //define some spacing for the board
    public float spacingX;
    public float spacingY;
    //get a reference to our candy prefabs
    public GameObject[] prefabs;
    //get a reference to the collection nodes board + GO
    public Node[,] board;
    public GameObject boardGO;

    public List<GameObject> candyToDestroy = new();
    public GameObject candyParent;

    [SerializeField]
    private Candy selectedCandy;

    [SerializeField]
    List<Candy> candyToRemove = new();

    [SerializeField]
    private bool isProcessingMove = false;

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

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject.GetComponent<Candy>())
            {
                if (isProcessingMove)
                    return;

                Candy candy = hit.collider.gameObject.GetComponent<Candy>();
                Debug.Log("I have clicked a candy it is: " + candy.gameObject);

                SelectCandy(candy);
            }
        }
    }



    void InitializeBoard()
    {
        Debug.Log("height" + height);
        Debug.Log("width" + width);
        DestroyCandy();
        board = new Node[width, height];

        spacingX = (float)(width - 1) / 2;
        spacingY = (float)((height - 1) / 2) + 1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 position = new Vector2(x - spacingX, y - spacingY);
                Debug.Log("x :" + x);
                Debug.Log("y :" + y);
                if (arrayLayout.rows[y].row[x])
                {
                    board[x, y] = new Node(false, null);
                } else
                {
                    int randomIndex = Random.Range(0, prefabs.Length);

                    GameObject candy = Instantiate(prefabs[randomIndex], position, Quaternion.identity);
                    candy.transform.SetParent(candyParent.transform);
                    candy.GetComponent<Candy>().SetIndices(x, y);
                    board[x, y] = new Node(true, candy);
                    candyToDestroy.Add(candy);
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

    private void DestroyCandy()
    {
        if (candyToDestroy != null)
        {
            foreach (GameObject candy in candyToDestroy)
            {
                Destroy(candy);
            }
            candyToDestroy.Clear();
        }
    }

    public bool CheckBoard()
    {
        if (GameManager.Instance.isGameEnded)
            return false;
        Debug.Log("Checking Board");
        bool hasMatched = false;

        candyToRemove.Clear();

        foreach(Node nodeCandy in board)
        {
            if (nodeCandy.candy != null)
            {
                nodeCandy.candy.GetComponent<Candy>().isMatched = false;
            }
        }

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
                            MatchResult superMatchedCandy = SuperMatch(matchedCandy);

                            candyToRemove.AddRange(superMatchedCandy.connected);

                            foreach (Candy cand in superMatchedCandy.connected)
                                cand.isMatched = true;

                            hasMatched = true;
                        }
                    }
                }
            }

        }

        return hasMatched;
    }

    public IEnumerator ProcessTurnOnMatchedBoard(bool _subtractMoves)
    {
        foreach (Candy removeCandy in candyToRemove)
        {
            removeCandy.isMatched = false;
        }

        RemoveAndRefill(candyToRemove);
        //point system
        GameManager.Instance.ProcessTurn(candyToRemove.Count, _subtractMoves);
        yield return new WaitForSeconds(0.4f);

        if (CheckBoard())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }
    }

    private void RemoveAndRefill(List<Candy> _candyToRemove)
    {
        GameManager.Instance.TypeMatch(_candyToRemove[0].candyType);

        //removing candy and clearing the board
        foreach (Candy candy in _candyToRemove)
        {
            //getting it's x and y indicies and storing them
            int _xIndex = candy.xIndex;
            int _yIndex = candy.yIndex;

            //Destroy the candy
            Debug.Log("Destroying Candy");
            Destroy(candy.gameObject);

            //Create blank node on candy board
            board[_xIndex, _yIndex] = new Node(true, null);
        }

        for (int x=0; x < width; x++)
        {
            for (int y=0; y < height; y++)
            {
                if (board[x,y].candy == null)
                {
                    Debug.Log("The location X: " + x + "Y: " + y + " is empty, attempting to refill it.");
                    RefillCandy(x, y);
                }                
            }
        }
    }

    private void RefillCandy(int x, int y)
    {
        //y offset
        int yOffset = 1;

        //while cell above our current cell is null and we're below the height of the board
        while (y + yOffset < height && board[x,y + yOffset].candy == null)
        {
            yOffset++;
        }

        //we've either hit the top of the board or we found a candy

        if (y + yOffset < height && board[x, y + yOffset].candy != null)
        {
            //hit a candy

            Candy candyAbove = board[x, y + yOffset].candy.GetComponent<Candy>();

            //Move it to the correct location
            Vector3 targetPos = new Vector3(x - spacingX, y - spacingY, candyAbove.transform.position.z);
            //Move to location
            candyAbove.MoveToTarget(targetPos);
            //update indices
            candyAbove.SetIndices(x, y);
            //update our board
            board[x, y] = board[x, y + yOffset];
            //set location candy came from to null
            board[x, y + yOffset] = new Node(true, null);
        }

        //if we've hit the top of the board without finding a candy
        if (y + yOffset == height)
        {
            Debug.Log("Reached top of board without finding a candy");
            SpawnCandyAtTop(x);
        }
    }

    private void SpawnCandyAtTop(int x)
    {
        int index = FindIndexOfLowestNull(x);
        int locationToMoveTo = 8 - index;
        Debug.Log("About to spawn a candy, ideally i'd like to put in the index of: " + index);
        //get a random candy

        //TODO set this to the logic that the prof wants.

        int randomIndex = 0; 
        
        randomIndex = Random.Range(0, prefabs.Length);




        GameObject newCandy = Instantiate(prefabs[randomIndex], new Vector2(x - spacingX, height - spacingY), Quaternion.identity);
        newCandy.transform.SetParent(candyParent.transform);
        //set indices
        newCandy.GetComponent<Candy>().SetIndices(x, index);
        //set it on the board
        board[x, index] = new Node(true, newCandy);
        //move to that location
        Vector3 targetPos = new Vector3(newCandy.transform.position.x, newCandy.transform.position.y - locationToMoveTo, newCandy.transform.position.z);
        newCandy.GetComponent<Candy>().MoveToTarget(targetPos);
    }
    private int FindIndexOfLowestNull(int x)
    {
        int lowestNull = 99;
        for (int y = 7; y >= 0; y--)
        {
            if (board[x,y].candy == null)
            {
                lowestNull = y;
            }
        }
        return lowestNull;
    }

    private MatchResult SuperMatch(MatchResult _matchedResults)
    {
        //if we have a horizontal or long horizontal match
        if (_matchedResults.direction == MatchDirection.Horizontal || _matchedResults.direction == MatchDirection.LongHorizontal)
        {
            foreach (Candy cand in _matchedResults.connected)
            {
                List<Candy> extraConnectedCandy = new();
                //check up
                CheckDirection(cand, new Vector2Int(0, 1), extraConnectedCandy);
                //check down
                CheckDirection(cand, new Vector2Int(0, -1), extraConnectedCandy);

                if (extraConnectedCandy.Count >= 2)
                {
                    Debug.Log("I have a super Horizontal match");
                    extraConnectedCandy.AddRange(_matchedResults.connected);

                    return new MatchResult
                    {
                        connected = extraConnectedCandy,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connected = _matchedResults.connected,
                direction = _matchedResults.direction
            };
        }
        //if we have a vertical or long vertical match
        else if (_matchedResults.direction == MatchDirection.Vertical || _matchedResults.direction == MatchDirection.LongVertical)
        {
            foreach (Candy cand in _matchedResults.connected)
            {
                List<Candy> extraConnectedCandy = new();

                CheckDirection(cand, new Vector2Int(1, 0), extraConnectedCandy);

                CheckDirection(cand, new Vector2Int(-1, 0), extraConnectedCandy);

                if (extraConnectedCandy.Count >= 2)
                {
                    Debug.Log("I have a super Vertical match");
                    extraConnectedCandy.AddRange(_matchedResults.connected);

                    return new MatchResult
                    {
                        connected = extraConnectedCandy,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connected = _matchedResults.connected,
                direction = _matchedResults.direction
            };
        }
        return null;
        //loop through potions in match
        //create new list of extra matches
        //CheckDirection up
        //CheckDirection down
        //do we have 2 or more extra matches.
        //we've made a super match - return a new matchresult of type super
        //return extra matches

        //if we have a vertical or long vertical match
        //loop through potions in match
        //create new list of extra matches
        //CheckDirection up
        //CheckDirection down
        //do we have 2 or more extra matches.
        //we've made a super match - return a new matchresult of type super
        //return extra matches
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

    #region Swapping

    //select candy
    public void SelectCandy(Candy _candy)
    {
        // If we don't have candy currently selected, then set the candy just clicked to the selected candy
        if (selectedCandy == null)
        {
            Debug.Log(_candy);
            selectedCandy = _candy;
        }
        // if same is selected twice make it null
        else if (selectedCandy == _candy)
        {
            selectedCandy = null;
        }
        // if it is not null and isnt the current potion, attempt a swap
        // selectedpotion back to null
        else if (selectedCandy != _candy)
        {
            SwapCandy(selectedCandy, _candy);
            selectedCandy = null;
        }
    }
    //swap candy
    private void SwapCandy(Candy _currentCandy, Candy _targetCandy)
    {
        //!IsAdjacent don't do anything
        if (!IsAdjacent(_currentCandy, _targetCandy))
        {
            return;
        }

        DoSwap(_currentCandy, _targetCandy);

        isProcessingMove = true;

        StartCoroutine(ProcessMatches(_currentCandy, _targetCandy));
    }
    //do swap
    private void DoSwap(Candy _currentCandy, Candy _targetCandy)
    {
        GameObject temp = board[_currentCandy.xIndex, _currentCandy.yIndex].candy;

        board[_currentCandy.xIndex, _currentCandy.yIndex].candy = board[_targetCandy.xIndex, _targetCandy.yIndex].candy;
        board[_targetCandy.xIndex, _targetCandy.yIndex].candy = temp;

        //update indices
        int tempXIndex = _currentCandy.xIndex;
        int tempYIndex = _currentCandy.yIndex;
        _currentCandy.xIndex = _targetCandy.xIndex;
        _currentCandy.yIndex = _targetCandy.yIndex;
        _targetCandy.xIndex = tempXIndex;
        _targetCandy.yIndex = tempYIndex;

        _currentCandy.MoveToTarget(board[_targetCandy.xIndex, _targetCandy.yIndex].candy.transform.position);

        _targetCandy.MoveToTarget(board[_currentCandy.xIndex, _currentCandy.yIndex].candy.transform.position);
    }

    private IEnumerator ProcessMatches(Candy _currentCandy, Candy _targetCandy)
    {
        yield return new WaitForSeconds(0.2f);

        if (CheckBoard())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(true));
        }

        else
        {
            DoSwap(_currentCandy, _targetCandy);
        }
        isProcessingMove = false;
    }

    //Is Adjacent
    private bool IsAdjacent(Candy _currentCandy, Candy _targetCandy)
    {
        return Mathf.Abs(_currentCandy.xIndex - _targetCandy.xIndex) + Mathf.Abs(_currentCandy.yIndex - _targetCandy.yIndex) == 1;
    }


}
#endregion

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


