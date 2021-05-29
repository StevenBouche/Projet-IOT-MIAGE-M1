#!/bin/bash

# Update and upgrate
sudo apt-get update && apt-get upgrade

# Install Git
sudo apt install git-all
sudo git --version

# Install Docker

sudo apt-get remove docker docker-engine docker.io containerd runc

#apt-transport-https : permet au gestionnaire de packages de transférer des fichiers et des données via https
#certificats ca : permet au système (et au navigateur Web) de vérifier les certificats de sécurité
#curl : il s'agit d'un outil de transfert de données
#software-properties-common : ajoute des scripts pour gérer les logiciels
sudo apt-get install apt-transport-https ca-certificates curl gnupg lsb-release

#Pour vous assurer que le logiciel que vous installez est authentique
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg

#Pour installer le référentiel Docker
#La commande « $ (lsb_release –cs )» analyse et renvoie le nom de code de votre installation Ubuntu. 
#De plus, le dernier mot de la commande - stable - est le type de version de Docker.
 echo \
  "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

#Mettez à jour les référentiels après ajout
sudo apt-get update

#Pour installer la dernière version de docker
sudo apt-get install docker-ce docker-ce-cli containerd.io

sudo docker --version

# Install docker compose

sudo curl -L "https://github.com/docker/compose/releases/download/1.29.1/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
sudo docker-compose --version

mkdir github_repo
cd github_repo


# Git Clone repository


git clone https://github.com/StevenBouche/IOT-Second-Repo.git

