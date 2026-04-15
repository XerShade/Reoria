# Reoria

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://opensource.org/licenses/GPL-3.0)
[![C#](https://img.shields.io/badge/C%23-8.0+-blue.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![MonoGame](https://img.shields.io/badge/MonoGame-3.8+-purple.svg)](https://www.monogame.net/)

Reoria is a modern online action-adventure role-playing game built with C# and MonoGame. The project features a modular, server-authoritative multiplayer architecture designed for scalability and security.

## 🎮 Overview

Reoria is being developed as a cross-platform multiplayer RPG with a focus on:
- **Server-authoritative multiplayer** with robust security validation
- **Cross-platform support** (Windows, Desktop, Android, iOS)
- **Modular architecture** with separate client, server, and engine components
- **Entity Component System (ECS)** integration through MonoGame.Extended
- **Professional development practices** with dependency injection and comprehensive documentation

## 🏗️ Architecture

The project follows a clean, modular architecture:

### Core Components
- **Reoria.Engine** - Core game engine and networking framework
- **Reoria.Client** - Client-side logic and rendering
- **Reoria.Server** - Server-side game logic and multiplayer management

### Platform Support
- **Desktop Applications** - Windows, Linux, macOS
- **Mobile Applications** - Android, iOS
- **Docker Support** - Containerized server deployment

## 🚀 Features

### Multiplayer System
- **Server-authoritative architecture** with comprehensive security validation
- **Player management** with roles, permissions, and entity ownership
- **Session management** with secure authentication and lifecycle handling
- **Real-time networking** built on LiteNetLib for optimal performance

### Security & Permissions
- **Hierarchical permission system** for actions, commands, and entities
- **Entity ownership tracking** with access control validation
- **Security validation** for all client operations
- **Thread-safe components** designed for concurrent access

### Development Features
- **Dependency injection** using Autofac
- **Modular project structure** with shared projects for code reuse
- **Comprehensive documentation** including architecture guides
- **Docker containerization** for easy deployment

## 📁 Project Structure

```
Reoria/
├── Applications/           # Platform-specific applications
│   ├── Reoria.Client.Android/
│   ├── Reoria.Client.Desktop/
│   ├── Reoria.Client.iOS/
│   └── Reoria.Client.Windows/
├── Source/                # Core source code
│   ├── Client/           # Client-side components
│   ├── Engine/           # Game engine and networking
│   └── Server/           # Server-side components
├── Build/                # Build configurations and props
├── Documentation/        # Technical documentation
└── docker-compose.yml    # Docker deployment configuration
```

## 🛠️ Technology Stack

- **.NET 8.0+** - Modern C# development platform
- **MonoGame 3.8+** - Cross-platform game framework
- **MonoGame.Extended** - ECS and additional game utilities
- **LiteNetLib** - High-performance networking library
- **Autofac** - Dependency injection container
- **Docker** - Containerization and deployment

## 📖 Documentation

- [Server Authoritative Multiplayer System](Documentation/ServerAuthoritativeMultiplayer.md) - Comprehensive guide to the multiplayer architecture
- [License](LICENSE) - GPL v3 License

## 🔧 Development Status

**Current Status:** In Development

Reoria is actively being developed. The core architecture and multiplayer systems are designed and documented, with implementation in progress.

### Completed
- ✅ Project architecture and structure
- ✅ Server-authoritative multiplayer design
- ✅ Security and permission system design
- ✅ Cross-platform project setup
- ✅ Dependency injection configuration

### In Progress
- 🔄 Core engine implementation
- 🔄 Networking layer development
- 🔄 Client application frameworks
- 🔄 Server application logic

### Planned
- 📋 ECS integration
- 📋 Game content and systems
- 📋 UI/UX implementation
- 📋 Testing and optimization

## 🤝 Contributing

Reoria is open-source under the GPL v3 license. Contributions are welcome!

### Getting Started
1. Clone the repository
2. Ensure you have .NET 8.0 SDK installed
3. Open `Reoria.slnx` in Visual Studio or your preferred IDE
4. Build the solution to restore dependencies

### Development Guidelines
- Follow the existing architectural patterns
- Maintain the modular structure
- Update documentation for significant changes
- Ensure cross-platform compatibility

## 📄 License

This project is licensed under the GNU General Public License v3.0. See the [LICENSE](LICENSE) file for details.

## 🔮 Future Roadmap

- **Core Gameplay Systems** - Combat, inventory, quests
- **World Building** - Maps, environments, NPCs
- **User Interface** - Menus, HUD, game interfaces
- **Performance Optimization** - Network optimization, rendering improvements
- **Community Features** - Guilds, chat, social systems
- **Mod Support** - Plugin architecture for community content

---

**Note:** Reoria is currently in active development. Features and APIs may change as the project evolves.
