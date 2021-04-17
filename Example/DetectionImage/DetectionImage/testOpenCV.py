import numpy as np
import cv2 as cv

def displayBox(im, bbox):
	n = len(bbox)
	x1=0
	x2=10
	y1=0
	y2=10
	direction=""
	for j in range (n):
		b1=bbox[j][0]
		b2=bbox[(j+1) % n][0]
		t1=tuple(b1)
		t2=tuple(b2)
				
		valx1=t1[0]
		valx2= t2[0]
		valy1=t1[1]
		valy2=t2[1]
		#carré du qrcode
		cv.line(im, t1, t2, (200,0,0), 3)
		
		#Rectangle encadrant seulement une fois que le qr code est détecté
		a=220
		b=0
		c=440
		d=900
		rect1= cv.rectangle(frame, (a,b),(c,d),(0,0,255),5)

		#verifier si rectangle du qrcode est dans le rectangle central
		font=cv.FONT_HERSHEY_SIMPLEX

	#TODO : juste un todo parce que c'est stylé en fluo

		if (((valx1-a)>0) and ((c-valx2)>0)):
			cv.putText(frame,'IN',(10,500), font, 4,(0,255,0),2,cv.LINE_AA)
			direction+="DATA:25:25:-50:50"
			return direction
		else :
			#cv.putText(frame,'OUT',(10,500), font, 4,(0,0,255),2,cv.LINE_AA)
			#Droite et gauche inversée si on se met à la place de la caméra
			if ((valx1-a)<0):
				cv.putText(frame,'Droite',(10,500), font, 4,(0,0,255),2,cv.LINE_AA)
				direction +="DATA:25:-25:-50:50"
				return direction
				
			else:
				cv.putText(frame,'Gauche',(10,500), font, 4,(0,0,255),2,cv.LINE_AA)
				direction+="DATA:-25:25:-50:50"
				return direction

		#vo
		#cv.line(im, tuple(bbox[j][0]), tuple(bbox[(j+1) % n][0]), (255,0,0), 3)
		rectQr=cv.rectangle(frame,(x1,y1),(x2,y2),(255,0,0),2)
				
	cv.imshow("Results", im)
	return rotation
videoFlux = cv.VideoCapture(0)

while(1): 
	_, frame = videoFlux.read()
	hsv = cv.cvtColor(frame, cv.COLOR_BGR2HSV)
	lowColor = np.array([110,45,45])
	upColor = np.array([130,255,255])
	mask = cv.inRange(hsv, lowColor, upColor)
	res = cv.bitwise_and(frame, frame, mask = mask)	
		
	cv.imshow('frame', frame)		
	#cv.imshow('mask', mask)
	cv.imshow('res', res)

	# ------
	
	qrDecoder = cv.QRCodeDetector()
	qrCodeToFind = cv.imread("qrcode.png")
	data, bbox, rectifiedImage = qrDecoder.detectAndDecode(frame)
	if len(data)>0:
		rotation=displayBox(frame, bbox)
		rectifiedImage = np.uint8(rectifiedImage)
	#TODO : Return OK ??		

	cv.imshow("qrcode", frame)

	k = cv.waitKey(5) & 0xFF
	if k == 27:
		break;

cv.destroyAllWindows()



