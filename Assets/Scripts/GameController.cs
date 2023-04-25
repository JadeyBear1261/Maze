using System;
using UnityEngine;

[RequireComponent(typeof(MazeConstructor))]           

public class GameController : MonoBehaviour
{
    private MazeConstructor constructor;

    [SerializeField] private int rows; //How big the maze grid will be, Serialize field to set in editor
    [SerializeField] private int cols;

    void Awake()
    {
        constructor = GetComponent<MazeConstructor>();
    }
    
    void Start()
    {
        constructor.GenerateNewMaze(rows, cols);
    }
}