using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class BoardPvP : MonoBehaviour
{
    public static BoardPvP Instance { set; get; }

    //For instantating pieces
    public Pieces[,] pieces = new Pieces[8, 8];
    private GameObject redPiecePrefab;
    private GameObject bluePiecePrefab;
    //array lang ng 2 team
    public GameObject[] redPieces;
    public GameObject[] bluePieces;

    public GameObject forcedPieceHighlightsContainer;
    public GameObject selectedPieceHighlightContainer;
    public GameObject moveHighlighter;

    private Vector3 boardOffset = new Vector3(-4f, 0, -4f);
    private Vector3 offsetBlack = new Vector3(-4.5f, 0, -3.5f); //sakto na yang pwesto wag palitan
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f); //pls wag nyo na palitan pwesto
    public Vector3 highlightPieceOffset = new Vector3(0.5f, 0, -0.5f);

    public bool isPlayer1Turn;
    public bool isPlayer1Color;
    private bool isRed;
    public bool hasDestroyed;
    private bool hasJumped;
    private bool hasMultipleJumped;

    public Pieces selectedPiece;
    private Pieces presentPiece;
    private Pieces previousPiece;
    private List<Pieces> forcedPieces;
    private List<Pieces> selectedPieces;

    private Vector2 touchOver;
    private Vector2 startDrag;
    private Vector2 endDrag;

    public GameObject computeCanvas;
    public Text questionUI;

    private int ctrTurn = 0;
    private int p1TurnCtr = 20;
    private int p2TurnCtr = 20;
    public int p1Pieces = 12;
    public int p2Pieces = 12;

    public GameObject victoryUI;
    public GameObject winnerUI;
    public GameObject drawUI;
    public Text drawText;
    public Text winnerText;
    public Text gameTimerText;
    public Text playerWarningTimerText;
    public Text p1Turn;
    public Text p2Turn;
    public Text p1Text;
    public Text p2Text;
    public Text timerText;
    public Text p1PiecesCounter;
    public Text p2PiecesCounter;

    Coroutine timer, playerTimer;

    public Animator animColor;
    public Animator animPlayerTurn;

    public GameObject playerTimerCanvas;
    public GameObject scoreCanvas;
    public GameObject gameTimerCanvas;
    public GameObject chooseColorCanvas;
    public GameObject p1Red;
    public GameObject p1Blue;
    public GameObject p2Red;
    public GameObject p2Blue;
    public GameObject indicatorHighligter;
    GameObject temp;
    GameObject temp1;
    GameObject temp2;
    GameObject temp3;


    //private LANClient client;


    private void Awake()
    {
        chooseColorCanvas.SetActive(true);
        //scoreCanvas.SetActive(true);

        ShuffleArray();

    }
    private void Start()
    {
        foreach (Transform t in forcedPieceHighlightsContainer.transform)
        {
            t.position = Vector3.down * 100;
        }

       
        Instance = this;
        //client = FindObjectOfType<LANClient>();
        //isWhite = client.isHost;


        isPlayer1Turn = true;
        forcedPieces = new List<Pieces>();
        selectedPieces = new List<Pieces>();
        //GenerateBoard();

    }

    // Update is called once per frame
    void Update()
    {
        RotatePieceHighlight();
        UpdateTouch();

        //Debug.Log(touchOver);
        
        //if its player 1 turn
        if ((isPlayer1Color) ? isPlayer1Turn : !isPlayer1Turn)
        {

            int x = (int)touchOver.x;
            int y = (int)touchOver.y;

            if (selectedPiece != null)
            {
                UpdatePieceDrag(selectedPiece);
            }
                
            if (Input.GetMouseButtonDown(0))
            {
                SelectPiece(x, y);               
                SelectedPieceHighlight();
            }
            if (Input.GetMouseButtonUp(0))
            {
                TryMove((int)startDrag.x, (int)startDrag.y, x, y);
            }
           
        }

    }

    public void Color(string color)
    {
        if (color == "red")
        {
            isRed = true;
            p1Red.SetActive(true);
        }
        else
        {
            isRed = false;
            p1Blue.SetActive(true);
        }

        isPlayer1Color = true;

        GenerateBoard();
        StartCoroutine(PlayAndDisappear("Color"));

        timer = StartCoroutine(Timer(20));
        playerTimer = StartCoroutine(PlayerTimer(1));
    }
    public void SelectPiece(int x, int y)
    {
        //Out of Bounds

        if (x < 0 || x >= 8 || y < 0 || y >= 8)
            return;

        DestroyPrefab();
        Pieces p = pieces[x, y];
        if (p != null && p.isPlayer1Color == isPlayer1Color)
        {
            FindObjectOfType<AudioManager>().Play("Select");
            if (forcedPieces.Count == 0)
            {
                selectedPiece = p;
                startDrag = touchOver;   
                //checking lang to kung anong chip at kung may operator ba
                Debug.Log(selectedPiece.tag);
                Debug.Log(selectedPiece.hadOperator);

            }
            else
            {
                // look
                if (forcedPieces.Find(fp => fp == p) == null)
                    return;
                selectedPiece = p;
                startDrag = touchOver;
            }
        }
    }

    public void ShuffleArray()
    {
        //shuffle red
        for (int i = 0; i < redPieces.Length; i++)
        {
            int rnd = Random.Range(0, redPieces.Length);
            GameObject temp = redPieces[rnd];
            redPieces[rnd] = redPieces[i];
            redPieces[i] = temp;
        }
        //shuffle blue
        for (int i = 0; i < bluePieces.Length; i++)
        {
            int rnd = Random.Range(0, bluePieces.Length);
            GameObject temp = bluePieces[rnd];
            bluePieces[rnd] = bluePieces[i];
            bluePieces[i] = temp;
        }
    }
    private void GenerateBoard()
    {
        int ctr = 0;
        //Generate Player 1 Pieces
        for (int y = 0; y < 3; y++)
        {
            bool oddRow = (y % 2 == 1);
            for (int x = 0; x < 8; x += 2)
            {
                //Generate Piece
                GeneratePieces((oddRow) ? x : x + 1, y, ctr);
                ctr++;
            }
        }

        ctr = 0;
        //Generate Player 2 Pieces
        for (int y = 7; y > 4; y--)
        {
            bool oddRow = (y % 2 == 1);
            for (int x = 0; x < 8; x += 2)
            {
                //Generate Piece
                GeneratePieces((oddRow) ? x : x + 1, y, ctr);
                ctr++;
            }
        }

    }
    private void GeneratePieces(int x, int y, int ctr)
    {
        redPiecePrefab = redPieces[ctr] as GameObject;
        bluePiecePrefab = bluePieces[ctr] as GameObject;
        bool isPiecePlayer1 = !(y > 3);

        if (isRed)
        {
            GameObject gameObj = Instantiate((isPiecePlayer1) ? redPiecePrefab : bluePiecePrefab) as GameObject;
            gameObj.transform.SetParent(transform);
            Pieces p = gameObj.GetComponent<Pieces>();
            pieces[x, y] = p;
            MovePieces(p, x, y);

            if (pieces[x, y].isRed && isRed)
            {
                pieces[x, y].isPlayer1Color = true;
            }
        }
        else
        {
            GameObject gameObj = Instantiate((isPiecePlayer1) ? bluePiecePrefab : redPiecePrefab) as GameObject;
            gameObj.transform.SetParent(transform);
            Pieces p = gameObj.GetComponent<Pieces>();
            pieces[x, y] = p;
            MovePieces(p, x, y);

            if (!pieces[x, y].isRed && !isRed)
            {
                pieces[x, y].isPlayer1Color = true;
            }
        }

        scoreCanvas.SetActive(true);
        gameTimerCanvas.SetActive(true);
        p1PiecesCounter.text = "Pieces: " + p1Pieces;
        p2PiecesCounter.text = "Pieces: " + p2Pieces;
    }


    private void MovePieces(Pieces p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + offsetBlack + pieceOffset;
    }
    private void UpdateTouch()
    {
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            touchOver.x = (int)(hit.point.x - boardOffset.x);
            touchOver.y = (int)(hit.point.z - boardOffset.z);

        }
        else
        {
            touchOver.x = -1;
            touchOver.y = -1;
        }
    }
    private void UpdatePieceDrag(Pieces p)
    {
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }
    }
    public void TryMove(int x1, int y1, int x2, int y2)
    {
        forcedPieces = ScanForPossibleMove();

        //Multiplayer
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1];

        //cancelling selected piece move/ out of bounds
        if (x2 < 0 || x2 >= 8 || y2 < 0 || y2 >= 8)
        {
            if (selectedPiece != null)
            {
                MovePieces(selectedPiece, x1, y1);

            }

            startDrag = Vector2.zero;
            selectedPiece = null;
            Highlight();
            return;
        }

        if (selectedPiece != null)
        {
            //if it did not move
            if ((endDrag == startDrag))
            {
                MovePieces(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                Highlight();
                return;
            }

            //check if move is valid
            if (selectedPiece.ValidMove(pieces, x1, y1, x2, y2))
            {
                // if a piece is eaten
                //if this is a jump

                if (Mathf.Abs(x2 - x1) == 2)
                {
                    Pieces p = pieces[(x1 + x2) / 2, (y1 + y2) / 2];
                    if (p != null)
                    {
                        pieces[(x1 + x2) / 2, (y1 + y2) / 2] = null;
                        presentPiece = selectedPiece;
                        previousPiece = p;

                        //Pieces Counter
                        if (p.isPlayer1Color)
                        {
                            p1Pieces--;
                            p1PiecesCounter.text = "Pieces: " + p1Pieces;
                            Debug.Log("p1 " + p1Pieces);
                        }
                        else
                        {
                            p2Pieces--;
                            p2PiecesCounter.text = "Pieces: " + p2Pieces;
                            Debug.Log("p2 " + p2Pieces);
                        }

                        Destroy(p.gameObject);
                        hasDestroyed = true;
                        FindObjectOfType<AudioManager>().Play("Capture");
                        selectedPieceHighlightContainer.SetActive(false);
                        DestroyPrefab();
                        hasJumped = true;
                    }
                }

                //is were supposed to destroy anything
                if (forcedPieces.Count != 0 && !hasDestroyed)
                {
                    MovePieces(selectedPiece, x1, y1);
                    FindObjectOfType<AudioManager>().Play("Move");
                    startDrag = Vector2.zero;
                    selectedPiece = null;
                    Highlight();
                    return;
                }
                //piece moved
                pieces[x2, y2] = selectedPiece;
                pieces[x1, y1] = null;
                MovePieces(selectedPiece, x2, y2);
                FindObjectOfType<AudioManager>().Play("Move");
                EndTurn();

            }

            else
            {
                MovePieces(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                Highlight();
                selectedPieceHighlightContainer.SetActive(false);
                DestroyPrefab();
                Debug.Log("PIECE RETURNED");
                return;
            }

        }


    }

   
    private void EndTurn()
    {

        int x = (int)endDrag.x;
        int y = (int)endDrag.y;


        if (selectedPiece != null)
        {
            //white piece will become dama
            if (selectedPiece.isPlayer1Color && !selectedPiece.isDama && y == 7)
            {
                selectedPiece.isDama = true;
                selectedPiece.RotateDamaPiece();
                FindObjectOfType<AudioManager>().Play("Dama");
            }
            //black piece will become dama
            else if (!selectedPiece.isPlayer1Color && !selectedPiece.isDama && y == 0)
            {
                selectedPiece.isDama = true;
                selectedPiece.RotateDamaPiece();
                FindObjectOfType<AudioManager>().Play("Dama");
            }
        }


        selectedPiece = null;
        startDrag = Vector2.zero;

        if (ScanForPossibleMove(selectedPiece, x, y).Count != 0 && hasDestroyed)
        {
            Jump(); //pag multiple
            hasMultipleJumped = true;
            return;
        }

        if (hasMultipleJumped && hasDestroyed)
        {
            Debug.Log("Piece Multiple Jumped");
        }


        if (hasJumped)
        {
            Jump(); //single jump
        }

        isPlayer1Turn = !isPlayer1Turn;
        isPlayer1Color = !isPlayer1Color;
        hasDestroyed = false;
        hasMultipleJumped = false;
        selectedPieceHighlightContainer.SetActive(false);
        DestroyPrefab();
        hasJumped = false;
        playerTimerCanvas.SetActive(false);

        //PLAYER TURN INDICATOR
        if (isPlayer1Color == !isRed && isPlayer1Turn)
        {
            p1Blue.SetActive(true);
            p2Red.SetActive(false);
        }
        if (!isPlayer1Color == !isRed && !isPlayer1Turn)
        {
            p2Red.SetActive(true);
            p1Blue.SetActive(false);
        }

        if (isPlayer1Color == isRed && isPlayer1Turn)
        {
            p1Red.SetActive(true);
            p2Blue.SetActive(false);
        }
        if (!isPlayer1Color == isRed && !isPlayer1Turn)
        {
            p2Blue.SetActive(true);
            p1Red.SetActive(false);
        }

        if(isPlayer1Turn == false)
        {
            p1TurnCtr--;
            p1Turn.text = "TURN : " + p1TurnCtr;
        }
        else if (isPlayer1Turn == true)
        {
            p2TurnCtr--;
            p2Turn.text = "TURN : " + p2TurnCtr;
        }
        //CheckVictory();

        ctrTurn++;
        Debug.Log("Counter " + ctrTurn);


        ScanForPossibleMove();


        //check victory??
        if (p1TurnCtr == 0 && p2TurnCtr == 0)
        {
            CheckVictory();
        }
        if (p1Pieces == 0 || p2Pieces == 0)
            CheckVictory();

        StopCoroutine(playerTimer);
        playerTimer = StartCoroutine(PlayerTimer(1));
    }

    public void Jump()
    {
        //kinukuha ko mga numbers at operators 
        string numOne = presentPiece.name.Substring(0, 2);
        Debug.Log(numOne);
        string numTwo = previousPiece.name.Substring(0, 2);
        Debug.Log(numTwo);

        int num1 = int.Parse(numOne);
        int num2 = int.Parse(numTwo);

        string op = previousPiece.myOperator.name;

        if (op == "a")
        {
            op = "+";
        }
        if (op == "s")
        {
            op = "-";
        }
        if (op == "m")
        {
            op = "x";
        }
        if (op == "d")
        {
            op = "%";
        }

        //check kung tama ung question
        Debug.Log(num1 + " " + op + " " + num2);

        if (isPlayer1Turn)
        {
            ComputeAnswer.Compute(num1, num2, op, "white");
        }
        else
        {
            ComputeAnswer.Compute(num1, num2, op, "black");
        }

        questionUI.text = num1 + " " + op + " " + num2 + " = ?";
        computeCanvas.SetActive(true);
        FindObjectOfType<AudioManager>().Play("Question");
        return;
    }
    private void CheckVictory()
    {
        //engk ko titignan kung seno nanalo
        if (ComputeAnswer.whitePoints > ComputeAnswer.blackPoints)
        {
            Time.timeScale = 1f;
            victoryUI.SetActive(true);
            drawUI.SetActive(false);
            Clear();
            FindObjectOfType<AudioManager>().StopPlaying("GameBGMusic");
            FindObjectOfType<AudioManager>().Play("Victory");
            winnerText.text = "PLAYER 1";
            Debug.Log("the winner is : PLAYER");

        }
        else if (ComputeAnswer.whitePoints > ComputeAnswer.blackPoints)
        {
            Time.timeScale = 1f;
            victoryUI.SetActive(true);
            drawUI.SetActive(false);
            Clear();
            FindObjectOfType<AudioManager>().StopPlaying("GameBGMusic");
            FindObjectOfType<AudioManager>().Play("Victory");
            winnerText.text = "PLAYER 2";
            Debug.Log("the winner is : PLAYER");
        }
        else if (ComputeAnswer.whitePoints == ComputeAnswer.blackPoints)
        {
            Time.timeScale = 1f;
            victoryUI.SetActive(true);
            winnerUI.SetActive(false);
            Clear();
            FindObjectOfType<AudioManager>().StopPlaying("GameBGMusic");
            FindObjectOfType<AudioManager>().Play("Draw");
            winnerText.text = "DRAW";
            Debug.Log("the winner is : DRAW");
        }

    }

    private List<Pieces> ScanForPossibleMove(Pieces p, int x, int y)
    {
        forcedPieces = new List<Pieces>();
        //piece move
        if (pieces[x, y].isForceToMove(pieces, x, y))
            forcedPieces.Add(pieces[x, y]);
        Debug.Log("Piece Moved");
        Highlight();
        return forcedPieces;
    }
    private List<Pieces> ScanForPossibleMove()
    {
        forcedPieces = new List<Pieces>();

        //check all the pieces
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                if (pieces[i, j] != null && pieces[i, j].isPlayer1Color == isPlayer1Turn)
                    if (pieces[i, j].isForceToMove(pieces, i, j))
                    {
                        //FORCED MOVE UNG PIECE
                        forcedPieces.Add(pieces[i, j]);
                        //Debug.Log("Jump Piece" + " " );
                    }
        Highlight();
        return forcedPieces;
    }
    private void Highlight()
    {
        foreach (Transform t in forcedPieceHighlightsContainer.transform)
        {
            t.position = Vector3.down * 100;
        }

        if (forcedPieces.Count > 0)
        {
            forcedPieceHighlightsContainer.SetActive(true);
            forcedPieceHighlightsContainer.transform.GetChild(0).position = forcedPieces[0].transform.position + Vector3.up * 0.015f + highlightPieceOffset;
            Debug.Log("Jump Piece" + " " + forcedPieces[0].tag);
        }


        if (forcedPieces.Count > 1)
        {
            forcedPieceHighlightsContainer.SetActive(true);
            forcedPieceHighlightsContainer.transform.GetChild(1).position = forcedPieces[1].transform.position + Vector3.up * 0.015f + highlightPieceOffset;
            Debug.Log("Jump Piece" + " " + forcedPieces[1].tag);
        }

    }
    private void RotatePieceHighlight()
    {
        foreach (Transform t in forcedPieceHighlightsContainer.transform)
        {
            t.Rotate(Vector3.up * 90 * Time.deltaTime);
        }

        foreach (Transform t in selectedPieceHighlightContainer.transform)
        {
            t.Rotate(Vector3.up * 90 * Time.deltaTime);
        }
    }
    private void SelectedPieceHighlight()
    {
        foreach (Transform t in selectedPieceHighlightContainer.transform)
        {
            t.position = Vector3.down * 100;
        }

        if (selectedPiece != null)
        {
            selectedPieceHighlightContainer.SetActive(true);
            selectedPieceHighlightContainer.transform.GetChild(0).position = selectedPiece.transform.position + Vector3.up * 0.015f + highlightPieceOffset;


            ScanForPossiblePiecesToMove(pieces);

        }
    }

    private void DestroyPrefab()
    {
        Destroy(temp);
        Destroy(temp1);
        Destroy(temp2);
        Destroy(temp3);
    }

    IEnumerator PlayAndDisappear(string ani)
    {
        if (ani == "Color")
        {
            //yield return new WaitForSeconds(2f);

            animColor.SetTrigger("Closing");

            yield return new WaitForSeconds(2f);

            chooseColorCanvas.SetActive(false); // deactivate object

        }
    }

    
    private void Clear()
    {
        p1Red.SetActive(false);
        p2Red.SetActive(false);
        p2Blue.SetActive(false);
        p1Blue.SetActive(false);
        computeCanvas.SetActive(false);
        forcedPieceHighlightsContainer.SetActive(false);
        selectedPieceHighlightContainer.SetActive(false);
        DestroyPrefab();
        Time.timeScale = 0f;
        playerTimerCanvas.SetActive(false);
    }

    IEnumerator PlayerTimer(int minutes)
    {
        Debug.Log("start timer");
        while (minutes > 0)
        {
            minutes--;
            int seconds = 59;

            while (seconds >= 0)
            {
                if (seconds == 10)
                {
                    playerTimerCanvas.SetActive(true);
                }
                playerWarningTimerText.text = seconds.ToString();
                yield return new WaitForSeconds(1);
                seconds--;

            }
        }

        playerTimerCanvas.SetActive(false);
        isPlayer1Turn = !isPlayer1Turn;
        isPlayer1Color = !isPlayer1Color;
        hasDestroyed = false;
        hasMultipleJumped = false;
        selectedPieceHighlightContainer.SetActive(false);
        DestroyPrefab();
        hasJumped = false;

        //PLAYER TURN INDICATOR
        if (isPlayer1Color == !isRed && isPlayer1Turn)
        {
            p1Blue.SetActive(true);
            p2Red.SetActive(false);
        }
        if (!isPlayer1Color == !isRed && !isPlayer1Turn)
        {
            p2Red.SetActive(true);
            p1Blue.SetActive(false);
        }

        if (isPlayer1Color == isRed && isPlayer1Turn)
        {
            p1Red.SetActive(true);
            p2Blue.SetActive(false);
        }
        if (!isPlayer1Color == isRed && !isPlayer1Turn)
        {
            p2Blue.SetActive(true);
            p1Red.SetActive(false);
        }

        if (isPlayer1Turn == false)
        {
            p1TurnCtr--;
            p1Turn.text = "TURN : " + p1TurnCtr;
        }
        else if (isPlayer1Turn == true)
        {
            p2TurnCtr--;
            p2Turn.text = "TURN : " + p2TurnCtr;
        }
        //CheckVictory();

        ctrTurn++;
        Debug.Log("Counter " + ctrTurn);


        ScanForPossibleMove();


        //check victory??
        if (p1TurnCtr == 0 && p2TurnCtr == 0)
        {
            CheckVictory();
        }   


        playerTimer = StartCoroutine(PlayerTimer(1));

    }

    IEnumerator Timer(int minutes)
    {
        //int counter = minutes;
        while (minutes > 0)
        {
            minutes--;
            int seconds = 59;

            while (seconds >= 0)
            {
                gameTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                yield return new WaitForSeconds(1);
                seconds--;

            }
        }

        CheckVictory();
    }

    private void ScanForPossiblePiecesToMove(Pieces[,] pieces) //aalamin ano mga pieces na pwede imove
    {
        Debug.Log(selectedPiece);
        
        //selectedPieces = new List<Pieces>();
        //indicatorHighligter = new GameObject();

        //check all the pieces
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (selectedPiece.isDama)
                {
                    if (pieces[x, y] == selectedPiece) //upper player
                    {
                        //method na titignan ung katabi kung may space
                        //pag nasa dulo mga piece

                        if (x == 7) //kaliwa lang pwede
                        {
                            if (pieces[x - 1, y - 1] == null && y > 1) //pag may space ung katabi
                            {
                                Debug.Log((x - 1) + " : " + (y - 1));
                                //selectedPieces.Add(pieces[x - 1, y - 1]);
                                temp = Instantiate(indicatorHighligter, new Vector3(pieces[x, y].transform.position.x - .5f, pieces[x, y].transform.position.y, pieces[x, y].transform.position.z - 1.5f), Quaternion.identity);
                                indicatorHighligter.transform.position += pieceOffset;
                            }

                            if (pieces[x - 1, y + 1] == null && y < 7 ) //pag may space ung katabi
                            {
                                Debug.Log((x - 1) + " : " + (y + 1));
                                //selectedPieces.Add(pieces[x - 1, y - 1]);
                                temp = Instantiate(indicatorHighligter, new Vector3(pieces[x, y].transform.position.x - .5f, pieces[x, y].transform.position.y, pieces[x, y].transform.position.z + .5f), Quaternion.identity);
                                indicatorHighligter.transform.position += pieceOffset;
                            }
                        }
                        else if (x == 0) //kanan lang pwede
                        {
                            if (pieces[x + 1, y - 1] == null && y > 1) //pag may space ung katabi
                            {
                                Debug.Log((x + 1) + " : " + (y - 1));
                                //selectedPieces.Add(pieces[x + 1, y - 1]);
                                temp1 = Instantiate(indicatorHighligter, new Vector3(pieces[x, y].transform.position.x + 1.5f, pieces[x, y].transform.position.y, pieces[x, y].transform.position.z - 1.5f), Quaternion.identity);
                                indicatorHighligter.transform.position += pieceOffset;
                            }

                            if (pieces[x + 1, y + 1] == null && y < 7) //pag may space ung katabi
                            {
                                Debug.Log((x + 1) + " : " + (y + 1));
                                //selectedPieces.Add(pieces[x + 1, y - 1]);
                                temp1 = Instantiate(indicatorHighligter, new Vector3(pieces[x, y].transform.position.x + 1.5f, pieces[x, y].transform.position.y, pieces[x, y].transform.position.z + .5f), Quaternion.identity);
                                indicatorHighligter.transform.position += pieceOffset;
                            }
                        }
                        else //both sides (top left and right, and lower left and right)
                        {
                            if (pieces[x - 1, y - 1] == null && y >= 2) //pag may space ung katabi
                            {
                                Debug.Log((x - 1) + " : " + (y - 1));
                                //selectedPieces.Add(pieces[x - 1, y - 1]);
                                temp2 = Instantiate(indicatorHighligter, new Vector3(pieces[x, y].transform.position.x - .5f, pieces[x, y].transform.position.y, pieces[x, y].transform.position.z - 1.5f), Quaternion.identity);
                                indicatorHighligter.transform.position += pieceOffset;
                            }
                            if (pieces[x + 1, y - 1] == null && y >= 2) //pag may space ung katabi
                            {
                                Debug.Log((x + 1) + " : " + (y - 1));
                                //selectedPieces.Add(pieces[x + 1, y - 1]);
                                temp3 = Instantiate(indicatorHighligter, new Vector3(pieces[x, y].transform.position.x + 1.5f, pieces[x, y].transform.position.y, pieces[x, y].transform.position.z - 1.5f), Quaternion.identity);
                                indicatorHighligter.transform.position += pieceOffset;
                            }

                            if (pieces[x - 1, y + 1] == null && y <= 7) //pag may space ung katabi
                            {
                                Debug.Log((x - 1) + " : " + (y + 1));
                                //selectedPieces.Add(pieces[x - 1, y - 1]);
                                temp = Instantiate(indicatorHighligter, new Vector3(pieces[x, y].transform.position.x - .5f, pieces[x, y].transform.position.y, pieces[x, y].transform.position.z + .5f), Quaternion.identity);
                                indicatorHighligter.transform.position += pieceOffset;
                            }
                            if (pieces[x + 1, y + 1] == null && y <= 7) //pag may space ung katabi
                            {
                                Debug.Log((x + 1) + " : " + (y + 1));
                                //selectedPieces.Add(pieces[x + 1, y - 1]);
                                temp1 = Instantiate(indicatorHighligter, new Vector3(pieces[x, y].transform.position.x + 1.5f, pieces[x, y].transform.position.y, pieces[x, y].transform.position.z + .5f), Quaternion.identity);
                                indicatorHighligter.transform.position += pieceOffset;
                            }
                        }
                    }
                }
                else
                {
                    if (!isPlayer1Turn)
                    {
                        Debug.Log("upper");
                        if (pieces[x, y] == selectedPiece) //upper player
                        {
                            //method na titignan ung katabi kung may space
                            //pag nasa dulo mga piece

                            if (x == 7) //kaliwa lang pwede
                            {
                                if (pieces[x - 1, y - 1] == null) //pag may space ung katabi
                                {
                                    Debug.Log((x - 1) + " : " + (y - 1));
                                    //selectedPieces.Add(pieces[x - 1, y - 1]);
                                    temp = Instantiate(indicatorHighligter, new Vector3(pieces[x, y].transform.position.x - .5f, pieces[x, y].transform.position.y, pieces[x, y].transform.position.z - 1.5f), Quaternion.identity);
                                    indicatorHighligter.transform.position += pieceOffset;
                                }
                            }
                            else if (x == 0) //kanan lang pwede
                            {
                                if (pieces[x + 1, y - 1] == null) //pag may space ung katabi
                                {
                                    Debug.Log((x + 1) + " : " + (y - 1));
                                    //selectedPieces.Add(pieces[x + 1, y - 1]);
                                    temp1 = Instantiate(indicatorHighligter, new Vector3(pieces[x, y].transform.position.x + 1.5f, pieces[x, y].transform.position.y, pieces[x, y].transform.position.z - 1.5f), Quaternion.identity);
                                    indicatorHighligter.transform.position += pieceOffset;
                                }
                            }
                            else //both sides
                            {
                                if (pieces[x - 1, y - 1] == null) //pag may space ung katabi
                                {
                                    Debug.Log((x - 1) + " : " + (y - 1));
                                    //selectedPieces.Add(pieces[x - 1, y - 1]);
                                    temp = Instantiate(indicatorHighligter, new Vector3(pieces[x, y].transform.position.x - .5f, pieces[x, y].transform.position.y, pieces[x, y].transform.position.z - 1.5f), Quaternion.identity);
                                    indicatorHighligter.transform.position += pieceOffset;
                                }
                                if (pieces[x + 1, y - 1] == null) //pag may space ung katabi
                                {
                                    Debug.Log((x + 1) + " : " + (y - 1));
                                    //selectedPieces.Add(pieces[x + 1, y - 1]);
                                    temp1 = Instantiate(indicatorHighligter, new Vector3(pieces[x, y].transform.position.x + 1.5f, pieces[x, y].transform.position.y, pieces[x, y].transform.position.z - 1.5f), Quaternion.identity);
                                    indicatorHighligter.transform.position += pieceOffset;
                                }

                            }
                        }
                    }
                    else
                    {
                        Debug.Log("lower");
                        if (pieces[x, y] == selectedPiece) //lower player
                        {
                            //method na titignan ung katabi kung may space
                            //pag nasa dulo mga piece

                            if (x == 7) //kaliwa lang pwede
                            {
                                if (pieces[x - 1, y + 1] == null) //pag may space ung katabi
                                {
                                    Debug.Log((x - 1) + " : " + (y + 1));
                                    //selectedPieces.Add(pieces[x - 1, y - 1]);
                                    temp = Instantiate(indicatorHighligter, new Vector3(pieces[x, y].transform.position.x - .5f, pieces[x, y].transform.position.y, pieces[x, y].transform.position.z + .5f), Quaternion.identity);
                                    indicatorHighligter.transform.position += pieceOffset;
                                }
                            }
                            else if (x == 0) //kanan lang pwede
                            {
                                if (pieces[x + 1, y + 1] == null) //pag may space ung katabi
                                {
                                    Debug.Log((x + 1) + " : " + (y + 1));
                                    //selectedPieces.Add(pieces[x + 1, y - 1]);
                                    temp1 = Instantiate(indicatorHighligter, new Vector3(pieces[x, y].transform.position.x + 1.5f, pieces[x, y].transform.position.y, pieces[x, y].transform.position.z + .5f), Quaternion.identity);
                                    indicatorHighligter.transform.position += pieceOffset;
                                }
                            }
                            else //both sides
                            {
                                if (pieces[x - 1, y + 1] == null) //pag may space ung katabi
                                {
                                    Debug.Log((x - 1) + " : " + (y + 1));
                                    //selectedPieces.Add(pieces[x - 1, y - 1]);
                                    temp = Instantiate(indicatorHighligter, new Vector3(pieces[x, y].transform.position.x - .5f, pieces[x, y].transform.position.y, pieces[x, y].transform.position.z + .5f), Quaternion.identity);
                                    indicatorHighligter.transform.position += pieceOffset;
                                }
                                if (pieces[x + 1, y + 1] == null) //pag may space ung katabi
                                {
                                    Debug.Log((x + 1) + " : " + (y + 1));
                                    //selectedPieces.Add(pieces[x + 1, y - 1]);
                                    temp1 = Instantiate(indicatorHighligter, new Vector3(pieces[x, y].transform.position.x + 1.5f, pieces[x, y].transform.position.y, pieces[x, y].transform.position.z + .5f), Quaternion.identity);
                                    indicatorHighligter.transform.position += pieceOffset;
                                }

                            }
                        }
                    }
                }
            }
        }

    }

    
        
}

