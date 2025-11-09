# Stage 1 - Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY backend.csproj ./
RUN dotnet restore backend.csproj

COPY . ./

RUN dotnet publish ./backend.csproj -c Release -o /out


# Stage 2 - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /out .

ENTRYPOINT ["dotnet", "backend.dll"]
