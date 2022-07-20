# XRUIInputModuleWithGaze
UnityXR XRUIInputModule extended with gaze input.

To use these scripts:

- XR Plugin Management is required (can be installed from Package Manager)
- Replace XRUIInputModule.cs with XRUIInputModuleWithGaze.cs
- Add GazePointer to the scene and add the reference to the progress bar image (the image fillAmount will be increased from 0% to 100% during GazeTime)
- GazeTime can be changed on the Input Module
- If UseGaze is set to false the gaze is disabled
