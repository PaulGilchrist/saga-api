# docker build --rm -f "Dockerfile" --no-cache --platform linux/arm64 -t paulgilchrist/mongodb-api:arm64 .
# docker push paulgilchrist/mongodb-api:arm64
# docker build --rm -f "Dockerfile" --no-cache --platform linux/amd64 -t paulgilchrist/mongodb-api:amd64 .
# docker push paulgilchrist/mongodb-api:amd64
# docker manifest create paulgilchrist/mongodb-api:latest --amend paulgilchrist/mongodb-api:arm64 --amend paulgilchrist/mongodb-api:amd64
# docker manifest push paulgilchrist/mongodb-api:latest
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

# docker run -d -p 8081:80 paulgilchrist/mongodb-api
# docker rm -f <containerID>
