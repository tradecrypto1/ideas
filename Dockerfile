# Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY src/ClaudeCodeInstaller.Core/*.csproj ./src/ClaudeCodeInstaller.Core/
COPY src/ClaudeCodeInstaller.Console/*.csproj ./src/ClaudeCodeInstaller.Console/
COPY src/ClaudeCodeInstaller.WinForms/*.csproj ./src/ClaudeCodeInstaller.WinForms/
COPY tests/ClaudeCodeInstaller.Tests/*.csproj ./tests/ClaudeCodeInstaller.Tests/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY . .

# Build
RUN dotnet build --configuration Release --no-restore

# Publish console app
FROM build AS publish-console
WORKDIR /src/src/ClaudeCodeInstaller.Console
RUN dotnet publish --configuration Release --no-build --output /app/publish

# Publish WinForms app
FROM build AS publish-winforms
WORKDIR /src/src/ClaudeCodeInstaller.WinForms
RUN dotnet publish --configuration Release --no-build --output /app/publish

# Runtime image for console
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime-console
WORKDIR /app
COPY --from=publish-console /app/publish .
ENTRYPOINT ["dotnet", "ClaudeCodeInstaller.Console.dll"]

# Runtime image for WinForms (requires Windows base image)
FROM mcr.microsoft.com/dotnet/runtime:8.0-nanoserver-ltsc2022 AS runtime-winforms
WORKDIR /app
COPY --from=publish-winforms /app/publish .
ENTRYPOINT ["dotnet", "ClaudeCodeInstaller.WinForms.dll"]
