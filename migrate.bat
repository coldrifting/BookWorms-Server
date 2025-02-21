@ECHO OFF

if exist Bookworms-Server\Migrations rmdir /s /q Bookworms-Server\Migrations

dotnet ef database drop --project Bookworms-Server\Bookworms-Server.csproj --context BookwormsServer.BookwormsDbContext --configuration Debug --force
if %errorlevel% neq 0 exit /b %errorlevel%

dotnet ef migrations add --project Bookworms-Server\Bookworms-Server.csproj --context BookwormsServer.BookwormsDbContext --configuration Debug Initial --output-dir Migrations
if %errorlevel% neq 0 exit /b %errorlevel%

git add Bookworms-Server/Migrations/
