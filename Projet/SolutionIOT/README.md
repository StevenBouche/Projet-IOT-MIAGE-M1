# Solution IOT

Solution IOT contient :

- Serveur d'application ASPNET : Projects/APIRobot
- L'application de controlle du robot : Projects/ControlerWPF

# Description du serveur applicatif ASPNET

- Serveur contenant une API permettant le listening de la flotte de robot. 
- Serveur TCP pour la récuperation des images des caméras de la flotte de robot.
- Serveur MQTT pour la récupèration des données de la flotte de robot, ainsi que transmettre les controlles à certains robot.
- Serveur disponible sous websocket pour récupèrer le listening des robots, les données des robots ainsi que le stream des images et du controlle du robot.

## Configuration de l'application 

L'application est configurer via plusieurs données contenu dans des fichiers JSON. Cela permet de configurer les différentes fonctionnalités du serveur d'application. Tous les fichiers sont contenu dans : APIRobot/AppSettings.
Il y à deux versions de chaque JSON, une pour le developement local, et la deuxième version pour la mise en production de l'application. 

### Configuration général : APIRobot/AppSettings/

Contient la configuration pour les CORS policy ainsi que pour l'import des certificats permettant l'activation SSL/TLS pour les websockets, MQTT et l'API. 

### Configuration de la base de données : APIRobot/AppSettings/Database

Contient toutes les strings de connections aux différentes Databases ou Collections. 

### Configuration des services : APIRobot/AppSettings/Services 

Contient toutes les configurations des services de l'application. En particulier la configuration lié à la génération des JWT tokens d'authentification.

### Configuration des services : APIRobot/AppSettings/HostedServices 

Contient toutes les configurations des services hébérgés c'est à dire : TCP et MQTT. 

## Congiguration actuel des services 

- API disponible sur le port 8000 et sur le port 8001 pour HTTPS.
- MQTT disponible sur le port 1883 et sur le port 8884 pour TLS.
- TCP disponible sur le port 11000.




