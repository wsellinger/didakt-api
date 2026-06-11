@echo off
docker ps --filter "name=didakt-redis" --filter "status=running" -q | find /i "." >nul 2>&1
if errorlevel 1 (
    docker ps -a --filter "name=didakt-redis" -q | find /i "." >nul 2>&1
    if errorlevel 1 (
        docker run -d --name didakt-redis -p 6379:6379 redis
    ) else (
        docker start didakt-redis
    )
)