import numpy as np
import cv2 as cv

def displayBox(im, bbox):
    n = len(bbox)
    for j in range (n):
        cv.line(im, tuple(bbox[j][0]), tuple(bbox[(j+1) % n][0]), (255,0,0), 3)
    cv.imshow("Results", im)

videoFlux = cv.VideoCapture(0)

def detecQRCode():
    qrcode_value=0
    while(1):
        print("test")
        _, frame = videoFlux.read()
        hsv = cv.cvtColor(frame, cv.COLOR_BGR2HSV)
        lowColor = np.array([110,45,45])
        upColor = np.array([130,255,255])
        mask = cv.inRange(hsv, lowColor, upColor)
        res = cv.bitwise_and(frame, frame, mask = mask)   
        a=220
        b=0
        c=440
        d=900
        rect1= cv.rectangle(frame, (a,b),(c,d),(0,0,255),5)
        a2=270
        b2=150
        c2=400
        d2=300
        rect2 = cv.rectangle(frame, (a2, b2),(c2,d2),(255,0,0),3)
        font=cv.FONT_HERSHEY_SIMPLEX
        if ((c-a)>(c2-a2) and (d-b)>(d2-b2)):
            cv.putText(frame,'IN',(10,500), font, 4,(0,255,0),2,cv.LINE_AA)
        else :
            cv.putText(frame,'OUT',(10,500), font, 4,(0,0,255),2,cv.LINE_AA)

    		#r=cv.selectROI(frame)
    		#imCrop = frame[int(r[1]):int(r[1]+r[3]), int(r[0]):int(r[0]+r[2])]
    		#cv.imshow('im', imCrop)
            cv.imshow('frame', frame)        
    		#cv.imshow('mask', mask)
            cv.imshow('res', res)

    		# ------
            qrDecoder = cv.QRCodeDetector()
            qrCodeToFind = cv.imread("qrcode.png")
            
            data, bbox, rectifiedImage = qrDecoder.detectAndDecode(frame)
            if len(data)>0:
                qrcode_value=1
                displayBox(frame, bbox)
                rectifiedImage = np.uint8(rectifiedImage)
            cv.imshow("qrcode", frame)
            k = cv.waitKey(5) & 0xFF
            if k == 27:
                break;
    return qrcode_value

detecQRCode()

def gestionImage():
    while(detecQRCode()):
        print("fonctionne")
        #on gere ici la position de la camera pour que le qrcode soit toujours au centre
cv.destroyAllWindows()
