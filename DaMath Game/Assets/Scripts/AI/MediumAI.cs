using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MediumAI : MonoBehaviour
{
    //private static bool AiFirstTurn = false;
    private static List<Pieces> selectedPieces;
    private static Pieces selectedPiece;
    private static Pieces previousPiece;
    public static int randomPiece;
    public static Pieces randomSelectedPiece;
    public static int rightAnswer;

    public static int x2 = 0;
    public static int y2 = 0;
    public static int x1 = 0;
    public static int y1 = 0;

    public static string jump = "";
    public static int expectedScore = 0;

    public static void Greedy(Pieces[,] pieces, List<Pieces> forcedPieces)
    {
        randomPiece = 0;
        randomSelectedPiece = null;

        ScanForPossiblePiecesToMove(pieces);

        //checking kung seno mga pwedeng magmove
        foreach (var x in selectedPieces)
        {
            //Debug.Log(x.ToString());
        }

    }

    private static Pieces selectPieceToJump(Pieces[,] pieces, List<Pieces> forcedPieces)
    {
        int highestIndex = -1;
        int highestScore = -100;
        jump = "";

        for (int i = 0; i < forcedPieces.Count; i++)
        {

            Pieces currentPiece = forcedPieces[i];

            //Debug.Log("Checking: " + currentPiece.name);
            //Debug.Log("Current Score: " + highestScore);
            //Debug.Log("Jump: " + jump);

            Vector2 pos = getPos(pieces, currentPiece);
            int x = (int)pos.x;
            int y = (int)pos.y;

            bool isLeftValid = isAIJumpToLeftValid(x, y, pieces);
            bool isRightValid = isAIJumpToRightValid(x, y, pieces);

            int left = -100;
            int right = -100;
            int leftBack = -100;
            int rightBack = -100;

            Pieces pieceToEat;
            string selectedJump = "";
            int selectedScore = 0;

            //Get left jump points
            if (isLeftValid)
            {
                pieceToEat = pieces[x - 1, y - 1];
                left = getPoints(currentPiece, pieceToEat, "left");
            }

            //Get right jump points
            if (isRightValid)
            {
                pieceToEat = pieces[x + 1, y - 1];
                right = getPoints(currentPiece, pieceToEat, "right");
            }



            //Dama
            if (currentPiece.isDama)
            {


                bool isLeftBackValid = isAIJumpBackToLeftValid(x, y, pieces);
                bool isRightBackValid = isAIJumpBackToRightValid(x, y, pieces);

                //Get back left jump points
                if (isLeftBackValid)
                {
                    pieceToEat = pieces[x - 1, y + 1];
                    leftBack = getPoints(currentPiece, pieceToEat, "leftBack");
                }

                //Get back right jump points
                if (isRightBackValid)
                {
                    pieceToEat = pieces[x + 1, y + 1];
                    rightBack = getPoints(currentPiece, pieceToEat, "rightBack");
                }

                //Check higher score
                if (leftBack > rightBack && isLeftBackValid)
                {
                    if (leftBack > selectedScore)
                    {
                        selectedJump = "leftBack";
                        selectedScore = leftBack;
                    }

                }
                else if (isRightBackValid)
                {
                    if (rightBack > selectedScore)
                    {
                        selectedJump = "rightBack";
                        selectedScore = rightBack;
                    }
                }
            }
            else
            {
                //Check higher score
                if (left > right && isLeftValid)
                {
                    selectedJump = "left";
                    selectedScore = left;
                }
                else if (isRightValid)
                {
                    selectedJump = "right";
                    selectedScore = right;
                }
            }

            //Set piece to jump
            if (jump != "")
            {
                if (selectedScore > highestScore)
                {
                    jump = selectedJump;
                    highestScore = selectedScore;
                    highestIndex = i;

                    //Debug.Log("CHANGED");
                }
            }
            else
            {
                jump = selectedJump;
                highestScore = selectedScore;
                highestIndex = i;
            }

            //Debug.Log("Left: " + left);
            //Debug.Log("Right: " + right);
            //Debug.Log("Left Back: " + leftBack);
            //Debug.Log("Right Back: " + rightBack);
            //Debug.Log("Selected Jump: " + selectedJump);
        }

        expectedScore = highestScore;
        return forcedPieces[highestIndex];
    }

    private static int getPoints(Pieces pieceToJump, Pieces pieceToEat, string dir)
    {
        int num1 = getValue(pieceToJump);
        int num2 = getValue(pieceToEat);
        int answer = 0;

        Operation operand = Operation.None;

        Square square = pieceToJump.myOperator.GetComponent<Square>();
        if (dir == "left")
            operand = square.bottomLeft;
        else if (dir == "right")
            operand = square.bottomRight;
        else if (dir == "leftBack")
            operand = square.topLeft;
        else if (dir == "rightBack")
            operand = square.topRight;


        if (operand == Operation.Addition)
        {
            answer = num1 + num2;
        }
        else if (operand == Operation.Subtraction)
        {
            answer = num1 - num2;
        }
        else if (operand == Operation.Multiplication)
        {
            answer = num1 * num2;
        }
        else if (operand == Operation.Division)
        {
            if (num2 != 0)
            {
                answer = num1 / num2;
            }
        }

        return answer;
    }

    private static int getValue(Pieces piece)
    {
        string name = piece.name.Substring(0, 2);

        return int.Parse(name);
    }

    private static bool isAIJumpToLeftValid(int x, int y, Pieces[,] pieces)
    {
        if (x > 1 && y > 1)
        {
            Pieces pieceToJump = pieces[x - 1, y - 1];
            if (pieceToJump != null && pieceToJump.isPlayer1Color && pieces[x - 2, y - 2] == null)
            {
                return true;
            }
            else return false;
        }

        else return false;
    }

    private static bool isAIJumpToRightValid(int x, int y, Pieces[,] pieces)
    {
        if (x < 6 && y > 1)
        {
            Pieces pieceToJump = pieces[x + 1, y - 1];
            if (pieceToJump != null && pieceToJump.isPlayer1Color && pieces[x + 2, y - 2] == null)
            {
                return true;
            }
            else return false;
        }
        else return false;
    }

    private static bool isAIJumpBackToLeftValid(int x, int y, Pieces[,] pieces)
    {
        if (x > 1 && y < 6)
        {
            Pieces pieceToJump = pieces[x - 1, y + 1];
            if (pieceToJump != null && pieceToJump.isPlayer1Color && pieces[x - 2, y + 2] == null)
            {
                return true;
            }
            else return false;
        }
        else return false;
    }

    private static bool isAIJumpBackToRightValid(int x, int y, Pieces[,] pieces)
    {
        if (x < 6 && y < 6)
        {
            Pieces pieceToJump = pieces[x + 1, y + 1];
            if (pieceToJump != null && pieceToJump.isPlayer1Color && pieces[x + 2, y + 2] == null)
            {
                return true;
            }
            else return false;
        }
        else return false;
    }


    private static Vector2 getPos(Pieces[,] pieces, Pieces piece)
    {

        Vector2 pos = new Vector2();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (pieces[i, j] == piece)
                {
                    pos.x = i;
                    pos.y = j;
                }
            }
        }

        return pos;
    }

    public static void Jump(Pieces[,] pieces, List<Pieces> forcedPieces)
    {
        int x = 0;
        int y = 0;

        selectedPiece = selectPieceToJump(pieces, forcedPieces);

        previousPiece = null;

        //Debug.Log("jump");

        Vector2 pos = getPos(pieces, selectedPiece);
        x = (int)pos.x;
        y = (int)pos.y;

        x1 = x;
        y1 = y;

        if (jump == "left")
        {
            x2 = x - 2;
            y2 = y - 2;

            previousPiece = pieces[x - 1, y - 1];

        }
        else if (jump == "right")
        {
            x2 = x + 2;
            y2 = y - 2;

            previousPiece = pieces[x + 1, y - 1];
        }
        else if (jump == "leftBack")
        {
            x2 = x - 2;
            y2 = y + 2;

            previousPiece = pieces[x - 1, y + 1];
        }
        else if (jump == "rightBack")
        {
            x2 = x + 2;
            y2 = y + 2;

            previousPiece = pieces[x + 1, y + 1];
        }

        //Debug.Log(jump);

        //Debug.Log("tatalon si: " + selectedPiece.name + " " + x1 + " : " + y1 + ", sa " + previousPiece.name + " " + x2 + " : " + y2);

        ComputeAnswer.blackPoints += expectedScore;
        expectedScore = 0;

    }

    private static void ComputeScore(Pieces presentPiece, Pieces previousPiece)
    {
        //kinukuha ko mga numbers at operators 
        string numOne = presentPiece.name.Substring(0, 2);
        //Debug.Log(numOne);
        string numTwo = previousPiece.name.Substring(0, 2);
        //Debug.Log(numTwo);

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
        //Debug.Log(num1 + " " + op + " " + num2);

        if (op == "+")
        {
            rightAnswer = num1 + num2;
        }
        if (op == "-")
        {
            rightAnswer = num1 - num2;
        }
        if (op == "x")
        {
            rightAnswer = num1 * num2;
        }
        if (op == "%")
        {
            if (num2 == 0)
            {
                rightAnswer = 0;
            }
            else
            {
                rightAnswer = num1 / num2;
            }

        }

        ComputeAnswer.blackPoints += rightAnswer;

    }

    private static List<Pieces> ScanForPossiblePiecesToMove(Pieces[,] pieces) //aalamin ano mga pieces na pwede imove
    {
        //Debug.Log("move na ni black - 1");
        selectedPieces = new List<Pieces>();

        //check all the pieces
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (pieces[x, y] != null && pieces[x, y].isPlayer1Color == false && pieces[x, y].isDama == false)
                {
                    //method na titignan ung katabi kung may space
                    //pag nasa dulo mga piece

                    if (x == 7) //kaliwa lang pwede
                    {
                        if (pieces[x - 1, y - 1] == null) //pag may space ung katabi
                        {
                            selectedPieces.Add(pieces[x, y]);
                        }
                    }
                    else if (x == 0) //kanan lang pwede
                    {
                        if (pieces[x + 1, y - 1] == null) //pag may space ung katabi
                        {
                            selectedPieces.Add(pieces[x, y]);
                        }
                    }
                    else //both sides
                    {
                        bool alreadyAdded = false;

                        if (pieces[x - 1, y - 1] == null) //pag may space ung katabi
                        {
                            if (!alreadyAdded)
                            {
                                selectedPieces.Add(pieces[x, y]);

                                alreadyAdded = true;
                            }
                        }
                        if (pieces[x + 1, y - 1] == null) //pag may space ung katabi
                        {
                            if (!alreadyAdded)
                            {
                                selectedPieces.Add(pieces[x, y]);
                            }
                        }
                    }
                }

                if (pieces[x, y] != null && pieces[x, y].isPlayer1Color == false && pieces[x, y].isDama == true) // dama
                {
                    //method na titignan ung katabi kung may space
                    //pag nasa dulo mga piece

                    if (x == 7) //kaliwa lang pwede
                    {
                        if (pieces[x - 1, y + 1] == null) //pag may space ung katabi
                        {
                            selectedPieces.Add(pieces[x, y]);
                        }
                        else if (pieces[x - 1, y - 1] == null)
                        {
                            selectedPieces.Add(pieces[x, y]);
                        }
                    }
                    else if (x == 0) //kanan lang pwede
                    {
                        if (pieces[x + 1, y + 1] == null) //pag may space ung katabi
                        {
                            selectedPieces.Add(pieces[x, y]);
                        }
                        else if (pieces[x + 1, y - 1] == null)
                        {
                            selectedPieces.Add(pieces[x, y]);
                        }
                    }
                    else //both sides
                    {
                        bool alreadyAdded = false;

                        if (pieces[x - 1, y + 1] == null) //pag may space ung katabi
                        {
                            if (!alreadyAdded)
                            {
                                selectedPieces.Add(pieces[x, y]);

                                alreadyAdded = true;
                            }
                        }
                        /**else if (pieces[x - 1, y - 1] == null)
                        {
                            if (!alreadyAdded)
                            {
                                selectedPieces.Add(pieces[x, y]);

                                alreadyAdded = true;
                            }
                        }**/


                        if (pieces[x + 1, y + 1] == null) //pag may space ung katabi
                        {
                            if (!alreadyAdded)
                            {
                                selectedPieces.Add(pieces[x, y]);
                            }
                        }
                        /**else if (pieces[x + 1, y - 1] == null)
                        {
                            if (!alreadyAdded)
                            {
                                selectedPieces.Add(pieces[x, y]);
                            }
                        }**/
                    }
                }
            }

        }


        randomSelectedPiece = selectedPieces[Random.Range(0, selectedPieces.Count)];
        Debug.Log("random to tropa no choice e :" + randomSelectedPiece);

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (pieces[i, j] == randomSelectedPiece)
                {
                    x1 = i;
                    y1 = j;

                    if (x1 == 7) // kaliwa
                    {
                        if (pieces[i, j].isDama)
                        {
                            x2 = (x1 + (x1 - 2)) / 2;
                            y2 = (y1 + (y1 + 1)) / 2;
                        }
                        else
                        {
                            x2 = (x1 + (x1 - 2)) / 2;
                            y2 = (y1 + (y1 - 1)) / 2;
                        }

                    }
                    else if (x1 == 0) // kanan
                    {
                        if (pieces[i, j].isDama)
                        {
                            x2 = (x1 + (x1 + 2)) / 2;
                            y2 = (y1 + (y1 + 1)) / 2;
                        }
                        else
                        {
                            x2 = (x1 + (x1 + 2)) / 2;
                            y2 = (y1 + (y1 - 1)) / 2;
                        }
                    }
                    else // both
                    {
                        if ((x1 == 5 || x1 == 6)) //kaliwa lang open
                        {
                            if (pieces[i, j].isDama)
                            {
                                x2 = (x1 + (x1 - 2)) / 2;
                                y2 = (y1 + (y1 + 1)) / 2;
                            }
                            else
                            {
                                x2 = (x1 + (x1 - 2)) / 2;
                                y2 = (y1 + (y1 - 1)) / 2;
                            }
                        }
                        else if ((x1 == 1 || x1 == 2)) //kanan lang open
                        {
                            if (pieces[i, j].isDama)
                            {
                                x2 = (x1 + (x1 + 2)) / 2;
                                y2 = (y1 + (y1 + 1)) / 2;
                            }
                            else
                            {
                                x2 = (x1 + (x1 + 2)) / 2;
                                y2 = (y1 + (y1 - 1)) / 2;
                            }
                        }
                        else // pag parehas may space
                        {
                            int random = Random.Range(0, 1);

                            if (random == 0)
                            {
                                if (pieces[i, j].isDama)
                                {
                                    x2 = (x1 + (x1 - 2)) / 2;
                                    y2 = (y1 + (y1 + 1)) / 2;
                                }
                                else
                                {
                                    x2 = (x1 + (x1 - 2)) / 2;
                                    y2 = (y1 + (y1 - 1)) / 2;
                                }
                            }
                            else
                            {
                                if (pieces[i, j].isDama)
                                {
                                    x2 = (x1 + (x1 + 2)) / 2;
                                    y2 = (y1 + (y1 + 1)) / 2;
                                }
                                else
                                {
                                    x2 = (x1 + (x1 + 2)) / 2;
                                    y2 = (y1 + (y1 - 1)) / 2;
                                }
                            }
                        }
                    }
                }
            }
        }
        Debug.Log("random pwesto ni :" + randomSelectedPiece + " " + x1 + " & " + y1 + " dito : " + x2 + " & " + y2);

        return selectedPieces;
    }

    private static Pieces getPiece(Pieces[,] pieces, int x, int y)
    {
        return pieces[x, y];
    }

    private static bool CheckBottomLeftPiece(Pieces[,] pieces, int x, int y)
    {
        if (x > 1 && y > 1)
        {
            Pieces p = getPiece(pieces, x - 2, y - 2);
            if (p != null && p.isPlayer1Color)
            {
                return true;
            }
        }

        return false;
    }

    private static bool CheckBottomRightPiece(Pieces[,] pieces, int x, int y)
    {
        if (x < 6 && y > 1)
        {
            Pieces p = getPiece(pieces, x + 2, y - 2);
            if (p != null && p.isPlayer1Color)
            {
                return true;
            }
        }

        return false;
    }

    private static bool CheckTopLeftPiece(Pieces[,] pieces, int x, int y)
    {
        if (x > 1 && y < 6)
        {
            Pieces p = getPiece(pieces, x - 2, y + 2);
            if (p != null && p.isPlayer1Color)
            {
                return true;
            }
        }

        return false;
    }

    private static bool CheckTopRightPiece(Pieces[,] pieces, int x, int y)
    {
        if (x < 6 && y < 6)
        {
            Pieces p = getPiece(pieces, x + 2, y + 2);
            if (p != null && p.isPlayer1Color)
            {
                return true;
            }
        }

        return false;
    }
}
