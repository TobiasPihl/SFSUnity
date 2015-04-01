using UnityEngine;
using System.Collections;

//smartfox
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Requests;
using Sfs2X.Entities.Data;

public class SFSHandler : MonoBehaviour {

	SmartFox sfs; //server object
	
	//public string UserName = ""; // stores user name used for login
	
	private string createdRoomName = ""; //temporary room name
	private RoomSettings roomSettings; // setting used for creating 
	
	// Use this for initialization
	void Start () {

	}

	//avoid to use start since we dont want to reset any server properties when switching scenes
	void Awake() {

		//the server object and its variables stays alive through all scenes and should never be resetted
		DontDestroyOnLoad(gameObject);

		//initiate smartfox 
		sfs = new SmartFox();
		sfs.ThreadSafeMode = true;
		
		//add event listeners for events
		sfs.AddEventListener (SFSEvent.CONFIG_LOAD_SUCCESS, OnConfigLoadSuccessHandler);
		sfs.AddEventListener (SFSEvent.CONNECTION, OnConnection);
		sfs.AddEventListener (SFSEvent.LOGIN, OnLogin);
		sfs.AddEventListener (SFSEvent.LOGIN_ERROR, OnLoginError);
		sfs.AddEventListener (SFSEvent.ROOM_ADD, OnRoomCreated);
		sfs.AddEventListener (SFSEvent.ROOM_CREATION_ERROR, OnRoomCreationError);
		sfs.AddEventListener (SFSEvent.ROOM_JOIN, OnJoinRoom);
		sfs.AddEventListener (SFSEvent.ROOM_JOIN_ERROR, OnJoinRoomError);
		sfs.AddEventListener (SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
		sfs.AddEventListener (SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);

		//load config file on startup, and connect on success
		sfs.LoadConfig("sfs-config.xml", true);
	}
	
	// Update is called once per frame
	void Update () {
		sfs.ProcessEvents();
	}

	// Private functions used for event actions and debugging
	//*********************************************************************

	//connect to server if config file successfully loads
	private void OnConfigLoadSuccessHandler(BaseEvent e) {
		Debug.Log("Config file loaded");
	}

	//On successful connection
	private void OnConnection(BaseEvent e) {
		bool success = (bool)e.Params["success"];
		if (success) {
			Debug.Log("Successfully Connected");
		} else {
			Debug.Log("Failed to connect");
		}
	}
	
	private void OnLogin(BaseEvent e) {
		Debug.Log("Logged In: " + e.Params["user"]);
		sfs.InitUDP();
		Application.LoadLevel("GameLobby"); // if login successful, load lobby
	}

	private void OnLoginError(BaseEvent e) {
		Debug.Log("Failed to login");
	}

	//successful room creation
	private void OnRoomCreated(BaseEvent e) {
		Debug.Log("Room created: ");
		joinRoom(createdRoomName); //(auto) join room when created successfully
	}

	//unsuccessful room creation
	private void OnRoomCreationError(BaseEvent e) {
		Debug.Log("Room creation failed: ");
	}              

	//Successfully join room. Load the room gui
	private void OnJoinRoom(BaseEvent e) {
		Debug.Log("Joined room: " + e.Params["room"]);
		Application.LoadLevel("GameRoom"); // if successful, load room GUI
	}

	//fail to join room
	private void OnJoinRoomError(BaseEvent e) {
		Debug.Log(e.Params["errorMessage"]);
	}

	//When leaving room, the lobby GUI is loaded
	private void OnUserExitRoom(BaseEvent e) {
		Debug.Log ("Left room: " + e.Params["room"]);
		Application.LoadLevel("GameLobby"); //if successfully leaving room, load lobby scene
	}
	
	// Public functions which can be called from other scripts
	//*********************************************************************

	//connect to server
	public void connectToServer(string _ip) {
		//sfs.Connect(_ip, ServerPort); //with dialogue box
	}

	//login function
	public void loginOnServer(string _userName){
		sfs.Send (new LoginRequest (_userName, "", sfs.Config.Zone));
	}

	//logout user
	public void logoutUser() {
		sfs.Send( new LogoutRequest() );
	}

	// Request to create a new room
	public void createRoom(string roomName) {

		createdRoomName = roomName; // keep this in order to access the room later 
		roomSettings = new RoomSettings(roomName);
        roomSettings.Extension = new RoomExtension("myExt", "myExtPackage.MainExtension"); //add extensions to the new room
		
        sfs.Send (new CreateRoomRequest (roomSettings, false, null));
	}

	//Request to join existing room
	public void joinRoom(string roomName) {
		sfs.Send( new JoinRoomRequest(roomName) );
	}

	//Request to leave current room
	public void leaveRoom() {
		sfs.Send (new LeaveRoomRequest ());
	}

	//Force quit the aplication and disconnect from server
	public void exitGame() {
		sfs.Disconnect();
		Application.Quit();
		System.Diagnostics.Process.GetCurrentProcess ().Kill ();
	}

	private void OnApplicationQuit() {
		sfs.Disconnect();
	}

	//functions for sending requests and recieving responses
	//*********************************************************************
	public void requestPilotTransform(int _rotX, int _rotY , int _thrust) {

		ISFSObject objOut = new SFSObject ();
		objOut.PutInt ("rotX", _rotX);
		objOut.PutInt ("rotY", _rotY);
		objOut.PutInt ("thrust", _thrust);

		sfs.Send (new ExtensionRequest ("RequestTransform", objOut, sfs.LastJoinedRoom, true));
	}

	public void requestGunnerRotation(int _rotX, int _rotY) {
		ISFSObject objOut = new SFSObject ();
		objOut.PutInt("GrotX", _rotX);
		objOut.PutInt("GrotY", _rotY);

		sfs.Send (new ExtensionRequest ("RequestGunnerRotation", objOut, sfs.LastJoinedRoom, true));

		Debug.Log("Request gunner Rotation!");
	}

	//ask server for permission to shoot
	public void requestGunnerFire() {
		ISFSObject objOut = new SFSObject ();
		sfs.Send (new ExtensionRequest("Fire", objOut, sfs.LastJoinedRoom, true)); 

		Debug.Log("Request gunner fire!");
	}

    //ask permission to change shield power
    public void requestShieldPowerTransfer(float _amount) {
        ISFSObject objOut = new SFSObject();
        objOut.PutFloat("ShieldPowerAmount", _amount);
        //sfs.Send(new ExtensionRequest("ReqShieldPower", objOut, sfs.LastJoinedRoom, true));

        Debug.Log("Requesting to change shield power");
    }

    //ask permission to change enginge power
    public void requestEnginePowerTransfer(float _amount) {
        ISFSObject objOut = new SFSObject();
        objOut.PutFloat("EnginePowerAmount", _amount);
        //sfs.Send(new ExtensionRequest("ReqEnginePower", objOut, sfs.LastJoinedRoom, true));

        Debug.Log("Requesting to change engine power");
    }

    //ask permission to change turret power
    public void requestTurretPowerTransfer(float _amount) {
        ISFSObject objOut = new SFSObject();
        objOut.PutFloat("TurretPowerAmount", _amount);
        //sfs.Send(new ExtensionRequest("ReqTurretPower", objOut, sfs.LastJoinedRoom, true));

        Debug.Log("Requesting to change turret power");
    }
	
	//server responses, handle data sent from server
	//*********************************************************************
	void OnExtensionResponse(BaseEvent e) {
		string cmd = (string)e.Params["cmd"];
		ISFSObject objIn = (SFSObject)e.Params["params"];

		//check what response the server is giving
		switch (cmd) {

		//new pilot transform data from server
		case "ShipTransform": 

			//check which scene is running, get the gameobject script and update
			switch(Application.loadedLevelName) {

			case "InGamePilot":
				//get pilot script
				GameObject pilotObject = GameObject.Find("Pilot");
				PilotControlls pilotScript = (PilotControlls) pilotObject.GetComponent(typeof(PilotControlls));

				pilotScript.updatePosition(objIn.GetDouble("x"), objIn.GetDouble("y"), objIn.GetDouble("z"));
				pilotScript.updateRotation(objIn.GetDouble("rotX"), objIn.GetDouble("rotY"), objIn.GetDouble("rotZ"));
				break;

			case "InGameGunner":
				//get pilot script
				GameObject gunnerObject = GameObject.Find("Gunner");
				GunnerControlls gunnerScript = (GunnerControlls) gunnerObject.GetComponent(typeof(GunnerControlls));

				//update rotation
				gunnerScript.updateRotation(objIn.GetDouble("rotX"), objIn.GetDouble("rotY"), objIn.GetDouble("rotZ"));
				break;
			}
			break;

		//new roatation data from server to gunner
		case "GunnerRotation":
			if(Application.loadedLevelName == "InGameGunner"){
				//get gunner script
				GameObject gunnerObject = GameObject.Find("Gunner");
				GunnerControlls gunnerScript = (GunnerControlls) gunnerObject.GetComponent(typeof(GunnerControlls));

				//update rotation
				gunnerScript.updateRotation(objIn.GetDouble("rotX"), objIn.GetDouble("rotY"), objIn.GetDouble("rotZ"));
			}
			break;
		
		case "GunnerFire":
			if(Application.loadedLevelName == "InGameGunner"){
				//get gunner script
				GameObject gunnerObject = GameObject.Find("Gunner");
				GunnerControlls gunnerScript = (GunnerControlls) gunnerObject.GetComponent(typeof(GunnerControlls));
				
				//tell gunner its ok to shoot
				gunnerScript.shoot();
			}
			break;

		}
	}
}





