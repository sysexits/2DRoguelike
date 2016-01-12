using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;

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
        
        public enum holderID
        {
            BOARD, FLOOR, CLIFF, BOARD_COLLIDER, PLAYER, BLOCK, POTION, WEAPON, PEER,
            START = BOARD, END = PEER
        }
        public string[] holderNames =
        {  
            "Board", "Floors", "Cliffs", "Colliders", "Player", "Blocks", "Potions", "Weapons", "Peer"
        };

        // list of game objects which hold game objects for organization.
        // in order to create a new holder in this code, add a new member into holderID and holderNames
        private Transform[] holders = new Transform[(int)(holderID.END) + 1];

        private Player player = null;

        // hash value of the current stage
        public string currentStageHash
        {
            get; private set;
        }

        // enum of items
        public enum itemID
        {
            POTION1, POTION2, WEAPON,
            START = POTION1, END = WEAPON
        }

        private PlayerSpawnDir nextSpawnDir;

        private UDPListener listener;

        private List<string> peerIPList;
        private List<GameObject> peerUDPClients;
        private List<PeerPlayer> peerPlayers;

        public List<Hashtable> actionQueue;

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

        public void generateEnemy(int posx, int posy, string username)
        {
            PeerPlayer peer = ObjectFactory.createPeer(posx, posy);
            peer.username = username;
            peer.transform.SetParent(holders[(int)holderID.PEER]);

            peerPlayers.Add(peer);
        }

        private enum PlayerSpawnDir
        {
            SPAWN_EAST, SPAWN_NORTH, SPAWN_WEST, SPAWN_SOUTH, SPAWN_NONE
        }

        void generateMapAndPlayer(Hashtable mapInfo, PlayerSpawnDir spawnDir)
        {
            // get map information from the hash table
            string mapString = (string)mapInfo["map"];
            rows = (int)mapInfo["row"];
            columns = (int)mapInfo["column"];
            currentStageHash = (string)mapInfo["hash"];

            int HPStatus, APStatus;
            if (player != null)
            {
                HPStatus = player.m_HP;
                APStatus = player.m_AP;
            }
            else
            {
                HPStatus = (int)mapInfo["hp"];
                APStatus = (int)mapInfo["ap"];
            }

            string[] arrayMap = mapString.Split(',');

            // get item information from the hash table
            int[] itemCounts = new int[(int)(itemID.END + 1)];
            int[] itemValues = new int[(int)(itemID.END + 1)];
            for (int i = 0; i <= (int)(itemID.END); i++)
            {
                if (mapInfo.ContainsKey(i.ToString()))
                {
                    Hashtable itemTable = (Hashtable)mapInfo[i.ToString()];
                    itemCounts[i] = (int)itemTable["items"];
                    itemValues[i] = (int)itemTable["value"];
                }
                else
                {
                    itemCounts[i] = 0;
                }
            }

            // find the gates
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

                    instantiateAndAdd(floorTiles[tileIndex], x, y, holders[(int)(holderID.FLOOR)]);
                }
            }

            // cliff generation
            List<Vector3> rockTilePositions = new List<Vector3>();
            List<Vector3> rockPositions = new List<Vector3>();

            // cliff, SW side of the map
            instantiateAndAdd(cliffTiles[3], -1, -1, holders[(int)(holderID.CLIFF)]);
            for (int y = -3; y <= -2; y++)
                for (int x = -2; x <= 0; x++)
                    rockTilePositions.Add(new Vector3(x, y, 0));
            for (int y = -1; y <= 0; y++)
                rockTilePositions.Add(new Vector3(-2, y, 0));

            // cliff, SE side of the map
            instantiateAndAdd(cliffTiles[15], columns, -1, holders[(int)(holderID.CLIFF)]);
            for (int y = -3; y <= -2; y++)
                for (int x = columns-1; x<=columns+1; x++)
                    rockTilePositions.Add(new Vector3(x, y, 0));
            for (int y = -1; y <= 0; y++)
                rockTilePositions.Add(new Vector3((columns + 1), y, 0));

            // cliff, NW side of the map
            instantiateAndAdd(cliffTiles[7], -1, rows + 1, holders[(int)(holderID.CLIFF)]);
            for (int x = -2; x <= 0; x++)
                rockTilePositions.Add(new Vector3(x, rows + 2, 0));
            for (int y = rows-1; y <= rows + 1; y++)
                rockTilePositions.Add(new Vector3(-2, y, 0));

            // cliff, NE side of the map
            instantiateAndAdd(cliffTiles[11], columns, rows + 1, holders[(int)(holderID.CLIFF)]);
            for (int x = columns - 1; x <= columns + 1; x++)
                rockTilePositions.Add(new Vector3(x, rows + 2, 0));
            for (int y = rows-1; y <= rows + 1; y++)
                rockTilePositions.Add(new Vector3((columns + 1), y, 0));

            // cliff, south side
            for (int x = 1; x < columns - 1; x++)
            {
                if (x == southGate - 2)
                {
                    instantiateAndAdd(cliffTiles[2], x, -1, holders[(int)(holderID.CLIFF)]);
                    for (int y = -3; y <= -2; y++)
                        instantiateAndAdd(cliffTiles[4 + Random.Range(0, 2)], x, y, holders[(int)(holderID.CLIFF)]);
                }
                else if (x == southGate + 2)
                {
                    instantiateAndAdd(cliffTiles[14], x, -1, holders[(int)(holderID.CLIFF)]);
                    for (int y = -3; y <= -2; y++)
                        instantiateAndAdd(cliffTiles[12 + Random.Range(0, 2)], x, y, holders[(int)(holderID.CLIFF)]);
                }
                else if (Mathf.Abs(x - southGate) >= 3)
                {
                    instantiateAndAdd(cliffTiles[Random.Range(0, 2)], x, -1, holders[(int)(holderID.CLIFF)]);
                    for (int y = -3; y <= -2; y++)
                        rockTilePositions.Add(new Vector3(x, y, 0));
                }
            }

            // cliff, north side
            for (int x = 1; x < columns - 1; x++)
            {
                if (x == northGate - 2)
                {
                    instantiateAndAdd(cliffTiles[6], x, rows + 1, holders[(int)(holderID.CLIFF)]);
                    instantiateAndAdd(cliffTiles[4 + Random.Range(0, 2)], x, rows + 2, holders[(int)(holderID.CLIFF)]);
                }
                else if (x == northGate + 2)
                {
                    instantiateAndAdd(cliffTiles[10], x, rows + 1, holders[(int)(holderID.CLIFF)]);
                    instantiateAndAdd(cliffTiles[12 + Random.Range(0, 2)], x, rows + 2, holders[(int)(holderID.CLIFF)]);
                }
                else if (Mathf.Abs(x - northGate) >= 3)
                {
                    instantiateAndAdd(cliffTiles[8 + Random.Range(0, 2)], x, rows + 1, holders[(int)(holderID.CLIFF)]);
                    rockTilePositions.Add(new Vector3(x, rows + 2, 0));
                }
            }

            // cliff, west side
            for (int y = 1; y <= rows - 1; y++)
            {
                if (y == westGate + 3)
                {
                    instantiateAndAdd(cliffTiles[6], -1, y, holders[(int)(holderID.CLIFF)]);
                    instantiateAndAdd(cliffTiles[8 + Random.Range(0, 2)], -2, y, holders[(int)(holderID.CLIFF)]);
                }
                else if (y == westGate - 1)
                {
                    instantiateAndAdd(cliffTiles[2], -1, y, holders[(int)(holderID.CLIFF)]);
                    instantiateAndAdd(cliffTiles[Random.Range(0, 2)], -2, y, holders[(int)(holderID.CLIFF)]);
                }
                else if (Mathf.Abs(y - (westGate + 1)) >= 3)
                {
                    instantiateAndAdd(cliffTiles[4 + Random.Range(0, 2)], -1, y, holders[(int)(holderID.CLIFF)]);
                    rockTilePositions.Add(new Vector3(-2, y, 0));
                }
            }

            // cliff, east side
            for (int y = 1; y <= rows - 1; y++)
            {
                if (y == eastGate + 3)
                {
                    instantiateAndAdd(cliffTiles[10], columns, y, holders[(int)(holderID.CLIFF)]);
                    instantiateAndAdd(cliffTiles[8 + Random.Range(0, 2)], columns + 1, y, holders[(int)(holderID.CLIFF)]);
                }
                else if (y == eastGate - 1)
                {
                    instantiateAndAdd(cliffTiles[14], columns, y, holders[(int)(holderID.CLIFF)]);
                    instantiateAndAdd(cliffTiles[Random.Range(0, 2)], columns + 1, y, holders[(int)(holderID.CLIFF)]);
                }
                else if (Mathf.Abs(y - (eastGate + 1)) >= 3)
                {
                    instantiateAndAdd(cliffTiles[12 + Random.Range(0, 2)], columns, y, holders[(int)(holderID.CLIFF)]);
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
                    holders[(int)(holderID.CLIFF)]
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
                        border.SetParent(holders[(int)(holderID.BOARD_COLLIDER)]);
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
                    border.SetParent(holders[(int)(holderID.BOARD_COLLIDER)]);
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
                    border.SetParent(holders[(int)(holderID.BOARD_COLLIDER)]);
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
                    border.SetParent(holders[(int)(holderID.BOARD_COLLIDER)]);
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
                    border.SetParent(holders[(int)(holderID.BOARD_COLLIDER)]);
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
                        instantiateAndAdd(blockTiles, x, y, 0, holders[(int)(holderID.BLOCK)]);
                    }
                }
            }

            // item generation
            for (int y = rows - 1; y >= 0; y--)
            {
                for (int x = 0; x < columns; x++)
                {
                    for (int itemIdx = (int)(itemID.START); itemIdx <= (int)(itemID.END); itemIdx++)
                    {
                        if (arrayMap[x + (rows - 1 - y) * columns] == itemIdx.ToString()
                            && itemCounts[itemIdx] > 0)
                        {
                            switch ((itemID)itemIdx)
                            {
                                case itemID.POTION1:
                                    Potion potion1 = ObjectFactory.createPotion(x, y, itemValues[itemIdx], (int)(itemID.POTION1));
                                    potion1.transform.SetParent(holders[(int)(holderID.POTION)]);
                                    break;
                                case itemID.POTION2:
                                    Potion potion2 = ObjectFactory.createPotion(x, y, itemValues[itemIdx], (int)(itemID.POTION2));
                                    potion2.transform.SetParent(holders[(int)(holderID.POTION)]);
                                    break;
                                case itemID.WEAPON:
                                    Weapon weapon = ObjectFactory.createWeapon(x, y, itemValues[itemIdx], (int)(itemID.WEAPON));
                                    weapon.transform.SetParent(holders[(int)(holderID.WEAPON)]);
                                    break;
                            }
                        }
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
                player = ObjectFactory.createPlayer(playerX, playerY, HPStatus, APStatus);
                player.transform.SetParent(holders[(int)(holderID.PLAYER)]);
            }
            else
            {
                player.Initialize(playerX, playerY, HPStatus, APStatus);
            }

            // get peer's ip addresses who are in this area!
            peerIPList.Clear();
            ArrayList listIPs = (ArrayList)mapInfo["ips"];
            foreach (string ip in listIPs)
            {
                if (ip != getMyIP())
                    peerIPList.Add(ip);
            }

            // peer player initialization
            peerUDPClients.Clear();
            foreach (string ip in peerIPList)
            {
                GameObject peer = new GameObject("peer UDP client");
                UDPClient peerClient = peer.AddComponent<UDPClient>();
                peerClient.InitiateSocket(ip, 12346);
                peerUDPClients.Add(peer);

                Hashtable data = new Hashtable();
                data.Add("action", "myinfo");
                data.Add("hash", currentStageHash);
                data.Add("username", SystemInfo.deviceUniqueIdentifier);
                data.Add("ip", getMyIP());
                data.Add("xpos", playerX);
                data.Add("ypos", playerY);
                peerClient.sendJSONObject(data);
            }
        }

        public void sendMove(int playerX, int playerY)
        {
            foreach (GameObject peer in peerUDPClients)
            {
                UDPClient peerClient = peer.GetComponent<UDPClient>();
                
                Hashtable data = new Hashtable();
                data.Add("action", "move");
                data.Add("hash", currentStageHash);
                data.Add("username", SystemInfo.deviceUniqueIdentifier);
                data.Add("ip", getMyIP());
                data.Add("xpos", playerX);
                data.Add("ypos", playerY);
                peerClient.sendJSONObject(data);
            }
            UDPClient serverClient = GetComponent<UDPClient>();

            Hashtable data1 = new Hashtable();
            data1.Add("action", "move");
            data1.Add("hash", currentStageHash);
            data1.Add("username", SystemInfo.deviceUniqueIdentifier);
            data1.Add("ip", getMyIP());
            data1.Add("xpos", playerX);
            data1.Add("ypos", playerY);
            serverClient.sendJSONObject(data1);
        }

        void BoardHolderInit()
        {
            Transform board_holder = holders[(int)(holderID.BOARD)];
            if (board_holder == null)
            {
                board_holder = new GameObject(holderNames[(int)(holderID.BOARD)]).transform;
                board_holder.localScale += new Vector3(1.0f / SQUARESIZE_PER_UNIT - 1.0f,
                    1.0f / SQUARESIZE_PER_UNIT - 1.0f,
                    0.0f);
                board_holder.Translate(new Vector3(
                    (-rows / 2.0f + 0.5f) * SQUARESIZE_PER_UNIT,
                    (-columns / 2.0f + 0.5f) * SQUARESIZE_PER_UNIT,
                    0
                ));
                holders[(int)(holderID.BOARD)] = board_holder;
            }
            for (int i = (int)holderID.START; i <= (int)holderID.END; i++)
            {
                // do not add new board holder into the board holder
                if (i == (int)holderID.BOARD)
                    continue;
                
                if (holders[i] == null)
                {
                    holders[i] = new GameObject(holderNames[i]).transform;
                    holders[i].SetParent(board_holder);
                }
            }
        }

        void BoardHolderClear()
        {
            for (int i = (int)holderID.START; i <= (int)holderID.END; i++)
            {
                // do not destroy board holder object and player holder object
                if (i == (int)holderID.BOARD || i == (int)holderID.PLAYER)
                    continue;

                Transform[] lstChildren = holders[i].GetComponentsInChildren<Transform>();
                if (lstChildren != null)
                {
                    foreach (Transform t in lstChildren)
                        Destroy(t.gameObject);
                }
                Destroy(holders[i]);
                holders[i] = null;
            }
        }

        void BoardSetup()
        {
            Hashtable data = new Hashtable();
            data.Add("username", SystemInfo.deviceUniqueIdentifier);

            string strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            data.Add("ip", addr[addr.Length - 1].ToString());

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
                    if ((string)(result[str].ToString()) == "System.Collections.Hashtable")
                    {
                        Hashtable rr = result[str] as Hashtable;
                        foreach (string strstr in rr.Keys)
                        {
                            Debug.Log(strstr + ": " + rr[strstr]);
                        }
                    }
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
                    generateMapAndPlayer(hashmap, PlayerSpawnDir.SPAWN_NONE);
                }
            });
        }

        // Use this for initialization
        void Start()
        {
            peerIPList = new List<string>();
            peerUDPClients = new List<GameObject>();
            peerPlayers = new List<PeerPlayer>();
            actionQueue = new List<Hashtable>();
            GetComponent<UDPClient>().InitiateSocket("143.248.139.70");

            listener = new UDPListener();

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
            
            if (actionQueue.Count > 0)
            {
                Hashtable data = actionQueue[0];
                switch (data["action"].ToString())
                {
                    case "myinfo":
                        generateEnemy(
                            System.Int32.Parse(data["xpos"].ToString()),
                            System.Int32.Parse(data["ypos"].ToString()),
                            data["username"].ToString()
                        );
                        GameObject peer1 = new GameObject("peer UDP client");
                        UDPClient peerClient = peer1.AddComponent<UDPClient>();
                        peerClient.InitiateSocket(data["ip"].ToString(), 12346);
                        peerUDPClients.Add(peer1);
                        
                        Hashtable data2 = new Hashtable();
                        data2.Add("action", "myinfo2");
                        data2.Add("hash", currentStageHash);
                        data2.Add("username", SystemInfo.deviceUniqueIdentifier);
                        data2.Add("ip", getMyIP());
                        data2.Add("xpos", player.m_posX);
                        data2.Add("ypos", player.m_posY);
                        peerClient.sendJSONObject(data2);

                        break;
                    case "myinfo2":
                        generateEnemy(
                            System.Int32.Parse(data["xpos"].ToString()),
                            System.Int32.Parse(data["ypos"].ToString()),
                            data["username"].ToString()
                        );
                        break;
                    case "move":
                        foreach (PeerPlayer peer in peerPlayers)
                        {
                            Debug.Log("peer username = " + peer.username);
                            if (peer.username == data["username"].ToString())
                            {
                                peer.Move(
                                    System.Int32.Parse(data["xpos"].ToString()),
                                    System.Int32.Parse(data["ypos"].ToString())
                                );
                            }
                        }
                        break;
                }
                actionQueue.Remove(data);
            }
        }

        // when this application is going to end...
        void OnApplicationQuit()
        {
            // send EXIT signal to server
            Hashtable data = new Hashtable();

            string strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            data.Add("ip", addr[addr.Length - 1].ToString());
            data.Add("action", "exit");
            data.Add("hash", currentStageHash);

            GetComponent<UDPClient>().sendJSONObject(data);
        }

        private string getMyIP()
        {
            string strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            return addr[addr.Length - 1].ToString();
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

            string strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            data.Add("ip", addr[addr.Length - 1].ToString());

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
                    if ((string)(result[str].ToString()) == "System.Collections.Hashtable")
                    {
                        Hashtable rr = result[str] as Hashtable;
                        foreach (string strstr in rr.Keys)
                        {
                            Debug.Log(strstr + ": " + rr[strstr]);
                        }
                    }
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
                    BoardHolderClear();
                    BoardHolderInit();
                    generateMapAndPlayer(hashmap, nextSpawnDir);
                }
            });
        }
    }
}