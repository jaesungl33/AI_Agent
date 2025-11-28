# Tank Online 1 - Multiplayer Tank Battle Game

A Unity-based multiplayer tank battle game using Fusion networking with modular architecture and Firebase backend integration.

## 📖 Table of Contents
- [Project Overview](#project-overview)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Core Systems](#core-systems)
- [Setup Instructions](#setup-instructions)
- [Development Guidelines](#development-guidelines)

## 🎮 Project Overview

Tank Online 1 is a real-time multiplayer tank combat game built with Unity and Photon Fusion. The game features:

- **Real-time Multiplayer Combat**: Up to 10 players per match
- **Multiple Game Modes**: Team Deathmatch, Capture the Base, Battle Royale
- **Tank Customization**: Different hulls, weapons, and abilities
- **Firebase Integration**: Remote config, analytics, and cloud saves
- **Modular Architecture**: Clean, maintainable code structure

## 🏗️ Architecture

### Core Design Patterns
- **Singleton Pattern**: For manager classes
- **Observer Pattern**: Event-driven communication
- **State Machine**: Game state management
- **Module Pattern**: Separation of concerns
- **Factory Pattern**: Object creation and pooling

### Key Technologies
- **Unity 2022.3 LTS**
- **Photon Fusion**: Networking
- **Firebase**: Backend services
- **Universal Render Pipeline (URP)**
- **Newtonsoft JSON**: Data serialization
- **TextMeshPro**: UI text rendering

## 📁 Project Structure

```
Assets/
├── _ExternalAssets/          # Third-party visual assets
│   ├── EffectCore/           # Particle effects
│   ├── FX_Kandol_Pack/      # Additional VFX
│   ├── Tanks/               # Tank models and textures
│   └── Towers/              # Environment models
│
├── _ExternalPackages/        # Third-party packages
│   ├── Battle Arena - Cartoon Assets/
│   ├── Joystick Pack/       # Mobile controls
│   └── QuickOutline/        # Object highlighting
│
├── _GameAssets/             # Project-specific assets
│   ├── Fonts/               # Custom fonts
│   ├── GameIcons/           # UI icons and sprites
│   ├── Materials/           # Shaders and materials
│   ├── Models/              # 3D models
│   ├── Prefabs/             # Game object prefabs
│   ├── Scenes/              # Game scenes
│   ├── Scripts/             # Non-modular scripts
│   ├── Sounds/              # Audio files
│   ├── Textures/            # 2D textures
│   └── UI/                  # UI prefabs and sprites
│
├── _GameData/               # Data management
│   ├── Resources/           # ScriptableObject collections
│   ├── Scripts/             # Database and data managers
│   └── TankWars_Setting/    # Game configuration
│
├── _GameModules/            # Modular system architecture
│   ├── APIModule/           # External API integration
│   ├── AudioModule/         # Sound management
│   ├── BaseModule/          # Core functionality
│   ├── BotModule/           # AI opponents
│   ├── CameraModule/        # Camera controls
│   ├── EventsModule/        # Event system
│   ├── FirebaseModule/      # Firebase integration
│   ├── GameDataModule/      # Data structures
│   ├── MatchMakingFusionModule/  # Multiplayer matching
│   ├── NetworkingModule/    # Network communication
│   ├── PlayerModule/        # Player management
│   ├── TankFusionModule/    # Tank gameplay
│   ├── UIModule/            # User interface
│   └── UtilsModule/         # Utility functions
│
├── _GamePlay/               # Core gameplay logic
│   └── Scripts/             # Game managers and states
│
└── _GameUI/                 # User interface system
    └── Scripts/             # UI controllers and managers
```

## 🎯 Core Systems

### 1. Game Management (`_GamePlay/`)
- **GameManager**: Main game controller and initialization
- **GameStateMachine**: State transitions (Loading → MainMenu → Matchmaking → Lobby → GamePlay → Final)
- **StateType Enum**: Game state definitions

### 2. Data Management (`_GameData/`)
- **DatabaseManager**: Central data access point
- **GameDatabase**: ScriptableObject collections container
- **CollectionBase<T>**: Generic collection template
- **Document Types**: TankDocument, WeaponDocument, HullDocument, etc.

### 3. Networking (`_GameModules/NetworkingModule/`)
- **Photon Fusion Integration**: Real-time multiplayer
- **MatchmakingManager**: Player matching system
- **NetworkRunner**: Network session management
- **Runtime Collections**: Dynamic data updates

### 4. Tank System (`_GameModules/TankFusionModule/`)
- **Modular Tank Design**: Separate Hull, Weapon, and Ability components
- **Tank Types**: Scout, Assault, Heavy, Outpost
- **Weapon System**: Primary and secondary weapons
- **Ability System**: Special tank abilities

### 5. UI System (`_GameModules/UIModule/` & `_GameUI/`)
- **UIManager**: Central UI controller
- **Screen Management**: Different game screens
- **Mobile Controls**: Touch and joystick input

### 6. Event System (`_GameModules/EventsModule/`)
- **EventManager**: Type-safe event handling
- **GameEvent<T>**: Generic event structure
- **Publisher-Subscriber Pattern**: Decoupled communication

### 7. Firebase Integration (`_GameModules/FirebaseModule/`)
- **RemoteConfigManager**: Dynamic configuration
- **RuntimeCollection<T>**: Live data updates
- **Analytics**: Player behavior tracking
- **Authentication**: User management

## 🚀 Setup Instructions

### Prerequisites
- Unity 2022.3 LTS or later
- Visual Studio 2019+ or JetBrains Rider
- Git for version control

### Installation Steps

1. **Clone Repository**
   ```bash
   git clone [repository-url]
   cd tank_online_1
   git branch dev
   ```

2. **Unity Setup**
   - Open project in Unity Hub
   - Install required packages via Package Manager:
     - Fusion
     - Firebase SDK
     - Newtonsoft JSON
     - Universal Render Pipeline

3. **Firebase Configuration**
   - Place `google-services.json` in Assets folder
   - Configure Firebase project settings
   - Set up Remote Config parameters

4. **Photon Setup**
   - Create Photon account
   - Configure App ID in Fusion settings
   - Set up matchmaking regions

5. **Build Settings**
   - Add scenes to build settings in order:
     - Loading Scene
     - Main Menu Scene
     - Lobby Scene
     - Game Scene

### Development Environment

1. **Code Style**
   - Use C# naming conventions
   - Follow SOLID principles
   - Comment complex logic
   - Use regions for organization

2. **Git Workflow**
   - Create feature branches
   - Use meaningful commit messages
   - Pull request reviews required

## 🛠️ Development Guidelines

### Module Structure Guidelines

Each module in the `_GameModules/` directory follows a standardized structure to ensure consistency, maintainability, and ease of development:

```
├── _GameModules/            # Modular system architecture
   ├── _[Name]Module/       # Individual module (e.g., AudioModule, PlayerModule)
      ├── Resources/       # Module-specific assets and configurations
      │                   # - ScriptableObjects for module settings
      │                   # - Prefabs used exclusively by this module
      │                   # - Configuration files and data assets
      ├── Editor/         # Unity Editor extensions and tools
      │                   # - Custom inspectors for module components
      │                   # - Editor windows and utilities
      │                   # - Build pipeline extensions
      │                   # - Development tools and validators
      ├── Runtime/        # Core module implementation
      │   ├── Scripts/    # Main runtime scripts
      │   ├── Data/       # Data structures and models
      │   ├── Interfaces/ # Contracts and abstractions
      │   └── Utils/      # Helper classes and utilities
      ├── Tests/          # Unit and integration tests
      │   ├── Runtime/    # Runtime test scripts
      │   └── Editor/     # Editor test scripts
      ├── Samples/        # Example scenes and demonstrations
      │                   # - Demo scenes showcasing module features
      │                   # - Example implementations
      │                   # - Integration examples with other modules
      ├── Documents/      # Module-specific documentation
      │                   # - API documentation
      │                   # - Usage guides and examples
      │                   # - Architecture decisions and design notes
      └── README.md       # Module overview and quick start guide
```

### Module Development Principles

#### 1. **Encapsulation**
- Keep module internals private and expose only necessary APIs
- Use clear interfaces to define module contracts
- Minimize dependencies between modules

#### 2. **Initialization Order**
- Each module should handle its own initialization
- Use dependency injection for cross-module communication
- Implement proper cleanup on module destruction

#### 3. **Configuration Management**
- Store module settings in ScriptableObjects within Resources/
- Support runtime configuration changes where applicable
- Provide editor tools for easy configuration

#### 4. **Error Handling**
- Implement comprehensive error handling and logging
- Provide meaningful error messages for developers
- Fail gracefully without breaking other modules

#### 5. **Documentation Standards**
- Every public API must have XML documentation comments
- Include usage examples in Documents/ folder
- Keep README.md updated with latest changes
- Document any external dependencies or requirements

### Module Development
- Each module should be self-contained
- Use interfaces for external dependencies
- Follow dependency injection patterns
- Implement proper error handling

### Data Management
- Use ScriptableObjects for game data
- Implement IReadable/IWriteable for persistence
- Use RuntimeCollections for dynamic updates
- Validate data integrity

### Networking
- Always consider network authority
- Use NetworkBehaviour for networked objects
- Implement proper client prediction
- Handle connection failures gracefully

### Performance
- Use object pooling for frequently created objects
- Optimize render pipeline settings
- Profile regularly with Unity Profiler
- Consider mobile performance constraints

### Testing
- Write unit tests for core logic
- Test network scenarios thoroughly
- Validate data serialization
- Test on target platforms

## 📝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Update documentation
6. Submit a pull request

## 🐛 Known Issues

- Refer to Issues tab in repository
- Check Unity Console for runtime errors
- Monitor network performance in builds

## 📄 License

[Add your license information here]

---

**Last Updated**: January 2025  
**Unity Version**: 2022.3 LTS  
**Target Platforms**: Windows, Android, iOS