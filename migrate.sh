#!/bin/sh
if test -d "Bookworms-Server/Migrations"; then
	rm -rf "Bookworms-Server/Migrations"
fi

if ! dotnet ef database drop --project Bookworms-Server/Bookworms-Server.csproj --context BookwormsServer.BookwormsDbContext --configuration Debug --force; then
	exit 1
fi

if ! dotnet ef migrations add --project Bookworms-Server/Bookworms-Server.csproj --context BookwormsServer.BookwormsDbContext --configuration Debug Initial --output-dir Migrations; then
	exit 1
fi

git add "Bookworms-Server/Migrations"