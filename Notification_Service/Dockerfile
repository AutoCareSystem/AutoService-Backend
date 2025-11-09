# Stage 1 - Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY Notification_Service.csproj ./
RUN dotnet restore Notification_Service.csproj

COPY . ./

RUN dotnet publish ./Notification_Service.csproj -c Release -o /out


# Stage 2 - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /out .

ENTRYPOINT ["dotnet", "Notification_Service.dll"]
