# Stage 1 - Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY Service_Management_Service.csproj ./
RUN dotnet restore Service_Management_Service.csproj

COPY . ./
RUN dotnet publish ./Service_Management_Service.csproj -c Release -o /out

# Stage 2 - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Use the Render-provided PORT environment variable if available
ENV ASPNETCORE_URLS=http://*:${PORT:-8080}
EXPOSE ${PORT:-8080}

COPY --from=build /out .

ENTRYPOINT ["dotnet", "Service_Management_Service.dll"]
