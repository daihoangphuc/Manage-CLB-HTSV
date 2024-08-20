# Step 1: Base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Step 2: Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .

# Restore, build, and publish
RUN dotnet restore "Manage_CLB_HTSV.csproj"
RUN dotnet build "Manage_CLB_HTSV.csproj" -c Release -o /app/build
RUN dotnet publish "Manage_CLB_HTSV.csproj" -c Release -o /app/publish

# Step 3: Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Copy certificates
COPY Certificates/your_certificate.pfx /app
COPY Certificates/private.key /app

# Setting up environment variables
ENV ASPNETCORE_URLS="http://+:80;https://+:443"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/app/your_certificate.pfx"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="${PFX_PASSWORD}"

ENTRYPOINT ["dotnet", "Manage_CLB_HTSV.dll"]
