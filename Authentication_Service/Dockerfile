# Stage 1 - Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project files first (for caching)
COPY *.csproj ./
RUN dotnet restore ./backend.csproj

# Copy the rest of the source code
COPY . ./

# ✅ Copy .env file (so the app can load it if not passed from docker run)
COPY .env .env

# Build and publish
RUN dotnet publish ./backend.csproj -c Release -o /out


# Stage 2 - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Expose and set app URL
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Copy published output
COPY --from=build /out .

# ✅ Optional: copy .env again (for clarity)
COPY .env .env

ENTRYPOINT ["dotnet", "backend.dll"]
