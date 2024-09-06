# Step 1: Use ASP.NET 6.0 image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Step 2: Add certificate and private key files to the container
COPY Certificates/certificate.crt /app
COPY Certificates/private.key /app
COPY Certificates/your_certificate.pfx /app

# Step 3: Set up HTTPS for Kestrel
ENV ASPNETCORE_URLS=http://+:80;https://+:443
RUN sed -i 's/TLSv1.2/TLSv1.0 TLSv1.1 TLSv1.2/g' /etc/ssl/openssl.cnf

# Step 4: Create a new stage to execute dotnet dev-certs commands
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS certs
WORKDIR /app

ARG PFX_PASSWORD
RUN dotnet dev-certs https -ep /https/aspnetapp.pfx -p $PFX_PASSWORD
RUN openssl pkcs12 -in /https/aspnetapp.pfx -out /https/aspnetapp.pem -nodes -password pass:$PFX_PASSWORD

# Step 5: Install the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .

ARG DB_PASSWORD
ARG SMTP_PASSWORD
ARG PFX_PASSWORD
ARG MICROSOFT_CLIENT_ID
ARG MICROSOFT_CLIENT_SECRET

ENV DB_PASSWORD=$DB_PASSWORD
ENV SMTP_PASSWORD=$SMTP_PASSWORD
ENV PFX_PASSWORD=$PFX_PASSWORD
ENV MICROSOFT_CLIENT_ID=$MICROSOFT_CLIENT_ID
ENV MICROSOFT_CLIENT_SECRET=$MICROSOFT_CLIENT_SECRET

RUN sed -i "s|\${secrets.DB_PASSWORD}|$DB_PASSWORD|g" appsettings.json
RUN sed -i "s|\${secrets.SMTP_PASSWORD}|$SMTP_PASSWORD|g" appsettings.json
RUN sed -i "s|\${secrets.PFX_PASSWORD}|$PFX_PASSWORD|g" appsettings.json
RUN sed -i "s|\${secrets.MICROSOFT_CLIENT_ID}|$MICROSOFT_CLIENT_ID|g" appsettings.json
RUN sed -i "s|\${secrets.MICROSOFT_CLIENT_SECRET}|$MICROSOFT_CLIENT_SECRET|g" appsettings.json

RUN dotnet restore Manage-CLB-HTSV.generated.sln
RUN dotnet build Manage-CLB-HTSV.generated.sln -c Release -o /app/build

# Step 6: Publish the application
FROM build AS publish
RUN dotnet publish Manage-CLB-HTSV.generated.sln -c Release -o /app/publish

# Step 7: Build the final application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=certs /https/aspnetapp.pem /https/aspnetapp.pem

ENTRYPOINT ["dotnet", "Manage_CLB_HTSV.dll"]
