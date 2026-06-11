@echo off
for /f %%i in ('docker ps --filter "name=didakt-postgres" --filter "status=running" -q') do set RUNNING=%%i
if defined RUNNING exit /b 0

for /f %%i in ('docker ps -a --filter "name=didakt-postgres" -q') do set EXISTS=%%i
if defined EXISTS (
    docker start didakt-postgres
) else (
    docker run -d --name didakt-postgres -p 5432:5432 -e POSTGRES_USER=didakt_user -e POSTGRES_PASSWORD=didakt_dev -e POSTGRES_DB=didakt postgres:17
)