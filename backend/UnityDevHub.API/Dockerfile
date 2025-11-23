FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["UnityDevHub.API.csproj", "./"]
RUN dotnet restore "UnityDevHub.API.csproj"
COPY . .
RUN dotnet publish "UnityDevHub.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UnityDevHub.API.dll"]
