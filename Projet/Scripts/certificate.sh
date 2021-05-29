#!/bin/bash

# Update 
sudo apt update

# Install
sudo apt install build-essential checkinstall zlib1g-dev -y

# Install OpenSSL

cd /usr/local/src/

wget https://www.openssl.org/source/openssl-1.1.1k.tar.gz
tar -xf openssl-1.1.1k.tar.gz
cd openssl-1.1.1k

./config --prefix=/usr/local/ssl --openssldir=/usr/local/ssl shared zlib

make
make test
make install

cd /etc/ld.so.conf.d/
nano openssl-1.1.1k.conf

#/usr/local/ssl/lib copy past

#echo /usr/local/ssl/lib > openssl-1.1.1k.conf

sudo ldconfig -v
mv /usr/bin/c_rehash /usr/bin/c_rehash.BEKUP
mv /usr/bin/openssl /usr/bin/openssl.BEKUP

#vim /etc/environment PATH="/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:/usr/games:/usr/local/games:/usr/local/ssl/bin"

openssl version -a

# Generation Certificats

# Generate CA private key
openssl genrsa -des3 -out rootCA.key 2048
# Generate CA certificat
openssl req -x509 -new -nodes -key rootCA.key -sha256 -days 1024 -out rootCA.crt
# Generate Server private key
openssl genrsa -out privateKey.key 2048
# Generate Corporate social responsibility
openssl req -new -sha256 -key privateKey.key -config openssl.cnf -out certificate.csr
# Generate Server certificat signed with CA
openssl x509 -req -in certificate.csr -CA rootCA.crt -CAkey rootCA.key -CAcreateserial -out certificat.crt -days 3650 -sha256
# Generate pfx for storing many cryptography objects
openssl pkcs12 -export -out certificate.pfx -inkey private.key -in certificat.crt

#openssl req -x509 -nodes -days 3650 -newkey rsa:2048 -sha256 -keyout privateKey.key -out certificat.crs -config openssl.cnf

