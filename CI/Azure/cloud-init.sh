#!/bin/bash

# Update system
apt-get update -y
apt-get upgrade -y

# Install Nginx
apt-get install nginx -y
ufw allow 'Nginx Full'

# Install Certbot for Let's Encrypt
apt-get install certbot python3-certbot-nginx -y

# Install Docker
apt-get install apt-transport-https ca-certificates curl software-properties-common -y
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -
add-apt-repository \
   "deb [arch=amd64] https://download.docker.com/linux/ubuntu \
   $(lsb_release -cs) \
   stable"
apt-get update -y
apt-get install docker-ce -y
usermod -aG docker ${adminUsername}

# Install .NET 8 SDK
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
apt-get update -y
apt-get install -y dotnet-sdk-8.0

# Example SSL setup (requires domain name already pointing to this VM IP)
# certbot --nginx -d yourdomain.com --non-interactive --agree-tos -m your-email@example.com
# systemctl reload nginx
