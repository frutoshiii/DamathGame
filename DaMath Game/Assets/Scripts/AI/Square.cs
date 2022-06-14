using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Operation { None, Addition, Subtraction, Multiplication, Division };

public class Square : MonoBehaviour
{


    public Operation operation;
    public Operation topRight;
    public Operation topLeft;
    public Operation bottomRight;
    public Operation bottomLeft;
}
