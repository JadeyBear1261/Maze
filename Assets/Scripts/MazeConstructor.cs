using UnityEngine;


public class MazeConstructor : MonoBehaviour
{
    public bool showDebug;  //Toggles maze data in UI
    public float hallWidth{ get; private set; }  // Used for placing player and treasure in middle of hallway
    public int goalRow{ get; private set; }     // References for placement of treasure and Scary Man
    public int goalCol{ get; private set; }
    public Node[,] graph;

    // 4 Material parts for maze textures. Private but accessible from inspector
    [SerializeField] private Material mazeMat1;
    [SerializeField] private Material mazeMat2;
    [SerializeField] private Material startMat;
    [SerializeField] private Material treasureMat;

    public float placementThreshold = 0.1f;   // chance of empty space

    private MazeMeshGenerator meshGenerator;
    public int[,] data
    {
        get; private set;
    }

    void Awake()
    {
        meshGenerator = new MazeMeshGenerator();
        hallWidth = meshGenerator.width;    //set hallwidth
        // default to walls surrounding a single empty cell
        data = new int[,]
        {
            {1, 1, 1},
            {1, 0, 1},
            {1, 1, 1}
        };
    }

    void OnGUI() //for debugging the maze - visual confirmation the generation works
    {
        if (!showDebug)
            return;

        int[,] maze = data;
        int rMax = maze.GetUpperBound(0); 
        int cMax = maze.GetUpperBound(1);

        string msg = "";

        for (int i = rMax; i >= 0; i--)
        {
            for (int j = 0; j <= cMax; j++)
                msg += maze[i, j] == 0 ? "...." : "==";
            msg += "\n";
        }

        GUI.Label(new Rect(20, 20, 500, 500), msg);
    }


    public void GenerateNewMaze(int sizeRows, int sizeCols, TriggerEventHandler treasureCallback) //Size method called in GameController and treasure collision callback
    {
        
        DisposeOldMaze();  

        if (sizeRows % 2 == 0 && sizeCols % 2 == 0)
            Debug.LogError("Odd numbers work better for dungeon size."); // checking for even or odd numbers and send console note

        data = FromDimensions(sizeRows, sizeCols);

        goalRow = data.GetUpperBound(0) - 1;    // Both last row and column from maze data but minus 1 for wall
        goalCol = data.GetUpperBound(1) - 1;

        graph = new Node[sizeRows,sizeCols];    // New Node for each cell

        for (int i = 0; i < sizeRows; i++)        
            for (int j = 0; j < sizeCols; j++)            
                graph[i, j] = data[i,j] == 0 ? new Node(i, j, true) : new Node(i, j, false);

        DisplayMaze();
        PlaceGoal(treasureCallback); 
    }

    public void DisposeOldMaze()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Generated");
        foreach (GameObject go in objects) {
            Destroy(go);
        }
    }

    public int[,] FromDimensions(int sizeRows, int sizeCols) //Creates 2D array to the specified rows and cols
    {
        int[,] maze = new int[sizeRows, sizeCols];
        int rMax = maze.GetUpperBound(0); 
        int cMax = maze.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)        //Upper indices 
            for (int j = 0; j <= cMax; j++)            
                if (i == 0 || j == 0 || i == rMax || j == cMax)         //Checking for maze boundaries - must be wall       
                    maze[i, j] = 1;                                    
                else if (i % 2 == 0 && j % 2 == 0 && Random.value > placementThreshold)     //Whether non-boundary spaces should be wall or corridor                                
                {
                    maze[i, j] = 1;

                    int a = Random.value < .5 ? 0 : (Random.value < .5 ? -1 : 1); //short hand ternary statement
                    int b = a != 0 ? 0 : (Random.value < .5 ? -1 : 1);
                    maze[i+a, j+b] = 1;
                }  
        return maze;
    }

    

    private void DisplayMaze()
    {
        GameObject go = new GameObject();
        go.transform.position = Vector3.zero;
        go.name = "Procedural Maze";
        go.tag = "Generated";

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = meshGenerator.FromData(data);
        
        MeshCollider mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mf.mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.materials = new Material[2] {mazeMat1, mazeMat2};
    }


    private void PlaceGoal(TriggerEventHandler treasureCallback)    // Goal
    {            
        GameObject treasure = GameObject.CreatePrimitive(PrimitiveType.Cube);
        treasure.transform.position = new Vector3(goalCol * hallWidth, .5f, goalRow * hallWidth);
        treasure.name = "Treasure";
        treasure.tag = "Generated";

        treasure.GetComponent<BoxCollider>().isTrigger = true;
        treasure.GetComponent<MeshRenderer>().sharedMaterial = treasureMat;

        TriggerEventRouter tc = treasure.AddComponent<TriggerEventRouter>();
        tc.callback = treasureCallback;
    }
}