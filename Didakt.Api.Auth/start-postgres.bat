@echo off
docker ps --filter "name=didakt-postgres" --filter "status=running" -q | find /i "." >nul 2>&1
if errorlevel 1 (
    docker ps -a --filter "name=didakt-postgres" -q | find /i "." >nul 2>&1
    if errorlevel 1 (
        docker run -d --name didakt-postgres -p 5432:5432 -e POSTGRES_USER=didakt_user -e POSTGRES_PASSWORD=didakt_dev -e POSTGRES_DB=didakt postgres:17
    ) else (
        docker start didakt-postgres
    )
)