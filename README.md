# GMMenu
An UDON menu panel designed for VRChat roleplay. 

Features Include:
- Works in both Desktop and in VR. To activate the menu:
  - In Desktop, push the Tab Key
  - In VR, quickly move the right joystick/trackpad down, and then up.
- A synced ping system, allowing players to request assistance from Game Masters
  - GMs are notified via their HUD when a player makes a ping, and can see all active pings sorted by priority, and how long ago they were sent
  - Players are notified via their HUD when a GM responds to their ping
- The ability for GMs to teleport to others or teleport others to them, and the ability to undo teleports.
- The ability to remotely check where players are using a camera system
- NoClip for quick navigation around the map
- A permission system, allowing for both hard-coded permissions, and self-assigned permissions.
<p align="center">
  <img src="https://github.com/SylanTroh/GMMenu/blob/InstallGuide/Images/alerts.png" height="250" />
  <img src="https://github.com/SylanTroh/GMMenu/blob/InstallGuide/Images/playerlist.png" height="250" />
</p>

# Installation
1. Go to https://sylantroh.github.io/SylanVCC/ and click "Add to VCC"
2. Click Manage Project in the creator companion and press the plus button next to GMMenu

## Known Issues
Upon uploading the world and coming to the World Configuration screen, the Upload button may be unresponsive. This is apparently caused by a conflict between the VRChat uploader, and additional canvases in the scene. If this occurs, simply disable the GMMenu in the hierarchy (since the world has already been built, this will not affect the upload). The upload button should then become responsive.
