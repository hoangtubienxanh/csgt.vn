FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS base

# Workaround for local development environment
# See original issue: https://github.com/dotnet/runtime/issues/98797
COPY --from=mcr.microsoft.com/dotnet/aspnet:7.0 /etc/ssl/openssl.cnf /etc/ssl/openssl.cnf

# Install missing depedencies for TesseractCSharp
RUN apt-get update \
  && apt-get install -y --no-install-recommends \
    libleptonica-dev \
    libtesseract-dev \
  && apt-get clean 

# TesseractCSharp doesn't support platform-specific runtime packages
# Link system Tesseract libraries
WORKDIR /app/tesseractLib/linux_x64
RUN ln -s /usr/lib/x86_64-linux-gnu/liblept.so.5 /app/tesseractLib/linux_x64/libleptonica.so.6.0.0.so
RUN ln -s /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /app/tesseractLib/linux_x64/libtesseract.so.5.3.4.so
    
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Directory.Packages.props", "src/Directory.Packages.props"]
COPY ["src/VietnamTrafficPolice.WebApi/VietnamTrafficPolice.WebApi.csproj", "src/VietnamTrafficPolice.WebApi/"]
COPY ["src/VietnamTrafficPolice/VietnamTrafficPolice.csproj", "src/VietnamTrafficPolice/"]
COPY ["src/VietnamTrafficPolice.ServiceDefaults/VietnamTrafficPolice.ServiceDefaults.csproj", "src/VietnamTrafficPolice.ServiceDefaults/"]
COPY ["Ringleader/Ringleader/Ringleader.csproj", "Ringleader/Ringleader/"]
RUN dotnet restore "src/VietnamTrafficPolice.WebApi/VietnamTrafficPolice.WebApi.csproj"
COPY . .
WORKDIR "/src/src/VietnamTrafficPolice.WebApi"
RUN dotnet build "VietnamTrafficPolice.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "VietnamTrafficPolice.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VietnamTrafficPolice.WebApi.dll"]
