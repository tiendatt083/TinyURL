@echo off
echo Starting TinyURL Microservices...
echo.

echo Starting AuthService on port 7123...
start "AuthService" cmd /k "cd /d %~dp0AuthService && dotnet run"
timeout /t 3 /nobreak > nul

echo Starting ShortenURL Service on port 7098...
start "ShortenURL" cmd /k "cd /d %~dp0ShortenURL && dotnet run"
timeout /t 3 /nobreak > nul

echo Starting ManagerURL Service on port 7227...
start "ManagerURL" cmd /k "cd /d %~dp0ManagerURL && dotnet run"
timeout /t 3 /nobreak > nul

echo Starting QRService on port 7196...
start "QRService" cmd /k "cd /d %~dp0QRService && dotnet run"
timeout /t 3 /nobreak > nul

echo Starting OcelotGateway on port 7210...
start "OcelotGateway" cmd /k "cd /d %~dp0OcelotGateway && dotnet run"

echo.
echo All services are starting...
echo.
echo Service URLs:
echo - Gateway: https://localhost:7210
echo - AuthService: https://localhost:7123
echo - ShortenURL: https://localhost:7098
echo - ManagerURL: https://localhost:7227
echo - QRService: https://localhost:7196
echo.
echo Wait for all services to start before testing.
pause