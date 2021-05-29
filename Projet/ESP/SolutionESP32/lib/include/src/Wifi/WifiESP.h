#ifndef WifiESP_H
#define WifiESP_H

#include <WiFi.h>
#include <WiFiMulti.h>
#include <mutex> 
#include <condition_variable> 
#include "LinkedList.h"
#include "IdentifiantWifi.h"
#include "../Tasks/TaskRobot.h"
#include <esp_wifi.h>

typedef void (*OnConnected)();
typedef void (*OnDisconnected)();

class WifiESP : public TaskRobot {

    private:

        bool connected;
        int waitingTime;
        WiFiMulti wifiMulti;
        LinkedList <IdentifiantWifi> identifiants;

        //tasks
        std::mutex m;
        std::condition_variable cv;

        //callbacks
        OnConnected connect;
        OnDisconnected disconnect;

    protected:
    
        void execute();
        void task();
        void stop();

    public: 

        WifiESP() : TaskRobot("WIFI", 4096, 1)
        {
            this->connected = false;
            this->waitingTime = 200;
        }

        void print_network_status();
        
        String getAddrMac(){ return WiFi.macAddress(); }
        bool isConnected(){ return connected; }
        void addIdentifiantWifi(IdentifiantWifi identifiant){ identifiants.add(identifiant); }
        void subEventConnect(OnConnected callback){ this->connect = callback; }
        void subEventDisconnect(OnDisconnected callback){ this->disconnect = callback; }

};

/**
 * Execute task WiFi.
 * Try to connect from a WiFi AP list.
 */
void WifiESP::task(){

    while(this->running){
        
        Serial.println("Try to connect to an Wifi network.");

        //try connection while not connected
        while(this->wifiMulti.run() != WL_CONNECTED){
            vTaskDelay(this->waitingTime);
        }

        //wait status connected
        while(WiFi.status() != WL_CONNECTED){
            vTaskDelay(1000); // ms
            Serial.print(".");
        }

        //thread to sleep waiting deconnection for retry connection
        std::unique_lock<std::mutex> verrou{this->m};
        this->cv.wait(verrou);
    }
    Serial.println("Stop task wifi.");
}

/**
 * For start task WiFi in another thread.
 */
void WifiESP::execute(){

    WiFi.mode(WIFI_STA);
    //esp_wifi_set_ps(WIFI_PS_NONE);

    //for all identifiants, add ssid and password to multi wifi
    for(int i = 0; i < identifiants.size(); i++){
        IdentifiantWifi id = identifiants.get(i);
        wifiMulti.addAP(id.getSSID().c_str(), id.getPasswd().c_str());
    }

    //when disconnected wake lock for retry connection and callback disconnecetd
    auto lambdaDisconnected = [this] (WiFiEvent_t event, WiFiEventInfo_t info) {
        this->connected = false;
        cv.notify_all();
        if(this->disconnect)
            this->disconnect();
    };

    //when have an adress ip callback connect
    auto lambdaGotIP = [this] (WiFiEvent_t event, WiFiEventInfo_t info) {
         this->print_network_status();
         this->connected = true;
          if(this->connect)
            this->connect();
    };
    
    //subscribe event
    WiFi.onEvent(lambdaDisconnected, SYSTEM_EVENT_STA_DISCONNECTED);
    //WiFi.onEvent(lambdaConnected, SYSTEM_EVENT_STA_CONNECTED);
    WiFi.onEvent(lambdaGotIP, SYSTEM_EVENT_STA_GOT_IP);


    //create thread task wifi
    //xTaskCreate(this->task, "Handle Wifi", 2048, this, 1, &taskHandle);
}

/**
 * For stop current task WiFi.
 */
void WifiESP::stop(){
    //this->running = false;
    this->cv.notify_all();
    WiFi.disconnect();
}

/**
 * For print state WiFi.
 */
void WifiESP::print_network_status(){ 
    String s = "Network status : \n";
    s += "\tIP address : " + WiFi.localIP().toString() + "\n";
    s += "\tMAC address : " + String(WiFi.macAddress()) + "\n";
    s += "\tWifi SSID : " + String(WiFi.SSID()) + "\n";
    s += "\tWifi Signal Strength : " + String(WiFi.RSSI()) + "\n";
    s += "\tWifi BSSID : " + String(WiFi.BSSIDstr()) + "\n";
    Serial.println(s);
}

#endif