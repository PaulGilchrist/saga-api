# bash build.sh
# kubectl rollout restart deployment contacts-api -n demo
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["contacts-api.csproj", "."]
RUN dotnet restore "./contacts-api.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "contacts-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "contacts-api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "contacts-api.dll"]

