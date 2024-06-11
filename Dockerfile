# Bước 1: Sử dụng image aspnet 6.0
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Bước 2: Thêm các tệp certificate và private key vào container
COPY Certificates/certificate.crt /app
COPY Certificates/private.key /app
COPY Certificates/your_certificate.pfx /app

# Bước 3: Thiết lập HTTPS cho Kestrel
ENV ASPNETCORE_URLS=http://+:80;https://+:443
RUN sed -i 's/TLSv1.2/TLSv1.0 TLSv1.1 TLSv1.2/g' /etc/ssl/openssl.cnf

# Bước 4: Tạo stage mới để thực thi các lệnh dotnet dev-certs
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS certs
WORKDIR /app

# Khai báo ARG để truyền biến từ build command 
ARG PFX_PASSWORD

# Sử dụng biến ARG với lệnh dotnet dev-certs
RUN dotnet dev-certs https -ep /https/aspnetapp.pfx -p $PFX_PASSWORD
RUN openssl pkcs12 -in /https/aspnetapp.pfx -out /https/aspnetapp.pem -nodes -password pass:$PFX_PASSWORD

# Bước 5: Cài đặt ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .

#Khai báo các biến ARG để truyền từ secret của github trong quá trình build docker images
ARG DB_PASSWORD
ARG SMTP_PASSWORD
ARG PFX_PASSWORD
# Bước 6: Thiết lập biến môi trường trong runtime
ENV DB_PASSWORD=$DB_PASSWORD
ENV SMTP_PASSWORD=$SMTP_PASSWORD
ENV PFX_PASSWORD=$PFX_PASSWORD

# Thay thế chuỗi ${secrets.DB_PASSWORD} trong tệp appsettings.json bằng giá trị của biến môi trường $DB_PASSWORD
RUN sed -i "s|\${secrets.DB_PASSWORD}|$DB_PASSWORD|g" appsettings.json

RUN sed -i "s|\${secrets.SMTP_PASSWORD}|$SMTP_PASSWORD|g" appsettings.json
RUN sed -i "s|\${secrets.PFX_PASSWORD}|$PFX_PASSWORD|g" appsettings.json

RUN dotnet restore
RUN dotnet build -c Release -o /app/build

# Bước 7: Publish ứng dụng
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Bước 8: Build ứng dụng cuối cùng
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=certs /https/aspnetapp.pem /https/aspnetapp.pem

ENTRYPOINT ["dotnet", "website_CLB_HTSV.dll"]
