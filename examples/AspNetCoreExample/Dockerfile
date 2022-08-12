# FROM mcr.microsoft.com/dotnet/aspnet:3.1-focal AS base
FROM mcr.microsoft.com/dotnet/sdk:3.1-focal AS build
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

# FROM base AS final
WORKDIR /app
# COPY --from=publish /app/publish .
ADD ./bin/Release/netcoreapp3.1/* /app
ENTRYPOINT ["dotnet", "AspNetCoreExample.dll"]
