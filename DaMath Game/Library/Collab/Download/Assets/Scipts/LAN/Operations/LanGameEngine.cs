using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class LanGameEngine : NetworkBehaviour
{
    public GameObject chooseColorCanvas;
    public string player1 = "";
    public string player2 = "";

    public GameObject goPlayer1 = null;
    public GameObject goPlayer2 = null;
    public GameObject myLocal = null;

    [Header("Game Setting")]
    public bool isFull = false;
    public int turn = 1; //1 means player1, 2 means player2

    public static LanGameEngine Instance { set; get; }

    //For instantating pieces
    [Header("For instantating pieces")]
    public Pieces[,] pieces = new Pieces[8, 8];
    public List<GameObject> goPieces = new List<GameObject>();
    private GameObject redPiecePrefab;
    private GameObject bluePiecePrefab;
    //array lang ng 2 team
    public GameObject[] redPieces;
    public GameObject[] bluePieces;

    public GameObject highlightsContainer;

    public CanvasGroup alertCanvas;
    private float lastAlert;
    private bool alertActive;

    private Vector3 boardOffset = new Vector3(-4f, 0, -4f);
    private Vector3 offsetBlack = new Vector3(-4.5f, 0, -3.5f); //sakto na yang pwesto wag palitan
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f); //pls wag nyo na palitan pwesto

    private Vector3 highlightPieceOffset = new Vector3(0.5f, 0, -0.5f);

    public bool isPlayer1Turn;
    public bool isPlayer1Color;
    private bool isRed;
    public bool hasDestroyed;
    private bool hasJumped;
    private bool hasMultipleJumped;

    private Pieces selectedPiece;
    private Pieces presentPiece;
    private Pieces previousPiece;
    private List<Pieces> forcedPieces;

    private Vector2 touchOver;
    private Vector2 startDrag;
    private Vector2 endDrag;

    public GameObject computeCanvas;
    public Text questionUI;

    private int ctrTurn = 0;

    public GameObject victoryUI;
    public Text winnerText;

    public Animator animColor;
    public Animator animPlayerTurn;

    public GameObject scoreCanvas;

    public bool isGameStart = false;

    [SyncVar]
    public int randPos = 0;

    public readonly SyncList<GameObject> SyncListRed = new SyncList<GameObject>();
    public readonly SyncList<GameObject> SyncListBlue = new SyncList<GameObject>();


    // Start is called before the first frame update
    public void HostInit()
    {
        Debug.Log("EXECUTED");
        chooseColorCanvas.SetActive(true);
    }

    public void setPlayerColor(int color)
    {
        if (color == 1)
        {
            player1 = "red";
            player2 = "blue";
        }
        else
        {
            player1 = "blue";
            player2 = "red";
        }

        if (player1 == "red")
        {
            isRed = true;
        }
        else
        {
            isRed = false;
        }

        isPlayer1Color = true;
        StartCoroutine(PlayAndDisappear("Color"));

    }

    [Command(requiresAuthority = false)]
    public void CmdSetObject(GameObject me, int player) 
    {
        RpcsetGameObject(me, player);
    }

    [ClientRpc]
    public void RpcsetGameObject(GameObject me, int player)
    {
        if (!isFull)
        {
            if (player == 1)
            {
                goPlayer1 = me;
            }
            else
            {
                goPlayer2 = me;
                CmdIsFull(true);
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdIsFull(bool stat) 
    {
        Debug.Log("CmdIsFull Executed");
        CmdSetIsFull(stat);
    }

    [Command(requiresAuthority = false)]
    public void CmdSetIsFull(bool stat) 
    {
        isFull = true;
        Debug.Log("Starting game");
        CmdShuffleArray();
        startGame();

    }

    [Command(requiresAuthority = false)]
    public void startGame()
    {
        foreach (Transform t in highlightsContainer.transform)
        {
            t.position = Vector3.down * 100;
        }


        Instance = this;
        //client = FindObjectOfType<LANClient>();
        //isWhite = client.isHost;

        isPlayer1Turn = true;
        forcedPieces = new List<Pieces>();
        GenerateBoard();
    }


    [Command(requiresAuthority = false)]
    public void CmdTestMove() 
    {
        goPieces[Random.Range(0, goPieces.Count)].transform.position = new Vector3(Random.Range(0, 15), Random.Range(0, 15), Random.Range(0, 15));
    }

    // Update is called once per frame
    void Update()
    {
        if (myLocal != null) 
        {
            if (myLocal.GetComponent<PlayerLanScript>().isLocalPlayer && Input.GetKeyDown(KeyCode.I))
            {
                CmdTestMove();
            }
        }
        

        UpdateAlert();
        UpdateTouch();

        //Debug.Log(touchOver);

        //if its player 1 turn
        if ((isPlayer1Color) ? isPlayer1Turn : !isPlayer1Turn)
        {
            int x = (int)touchOver.x;
            int y = (int)touchOver.y;

            if (selectedPiece != null)
                UpdatePieceDrag(selectedPiece);

            if (Input.GetMouseButtonDown(0))
            {
                SelectPiece(x, y);
                Debug.Log("Select Piece Executed 1");
            }
                

            if (Input.GetMouseButtonUp(0))
                TryMove((int)startDrag.x, (int)startDrag.y, x, y);
        }

    }
    public void Color(string color)
    {
        if (color == "red")
        {
            isRed = true;
        }
        else
        {
            isRed = false;
        }

        isPlayer1Color = true;

        GenerateBoard();
        StartCoroutine(PlayAndDisappear("Color"));
    }

    private void SelectPiece(int x, int y)
    {
        
        //Out of Bounds

        if (x < 0 || x >= 8 || y < 0 || y >= 8)
            return;


        Pieces p = pieces[x, y];
        if (p != null && p.isPlayer1Color == isPlayer1Color)
        {
            if (forcedPieces.Count == 0)
            {
                
                selectedPiece = p;
                startDrag = touchOver;
                Debug.Log("Select Piece Executed 2");
                //checking lang to kung anong chip at kung may operator ba
                Debug.Log(selectedPiece.tag);
                Debug.Log(selectedPiece.hadOperator);

            }
            else
            {
                // look
                if (forcedPieces.Find(fp => fp == p) == null)
                    return;
                Debug.Log("Select Piece Executed 3");
                selectedPiece = p;
                startDrag = touchOver;
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdShuffleArray()
    {
        //shuffle red
        for (int i = 0; i < redPieces.Length; i++)
        {
            int rand = Random.Range(0, redPieces.Length);
            GameObject temp = redPieces[rand];
            redPieces[rand] = redPieces[i];
            redPieces[i] = temp;
        }

        for (int i = 0; i < bluePieces.Length; i++)
        {
            int rand = Random.Range(0, bluePieces.Length);
            GameObject temp = bluePieces[rand];
            bluePieces[rand] = bluePieces[i];
            bluePieces[i] = temp;
        }

        for (int i = 0; i < bluePieces.Length; i++) 
        {
            SyncListBlue.Add(bluePieces[i]);
            SyncListRed.Add(redPieces[i]);
        }
    }

    [Command(requiresAuthority = false)]
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

    [Command(requiresAuthority =false)]
    private void GeneratePieces(int x, int y, int ctr)
    {
        /*redPiecePrefab = redPieces[ctr] as GameObject;
        bluePiecePrefab = bluePieces[ctr] as GameObject;*/
        redPiecePrefab = SyncListRed[ctr] as GameObject;
        bluePiecePrefab = SyncListBlue[ctr] as GameObject;
        bool isPiecePlayer1 = !(y > 3);

        if (isRed)
        {
            GameObject gameObj = Instantiate((isPiecePlayer1) ? redPiecePrefab : bluePiecePrefab) as GameObject;
            NetworkServer.Spawn(gameObj);
            RpcMovePieces(x,y, gameObj, transform);
            Pieces p = gameObj.GetComponent<Pieces>();
            pieces[x, y] = p;

            if (pieces[x, y].isRed && isRed)
            {
                pieces[x, y].isPlayer1Color = true;
            }
        }
        else
        {
            GameObject gameObj = Instantiate((isPiecePlayer1) ? bluePiecePrefab : redPiecePrefab) as GameObject;
            RpcMovePieces(x, y, gameObj, transform);
            Pieces p = gameObj.GetComponent<Pieces>();
            pieces[x, y] = p;

            if (!pieces[x, y].isRed && !isRed)
            {
                pieces[x, y].isPlayer1Color = true;
            }
        }

        scoreCanvas.SetActive(true);
    }


    private void MovePieces(Pieces p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + offsetBlack + pieceOffset;
    }

    [ClientRpc]
    private void RpcMovePieces(int x, int y, GameObject gameObj, Transform transform)
    {
        if (myLocal.GetComponent<PlayerLanScript>().isLocalPlayer) 
        {
            goPieces.Add(gameObj);
            Debug.Log(x + " " + y);
            gameObj.transform.SetParent(transform);
            gameObj.transform.position = (Vector3.right * x) + (Vector3.forward * y) + offsetBlack + pieceOffset;
        }
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
        Debug.Log("UpdatePieceDrag Executed");
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
        Debug.Log("Try Move Executed");
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
                        Destroy(p.gameObject);
                        hasDestroyed = true;
                        hasJumped = true;
                    }
                }

                //is were supposed to destroy anything
                if (forcedPieces.Count != 0 && !hasDestroyed)
                {
                    MovePieces(selectedPiece, x1, y1);
                    startDrag = Vector2.zero;
                    selectedPiece = null;
                    Highlight();
                    return;
                }
                //piece moved
                pieces[x2, y2] = selectedPiece;
                pieces[x1, y1] = null;
                MovePieces(selectedPiece, x2, y2);

                EndTurn();

            }

            else
            {
                MovePieces(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                Highlight();
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
            }
            //black piece will become dama
            else if (!selectedPiece.isPlayer1Color && !selectedPiece.isDama && y == 0)
            {
                selectedPiece.isDama = true;
                selectedPiece.RotateDamaPiece();
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

        hasJumped = false;
        //CheckVictory();

        ctrTurn++;
        Debug.Log("Counter " + ctrTurn);

        ScanForPossibleMove(); 

        //check victory??
        if (ctrTurn == 20)
        {
            CheckVictory();
            ctrTurn = 0;
        }

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
        return;
    }
    private void CheckVictory()
    {
        //engk ko titignan kung seno nanalo
        if (ComputeAnswer.whitePoints > ComputeAnswer.blackPoints)
        {
            Time.timeScale = 1f;
            victoryUI.SetActive(true);
            winnerText.text = "WHITE";
            Debug.Log("the winner is : WHITE");
        }
        else
        {
            Time.timeScale = 1f;
            victoryUI.SetActive(true);
            winnerText.text = "BLACK";
            Debug.Log("the winner is : BLACK");
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
        foreach (Transform t in highlightsContainer.transform)
        {
            t.position = Vector3.down * 100;
        }

        if (forcedPieces.Count > 0)
        {
            highlightsContainer.SetActive(true);
            highlightsContainer.transform.GetChild(0).position = forcedPieces[0].transform.position + Vector3.up * 0.015f + highlightPieceOffset;
            Debug.Log("Jump Piece" + " " + forcedPieces[0].tag);
        }


        if (forcedPieces.Count > 1)
        {
            highlightsContainer.SetActive(true);
            highlightsContainer.transform.GetChild(1).position = forcedPieces[1].transform.position + Vector3.up * 0.015f + highlightPieceOffset;
            Debug.Log("Jump Piece" + " " + forcedPieces[1].tag);
        }

    }
    public void Alert(string text)
    {
        alertCanvas.GetComponentInChildren<TMP_Text>().text = text;
        alertCanvas.alpha = 1;
        lastAlert = Time.time;
        alertActive = true;
    }
    public void UpdateAlert()
    {
        if (alertActive)
        {
            if (Time.time - lastAlert > 1.5)
            {
                alertCanvas.alpha = 1 - ((Time.time - lastAlert) - 1.5f);
                if (Time.time - lastAlert > 2.5f)
                {
                    alertActive = false;
                }
            }
        }
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
}
