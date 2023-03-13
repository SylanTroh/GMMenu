# GMMenu
An UDON menu panel designed for VRChat roleplay. 

Features Include:
- A synced ping system, allowing players to request assistance from Game Masters
  - GMs are notified via their HUD when a player makes a ping, and can see all active pings sorted by priority, and how long ago they were sent
  - Players are notified via their HUD when a GM responds to their ping
- The ability for GMs to teleport to others or teleport others to them, and the ability to undo teleports.
- The ability to remotely check where players are using a camera system
- NoClip for quick navigation around the map
- A permission system, allowing for both hard-coded permissions, and self-assigned permissions.
<p align="center">
  <img src="https://github.com/SylanTroh/GMMenu/blob/main/Images/alerts.png" height="300" />
  <img src="https://github.com/SylanTroh/GMMenu/blob/main/Images/playerlist.png" height="300" />
</p>

# Installation
1. Make sure you have UdonSharp and the VRChat Worlds SDK installed in your Unity project.
2. Download the [latest .unitypackage](https://github.com/SylanTroh/GMMenu/releases/latest) and import it into your Unity project.
3. Navigate to Window > Package Manager and locate the GMMenu Package. 
4. Import the GMMenu Prefab. Importing Example Scene is not neccessary if you already have a scene in your project. <img src="https://github.com/SylanTroh/GMMenu/blob/main/Images/install1.png" height="500"/>
5. In your Assets folder, navigate to Samples > GMMenu > 1.1.X > GMMenu Prefab. Drag and drop the GMMenu into your scene.
6. (Optional) Locate the GMMenu in your hierarchy, expand it and select the PlayerPermissions gameobject. You can uncheck "Everyone Is GM" and set specific permissions by entering VRChat display names into the GMList and FacilitatorList. These names must be case-sensitive.

## Known Issues
Upon uploading the world and coming to the World Configuration screen, the Upload button may be unresponsive. This is apparently caused by a conflict between the VRChat uploader, and additional canvases in the scene. If this occurs, simply disable the GMMenu in the hierarchy (since the world has already been built, this will not affect the upload). The upload button should then become responsive.
