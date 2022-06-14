using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;

public class LanGameEngine : NetworkBehaviour
{
    public static LanGameEngine Instance;

    //For instantating pieces
    public Pieces[,] pieces = new Pieces[8, 8];
    private GameObject redPiecePrefab;
    private GameObject bluePiecePrefab;
    //array lang ng 2 team
    public GameObject[] redPieces;
    public GameObject[] bluePieces;

    public GameObject forcedPieceHighlightsContainer;
    public GameObject selectedPieceHighlightContainer;

    private Vector3 boardOffset = new Vector3(-4f, 0, -4f);
    private Vector3 offsetBlack = new Vector3(-4.5f, 0, -3.5f); //sakto na yang pwesto wag palitan
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f); //pls wag nyo na palitan pwesto

    private Vector3 highlightPieceOffset = new Vector3(0.5f, 0, -0.5f);

    public bool isPlayer1Turn = true;
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
    public GameObject chooseColorCanvas;
    public GameObject p1Red;
    public GameObject p1Blue;
    public GameObject p2Red;
    public GameObject p2Blue;

    public GameObject localPlayer;
    public string localString;
    public string localColor;

    public SyncList<piecesRandomPos> pieceRandomNumber = new SyncList<piecesRandomPos>();

    bool isGameStarted = false;

    [SyncVar]
    public int x;

    [SyncVar]
    public int y;

    [SyncVar]
    public string turn = "Player1";

    [SyncVar]
    public string sync_color = "Red";

    //private LANClient client;
    public struct piecesRandomPos
    {
        public int[] blueRandomNumber;
        public int[] redRandomNumber;
    }


    private void Awake()
    {
        scoreCanvas.SetActive(true);

        
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
        isPlayer1Color = true;
        forcedPieces = new List<Pieces>();
        isRed = true;
        StartGame();
    }

    public void StartGame() 
    {
        StartCoroutine(DelayPlayer());
    }

    IEnumerator DelayPlayer() 
    {
        while(!isGameStarted)
        {
            if (localPlayer != null) 
            {
                cmdSetGameStarted(true);
                if (localPlayer.GetComponent<PlayerLanScript>().isServer)
                {
                    cmdGenerateRandomNumberForPieces();
                }
                ShuffleArray();
                GenerateBoard();
            }
            yield return new WaitForSeconds(1f);
        }
    }


    public void cmdSetGameStarted(bool status) 
    {
        isGameStarted = status;
    }

    // Update is called once per frame
    void Update()
    {
        RotatePieceHighlight();
        UpdateTouch();
        
        //Debug.Log(touchOver);

        //if its player 1 turn
        if (((isPlayer1Color) ? isPlayer1Turn : !isPlayer1Turn) && localString.Equals(turn) && localColor.Equals(sync_color))
        {
            int local_x = (int)touchOver.x;
            int local_y = (int)touchOver.y;

            if (selectedPiece != null)
                UpdatePieceDrag(selectedPiece);

            if (Input.GetMouseButtonDown(0))
            {
                Pieces p = pieces[local_x, local_y];
                if(p.isRed && localColor.Equals("Red"))
                {
                    CmdSelectPiece(local_x, local_y);
                }
                if(!p.isRed && localColor.Equals("Blue"))
                {
                    CmdSelectPiece(local_x, local_y);
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                TryMove((int)startDrag.x, (int)startDrag.y, local_x, local_y);
            }
                
        }
        if (!turn.Equals(localString))
        {
            if (selectedPiece != null)
                UpdatePieceDrag(selectedPiece);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            cmdFlipTurn();
        }
    }

    [Command(requiresAuthority = false)]
    public void cmdFlipTurn()
    {
        if (turn.Equals("Player1"))
        {
            turn = "Player2";
            sync_color = "Blue";
        }
        else
        {
            turn = "Player1";
            sync_color = "Red";
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdTryMove() 
    {
        RpcTryMove();
    }

    [ClientRpc]
    public void RpcTryMove() 
    {
        TryMove((int)startDrag.x, (int)startDrag.y, x, y);
    }

    [Command(requiresAuthority = false)]
    public void CmdSelectPiece(int x1, int y1) 
    {
        cmdXandY(x1, y1);
        RpcSelectPiece();
    }

    [ClientRpc]
    public void RpcSelectPiece() 
    {
        SelectPiece(x, y);
        SelectedPieceHighlight();
    }


    [Command(requiresAuthority = false)]
    public void cmdXandY(int x1, int y1) 
    {
        x = x1;
        y = y1;
    }


    [Command(requiresAuthority = false)]
    public void cmdGenerateRandomNumberForPieces()
    {
        Debug.Log("Shuffled Random Number");
        piecesRandomPos randNumber = new piecesRandomPos
        {
            blueRandomNumber = new int[12],
            redRandomNumber = new int[12]
        };

        for (int i = 0; i < 12; i++)
        {
            randNumber.blueRandomNumber[i] = UnityEngine.Random.Range(0, 11);
            randNumber.redRandomNumber[i] = UnityEngine.Random.Range(0, 11);
        }
        pieceRandomNumber.Add(randNumber);
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
    }

    private void SelectPiece(int x1, int y1)
    {
        //Out of Bounds

        if (x1 < 0 || x1 >= 8 || y1 < 0 || y1 >= 8)
            return;

        Pieces p = pieces[x1, y1];
        if (p != null && ((p.isPlayer1Color == isPlayer1Color) || (p.isPlayer1Color != isPlayer1Color)))
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
        for (int i = 0; i < 12; i++)
        {
            GameObject temp = redPieces[pieceRandomNumber[0].redRandomNumber[i]];
            redPieces[pieceRandomNumber[0].redRandomNumber[i]] = redPieces[i];
            redPieces[i] = temp;
        }
        //shuffle blue
        for (int i = 0; i < 12; i++)
        {
            GameObject temp = bluePieces[pieceRandomNumber[0].blueRandomNumber[i]];
            bluePieces[pieceRandomNumber[0].blueRandomNumber[i]] = bluePieces[i];
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

    [Command(requiresAuthority = false)]
    public void CmdCancelSelectedPiece(int x1, int y1, int sX, int sY) 
    {
        RpcCancelSelectedPiece(x1, y1, sX, sY);
    }

    [ClientRpc]
    public void RpcCancelSelectedPiece(int x1, int y1, int sX, int sY)
    {
        selectedPiece = pieces[sX, sY];
        if (selectedPiece != null)
            MovePieces(selectedPiece, x1, y1);
        
        startDrag = Vector2.zero;
        selectedPiece = null;
        Highlight();
    }

    [Command(requiresAuthority = false)]
    public void CmdNotMove(int x1, int y1, int sX, int sY)
    {
        RpcNotMove(x1, y1, sX, sY);
    }

    [ClientRpc]
    public void RpcNotMove(int x1, int y1, int sX, int sY)
    {
        selectedPiece = pieces[sX, sY];
        MovePieces(selectedPiece, x1, y1);
        startDrag = Vector2.zero;
        selectedPiece = null;
        Highlight();
    }

    [Command(requiresAuthority = false)]
    public void CmdCapturePiece(int x1, int y1, int x2, int y2) 
    {
        RpcCapturePiece(x1, y1, x2, y2);
    }

    [ClientRpc]
    public void RpcCapturePiece(int x1, int y1, int x2, int y2)
    {
        Pieces p = pieces[(x1 + x2) / 2, (y1 + y2) / 2];
        pieces[(x1 + x2) / 2, (y1 + y2) / 2] = null;
        presentPiece = selectedPiece;
        previousPiece = p;
        try
        {
            Destroy(p.gameObject);
        }
        catch (Exception e) { Debug.Log(e.Message); }
        FindObjectOfType<AudioManager>().Play("Capture");
        hasDestroyed = true;
        hasJumped = true;
        selectedPieceHighlightContainer.SetActive(false);
    }

    [Command(requiresAuthority = false)]
    public void CmdSetForcedPiece() 
    {
        RpcSetForcedPiece();
    }

    [ClientRpc]
    public void RpcSetForcedPiece()
    {
        forcedPieces = ScanForPossibleMove();
    }

    [Command(requiresAuthority = false)]
    public void CmdDestroyAnything(int x1, int y1)
    {
        RpcDestroyAnything(x1, y1);
    }

    [ClientRpc]
    public void RpcDestroyAnything(int x1, int y1)
    {
        selectedPiece = pieces[x1, y1];
        MovePieces(selectedPiece, x1, y1);
        FindObjectOfType<AudioManager>().Play("Move");
        startDrag = Vector2.zero;
        selectedPiece = null;
        Highlight();
    }

    [Command(requiresAuthority = false)]
    public void CmdPieceMove(int x1, int y1, int x2, int y2)
    {
        RpcPieceMove(x1, y1, x2, y2);
        cmdFlipTurn();
    }

    [ClientRpc]
    public void RpcPieceMove(int x1, int y1, int x2, int y2)
    {
        selectedPiece = pieces[x1, y1];
        pieces[x2, y2] = selectedPiece;
        pieces[x1, y1] = null;
        MovePieces(selectedPiece, x2, y2);
        FindObjectOfType<AudioManager>().Play("Move");
        EndTurn();
    }

    public void TryMove(int x1, int y1, int x2, int y2)
    {
        CmdSetForcedPiece();

        //Multiplayer
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1];

        //cancelling selected piece move/ out of bounds
        if (x2 < 0 || x2 >= 8 || y2 < 0 || y2 >= 8)
        {
            CmdCancelSelectedPiece(x1, y1, x1, y1);
            selectedPiece = null;
            return;
        }

        if (selectedPiece != null)
        {
            //if it did not move
            if ((endDrag == startDrag))
            {
                CmdNotMove(x1, y1, x1, y1);
                selectedPiece = null;
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
                        CmdCapturePiece(x1, y1, x2, y2);
                    }
                }

                //is were supposed to destroy anything
                if (forcedPieces.Count != 0 && !hasDestroyed)
                {
                    CmdDestroyAnything(x1, y1);
                    return;
                }
                //piece moved
                CmdPieceMove(x1, y1, x2, y2);
            }

            else
            {
                CmdPieceReturnd(x1, y1);
                return;
            }

        }
    }

    [Command(requiresAuthority = false)]
    public void CmdPieceReturnd(int x1, int y1)
    {
        RpcPieceReturnd(x1, y1);
    }

    [ClientRpc]
    public void RpcPieceReturnd(int x1, int y1)
    {
        selectedPiece = pieces[x1, y1];
        MovePieces(selectedPiece, x1, y1);
        startDrag = Vector2.zero;
        selectedPiece = null;
        Highlight();
        selectedPieceHighlightContainer.SetActive(false);
        Debug.Log("PIECE RETURNED");
    }

    private void EndTurn()
    {
        /*int x = (int)endDrag.x;
        int y = (int)endDrag.y;*/

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

        /*if (ScanForPossibleMove(selectedPiece, x, y).Count != 0 && hasDestroyed)
        {
            Jump(); //pag multiple
            hasMultipleJumped = true;
            return;
        }*/


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
            LanComputeAnswer.Compute(num1, num2, op, "white");
        }
        else
        {
            LanComputeAnswer.Compute(num1, num2, op, "black");
        }

        questionUI.text = num1 + " " + op + " " + num2 + " = ?";
        computeCanvas.SetActive(true);
        FindObjectOfType<AudioManager>().Play("Question");
        return;
    }
    private void CheckVictory()
    {
        //engk ko titignan kung seno nanalo
        if (LanComputeAnswer.whitePoints > LanComputeAnswer.blackPoints)
        {
            Time.timeScale = 1f;
            victoryUI.SetActive(true);
            Clear();
            FindObjectOfType<AudioManager>().Play("Victory");
            winnerText.text = "PLAYER 1";
            Debug.Log("the winner is : PLAYER 1");
        }
        else
        {
            Time.timeScale = 1f;
            victoryUI.SetActive(true);
            Clear();
            FindObjectOfType<AudioManager>().Play("Victory");
            winnerText.text = "PLAYER 2";
            Debug.Log("the winner is : PLAYER 2");
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

    private void Clear()
    {
        p1Red.SetActive(false);
        p2Red.SetActive(false);
        p2Blue.SetActive(false);
        p1Blue.SetActive(false);
        computeCanvas.SetActive(false);
        forcedPieceHighlightsContainer.SetActive(false);
        selectedPieceHighlightContainer.SetActive(false);
    }
}
