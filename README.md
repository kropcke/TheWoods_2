# TheWoods_2

## Description
The Woods is a ...

## Instructions
To run the woods, a specific setup is required which uses three devices.

The first device required is typically a laptop or PC which will be the "server" machine. This machine should be attached to a projector which is aimed at the floor as close to perpendicular as possible. 
When the projector is set up, take measurements for the projected screen size in meters. Enter the height of the projected screen as the "gameAreaSize" variable on the serverScript.cs.
Also adjust the server's spawn height in GameController.cs line 44. Adjust the position argument in the second parameter of the Instantiate call to be the height (in meters) of the projector.
If these settings dont fit the screen exactly, feel free to tweak the gameAreaSize until everything is contained within the screen.

The other two devices can be any phone that supports ARKit or ARCore positional tracking. Generally, any phone released after 2017 will do the job. We have done all testing and development on two iPhone X's.

Lastly, a single image target marker, which resides within Scott and Kyoungs Vuforia account, must be printed and put in the center of the play area.

When the setup is complete, make a build for each mobile device and deploy it. The server machine does not require a build, but is ideal for presentation purposes.
Keep in mind that if you develop for iOS devices, a Mac will be needed. Another thing to remember is the overhead for recompiling the project each time a build target environment is changed.

## Playing
Run the server computer first, and wait for the scene to load. After the project loads, the mobile devices may start the app in any order.

The players' goal is to collect all 8 birds and listen to the voicemail clips grandma left, all while avoiding the cloud of distractions that may wipe away any birds.
To capture a bird, have the bird land on the lighning bolt connection between the branches on each player. When there is a bird (or multiple birds) on the line, the players may come together to listen to the voicemail for one bird at a time.

When all 8 birds are collected, the entire voicemail will play and the game will be completed.

## Dependencies, software and packages
Unity 2018.1.6f1
Photon Unity Networking 2 version 2.6 lib 4.1.2.7

## Code
Since PUN is used, often PhotonView.isMine is used at the beginning of methods so that only the local player executes the code within.

### ServerScript.cs
This script is run only on the server machine by using PhotonView.isMine.
This script manages the game flow. All spawning and movement of distractions (clouds) and voicemail clips (birds) is controlled here. Also, all sound events are controlled here by calling SoundClipGameObjects.cs functions on other gameobjects.

Could use better formatting and be split into multiple scripts.

### GameController.cs
This script manages the spawning of the server and clients in the correct positions with the correct prefabs. Since prefabs can only be instansiated from the Resources folder, all prefabs are held there.

### PlayerScript.cs
This script is the interface between the AR positional tracking and the network showing that position for the player. 

### SoundClipGameObjects.cs
This script is put on to the audio visualization game objects which use cubes and FFT to create the audio visualization for each audio clip. Some work has been done to allow this to work for any number of sound clips, but updates are needed. Also, a better visualization may be a useful upgrade.

### LobbyManager.cs
This script is only used within SkylarNetworkingStartScene to load the networked room. If a room exists when the player enters this scene, they will automatically join the room. Otherwise, a room is created, and that player will be the "server" or master client.

## Scenes
### SkylarNetworkingStartScene.unity
This scene is used for the start of the networking. It creates a room automatically if one doesn't exist, and then will load SkylarNetworkingMainScene

### SkylarNetworkingMainScene.unity
This is the main game scene. If loaded into this scene before connected to a room, it will automatically load into SkylarNetworkingStartScene.
