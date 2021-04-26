#ifndef WifiESP_H
#define WifiESP_H

#include <WiFiMulti.h>
#include "LinkedList.h"
#include <condition_variable> 
#include <mutex> 
#include "IdentifiantWifi.h"
#include <functional>

class WifiESP {

    private:
        bool connected;
        std::function<void()> connect;
        std::function<void()> disconnect;

    public: 
        WifiESP();
        void execute();
        void addIdentifiantWifi(IdentifiantWifi identifiant);
        void print_network_status();
        void stop();

        void subEventConnect(std::function<void()> callback){ this->connect = callback; }
        void subEventDisconnect(std::function<void()> callback){ this->disconnect = callback; }
        bool isConnected() { return connected; }
        
};

#endif