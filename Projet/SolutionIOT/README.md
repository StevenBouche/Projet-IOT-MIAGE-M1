# Solution IOT

Solution IOT contient :

- Serveur d'application ASPNET : Projects/APIRobot
- L'application de contrôle du robot : Projects/ControlerWPF
- Application permettant d'initialiser des données dans MongoDB : Projects/MongoSeeding (surtout pour l'authentification et les authorisations)

# IDE 

Le projet s'ouvre avec le fichier SolutionIOT.sln avec Visual Studio.

## SDK pour le Controler WPF

Le projet de contrôleur nécessite plusieurs SDK pour le dévelopement :

-  SDK kinect 1.8 : https://www.microsoft.com/en-us/download/details.aspx?id=40278
- .NET Framework 4.8 : https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net48-developer-pack-offline-installer
- .NET Core 3.1 et .Net 5 (Disponible par défaut dans l'environnement Visual Studio 2019)
-  Lien pour tous les SDKs : https://dotnet.microsoft.com/download/visual-studio-sdks

# Description du serveur applicatif ASPNET

- Serveur contenant une API permettant l'écoute de la flotte de robot. 
- Serveur TCP pour la récuperation des images des caméras de la flotte de robot.
- Serveur MQTT pour la récupèration des données de la flotte de robot, ainsi que transmettre les contrôles à certains robots.
- Serveur disponible sous websocket pour récupèrer l'écoute des robots, les données des robots ainsi que le stream des images et du contrôle du robot.

## Configuration de l'application 

L'application est configurée via plusieurs données contenues dans des fichiers JSON. Cela permet de configurer les différentes fonctionnalités du serveur d'application. Tous les fichiers sont contenus dans : APIRobot/AppSettings.

Il y a deux versions de chaque JSON, une pour le developement local, et la deuxième version pour la mise en production de l'application. 

### Configuration général : APIRobot/AppSettings/

Contient la configuration pour les CORS policy ainsi que pour l'import des certificats permettant l'activation SSL/TLS pour les websockets, MQTT et l'API. 

### Configuration de la base de données : APIRobot/AppSettings/Database

Contient toutes les chaînes de caractère de connexions aux différentes BDD ou Collections. 

### Configuration des services : APIRobot/AppSettings/Services 

Contient toutes les configurations des services de l'application. En particulier la configuration liée à la génération des JWT tokens d'authentification.

### Configuration des services : APIRobot/AppSettings/HostedServices 

Contient toutes les configurations des services hébérgés c'est à dire : TCP et MQTT. 

## Configuration actuelle des services 

- API disponible sur le port 8000 et sur le port 8001 pour HTTPS.
- MQTT disponible sur le port 1883 et sur le port 8884 pour TLS.
- TCP disponible sur le port 11000.




