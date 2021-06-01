# IOT - Projet robot

Le projet contient :

- SolutionIOT : serveur applicatif et controller pour les robots,
- Angular : UI Web angular pour la visualisation de la flotte de robot, 
- Script : contient tous les scripts d'installation, ou d'éxecution,  
- ESP : tous les projets concernant les applications des ESP,
- PythonProject : tous les fichiers et le script pour la reconnaissance faciale,
- Mobile_App : application mobile de localisation GPS pour l'ESP,
- DockerCompose : contient tous les docker compose pour l'éxecution des applications, database, 
- Certificates : contient tous les certificats pour les différentes applications. 

A noter que chaque partie du projet contient son propre readme.md ! 

# Déploiment 

Le déploiement des conteneurs se fait sur une machine virtuelle Linux dans le Cloud ; se référer au dossier Script pour l'installation de l'environnement Linux. 

Nous utilisons les conteneurs Docker pour héberger nos applications, et DockerCompose pour la définition et l'éxecution de nos applications multi-conteneurs ; se référer au dossier DockerCompose pour la définition des applications et l'éxecution. 

