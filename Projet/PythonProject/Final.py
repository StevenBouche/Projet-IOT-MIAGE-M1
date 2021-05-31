#!/usr/bin/env python
# coding: utf-8


print('################################Chargement des packages... #####################################')
# # Reconnaissance Faciale
#Chargement des librairies nécessaires
#Installation à faire au préalable : 
#pip install face_recognition
#pip install opencv-python

import face_recognition
import cv2
import numpy as np
import os
import glob
import operator

#Chargement du modèle pour détection de visage

faceCascade = cv2.CascadeClassifier('D:/Cours/M1_MIAGE/Semestre2/IOT/PythonProject/data/HaarcascadeModels/haarcascade_frontalface_default.xml')


from cv2 import *

#Prendre les photos des personnes connues du système

faces_encodings = []
faces_names = []
list_of_files = ['D:/Cours/M1_MIAGE/Semestre2/IOT/PythonProject/data/faces/Jean-Francois.jpg',
 'D:/Cours/M1_MIAGE/Semestre2/IOT/PythonProject/data/faces/Lina-Belkarfa.jpg',
 'D:/Cours/M1_MIAGE/Semestre2/IOT/PythonProject/data/faces/Corentin-Garnier.jpg',
 'D:/Cours/M1_MIAGE/Semestre2/IOT/PythonProject/data/faces/Steven-Bouche.jpg',
 'D:/Cours/M1_MIAGE/Semestre2/IOT/PythonProject/data/faces/Pierre-Griseri.jpg']
names = list_of_files.copy()
number_files = len(list_of_files)
cur_direc = 'D:/Cours/M1_MIAGE/Semestre2/IOT/PythonProject/data/faces/'

#Chercher les visages dans les photos 

print('############## Detection et apprentissage des visages de la Base de données... ################# ')
print('################################   '+str(number_files)+' photos à analyser :  ######################################')
for i in range(number_files):

    globals()['image_{}'.format(i)] = face_recognition.load_image_file(list_of_files[i])
    
    if not(face_recognition.face_encodings(globals()['image_{}'.format(i)])):
        print('Pas de visage detecté!')
        names[i] = names[i].replace(cur_direc, "No detected ")  

    else:
        globals()['image_encoding_{}'.format(i)] = face_recognition.face_encodings(globals()['image_{}'.format(i)])[0]
        faces_encodings.append(globals()['image_encoding_{}'.format(i)])

        names[i] = names[i].replace(cur_direc, "")  
        names[i] = names[i].replace(".jpg", "")  
        faces_names.append(names[i])
        print('#  Visage n°'+str(i+1)+ ' détecté!')
    

#Voir les noms des visages reconnus
print('Noms des personnes connues de la Base de données :\n'+str(names))
names



face_locations = []
face_encodings = []
face_names = []
process_this_frame = True

#Lancement du programme de reconnaissance faciale : 

video_capture = cv2.VideoCapture(0)

detection=[]
nbInconnus=0
name=""

while True:
    ret, frame = video_capture.read()
    
    small_frame = cv2.resize(frame, (0, 0), fx=0.25, fy=0.25)
    
    rgb_small_frame = small_frame[:, :, ::-1]
    
    if process_this_frame:
        face_locations = face_recognition.face_locations(rgb_small_frame)
        face_encodings = face_recognition.face_encodings(rgb_small_frame, face_locations)
        
        face_names = []
        for face_encoding in face_encodings:
            matches = face_recognition.compare_faces(faces_encodings, face_encoding)
            name = "Unknown"
            
            face_distances = face_recognition.face_distance(faces_encodings, face_encoding)
            best_match_index = np.argmin(face_distances)
            if matches[best_match_index]:
                name = faces_names[best_match_index]

            face_names.append(name)

    process_this_frame = not process_this_frame
    
    # Display the results
    for (top, right, bottom, left), name in zip(face_locations, face_names):
        top *= 4
        right *= 4
        bottom *= 4
        left *= 4
    
    # Draw a rectangle around the face
        cv2.rectangle(frame, (left, top), (right, bottom), (0, 0, 255), 2)
    # Input text label with a name below the face
        cv2.rectangle(frame, (left, bottom - 35), (right, bottom), (0, 0, 255), cv2.FILLED)
        font = cv2.FONT_HERSHEY_DUPLEX
        cv2.putText(frame, name, (left + 6, bottom - 6), font, 1.0, (255, 255, 255), 1)

    font                   = cv2.FONT_HERSHEY_SIMPLEX
    topLeftCornerOfText = (10,25)
    fontScale              = 0.6
    fontColor              = (255,255,255)
    lineType               = 2

    cv2.putText(frame,'Detected:', topLeftCornerOfText, font, fontScale,fontColor,lineType)
    cv2.putText(frame,'Exit code: q', (500,25), font, 0.6,fontColor,lineType)    
    
    index=25
    for i in names :
        if name == i :
            if name not in detection :
                detection.append(name)
                
    if name == str("Unknown"):
        nbInconnus=nbInconnus+1
        name = ""

            
    detec=""        
    for i in detection :
        index=index+30
        cv2.putText(frame,i, (10,index), font, fontScale,fontColor,lineType)        

    cv2.putText(frame,"Unknowns:"+str(nbInconnus), (10,450), font, fontScale,fontColor,lineType)
    
    # Display the resulting image
    cv2.imshow('Video', frame)
    # Hit 'q' on the keyboard to quit!
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break
        
cv2.destroyAllWindows()

print('######################## Fin du programme de surveillance. ########################')

