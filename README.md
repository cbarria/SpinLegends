# SpinLegends - Multiplayer Spinning Tops Game

A real-time multiplayer spinning tops battle game (inspired by Beyblade) for Android, built with Unity and Photon.

## 🎮 Features

- **Real-time multiplayer** using Photon PUN
- **Touch controls** optimized for mobile
- **Visual effects** with particles and trails
- **Combat system** with damage and health
- **Adaptive UI** for different screen sizes
- **Dynamic health bars** that follow each player and change color (green/yellow/red)
- **Elegant, transparent multiplayer UI panels**

## 🛠️ Project Setup

### 1. Prerequisites

- Unity 2022.3 LTS or newer
- Free account at [Photon Engine](https://www.photonengine.com/)

### 2. Photon Installation

1. Open Unity and go to `Window > Package Manager`
2. Click the `+` and select `Add package from git URL`
3. Enter: `com.unity.textmeshpro`
4. Repeat for: `com.unity.inputsystem`

### 3. Photon Configuration

1. Go to `Window > Photon Unity Networking > PUN Wizard`
2. Create a Photon Engine account if you don't have one
3. Copy your **App ID** from the Photon dashboard
4. Paste the App ID in the corresponding field in Unity
5. Click `Setup Project`

### 4. Android Build Settings

1. Go to `File > Build Settings`
2. Select `Android` as the platform
3. Click `Switch Platform`
4. Go to `Player Settings` and set:
   - **Company Name**: Your name
   - **Product Name**: SpinLegends
   - **Package Name**: com.yourname.spinlegends
   - **Minimum API Level**: Android 6.0 (API level 23)
   - **Target API Level**: Android 13.0 (API level 33)

### 5. Scene Setup

1. Create a new scene called `MainGame`
2. Add the following GameObjects:
   - **GameManager** (with the GameManager script)
   - **AndroidSettings** (with the AndroidSettings script)
   - **Main Camera** (with the CameraShake script)
   - **Directional Light**
   - **Ground** (plane for the arena)
   - **Spawn Points** (player spawn locations)

### 6. UI Setup

1. Create a Canvas with `UI Scale Mode: Scale With Screen Size`
2. Add the following elements:
   - **Main Menu Panel**
   - **Game UI Panel**
   - **Pause Menu Panel**
   - **Joystick** (for movement)
   - **Spin Button**
   - **Jump Button**
   - **Health Bar** (see below)
   - **Timer Text**
   - **Multiplayer UI** (status and room info panels)

## 🏥 Health Bar System

- Each player has a health bar that **follows their spinner** in the arena.
- The health bar **changes color** based on health:
  - 🟢 Green: High health (70%+)
  - 🟡 Yellow: Medium health (30-69%)
  - 🔴 Red: Low health (below 30%)
- Health bars are visible for all players in multiplayer.
- Health bars include:
  - Smooth following and camera-facing
  - Animated effects for damage, healing, and critical health
  - Numeric health display (e.g., "85/100")

## 🖥️ Multiplayer UI Improvements

- **Status panel** (top-right):
  - Shows connection status with icon and text
  - Transparent black background (30% opacity)
- **Room info panel** (top-left):
  - Shows room name and player count with icons
  - Transparent black background (30% opacity)
- **All UI and debug messages are in English**
- **Elegant, non-intrusive design** for a professional look

## 🎯 How to Play

### Controls
- **Joystick**: Move your spinning top
- **Spin Button**: Start/stop spinning
- **Jump Button**: Jump
- **Swipe**: Dodge and special attacks

### Objective
- Win battles against other players
- Keep your spinning top spinning
- Avoid being knocked out
- Last player standing wins

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Player/
│   │   └── PlayerController.cs
│   ├── Managers/
│   │   ├── GameManager.cs
│   │   └── AndroidSettings.cs
│   ├── Input/
│   │   └── TouchController.cs
│   ├── UI/
│   │   ├── HealthBar.cs
│   │   ├── HealthBarManager.cs
│   │   ├── HealthBarEffects.cs
│   │   └── MultiplayerUI.cs
│   └── Effects/
│       ├── SpinEffects.cs
│       └── CameraShake.cs
├── Prefabs/
│   └── Player.prefab
├── Materials/
├── Audio/
└── PhotonAppSettings.asset
```

## 🔧 Customization

### Change Spinner Colors
1. Select the Player prefab
2. Edit the spinner's material
3. Adjust colors in the `SpinEffects` script

### Add New Effects
1. Create new particle systems
2. Assign effects in the `SpinEffects` script
3. Configure parameters in the inspector

### Modify Physics
1. Adjust values in `PlayerController`
2. Edit `spinSpeed`, `moveSpeed`, `jumpForce`
3. Test different settings

## 🚀 Building for Android

1. Go to `File > Build Settings`
2. Select `Android`
3. Click `Build`
4. Choose a location for the APK
5. Install on your Android device

## 🐛 Troubleshooting

### Photon Error
- Check that your App ID is correct
- Make sure you are connected to the internet
- Check the Unity console for errors

### Performance Issues
- Lower graphics quality in `Quality Settings`
- Optimize particle effects
- Adjust `targetFrameRate` in `AndroidSettings`

### Controls Not Responding
- Check that buttons are connected in the inspector
- Make sure `TouchController` is configured
- Test on a physical device

## 📞 Support

If you have issues:
1. Check the Unity console
2. Verify Photon configuration
3. Make sure all scripts are assigned

## 🎉 Enjoy Playing!

Your SpinLegends game is ready to play! Invite friends and family for epic spinning top battles.
