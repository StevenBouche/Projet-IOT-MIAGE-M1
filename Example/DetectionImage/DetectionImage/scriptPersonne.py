import numpy as np
import cv2


def inside(r, q):
    rx, ry, rw, rh = r
    qx, qy, qw, qh = q
    return rx > qx and ry > qy and rx + rw < qx + qw and ry + rh < qy + qh


def draw_detections(img, rects, thickness = 1):
	direction=""
	for x, y, w, h in rects:
		pad_w, pad_h = int(0.15*w), int(0.05*h)
		cv2.rectangle(img, (x+pad_w, y+pad_h), (x+w-pad_w, y+h-pad_h), (0, 255, 0), thickness)
		x1=x+pad_w
		x2=x+w-pad_w
		a=220
		b=0
		c=440
		d=900
		rect1= cv2.rectangle(img, (a,b),(c,d),(0,0,255),5)
		font=cv2.FONT_HERSHEY_SIMPLEX
		if (((x1-a)>0) and ((c-x2)>0)):
			cv2.putText(frame,'IN',(10,500), font, 4,(0,255,0),2,cv2.LINE_AA)
			direction+="DATA:25:25:-50:50"
			return direction
		else :
		#cv.putText(frame,'OUT',(10,500), font, 4,(0,0,255),2,cv.LINE_AA)
		#Droite et gauche inversee si on se met a la place de la camera
			if ((x1-a)<0):
				cv2.putText(frame,'Droite',(10,500), font, 4,(0,0,255),2,cv2.LINE_AA)
				direction +="DATA:25:-25:-50:50"
				return direction
			else:
				cv2.putText(frame,'Gauche',(10,500), font, 4,(0,0,255),2,cv2.LINE_AA)
				direction+="DATA:-25:25:-50:50"
				return direction

if __name__ == '__main__':

    hog = cv2.HOGDescriptor()
    hog.setSVMDetector( cv2.HOGDescriptor_getDefaultPeopleDetector() )
    cap=cv2.VideoCapture(0)
    while True:
        _,frame=cap.read()
        found,w=hog.detectMultiScale(frame, winStride=(8,8), padding=(32,32), scale=1.05)
        draw_detections(frame,found)
        cv2.imshow('feed',frame)
        ch = 0xFF & cv2.waitKey(1)
        if ch == 27:
            break
    cv2.destroyAllWindows()
