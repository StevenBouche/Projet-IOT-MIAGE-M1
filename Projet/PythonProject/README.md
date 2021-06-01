# FACE RECOGNITION with OpenCV
Ce dossier contient tous les fichiers nécessaire au lancement du module de <strong>reconnaissance faciale</strong>.

## Introduction 

L’idée globale est de relier à notre robot un module d’identification de personnes et reconnaissance de celles-ci via openCV et en python. Il y a donc un dossier dans lequel une photo de chaque visage des personnes “connues” serait enregistrée.

Nous avions également pour projet d’utiliser ce module pour suivre les personnes. Mais le manque de temps à jouer en notre défaveur, car nous ne pouvions pas reconnaître une personne de dos (donc par conséquent on ne peut la suivre). 

Nous devions donc reconnaître les corps des gens, hors dans les modèles déjà existant il n’y a que les “détections” de corps et pas la “reconnaissance”. 

Il faudrait alors faire un apprentissage (qui prend des jours), pour créer un modèle qui reconnaîtrait les “corps” qu’il a déjà vu, donc qui apprend à reconnaître un corps. La deuxième possibilité était de croiser corps et visage afin de suivre le corps qui correspond au visage “cible”. Cependant cela ne fonctionnait pas bien, et vous comprendrez pourquoi en comprenant comment fonctionne la reconnaissance faciale.


## Fonctionnalités Actuelles 

- Apprentissage des visages qui ont été fourni dans le dossier "faces"
- Détection de tous les visages du stream
- Reconnaissance des personnes connues dans le stream (liste des personnes détectés qui s'affiche sur le stream)
- Comptage du nombre d'inconnus détectés

## Ce qui est nécessaire 

Dossier <strong>“PythonProject”</strong> contenant :
  - Le script <strong>“Final.py“</strong>
  - dossier <strong>“data”</strong> qui contient deux sous dossiers : 
      - <strong>“HaarcascadesModels”</strong> (où ce trouve les modèles d’openCV pour la reconnaissance). Ils ne sont pas tous utilisés dans le script mais ont été utilisés lors des tests pour le suivi de personne.
      - <strong>“faces”</strong> (où se trouve les photos des personnes à reconnaître)
  - Une <strong>ESP32 CAM</strong> connecté à un <strong>serveur</strong>
  - Environnement <strong>Python</strong>



## Mise en route


## À terminer 


