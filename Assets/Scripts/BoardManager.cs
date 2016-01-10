using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Roguelike
{
    public class BoardManager : MonoBehaviour
    {
        public int rows = 16;
        public int columns = 16;

        public float SQUARESIZE_PER_UNIT = 0.4f;

        // Prefabs of floors
        public GameObject[] floorTiles;

        // Prefabs of rock floors
        public GameObject[] rockFloorTiles;

        // Prefabs of cliff tiles
        // N1, N2, NE1, NE2, E1, E2, ..., NW1, NW2
        public GameObject[] cliffTiles;

        private Transform boardHolder; // A variable to store a reference to the transform of our Board object.

        // for debug!
        public string theMapString;

        private void instantiateAndAdd(GameObject objToClone, int posX, int posY, Transform objParent)
        {
            GameObject tile = Instantiate(
                objToClone,
                new Vector3(posX * SQUARESIZE_PER_UNIT, posY * SQUARESIZE_PER_UNIT, 0),
                Quaternion.identity
            ) as GameObject;
            tile.transform.SetParent(objParent);
        }

        void generateMap(string mapString)
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
                if (arrayMap[columns - 1 + columns * i] == "E")
                {
                    eastGate = i;
                    break;
                }
            }
            for (int i = 0; i < rows; i++)
            {
                if (arrayMap[columns * i] == "W")
                {
                    westGate = i;
                    break;
                }
            }

            // floor generation
            Transform floorHolder = new GameObject("Floors").transform;
            floorHolder.SetParent(boardHolder);

            for (int x = -2; x < columns + 2; x++)
            {
                for (int y = -2; y <= rows + 2; y++)
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
            List<Vector3> rockPositions = new List<Vector3>();
            Transform cliffHolder = new GameObject("Cliffs").transform;
            cliffHolder.SetParent(boardHolder);

            // cliff, SW side of the map
            instantiateAndAdd(cliffTiles[3], -1, -1, cliffHolder);
            for (int x = -2; x <= 0; x++)
                rockPositions.Add(new Vector3(x, -2, 0));
            for (int y = -1; y <= 0; y++)
                rockPositions.Add(new Vector3(-2, y, 0));

            // cliff, SE side of the map
            instantiateAndAdd(cliffTiles[15], columns, -1, cliffHolder);
            for (int x = columns-1; x<=columns+1; x++)
                rockPositions.Add(new Vector3(x, -2, 0));
            for (int y = -1; y <= 0; y++)
                rockPositions.Add(new Vector3((columns + 1), y, 0));

            // cliff, NW side of the map
            instantiateAndAdd(cliffTiles[7], -1, rows + 2, cliffHolder);
            for (int y = rows; y <= rows + 2; y++)
                rockPositions.Add(new Vector3(-2, y, 0));

            // cliff, NE side of the map
            instantiateAndAdd(cliffTiles[11], columns, rows + 2, cliffHolder);
            for (int y = rows; y <= rows + 2; y++)
                rockPositions.Add(new Vector3((columns + 1), y, 0));

            // cliff, south side
            for (int x = 1; x < columns - 1; x++)
            {
                if (x == southGate - 2)
                {
                    instantiateAndAdd(cliffTiles[2], x, -1, cliffHolder);
                    instantiateAndAdd(cliffTiles[4 + Random.Range(0, 2)], x, -2, cliffHolder);
                }
                else if (x == southGate + 2)
                {
                    instantiateAndAdd(cliffTiles[14], x, -1, cliffHolder);
                    instantiateAndAdd(cliffTiles[12 + Random.Range(0, 2)], x, -2, cliffHolder);
                }
                else if (Mathf.Abs(x - southGate) >= 3)
                {
                    instantiateAndAdd(cliffTiles[Random.Range(0, 2)], x, -1, cliffHolder);
                    rockPositions.Add(new Vector3(x, -2, 0));
                }
            }

            // cliff, north side
            for (int x = 1; x < columns - 1; x++)
            {
                if (x == northGate - 2)
                {
                    instantiateAndAdd(cliffTiles[6], x, rows + 2, cliffHolder);
                }
                else if (x == northGate + 2)
                {
                    instantiateAndAdd(cliffTiles[10], x, rows + 2, cliffHolder);
                }
                else if (Mathf.Abs(x - northGate) >= 3)
                {
                    instantiateAndAdd(cliffTiles[8 + Random.Range(0, 2)], x, rows + 2, cliffHolder);
                }
            }

            // cliff, west side
            westGate = 7;
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
                    rockPositions.Add(new Vector3(-2, y, 0));
                }
            }

            // cliff, east side
            eastGate = 7;
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
                    rockPositions.Add(new Vector3(columns + 1, y, 0));
                }
            }

            foreach (Vector3 pos in rockPositions)
            {
                instantiateAndAdd(
                    rockFloorTiles[Random.Range(0, rockFloorTiles.Length)],
                    Mathf.FloorToInt(pos.x),
                    Mathf.FloorToInt(pos.y),
                    cliffHolder
                );
            }
        }

        void BoardSetup()
        {
            boardHolder = new GameObject("Board").transform;
            generateMap(theMapString);

            boardHolder.Translate(new Vector3(
                (-rows/2.0f + 0.5f) * SQUARESIZE_PER_UNIT, 
                (-columns/2.0f + 0.5f) * SQUARESIZE_PER_UNIT,
                0
            ));

            /*
            Hashtable data = new Hashtable();
            data.Add("username", SystemInfo.deviceUniqueIdentifier);

            HTTP.Request req = new HTTP.Request("post", "http://143.248.139.70:8000/randomMapGenerator", data);
            req.Send((request) =>
            {
                Hashtable result = request.response.Object;
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
                    Debug.Log(mapString);
                }
            });
            */
        }

        // Use this for initialization
        void Start()
        {
            BoardSetup();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}