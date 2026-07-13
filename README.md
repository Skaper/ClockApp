# Unity Clock Application

A clock application built with Unity, demonstrating clean architecture and development practices.

## Features

- **World Clock** with multiple timezones (Local, UTC, JST)
- **Timer** with countdown and audio notifications
- **Stopwatch** with lap time recording
- **NTP Synchronization** for accurate time
- **Smooth Animations** and responsive UI

## Architecture

**Clean Architecture** implementation with clear layer separation:

- **Domain**: Business logic (Clock, Timer, Stopwatch services)
- **Application**: Use cases and API integration
- **Infrastructure**: External services (Audio, NTP, Background tasks)
- **Presentation**: MVVM pattern with reactive UI binding

## Key Technologies

- **Unity 2022.3+** 
- **VContainer** - Dependency injection
- **UniRx** - Reactive programming
- **DOTween** - Animation system
- **C# Async/Await** - Asynchronous operations
- **NTP Protocol** - Network time synchronization

## Project Structure

```
├── Domain/          # Business logic and entities
├── Application/     # Use cases and services  
├── Infrastructure/  # External integrations
├── Presentation/    # MVVM UI layer
└── Bootstrap/       # DI setup and initialization
```

## Technical Highlights

- **Reactive Architecture** with Observable streams
- **Custom NTP Client** for network time sync
- **MVVM Pattern** with automatic UI binding
- **Dependency Injection** for testable, modular code
- **Background Processing** with lifecycle management
- **Assembly Definitions** for modular compilation

## Platform Support

Cross-platform: **iOS**, **Android**, **Desktop** (Windows/macOS/Linux)

## Code Quality

- **SOLID Principles** applied throughout
- **Clean Code** with proper separation of concerns
- **Memory Management** with IDisposable pattern
- **Error Handling** and resource cleanup
- **Performance Optimized** with reactive updates
