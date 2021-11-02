# ASP.NET Core Alpine 

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build-env
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
EXPOSE 80

# Move source files to /src folder
COPY . /src

WORKDIR /src
# Build the release executable
RUN dotnet publish "./OKRAdminService.WebCore/OKRAdminService.WebCore.csproj" -c Release -o /out

# Release image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add icu-libs

# Copy binaries from build-env /out to alpine's folder (/app)
COPY --from=build-env /out /app

WORKDIR /app
ENTRYPOINT ["dotnet", "OKRAdminService.WebCore.dll"]