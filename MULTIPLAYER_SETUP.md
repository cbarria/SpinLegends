# Multiplayer Setup - SpinLegends Beyblade

## 🎮 Description
This project now includes complete multiplayer functionality using Photon PUN 2. Players can connect and fight in real-time Beyblade battles.

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

### 2. Configure Player Prefab
1. Select the `playerspin` prefab in `Assets/Prefabs/`
2. Add the `PhotonView` component if it doesn't have it
3. In the PhotonView, assign `PlayerController` as Observed Component
4. Make sure the prefab is in the `Resources/` folder or configure it in `PhotonServerSettings`

### 3. Configure the Scene
1. Open the `SpinLegends` scene
2. Add an empty GameObject called "NetworkManager"
3. Add the `NetworkManager` component to the GameObject
4. Configure the references:
   - **Player Prefab**: Drag the `playerspin` prefab
   - **Spawn Points**: Drag the arena spawn points
   - **Max Players Per Room**: 4 (recommended)

## 🎯 Added Components

### NetworkManager
- Handles Photon connection
- Manages rooms and players
- Automatic player spawning

### PlayerController (Modified)
- Position and rotation synchronization
- Spin state synchronization
- Health synchronization
- Smooth interpolation for movement

### MultiplayerUI
- Connection panel
- Room information
- Player list
- Network controls

### NetworkPlayerCollision
- Multiplayer collision handling
- Synchronized damage system
- Knockback between players

## 🚀 How to Use

### For Developers
1. Configure your Photon App ID
2. Run the game
3. NetworkManager will connect automatically
4. Players will join rooms automatically

### For Players
1. Run the game
2. Game will connect automatically to Photon
3. Will join an existing room or create a new one
4. Start fighting!

## 🔧 Advanced Configuration

### Customize Spawn Points
```csharp
// In NetworkManager, modify the spawnPoints array
public Transform[] spawnPoints;
```

### Adjust Synchronization
```csharp
// In PlayerController, modify interpolation
transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
```

### Configure Collision Damage
```csharp
// In NetworkPlayerCollision
public float collisionDamageMultiplier = 1f;
public float knockbackForce = 5f;
```

## 🐛 Troubleshooting

### Error: "App ID not configured"
- Verify you've configured the App ID in PUN Wizard
- Make sure the App ID is correct

### Error: "Player prefab not found"
- Verify the prefab is in the Resources folder
- Or configure it manually in PhotonServerSettings

### Players not visible
- Verify PhotonView is configured correctly
- Make sure PlayerController is marked as Observed

### Lag or jerky movement
- Adjust interpolation in PlayerController
- Check your internet connection
- Consider using closer Photon regions

## 📱 Multiplayer Features

### ✅ Implemented
- Automatic Photon connection
- Player spawning at random points
- Real-time movement synchronization
- Spin state synchronization
- Multiplayer collision system
- Network information UI
- Player list in room

### 🔄 In Development
- Player chat
- Scoring system
- Different game modes
- Beyblade customization
- Synchronized particle effects

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

## 📞 Support
If you have configuration issues:
1. Verify Photon is configured correctly
2. Check Unity console for errors
3. Make sure all prefabs have PhotonView
4. Check your internet connection

Enjoy SpinLegends multiplayer! 🏆 