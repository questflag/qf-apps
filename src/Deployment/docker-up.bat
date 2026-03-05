@echo off
echo Starting Docker Compose...
docker compose pull
docker compose up -d
echo.
pause
