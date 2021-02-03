using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Connection Status")]
    public Text connectionStatus;

    [Header("Login UI Panel")]
    public InputField playerNameInput;
    public GameObject Login_UI_Panel;

    [Header("Game Option UI panel")]
    public GameObject Gameoption_UI_Panel;

    [Header("Create Room UI Panel")]
    public GameObject Create_Room_UI_Panel;
    public InputField RoomNameInputField;

    public InputField MaxPlayerInputField;

    [Header("Inside Room UI Panel")]
    public GameObject Inside_Room_UI_Panel;
    public Text roomInfoText;
    public GameObject playerListPrefab;
    public GameObject playerListContent;
    public GameObject startGameButton;

    [Header("Room List UI Panel")]
    public GameObject RoomLIst_UI_Panel;
    public GameObject roomListEntryprefab;
    public GameObject roomListParentGameObject;

    [Header("Join Room UI Panel")]
    public GameObject JoinRandom_Room_UI_Panel;

    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListGameObjects;
    private Dictionary<int, GameObject> playerListGameObjects;


    #region Unity Methods 
    // Start is called before the first frame update
    void Start()
    {
        ActivatePanel(Login_UI_Panel.name);

        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListGameObjects = new Dictionary<string, GameObject>();

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Update is called once per frame
    void Update()
    {
        connectionStatus.text = "Connection status: " + PhotonNetwork.NetworkClientState;
    }
    #endregion

    #region UI callbacks

    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInput.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("PLayer Name is Empty");
        }
    }

    public void OnRoomCreateButtonClicked()
    {
        string roomName = RoomNameInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(100, 1000);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)int.Parse(MaxPlayerInputField.text);

        PhotonNetwork.CreateRoom(roomName, roomOptions);

    }

    public void OnCancelButtonClicked()
    {
        ActivatePanel(Gameoption_UI_Panel.name);
    }

    public void OnShowRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        ActivatePanel(RoomLIst_UI_Panel.name);
    }

    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        ActivatePanel(Gameoption_UI_Panel.name);
    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnJoinRandomRoomButtonClicked()
    {
        ActivatePanel(JoinRandom_Room_UI_Panel.name);
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnstartGameButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("ARDraw");
        }

    }

    #endregion

    #region Photon Callbacks

    public override void OnConnected()
    {
        Debug.Log("Connected to Internet");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is Connected to Photon : )");
        ActivatePanel(Gameoption_UI_Panel.name);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " is Created ");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel(Inside_Room_UI_Panel.name);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }


        roomInfoText.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + "      " +
                             "Players/MaxPlayer : " +
                             PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                             PhotonNetwork.CurrentRoom.MaxPlayers;

        if(playerListGameObjects == null)
        {
            playerListGameObjects = new Dictionary<int, GameObject>();

        }


        //Instantiate playerList GameObject
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerListGameObject = Instantiate(playerListPrefab);
            playerListGameObject.transform.SetParent(playerListContent.transform);
            playerListGameObject.transform.localScale = Vector3.one;

            playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;

            if(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
            }
            else
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
            }

            playerListGameObjects.Add(player.ActorNumber, playerListGameObject);

        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //Update room info text 
        roomInfoText.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + "      " +
                             "Players/MaxPlayer : " +
                             PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                             PhotonNetwork.CurrentRoom.MaxPlayers;

        GameObject playerListGameObject = Instantiate(playerListPrefab);
        playerListGameObject.transform.SetParent(playerListContent.transform);
        playerListGameObject.transform.localScale = Vector3.one;

        playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = newPlayer.NickName;

        if (newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
        }
        else
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
        }

        playerListGameObjects.Add(newPlayer.ActorNumber, playerListGameObject);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomInfoText.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + "      " +
                                "Players/MaxPlayer : " +
                                PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                                PhotonNetwork.CurrentRoom.MaxPlayers;

        Destroy(playerListGameObjects[otherPlayer.ActorNumber].gameObject);
        playerListGameObjects.Remove(otherPlayer.ActorNumber);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }

    }

    public override void OnLeftRoom()
    {
        ActivatePanel(Gameoption_UI_Panel.name);
        
        foreach(GameObject playerListGameObject in playerListGameObjects.Values)
        {
            Destroy(playerListGameObject);
        }

        playerListGameObjects.Clear();
        playerListGameObjects = null;

    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        CLearRooListView();

        foreach (RoomInfo room in roomList)
        {
            Debug.Log(room.Name);
            if(!room.IsOpen || !room.IsVisible || room.RemovedFromList)
            {
                cachedRoomList.Remove(room.Name);
            }
            else
            {
                //update cached rooom list 
                if (cachedRoomList.ContainsKey(room.Name))
                {
                    cachedRoomList[room.Name] = room;
                }
                //addd a new room to the cached room list 
                else
                {
                    cachedRoomList.Add(room.Name, room);
                }
                
            }
        }

        foreach(RoomInfo room in cachedRoomList.Values)
        {
            GameObject roomListEntryGameObject = Instantiate(roomListEntryprefab);
            roomListEntryGameObject.transform.SetParent(roomListParentGameObject.transform);
            roomListEntryGameObject.transform.localScale = Vector3.one;

            roomListEntryGameObject.transform.Find("RoomNameText").GetComponent<Text>().text = room.Name;
            roomListEntryGameObject.transform.Find("RoomPlayersText").GetComponent<Text>().text = room.PlayerCount + " / " + room.MaxPlayers;
            roomListEntryGameObject.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() => OnJoinedRoomButtonClicked(room.Name));

            roomListGameObjects.Add(room.Name, roomListEntryGameObject);

        }

    }

    public override void OnLeftLobby()
    {
        CLearRooListView();
        cachedRoomList.Clear();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);

        string roomName = "Room " + Random.Range(1000, 10000);

        RoomOptions roomOption = new RoomOptions();
        roomOption.MaxPlayers = 10;
    }

    #endregion

    #region Private Methods

    void OnJoinedRoomButtonClicked(string _roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        PhotonNetwork.JoinRoom(_roomName);
    }

    private void CLearRooListView()
    {
        foreach (var roomListGameObject in roomListGameObjects.Values)
        {
            Destroy(roomListGameObject);
        }

        roomListGameObjects.Clear();

    }

    #endregion



    #region Methods

    public void ActivatePanel(string panelTobeActivated)
    {
        Login_UI_Panel.SetActive(panelTobeActivated.Equals(Login_UI_Panel.name));
        Gameoption_UI_Panel.SetActive(panelTobeActivated.Equals(Gameoption_UI_Panel.name));
        Create_Room_UI_Panel.SetActive(panelTobeActivated.Equals(Create_Room_UI_Panel.name));
        Inside_Room_UI_Panel.SetActive(panelTobeActivated.Equals(Inside_Room_UI_Panel.name));
        RoomLIst_UI_Panel.SetActive(panelTobeActivated.Equals(RoomLIst_UI_Panel.name));
        JoinRandom_Room_UI_Panel.SetActive(panelTobeActivated.Equals(JoinRandom_Room_UI_Panel.name));
    }

    #endregion
     
}













