# Multiplayer Setup - SpinLegends

## 🎮 Description
This project includes complete multiplayer functionality using Photon PUN 2. Players can connect and fight in real-time spinning top battles with dynamic health bars, elegant UI, and smooth network synchronization.

## 📋 Prerequisites
- Unity 2022.3 LTS or higher
- Photon PUN 2 (already included in the project)
- Free Photon account (https://www.photonengine.com/)

## ⚙️ Initial Setup

### 1. Configure Photon App ID
1. Go to [Photon Dashboard](https://dashboard.photonengine.com/)
2. Create a new application or use an existing one
3. Copy your **App ID**
4. In Unity, go to `Window > Photon Unity Networking > PUN Wizard`
5. Paste your App ID in the corresponding field
6. Click "Setup Project"

### 2. Automatic Setup (Recommended)
The project now includes automatic setup scripts:
1. Run the game
2. The `AutoSetup` script will automatically configure:
   - NetworkManager
   - PlayerSpawnManager
   - MultiplayerUI with transparent panels
   - HealthBarManager
   - AndroidSettings
   - JoystickFixer for mobile compatibility

### 3. Manual Configuration (Alternative)
If you prefer manual setup:
1. Select the `playerspin` prefab in `Assets/Prefabs/`
2. Add the `PhotonView` component if it doesn't have it
3. In the PhotonView, assign `PlayerController` and `NetworkPlayerCollision` as Observed Components
4. Make sure the prefab is in the `Resources/` folder

## 🎯 Added Components

### NetworkManager
- Handles Photon connection
- Manages rooms and players
- Automatic player spawning
- Room creation and joining

### PlayerController (Enhanced)
- Position and rotation synchronization
- Spin state synchronization with smooth interpolation
- Health synchronization
- Joystick input handling for mobile
- Network interpolation for smooth movement

### HealthBarManager
- Creates health bars for all players automatically
- Health bars follow each player in the arena
- Color-coded health display (green/yellow/red)
- Animated effects for damage and healing

### MultiplayerUI (Improved)
- Transparent status panel (top-right)
- Transparent room info panel (top-left)
- Connection status with icons
- Room information with player count
- All text in English

### NetworkPlayerCollision
- Multiplayer collision handling
- Synchronized damage system
- Knockback between players
- Visual and audio effects

### AutoSetup
- Automatically configures all multiplayer components
- Sets up UI panels with proper transparency
- Configures joystick for mobile devices
- Creates health bar system

## 🏥 Health Bar System

### Features
- **Dynamic Health Bars**: Each player has a health bar that follows their spinner
- **Color Coding**: 
  - 🟢 Green: High health (70%+)
  - 🟡 Yellow: Medium health (30-69%)
  - 🔴 Red: Low health (below 30%)
- **Visual Effects**: Flash effects on damage, glow effects on healing, pulse on critical health
- **Smooth Animation**: Health bars smoothly follow players and face the camera

### Configuration
```csharp
// In HealthBarManager
public Vector3 healthBarOffset = new Vector3(0, 3f, 0);
public bool showHealthBarsForAllPlayers = true;
public bool showHealthBarForLocalPlayer = true;
```

## 🖥️ UI Improvements

### Transparent Panels
- **Status Panel**: 30% opacity black background
- **Room Info Panel**: 30% opacity black background
- **Non-intrusive Design**: Panels don't block gameplay view

### English Interface
- All UI text in English
- Connection status messages
- Room information
- Debug messages

## 🚀 How to Use

### For Developers
1. Configure your Photon App ID
2. Run the game
3. AutoSetup will configure everything automatically
4. NetworkManager will connect automatically
5. Players will join rooms automatically

### For Players
1. Run the game
2. Game will connect automatically to Photon
3. Will join an existing room or create a new one
4. Health bars will appear for all players
5. Start fighting!

## 🔧 Advanced Configuration

### Customize Health Bar Appearance
```csharp
// In HealthBar.cs
public Color highHealthColor = Color.green;
public Color mediumHealthColor = Color.yellow;
public Color lowHealthColor = Color.red;
public float highHealthThreshold = 0.7f;
public float mediumHealthThreshold = 0.3f;
```

### Adjust Panel Transparency
```csharp
// In SceneSetup.cs
statusPanelImage.color = new Color(0, 0, 0, 0.3f); // 30% opacity
roomInfoPanelImage.color = new Color(0, 0, 0, 0.3f); // 30% opacity
```

### Customize Spawn Points
```csharp
// In NetworkManager, modify the spawnPoints array
public Transform[] spawnPoints;
```

### Adjust Network Synchronization
```csharp
// In PlayerController, modify interpolation speeds
public float interpolationSpeed = 15f;
public float rotationInterpolationSpeed = 15f;
public float velocityInterpolationSpeed = 15f;
public float spinInterpolationSpeed = 10f;
```

### Configure Collision System
```csharp
// In NetworkPlayerCollision
public float knockbackForce = 25f;
public float minCollisionForce = 1f;
public float collisionDamageMultiplier = 1f;
```

## 🐛 Troubleshooting

### Error: "App ID not configured"
- Verify you've configured the App ID in PUN Wizard
- Make sure the App ID is correct

### Error: "Player prefab not found"
- AutoSetup should handle this automatically
- Verify the prefab is in the Resources folder

### Players not visible
- Verify PhotonView is configured correctly
- Make sure PlayerController and NetworkPlayerCollision are marked as Observed

### Health bars not appearing
- Check that HealthBarManager is present in the scene
- Verify AutoSetup ran successfully

### Lag or jerky movement
- Adjust interpolation speeds in PlayerController
- Check your internet connection
- Consider using closer Photon regions

### Joystick not working on mobile
- AutoSetup includes JoystickFixer for mobile compatibility
- Check that EventSystem and Canvas are properly configured

## 📱 Multiplayer Features

### ✅ Implemented
- Automatic Photon connection
- Player spawning at random points
- Real-time movement synchronization
- Spin state synchronization with smooth interpolation
- Multiplayer collision system with knockback
- Dynamic health bar system
- Transparent UI panels
- Network information display
- Player list in room
- Mobile-optimized joystick controls
- English interface throughout

### 🔄 In Development
- Player chat system
- Scoring and ranking system
- Different game modes
- Spinner customization
- Enhanced particle effects

## 🎨 Customization

### Change Player Colors
```csharp
// In PlayerController, add a Renderer
public Renderer playerRenderer;
// Assign different colors per player
```

### Add Particle Effects
```csharp
// In PlayerController, synchronize effects
[PunRPC]
void PlayEffectRPC(string effectName)
{
    // Play effect locally
}
```

### Customize Health Bar Effects
```csharp
// In HealthBarEffects.cs
public bool enableDamageFlash = true;
public bool enableHealGlow = true;
public bool enableCriticalHealthEffect = true;
public bool enableShakeEffect = true;
```

## 📞 Support
If you have configuration issues:
1. Check that AutoSetup ran successfully
2. Verify Photon is configured correctly
3. Check Unity console for errors
4. Make sure all prefabs have PhotonView
5. Check your internet connection

Enjoy SpinLegends multiplayer! 🏆 