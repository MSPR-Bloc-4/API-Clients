# Use the official image as a parent image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Accept Firebase credentials as a build-arg
ARG FIREBASE_CREDENTIALS_JSON

# Set Firebase credentials as an environment variable
ENV FIREBASE_CREDENTIALS_JSON=${FIREBASE_CREDENTIALS_JSON}

# Copy the published output from build stage
COPY --from=build-env /app/out .

# Use CMD instead of ENTRYPOINT to allow easy overriding
CMD ["dotnet", "Client-Api.dll"]