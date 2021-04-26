#include "WifiESP.h"
#include <WiFi.h>
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"

WiFiMulti wifiMulti;
std::mutex m;
std::condition_variable cv;
LinkedList <IdentifiantWifi> identifiants;
bool running;
TaskHandle_t taskHandle;

int waitingTime = 2000;

void taskWifi(void * parameter){

    //WifiESP* wifiEsp =  reinterpret_cast<WifiESP*>(parameter);

    while(running){
        
        Serial.println("Try to connect to an Wifi network.");

        while(wifiMulti.run() != WL_CONNECTED){
            vTaskDelay(waitingTime);
        }

        while(WiFi.status() != WL_CONNECTED){
            delay(1000); // ms
            Serial.print(".");
        }

        std::unique_lock<std::mutex> verrou{m};
        cv.wait(verrou);

    }

    Serial.println("Stop task wifi.");
    vTaskDelete(NULL);
}
   
WifiESP::WifiESP(){
    running = true;
    taskHandle = NULL;
    connected = false;
}

void WifiESP::addIdentifiantWifi(IdentifiantWifi identifiant){
    identifiants.add(identifiant);
}

void WifiESP::execute(){

    WiFi.mode(WIFI_STA);

    for(int i = 0; i < identifiants.size(); i++){
        IdentifiantWifi id = identifiants.get(i);
        wifiMulti.addAP(id.getSSID().c_str(), id.getPasswd().c_str());
    }

    auto lambdaConnected = [this] (WiFiEvent_t event, WiFiEventInfo_t info) {
       
    };

    auto lambdaDisconnected = [this] (WiFiEvent_t event, WiFiEventInfo_t info) {
        this->connected = false;
        cv.notify_all();
        if(this->disconnect)
            this->disconnect();
    };

    auto lambdaGotIP = [this] (WiFiEvent_t event, WiFiEventInfo_t info) {
         this->print_network_status();
         this->connected = true;
          if(this->connect)
            this->connect();
    };
    
    WiFi.onEvent(lambdaDisconnected, SYSTEM_EVENT_STA_DISCONNECTED);
    WiFi.onEvent(lambdaConnected, SYSTEM_EVENT_STA_CONNECTED);
    WiFi.onEvent(lambdaGotIP, SYSTEM_EVENT_STA_GOT_IP);

    xTaskCreate(taskWifi, "Handle Wifi", 2048, this, 1, &taskHandle);
}

void WifiESP::stop(){
    running = false;
    WiFi.disconnect();
}

void WifiESP::print_network_status(){ 
    String s = "";
    s += "\tIP address : " + WiFi.localIP().toString() + "\n";
    s += "\tMAC address : " + String(WiFi.macAddress()) + "\n";
    s += "\tWifi SSID : " + String(WiFi.SSID()) + "\n";
    s += "\tWifi Signal Strength : " + String(WiFi.RSSI()) + "\n";
    s += "\tWifi BSSID : " + String(WiFi.BSSIDstr()) + "\n";
    Serial.print(s);
}
