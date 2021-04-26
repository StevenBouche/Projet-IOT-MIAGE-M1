#ifndef UDPClient_H
#define UDPClient_H

#include <Arduino.h>
#include <WiFi.h>
#include <WiFiUdp.h>

class UDPClient{

    private:
        String hostname;
        uint16_t port;
        WiFiUDP *udp;

    public:

        UDPClient(WiFiUDP *udp, String hostname, uint16_t port){
            this->hostname = hostname;
            this->port = port;
            this->udp = udp;
        }

        void sendData(byte *bytes, int length);
};

#endif

void UDPClient::sendData(byte *bytes, int length){
    int result = this->udp->beginPacket(this->hostname.c_str(), this->port);
    if(result == 1){
        this->udp->write(bytes, length);
        this->udp->endPacket();
    }
}