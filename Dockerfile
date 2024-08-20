# Step 1: Use ASP.NET 6.0 image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Step 2: Add the certificate file to the container
COPY Certificates/your_certificate.pfx /app/your_certificate.pfx

# Step 3: Set up HTTPS for Kestrel and configure certificate
ENV ASPNETCORE_URLS=http://+:80;https://+:443
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/your_certificate.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__KeyPassword=$PFX_PASSWORD

# Step 4: Build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .

# Declare ARG variables to pass from GitHub secrets during the Docker images build process
ARG DB_PASSWORD
ARG SMTP_PASSWORD
ARG PFX_PASSWORD

# Set environment variables at runtime
ENV DB_PASSWORD=$DB_PASSWORD
ENV SMTP_PASSWORD=$SMTP_PASSWORD
ENV PFX_PASSWORD=$PFX_PASSWORD

# Replace the ${secrets. } string in appsettings.json with the value of the environment variable
RUN sed -i "s|\${secrets.DB_PASSWORD}|$DB_PASSWORD|g" appsettings.json
RUN sed -i "s|\${secrets.SMTP_PASSWORD}|$SMTP_PASSWORD|g" appsettings.json
RUN sed -i "s|\${secrets.PFX_PASSWORD}|$PFX_PASSWORD|g" appsettings.json

RUN dotnet restore Manage_CLB_HTSV.csproj
RUN dotnet build Manage_CLB_HTSV.csproj -c Release -o /app/build

# Step 5: Publish the application
FROM build AS publish
RUN dotnet publish Manage_CLB_HTSV.csproj -c Release -o /app/publish

# Step 6: Build the final application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Manage_CLB_HTSV.dll"]
