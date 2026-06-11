@echo off
for /f %%i in ('docker ps --filter "name=didakt-redis" --filter "status=running" -q') do set RUNNING=%%i
if defined RUNNING exit /b 0

for /f %%i in ('docker ps -a --filter "name=didakt-redis" -q') do set EXISTS=%%i
if defined EXISTS (
    docker start didakt-redis
) else (
    docker run -d --name didakt-redis -p 6379:6379 redis
)