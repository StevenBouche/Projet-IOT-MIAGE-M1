import socket
import numpy as np
from io import StringIO
import pickle
import cv2
import time

class numpysocket():
    def __init__(self):
        pass

    @staticmethod
    def startServer():
        port=10200
        server_socket=socket.socket() 
        server_socket.bind(('',port))
        server_socket.listen(1)
        # ----
        rpi_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, socket.IPPROTO_UDP)
        rpi_ipPort = ("172.20.10.5", 10000)

        print ( 'waiting for a connection...')
        client_connection,client_address=server_socket.accept()
        print ( 'connected to ',client_address[0])
        client_connection.send("Wait:Image".encode("utf-8"))
        while not client_connection._closed:
            data= []
            while True:
                receiving_buffer = client_connection.recv(4096)
                end = receiving_buffer.find(b"Picture:END")
                if end != -1: 
                    endPicture = receiving_buffer.rstrip(b"Picture:END")   
                    data.append(endPicture)                
                    break
                data.append(receiving_buffer)
            frame = pickle.loads(b"".join(data))
            #final_image=np.load(StringIO(ultimate_buffer))['frame']
            #client_connection.close()
            hog = cv2.HOGDescriptor()
            hog.setSVMDetector( cv2.HOGDescriptor_getDefaultPeopleDetector() )
            # capture frames from the camera
            found,w=hog.detectMultiScale(frame, winStride=(8,8), padding=(32,32), scale=1.05)
            order = draw_detections(frame,found)
            if order == None:
                order="DATA:0:0:0:0:0"
            print(order)
            rpi_socket.sendto(order.encode("utf-8"), rpi_ipPort)
            cv2.imshow('feed',frame)
            cv2.waitKey(1)

            #cv2.destroyAllWindows()
            client_connection.send("Wait:Image".encode("utf-8"))
        server_socket.close()


def inside(r, q):
    rx, ry, rw, rh = r
    qx, qy, qw, qh = q
    return rx > qx and ry > qy and rx + rw < qx + qw and ry + rh < qy + qh


def draw_detections(frame, rects, thickness = 1):
    for x, y, w, h in rects:
        pad_w, pad_h = int(0.15*w), int(0.05*h)
        cv2.rectangle(frame, (x+pad_w, y+pad_h), (x+w-pad_w, y+h-pad_h), (0, 255, 0), thickness)
        x1=x+pad_w
        x2=x+w-pad_w
        a=220
        b=0
        c=440
        d=900
        rect1= cv2.rectangle(frame, (a,b),(c,d),(0,0,255),5)
        font=cv2.FONT_HERSHEY_SIMPLEX
        if (((x1-a)>0) and ((c-x2)>0)):
            cv2.putText(frame,'IN',(10,500), font, 4,(0,255,0),2,cv2.LINE_AA)
            return "DATA:30:30:-50:50:0"
        else :
            #cv2.putText(frame,'OUT',(10,500), font, 4,(0,0,255),2,cv2.LINE_AA)
        #Droite et gauche inversee si on se met a la place de la camera
            if ((x1-a)<0):
                cv2.putText(frame,'Droite',(10,500), font, 4,(0,0,255),2,cv2.LINE_AA)
                return "DATA:31:29.5:-50:50:0"
            else:
                cv2.putText(frame,'Gauche',(10,500), font, 4,(0,0,255),2,cv2.LINE_AA)
                return "DATA:29.5:31:-50:50:0"

if __name__ == "__main__":
    numpysocket.startServer()