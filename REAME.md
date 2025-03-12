
### To test the project:
Double-click on any scene file to open it

## Project Structure 

### Samples
 - Contains example scenes, prefabs, and scripts that demonstrate how to use various VR features. These are useful reference materials for learning and implementation.

 ### Scenes
  - Stores your Unity scenes (levels/environments) for the VR project.

 ### Setting
 Contains configuration files for your VR project, including quality settings, input settings, and other project-specific configuration

 ### TutorialInfo
 Usually contains documentation and tutorial-related assets to help developers understand how to use the VR framework.

 ### XR and XRI (XR Interaction) 
These are crucial folders for VR development:
- XR contains core Virtual Reality and Augmented Reality functionalities
- XRI contains the XR Interaction Toolkit assets, which handle:
    - VR controllers input 
    - Interaction systems (grabbing, touching, UI interaction)
    - Locomotion (movement in VR)
    - Ray interaction
    - Haptic feedback

- InputSystem_Actions.inputaction
These are Unity-specific files that store metadata about their corresponding assets. You don't need to modify these directly; Unity manages them automatically.

### .meta&.asset files
#### .meta files:
These are hidden metadata files that Unity automatically generates for EVERY file and folder in your Assets
They contain important information like:
Unique file IDs
Import settings
Asset configurations
References to other assets
Important:
Never delete .meta files manually
Always include them in version control (git)
If you delete a .meta file, Unity will create a new one with a different ID, which can break references in your project
#### .asset files:
These are Unity's serialized data files that store:
ScriptableObject data
Project settings
Custom configurations
Preset values