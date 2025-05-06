# Stage 1: Build Angular application
FROM node:20 AS angular-build
WORKDIR /app

# Copy Angular project files
COPY src/ReData.Angular/package.json src/ReData.Angular/package-lock.json ./angular/
WORKDIR /app/angular

# Install dependencies
RUN npm install

# Copy remaining Angular files
COPY src/ReData.Angular .

# Build Angular for production
RUN npm run build -- --configuration=production

# Stage 2: Build .NET application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dotnet-build
WORKDIR /app

# Create src directory structure
RUN mkdir -p src

COPY src/ReData.DemoApplication/*.csproj ./src/ReData.DemoApplication/
COPY src/Pattern/*.csproj ./src/Pattern/
COPY src/ReData.Common/*.csproj ./src/ReData.Common/
COPY src/ReData.Query.Common/*.csproj ./src/ReData.Query.Common/
COPY src/ReData.Query/*.csproj ./src/ReData.Query/
COPY src/ReData.Query.Core/*.csproj ./src/ReData.Query.Core/
COPY src/ReData.Query.Lang/*.csproj ./src/ReData.Query.Lang/

RUN dotnet restore src/ReData.DemoApplication/ReData.DemoApplication.csproj
# Copy all remaining source code
COPY src/ .

# Build and publish the main project

RUN dotnet publish ReData.DemoApplication/ReData.DemoApplication.csproj \
    -c Release \
    -o out \
    -p:WebRootPath=wwwroot

# Stage 3: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy the published output
COPY --from=dotnet-build /app/out . 

COPY --from=angular-build /app/angular/dist/re-data.angular/browser ./wwwroot

# Expose ports (adjust as needed)
EXPOSE 8080

ENTRYPOINT ["dotnet", "ReData.DemoApplication.dll"]