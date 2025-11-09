# Stage 1 - Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY Chatbot_Service.csproj ./
RUN dotnet restore Chatbot_Service.csproj

COPY . ./

RUN dotnet publish ./Chatbot_Service.csproj -c Release -o /out


# Stage 2 - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /out .

ENTRYPOINT ["dotnet", "Chatbot_Service.dll"]
