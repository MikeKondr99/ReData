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
# RUN npm run build -- --configuration=production
RUN node --max_old_space_size=1024 ./node_modules/@angular/cli/bin/ng build --configuration=production

# Stage 2: Build .NET application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dotnet-build
WORKDIR /app

# Create src directory structure
# RUN mkdir -p src

# COPY src ./src

# RUN dotnet restore src/ReData.DemoApplication/ReData.DemoApplication.csproj
# Copy all remaining source code
COPY src/ ./src

RUN dotnet restore src/ReData.DemoApp/ReData.DemoApp.csproj

# Build and publish the main project

RUN dotnet publish src/ReData.DemoApp/ReData.DemoApp.csproj \
    -c Release \
    --no-restore \
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

ENTRYPOINT ["dotnet", "ReData.DemoApp.dll"]