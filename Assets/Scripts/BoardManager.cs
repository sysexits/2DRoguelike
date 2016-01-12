using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Roguelike
{
    public class BoardManager : MonoBehaviour
    {
        public int rows = 16;
        public int columns = 16;

        public static float SQUARESIZE_PER_UNIT = 0.4f;

        // Prefabs of floors
        public GameObject[] floorTiles;
        
        // Prefabs of rock floors
        public GameObject[] rockFloorTiles;

        // Prefabs of cliff tiles
        // N1, N2, NE1, NE2, E1, E2, ..., NW1, NW2
        public GameObject[] cliffTiles;

        public GameObject potionTiles;

        // Prefab of an obstacle
        public GameObject blockTiles;
        
        private Transform boardHolder = null;
        private Transform floorHolder = null;
        private Transform cliffHolder = null;
        private Transform borderColliderHolder = null;
        private Transform playerHolder = null;
        private Transform blockHolder = null;
        private Transform potionHolder = null;

        private Player player = null;

        private string currentStageHash = null;

        private PlayerSpawnDir nextSpawnDir;

        private void instantiateAndAdd(GameObject objToClone, int posX, int posY, Transform objParent)
        {
            instantiateAndAdd(objToClone, posX, posY, 0, objParent);
        }
        private void instantiateAndAdd(GameObject objToClone, int posX, int posY, int posZ, Transform objParent)
        {
            GameObject tile = Instantiate(
                objToClone,
                new Vector3(posX, posY, posZ),
                Quaternion.identity
            ) as GameObject;
            tile.transform.SetParent(objParent);
        }

        private enum PlayerSpawnDir
        {
            SPAWN_EAST, SPAWN_NORTH, SPAWN_WEST, SPAWN_SOUTH, SPAWN_NONE
        }

        void generateMapAndPlayer(string mapString, int HPStatus, PlayerSpawnDir spawnDir)
        {
            string[] arrayMap = mapString.Split(',');
            int northGate = -111, southGate = -111, eastGate = -111, westGate = -111;
            for (int i = 0; i < columns; i++)
            {
                if (arrayMap[i] == "N")
                {
                    northGate = i;
                    break;
                }
            }
            for (int i = 0; i < columns; i++)
            {
                if (arrayMap[columns*(rows-1) + i] == "S")
                {
                    southGate = i;
                    break;
                }
            }
            for (int i = 0; i < rows; i++)
            {
                if (arrayMap[columns - 1 + columns * (rows - 1 - i)] == "E")
                {
                    eastGate = i;
                    break;
                }
            }
            for (int i = 0; i < rows; i++)
            {
                if (arrayMap[columns * (rows - 1 - i)] == "W")
                {
                    westGate = i;
                    break;
                }
            }

            // floor generation
            for (int x = -2; x < columns + 2; x++)
            {
                for (int y = -3; y < rows + 3; y++)
                {
                    int tileIndex = 0;
                    if (x<=0 || y<=0 || x>=columns-1 || y>=rows-1)
                    {
                        tileIndex = 9;
                    }
                    else if (x == 1)
                    {
                        if (y == 1) tileIndex = 6;
                        else if (y == rows - 2) tileIndex = 0;
                        else tileIndex = 3;
                    }
                    else if (x == columns - 2)
                    {
                        if (y == 1) tileIndex = 8;
                        else if (y == rows - 2) tileIndex = 2;
                        else tileIndex = 5;
                    }
                    else
                    {
                        if (y == 1) tileIndex = 7;
                        else if (y == rows - 2) tileIndex = 1;
                        else tileIndex = 4;
                    }

                    instantiateAndAdd(floorTiles[tileIndex], x, y, floorHolder);
                }
            }

            // cliff generation
            List<Vector3> rockTilePositions = new List<Vector3>();
            List<Vector3> rockPositions = new List<Vector3>();

            // cliff, SW side of the map
            instantiateAndAdd(cliffTiles[3], -1, -1, cliffHolder);
            for (int y = -3; y <= -2; y++)
                for (int x = -2; x <= 0; x++)
                    rockTilePositions.Add(new Vector3(x, y, 0));
            for (int y = -1; y <= 0; y++)
                rockTilePositions.Add(new Vector3(-2, y, 0));

            // cliff, SE side of the map
            instantiateAndAdd(cliffTiles[15], columns, -1, cliffHolder);
            for (int y = -3; y <= -2; y++)
                for (int x = columns-1; x<=columns+1; x++)
                    rockTilePositions.Add(new Vector3(x, y, 0));
            for (int y = -1; y <= 0; y++)
                rockTilePositions.Add(new Vector3((columns + 1), y, 0));

            // cliff, NW side of the map
            instantiateAndAdd(cliffTiles[7], -1, rows + 1, cliffHolder);
            for (int x = -2; x <= 0; x++)
                rockTilePositions.Add(new Vector3(x, rows + 2, 0));
            for (int y = rows-1; y <= rows + 1; y++)
                rockTilePositions.Add(new Vector3(-2, y, 0));

            // cliff, NE side of the map
            instantiateAndAdd(cliffTiles[11], columns, rows + 1, cliffHolder);
            for (int x = columns - 1; x <= columns + 1; x++)
                rockTilePositions.Add(new Vector3(x, rows + 2, 0));
            for (int y = rows-1; y <= rows + 1; y++)
                rockTilePositions.Add(new Vector3((columns + 1), y, 0));

            // cliff, south side
            for (int x = 1; x < columns - 1; x++)
            {
                if (x == southGate - 2)
                {
                    instantiateAndAdd(cliffTiles[2], x, -1, cliffHolder);
                    for (int y = -3; y <= -2; y++)
                        instantiateAndAdd(cliffTiles[4 + Random.Range(0, 2)], x, y, cliffHolder);
                }
                else if (x == southGate + 2)
                {
                    instantiateAndAdd(cliffTiles[14], x, -1, cliffHolder);
                    for (int y = -3; y <= -2; y++)
                        instantiateAndAdd(cliffTiles[12 + Random.Range(0, 2)], x, y, cliffHolder);
                }
                else if (Mathf.Abs(x - southGate) >= 3)
                {
                    instantiateAndAdd(cliffTiles[Random.Range(0, 2)], x, -1, cliffHolder);
                    for (int y = -3; y <= -2; y++)
                        rockTilePositions.Add(new Vector3(x, y, 0));
                }
            }

            // cliff, north side
            for (int x = 1; x < columns - 1; x++)
            {
                if (x == northGate - 2)
                {
                    instantiateAndAdd(cliffTiles[6], x, rows + 1, cliffHolder);
                    instantiateAndAdd(cliffTiles[4 + Random.Range(0, 2)], x, rows + 2, cliffHolder);
                }
                else if (x == northGate + 2)
                {
                    instantiateAndAdd(cliffTiles[10], x, rows + 1, cliffHolder);
                    instantiateAndAdd(cliffTiles[12 + Random.Range(0, 2)], x, rows + 2, cliffHolder);
                }
                else if (Mathf.Abs(x - northGate) >= 3)
                {
                    instantiateAndAdd(cliffTiles[8 + Random.Range(0, 2)], x, rows + 1, cliffHolder);
                    rockTilePositions.Add(new Vector3(x, rows + 2, 0));
                }
            }

            // cliff, west side
            for (int y = 1; y <= rows - 1; y++)
            {
                if (y == westGate + 3)
                {
                    instantiateAndAdd(cliffTiles[6], -1, y, cliffHolder);
                    instantiateAndAdd(cliffTiles[8 + Random.Range(0, 2)], -2, y, cliffHolder);
                }
                else if (y == westGate - 1)
                {
                    instantiateAndAdd(cliffTiles[2], -1, y, cliffHolder);
                    instantiateAndAdd(cliffTiles[Random.Range(0, 2)], -2, y, cliffHolder);
                }
                else if (Mathf.Abs(y - (westGate + 1)) >= 3)
                {
                    instantiateAndAdd(cliffTiles[4 + Random.Range(0, 2)], -1, y, cliffHolder);
                    rockTilePositions.Add(new Vector3(-2, y, 0));
                }
            }

            // cliff, east side
            for (int y = 1; y <= rows - 1; y++)
            {
                if (y == eastGate + 3)
                {
                    instantiateAndAdd(cliffTiles[10], columns, y, cliffHolder);
                    instantiateAndAdd(cliffTiles[8 + Random.Range(0, 2)], columns + 1, y, cliffHolder);
                }
                else if (y == eastGate - 1)
                {
                    instantiateAndAdd(cliffTiles[14], columns, y, cliffHolder);
                    instantiateAndAdd(cliffTiles[Random.Range(0, 2)], columns + 1, y, cliffHolder);
                }
                else if (Mathf.Abs(y - (eastGate + 1)) >= 3)
                {
                    instantiateAndAdd(cliffTiles[12 + Random.Range(0, 2)], columns, y, cliffHolder);
                    rockTilePositions.Add(new Vector3(columns + 1, y, 0));
                }
            }

            // random generation of rock tiles
            foreach (Vector3 pos in rockTilePositions)
            {
                instantiateAndAdd(
                    rockFloorTiles[Random.Range(0, rockFloorTiles.Length)],
                    Mathf.FloorToInt(pos.x),
                    Mathf.FloorToInt(pos.y),
                    cliffHolder
                );
            }

            // box colliders for the borders
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (arrayMap[x + (rows - 1 - y) * columns] == "#")
                    {
                        Transform border = new GameObject("collider").transform;
                        border.SetParent(borderColliderHolder);
                        border.Translate(x, y, 0);
                        border.gameObject.layer = LayerMask.NameToLayer("Object");

                        border.gameObject.AddComponent<BoxCollider2D>();
                    }
                }
            }
            if (northGate >= 1 && northGate < columns - 1)
            {
                for (int x = northGate-1; x <= northGate+1; x += 2)
                {
                    Transform border = new GameObject("collider").transform;
                    border.SetParent(borderColliderHolder);
                    border.Translate(x, rows + 1f, 0);
                    border.localScale += new Vector3(0, 2.0f, 0);
                    border.gameObject.layer = LayerMask.NameToLayer("Object");

                    border.gameObject.AddComponent<BoxCollider2D>();
                }
            }
            if (southGate >= 1 && southGate < columns - 1)
            {
                for (int x = southGate - 1; x <= southGate + 1; x += 2)
                {
                    Transform border = new GameObject("collider").transform;
                    border.SetParent(borderColliderHolder);
                    border.Translate(x, -2f, 0);
                    border.localScale += new Vector3(0, 2.0f, 0);
                    border.gameObject.layer = LayerMask.NameToLayer("Object");

                    border.gameObject.AddComponent<BoxCollider2D>();
                }
            }
            if (eastGate >= 1 && eastGate < rows - 1)
            {
                for (int y = eastGate - 1; y <= eastGate + 1; y += 2)
                {
                    Transform border = new GameObject("collider").transform;
                    border.SetParent(borderColliderHolder);
                    border.Translate(columns + 0.5f, y, 0);
                    border.localScale += new Vector3(1.0f, 0, 0);
                    border.gameObject.layer = LayerMask.NameToLayer("Object");

                    border.gameObject.AddComponent<BoxCollider2D>();
                }
            }
            if (westGate >= 1 && westGate < rows - 1)
            {
                for (int y = westGate - 1; y <= westGate + 1; y += 2)
                {
                    Transform border = new GameObject("collider").transform;
                    border.SetParent(borderColliderHolder);
                    border.Translate(-1.5f, y, 0);
                    border.localScale += new Vector3(1.0f, 0, 0);
                    border.gameObject.layer = LayerMask.NameToLayer("Object");

                    border.gameObject.AddComponent<BoxCollider2D>();
                }
            }

            // block generation
            for (int y = rows - 1; y >= 0; y--)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (arrayMap[x + (rows - 1 - y) * columns] == "b")
                    {
                        instantiateAndAdd(blockTiles, x, y, 0, blockHolder);
                    } else if (arrayMap[x + (rows - 1 - y) * columns] == "p")
                    {
                        instantiateAndAdd(potionTiles, x, y, 0, potionHolder);
                    }
                }
            }

            // player generation
            int playerX = 0;
            int playerY = 0;
            switch (spawnDir)
            {
                case PlayerSpawnDir.SPAWN_EAST:
                    playerX = columns - 2;
                    playerY = eastGate;
                    break;
                case PlayerSpawnDir.SPAWN_NORTH:
                    playerX = northGate;
                    playerY = rows - 2;
                    break;
                case PlayerSpawnDir.SPAWN_WEST:
                    playerX = 1;
                    playerY = westGate;
                    break;
                case PlayerSpawnDir.SPAWN_SOUTH:
                    playerX = southGate;
                    playerY = 1;
                    break;
                case PlayerSpawnDir.SPAWN_NONE:
                    bool playerPlaced = false;
                    bool hasEmptySpace = false;
                    for (int y = rows - 1; y >= 0; y--)
                    {
                        for (int x = 0; x < columns; x++)
                        {
                            if (arrayMap[x + (rows - 1 - y) * columns] == "u")
                            {
                                if (!playerPlaced)
                                {
                                    playerPlaced = true;
                                    playerX = x;
                                    playerY = y;
                                }
                            }
                            else if (arrayMap[x + (rows - 1 - y) * columns] == "f")
                            {
                                hasEmptySpace = true;
                            }
                        }
                    }
                    while (!playerPlaced && hasEmptySpace)
                    {
                        playerX = Random.Range(1, columns - 1);
                        playerY = Random.Range(1, columns - 1);
                        if (arrayMap[playerX + (rows - 1 - playerY) * columns] == "f")
                        {
                            playerPlaced = true;
                        }
                    }
                    break;
            }
            if (player == null)
            {
                player = ObjectFactory.createPlayer(playerX, playerY, HPStatus);
                player.transform.SetParent(playerHolder);
            }
            else
            {
                player.Initialize(playerX, playerY, HPStatus);
            }

            Debug.Log("player: (" + playerX + "," + playerY + ")");
            Debug.Log("NEWS gate:" + northGate + "," + eastGate + "," + westGate + "," + southGate);
        }

        void BoardHolderInit()
        {
            if (boardHolder == null)
            {
                boardHolder = new GameObject("Board").transform;
                boardHolder.localScale += new Vector3(1.0f / SQUARESIZE_PER_UNIT - 1.0f,
                    1.0f / SQUARESIZE_PER_UNIT - 1.0f,
                    0.0f);
                boardHolder.Translate(new Vector3(
                    (-rows / 2.0f + 0.5f) * SQUARESIZE_PER_UNIT,
                    (-columns / 2.0f + 0.5f) * SQUARESIZE_PER_UNIT,
                    0
                ));
            }
            if (floorHolder == null)
            {
                floorHolder = new GameObject("Floors").transform;
                floorHolder.SetParent(boardHolder);
            }
            if (cliffHolder == null)
            {
                cliffHolder = new GameObject("Cliffs").transform;
                cliffHolder.SetParent(boardHolder);
            }
            if (borderColliderHolder == null)
            {
                borderColliderHolder = new GameObject("BorderColliders").transform;
                borderColliderHolder.SetParent(boardHolder);
            }
            if (blockHolder == null)
            {
                blockHolder = new GameObject("Blocks").transform;
                blockHolder.SetParent(boardHolder);
            }
            if (playerHolder == null)
            {
                playerHolder = new GameObject("Player").transform;
                playerHolder.SetParent(boardHolder);
            }
            if (potionHolder == null)
            {
                potionHolder = new GameObject("Potion").transform;
                potionHolder.SetParent(boardHolder);
            }
        }

        void BoardHolderClear()
        {
            Transform[] lstChildren = floorHolder.GetComponentsInChildren<Transform>();
            if (lstChildren != null)
            {
                foreach (Transform t in lstChildren)
                    Destroy(t.gameObject);
            }
            Destroy(floorHolder);
            floorHolder = null;
            lstChildren = cliffHolder.GetComponentsInChildren<Transform>();
            if (lstChildren != null)
            {
                foreach (Transform t in lstChildren)
                    Destroy(t.gameObject);
            }
            Destroy(cliffHolder);
            cliffHolder = null;
            lstChildren = borderColliderHolder.GetComponentsInChildren<Transform>();
            if (lstChildren != null)
            {
                foreach (Transform t in lstChildren)
                    Destroy(t.gameObject);
            }
            Destroy(borderColliderHolder);
            borderColliderHolder = null;
            lstChildren = potionHolder.GetComponentsInChildren<Transform>();
            if (lstChildren != null)
            {
                foreach (Transform t in lstChildren)
                    Destroy(t.gameObject);
            }
            Destroy(potionHolder);
            potionHolder = null;
            lstChildren = blockHolder.GetComponentsInChildren<Transform>();
            if (lstChildren != null)
            {
                foreach (Transform t in lstChildren)
                    Destroy(t.gameObject);
            }
            Destroy(blockHolder);
            blockHolder = null;
        }

        void BoardSetup()
        {
            Hashtable data = new Hashtable();
            data.Add("username", SystemInfo.deviceUniqueIdentifier);

            Debug.Log("Send:");
            foreach (string str in data.Keys)
            {
                Debug.Log(str + ": " + data[str]);
            }

            HTTP.Request req = new HTTP.Request("post", "http://143.248.139.70:8000/login", data);
            req.Send((request) =>
            {
                Hashtable result = request.response.Object;
                Debug.Log("Result");
                foreach (string str in result.Keys)
                {
                    Debug.Log(str + ": " + result[str]);
                }
                if (result == null)
                {
                    Debug.LogWarning("Could not parse JSON response!");
                    return;
                }
                else
                {
                    // Receive String from server and generate room
                    Hashtable hashmap = (Hashtable)JSON.JsonDecode(request.response.Text);
                    string mapString = (string) hashmap["map"];
                    int rows = (int)hashmap["row"];
                    int columns = (int)hashmap["column"];
                    int hp = (int)hashmap["status"];
                    currentStageHash = (string)hashmap["hash"];
                    generateMapAndPlayer(mapString, hp, PlayerSpawnDir.SPAWN_NONE);
                }
            });
        }

        // Use this for initialization
        void Start()
        {
            BoardHolderInit();
            BoardSetup();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        public void gotoNextStage(Player.Direction dir)
        {
            switch (dir)
            {
                case Player.Direction.EAST:
                    nextSpawnDir = PlayerSpawnDir.SPAWN_WEST;
                    break;
                case Player.Direction.WEST:
                    nextSpawnDir = PlayerSpawnDir.SPAWN_EAST;
                    break;
                case Player.Direction.SOUTH:
                    nextSpawnDir = PlayerSpawnDir.SPAWN_NORTH;
                    break;
                case Player.Direction.NORTH:
                    nextSpawnDir = PlayerSpawnDir.SPAWN_SOUTH;
                    break;
            }

            Hashtable data = new Hashtable();
            data.Add("entrance", ((int)dir).ToString());
            data.Add("hash", currentStageHash);
            data.Add("username", SystemInfo.deviceUniqueIdentifier);

            Debug.Log("Send:");
            foreach (string str in data.Keys)
            {
                Debug.Log(str + ": " + data[str]);
            }

            HTTP.Request req = new HTTP.Request("post", "http://143.248.139.70:8000/randomMapGenerator", data);
            req.Send((request) =>
            {
                Hashtable result = request.response.Object;
                Debug.Log("Result");
                foreach (string str in result.Keys)
                {
                    Debug.Log(str + ": " + result[str]);
                }
                if (result == null)
                {
                    Debug.LogWarning("Could not parse JSON response!");
                    return;
                }
                else
                {
                    // Receive String from server and generate room
                    Hashtable hashmap = (Hashtable)JSON.JsonDecode(request.response.Text);
                    string mapString = (string)hashmap["map"];
                    int rows = (int)hashmap["row"];
                    int columns = (int)hashmap["column"];
                    int hp = (int)hashmap["status"];
                    currentStageHash = (string)hashmap["hash"];

                    BoardHolderClear();
                    BoardHolderInit();
                    Debug.Log("next spawn dir.:" + nextSpawnDir.ToString());
                    generateMapAndPlayer(mapString, hp, nextSpawnDir);
                }
            });
        }
    }
}