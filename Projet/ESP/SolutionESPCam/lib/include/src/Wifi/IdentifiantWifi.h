#ifndef IdentifiantWifi_H
#define IdentifiantWifi_H

#include <Arduino.h>

class IdentifiantWifi{

    private: 
        String ssid, passwd;

    public :

        IdentifiantWifi(){}
        
        IdentifiantWifi(String ssid, String passwd){
            this->ssid = ssid;
            this->passwd = passwd;
        }

        String getSSID(){
            return this->ssid;
        }

        String getPasswd(){
            return this->passwd;
        }
};

#endif