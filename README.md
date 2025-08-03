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
  <img src="https://github.com/SylanTroh/GMMenu/blob/InstallGuide/Images/20241029002823_1.jpg" height="250" />
  <img src="https://github.com/SylanTroh/GMMenu/blob/InstallGuide/Images/alerts.png" height="250" />
</p>

# Installation
1. Go to https://sylantroh.github.io/SylanVCC/ and click "Add to VCC"
2. Click Manage Project in the creator companion and press the plus button next to GMMenu
3. Import the GMMenu Prefab into the scene.
4. If you would like to use the features that change audio settings such as talk, whisper, yell, broadcast, radio and GM radio:
  1. Add the [AudioManager](https://github.com/SylanTroh/AudioManager) package to the project the same way as the GMMenu
  2. Create an empty game object in your scene and add an 'AudioSettingManager' component
  3. The presence of the audio manager automatically enables features depending on it
