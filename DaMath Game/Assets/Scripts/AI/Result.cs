using System.Collections;
using System.Collections.Generic;

public class Result
{
    public int availableMoves;
    //record if it is a valid and safe move
    public bool left;
    public bool right;
    public bool backLeft;
    public bool backRight;

    //possible highest score that the player can get
    public bool leftHighestScore;
    public bool rightHighestScore;
    public bool backLeftHighestScore;
    public bool backRightHighestScore;

    public int x;
    public int y;

    //Piece to move
    public Pieces piece;

}
