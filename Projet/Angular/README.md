# Dashboard Angular Iot

Ce front-office sert de <strong>dashboard</strong> aux ESP.

## Technologies utilisées

Cette application est codée en TypeScript sous le framework <strong>Angular</strong>.

## Fonctionnalités

Elle permet d'afficher les choses suivantes :

 - Température de l'environnement
 - Luminosité de l'environnement
 - Caméra en direct du robot
 - Localisation du robot via une carte
 - ESP connectés en direct
 - Utilisation des moteurs de l'ESP en direct

## Données

<strong>Toutes les données citées ci-dessus sont récupérées via des websocket !</strong>

En effet, dans un souci de réactivité, nous utilisons exclusivement ce protocole de temps réel.

Nous avons effectivement mis en place une API, mais celle-ci n'est pas connectée à ce front.

## Lancement de l'application 

Installation du CLI angular : npm install -g @angular/cli@11.2.12

Installation des packages : npm install 

Lancement de l'application :

- Locale sur la machine : npm run local
- Locale sur la machine SSL : npm run localSsl
- Dans docker SSL : npm run dockerSsl
- Dans docker SSL pour la production : npm run dockerSslProd


