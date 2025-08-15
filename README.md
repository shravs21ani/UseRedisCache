
# UseRedisCache – Redis Caching with .NET 8

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Redis](https://img.shields.io/badge/Redis-Local%20Setup-brightgreen)](https://redis.io/)
[![codecov](https://codecov.io/gh/shravs21ani/UseRedisCache/branch/main/graph/badge.svg)](https://codecov.io/gh/shravs21ani/UseRedisCache)

This project demonstrates how to use **Redis caching in a .NET 8 Console Application** to simulate **Azure Redis Cache** in a **cost-effective local development environment** using WSL and Ubuntu.

GitHub Repo: [shravs21ani/UseRedisCache](https://github.com/shravs21ani/UseRedisCache)

---

## Technologies Used

- .NET 8
- StackExchange.Redis
- Redis Server (local)
- WSL2 with Ubuntu 22.04
- Visual Studio 2022 or VS Code
- GitHub Actions
- Codecov

---

## Steps to Run This Locally

### 1. Enable and Install WSL with Ubuntu

Open **PowerShell as Administrator** and run:
```powershell
wsl --install -d Ubuntu
```

If needed, enable features manually:
```powershell
dism.exe /online /enable-feature /featurename:Microsoft-Windows-Subsystem-Linux /all /norestart
dism.exe /online /enable-feature /featurename:VirtualMachinePlatform /all /norestart
```
Restart your PC, then run the install command again.

---

### 2. Launch Ubuntu & Install Redis

```bash
sudo apt update
sudo apt install redis-server
sudo service redis-server start
```

Verify Redis is running:
```bash
redis-cli ping
# Output should be: PONG
```

---

### 3. (Optional) Remove Unused WSL Distros

List all:
```powershell
wsl -l -v
```

Remove extras:
```powershell
wsl --unregister podman-machine-default
```

---

### 4. Clone and Run This .NET Project

```bash
git clone https://github.com/shravs21ani/UseRedisCache.git
cd UseRedisCache
```

Open in Visual Studio / VS Code and restore NuGet packages.

Make sure this line in `Program.cs` points to local Redis:
```csharp
private static readonly string redisConnectionString = "localhost:6379";
```

Then build and run the app.

---

## Setup Script (Optional)

To quickly configure Redis on WSL Ubuntu:
```bash
#!/bin/bash
sudo apt update && sudo apt install redis-server -y
sudo service redis-server start
redis-cli ping
```

Save as `install_redis.sh`, then run:
```bash
bash install_redis.sh
```

---

## What the App Does

- Tries to get user preference data from Redis.
- If not cached, fetches simulated DB data and caches it.
- Applies **sliding expiration** and **cache invalidation**.
- Proves Redis caching logic works before deploying to Azure.

---

## CI/CD Integration

- GitHub Actions workflow:
  - Restores dependencies
  - Builds the solution
  - Runs tests
  - Starts Redis container service for integration testing
  - Uploads coverage reports (Cobertura + HTML)
  - Pushes coverage results to [Codecov](https://app.codecov.io/gh/shravs21ani/UseRedisCache)

---

## Screenshots

![Redis Local Walkthrough](Redis_Local_Cache_Complete_Walkthrough.png)

---

## Notes

- This simulates Azure Redis in local environments to **cut cloud costs during development**.
- Helps teams test caching logic before pushing to production cloud environments.

---

## Coverage

Coverage is uploaded and tracked via [Codecov](https://codecov.io/gh/shravs21ani/UseRedisCache)

---

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).
