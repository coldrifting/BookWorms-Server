# Stage 1: Build the .NET application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Bookworms-Server/Bookworms-Server.csproj", "./"]
RUN dotnet restore
COPY Bookworms-Server/ ./
RUN dotnet publish ./Bookworms-Server.csproj -c Release -o /server/publish

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /server
COPY --from=build /server/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Bookworms-Server.dll"]