#!/bin/bash

# Kéo bản sao mới nhất từ Docker Hub
docker pull phuchoang1910/webapp_htsv:latest

# Xóa container hiện tại nếu có
docker stop webapp_htsv || true
docker rm webapp_htsv || true

# Chạy container mới
docker run --name webapp_htsv -d -p 80:80 -p 443:443 phuchoang1910/webapp_htsv:latest
