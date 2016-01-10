using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Roguelike
{
    public class BoardManager : MonoBehaviour
    {
        public int rows = 16;
        public int columns = 16;

        public GameObject[] exits;  // Prefabs to spawn for exits
        public GameObject[] enemyTiles; // Array of enemy prefabs
        public GameObject[] outerWallTiles; // Array of outer tile prefabs

        private Transform boardHolder; // A variable to store a reference to the transform of our Board object.
        private List<Vector3> gridPositions = new List<Vector3>();   // A list of possible locations to place tiles.

        void InitializeList()
        {
            gridPositions.Clear();
            for (int y = 0; y < this.rows; y++)
            {
                for (int x = 0; x < this.columns; x++)
                {
                    gridPositions.Add(new Vector3(x, y, 0f));
                }
            }
        }

        void BoardSetup()
        {
            boardHolder = new GameObject("Board").transform;
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
                }
            });
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