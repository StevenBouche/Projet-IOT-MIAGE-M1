#include "IdentifiantWifi.h"

IdentifiantWifi::IdentifiantWifi(){

}

IdentifiantWifi::IdentifiantWifi(std::string ssid, std::string passwd){
    this->ssid = ssid;
    this->passwd = passwd;
}

std::string IdentifiantWifi::getSSID(){
    return this->ssid;
}

std::string IdentifiantWifi::getPasswd(){
    return this->passwd;
}

