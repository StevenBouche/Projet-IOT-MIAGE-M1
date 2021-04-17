import numpy as np
import pickle
import io
import struct
import os
import signal

from picamera.array import PiRGBArray
from picamera import PiCamera
import time

import cv2 as cv


from UDPThread import UDPThread
import socket

class UnicastDetection(UDPThread):
    
    def __init__(self, ip_portServer, name="Unicast Connection"):
        UDPThread.__init__(self, name)
        self.client_socket=socket.socket()
        try:
            self.client_socket.connect(ip_portServer)
        except socket.error:
            return
        self.client_socket.setsockopt(socket.SOL_SOCKET, 35, 1)
        # initialize the camera and grab a reference to the raw camera capture
        self.camera = PiCamera()
        self.camera.resolution = (640, 480)
        self.camera.framerate = 15
        self.rawCapture = PiRGBArray(self.camera, size=(640, 480))
        time.sleep(0.1)  

    def run(self):
        print("Start Unicast")
        while not self.stop.is_set():
            if (self.client_socket.recv(1024).decode("utf-8") == "Wait:Image"):
                frame = None
                for frame in self.camera.capture_continuous(self.rawCapture, format="bgr", use_video_port=True):
                    #cv.imshow("qrcode", frame.array)
                    frame = frame.array
                    k = cv.waitKey(5) & 0xFF
                    # clear the stream in preparation for the next frame
                    self.rawCapture.truncate(0)
                    break
                data = pickle.dumps(frame)
                #print(data)
                self.client_socket.sendall(data)
                #time.sleep(1.5)
                #self.client_socket.send(b"")                                                                    
                self.client_socket.send("Picture:END".encode("utf-8"))                                   
        

def Handler(self):
    os._exit(1)

def main():
    ud = UnicastDetection(("172.20.10.10", 10200))
    signal.signal(signal.SIGINT, Handler)
    ud.start()
    
main()
