# SpinLegends - Juego de Spinning Tops Multiplayer

Un juego de batalla de spinning tops (como Beyblade) para Android con funcionalidad multiplayer usando Unity y Photon.

## 🎮 Características

- **Multiplayer en tiempo real** usando Photon PUN
- **Controles táctiles** optimizados para móviles
- **Efectos visuales** con partículas y trails
- **Sistema de combate** con daño y salud
- **Interfaz adaptativa** para diferentes tamaños de pantalla

## 🛠️ Configuración del Proyecto

### 1. Requisitos Previos

- Unity 2022.3 LTS o superior
- Cuenta gratuita en [Photon Engine](https://www.photonengine.com/)

### 2. Instalación de Photon

1. Abre Unity y ve a `Window > Package Manager`
2. Haz clic en el `+` y selecciona `Add package from git URL`
3. Ingresa: `com.unity.textmeshpro`
4. Repite para: `com.unity.inputsystem`

### 3. Configuración de Photon

1. Ve a `Window > Photon Unity Networking > PUN Wizard`
2. Crea una cuenta en Photon Engine si no tienes una
3. Copia tu **App ID** desde el dashboard de Photon
4. Pega el App ID en el campo correspondiente en Unity
5. Haz clic en `Setup Project`

### 4. Configuración para Android

1. Ve a `File > Build Settings`
2. Selecciona `Android` como plataforma
3. Haz clic en `Switch Platform`
4. Ve a `Player Settings` y configura:
   - **Company Name**: Tu nombre
   - **Product Name**: SpinLegends
   - **Package Name**: com.tunombre.spinlegends
   - **Minimum API Level**: Android 6.0 (API level 23)
   - **Target API Level**: Android 13.0 (API level 33)

### 5. Configuración de la Escena

1. Crea una nueva escena llamada `MainGame`
2. Agrega los siguientes GameObjects:
   - **GameManager** (con el script GameManager)
   - **AndroidSettings** (con el script AndroidSettings)
   - **Main Camera** (con el script CameraShake)
   - **Directional Light**
   - **Ground** (plano para el suelo)
   - **Spawn Points** (puntos de aparición)

### 6. Configuración de UI

1. Crea un Canvas con `UI Scale Mode: Scale With Screen Size`
2. Agrega los siguientes elementos:
   - **Main Menu Panel**
   - **Game UI Panel**
   - **Pause Menu Panel**
   - **Joystick** (para movimiento)
   - **Spin Button**
   - **Jump Button**
   - **Health Bar**
   - **Timer Text**

## 🎯 Cómo Jugar

### Controles
- **Joystick**: Mover el spinning top
- **Botón Spin**: Activar/desactivar el giro
- **Botón Jump**: Saltar
- **Swipe**: Esquiva y ataques especiales

### Objetivo
- Gana batallas contra otros jugadores
- Mantén tu spinning top girando
- Evita ser eliminado
- El último en pie gana

## 📁 Estructura del Proyecto

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
│   │   └── Joystick.cs
│   └── Effects/
│       ├── SpinEffects.cs
│       └── CameraShake.cs
├── Prefabs/
│   └── Player.prefab
├── Materials/
├── Audio/
└── PhotonAppSettings.asset
```

## 🔧 Personalización

### Cambiar Colores del Spinning Top
1. Selecciona el prefab del Player
2. Modifica el material del spinning top
3. Ajusta los colores en el script `SpinEffects`

### Agregar Nuevos Efectos
1. Crea nuevos sistemas de partículas
2. Asigna los efectos en el script `SpinEffects`
3. Configura los parámetros en el inspector

### Modificar Física
1. Ajusta los valores en `PlayerController`
2. Modifica `spinSpeed`, `moveSpeed`, `jumpForce`
3. Prueba diferentes configuraciones

## 🚀 Build para Android

1. Ve a `File > Build Settings`
2. Selecciona `Android`
3. Haz clic en `Build`
4. Elige la ubicación para el APK
5. Instala en tu dispositivo Android

## 🐛 Solución de Problemas

### Error de Photon
- Verifica que el App ID esté correcto
- Asegúrate de estar conectado a internet
- Revisa la consola de Unity para errores

### Problemas de Rendimiento
- Reduce la calidad gráfica en `Quality Settings`
- Optimiza las partículas
- Ajusta el `targetFrameRate` en `AndroidSettings`

### Controles No Responden
- Verifica que los botones estén conectados en el inspector
- Asegúrate de que el `TouchController` esté configurado
- Prueba en un dispositivo físico

## 📞 Soporte

Si tienes problemas:
1. Revisa la consola de Unity
2. Verifica la configuración de Photon
3. Asegúrate de que todos los scripts estén asignados

## 🎉 ¡Disfruta Jugando!

¡Tu juego SpinLegends está listo para jugar! Invita a amigos y familiares a batallas épicas de spinning tops. "# SpinLegends"  
"# SpinLegends"  
