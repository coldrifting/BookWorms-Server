rmdir /s /q Bookworms-Server\Migrations

dotnet ef database drop --project Bookworms-Server\Bookworms-Server.csproj --context BookwormsServer.BookwormsDbContext --configuration Debug --force
dotnet ef migrations add --project Bookworms-Server\Bookworms-Server.csproj --context BookwormsServer.BookwormsDbContext --configuration Debug Initial --output-dir Migrations

git add Bookworms-Server/Migrations/
