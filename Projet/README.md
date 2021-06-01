# IOT - Projet robot

Le projet contient :

- SolutionIOT : serveur applicatif et controller pour les robots.
- Angular : UI Web angular pour la visualisation de la flotte de robot.
- Script : Contient tous les scripts d'installation, ou d'éxecution. 
- ESP : Tous les projets concernant les applications des ESP.
- PythonProject : Tous les fichiers et le script pour la reconnaissance faciale.
- Mobile_App : Application mobile de localisation GPS pour l'ESP.
- DockerCompose : Contient tous les docker compose pour l'éxecution des applications, database. 
- Certificates : Contient tous les certificats pour les différentes applications. 

A noter que chaque partie du projet contient son propre readme.md ! 

# Déploiment 

Le déploiment des conteneurs se fais sur une machine virtuel Linux dans le Cloud, se référer au dossier Script pour l'installation de l'environnement Linux. 

Nous utilisons les conteneurs Docker pour héberger nos applications, et Docker compose pour la définition et l'éxecution de nos applications multi-contenuers. Se référer au dossier DockerCompose pour la définition des applications et l'éxecution. 

