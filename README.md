# GMMenu
An UDON menu panel designed for VRChat roleplay. 

# Installation
1. Make sure you have UdonSharp and the VRChat Worlds SDK installed in your Unity project.
2. Download the [latest .unitypackage](https://github.com/SylanTroh/GMMenu/releases/latest) and import it into your Unity project.
3. Navigate to Window > Package Manager and locate the GMMenu Package. 
4. Import the GMMenu Prefab. Importing Example Scene is not neccessary if you already have a scene in your project.
![Permissions](https://github.com/SylanTroh/GMMenu/blob/main/Installation/install1.png)
6. In your Assets folder, navigate to Samples > GMMenu > 1.1.X > GMMenu Prefab. Drag and drop the GMMenu into your scene.
7. (Optional) Locate the GMMenu in your hierarchy, expand it and select the PlayerPermissions gameobject. You can uncheck "Everyone Is GM" and set specific permissions by entering VRChat usernames into the GMList and FacilitatorList. These names must be case-sensitive.
![Permissions](https://github.com/SylanTroh/GMMenu/blob/main/Installation/install3.png)

## Known Issues
Upon uploading the world and coming to the World Configuration screen, the Upload button may be unresponsive. This is apparently caused by a conflict between the VRChat uploader, and additional canvases in the scene. If this occurs, simply disable the GMMenu in the hierarchy (since the world has already been built, this will not affect the upload). The upload button should then become responsive.
