@echo off
echo Stopping TinyURL Microservices...
echo.

echo Stopping all dotnet processes...
taskkill /f /im dotnet.exe > nul 2>&1

echo Closing service windows...
taskkill /f /fi "WindowTitle eq AuthService*" > nul 2>&1
taskkill /f /fi "WindowTitle eq ShortenURL*" > nul 2>&1
taskkill /f /fi "WindowTitle eq ManagerURL*" > nul 2>&1
taskkill /f /fi "WindowTitle eq QRService*" > nul 2>&1
taskkill /f /fi "WindowTitle eq OcelotGateway*" > nul 2>&1

echo.
echo All TinyURL services have been stopped.
pause