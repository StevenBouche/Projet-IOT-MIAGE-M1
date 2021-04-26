#ifndef IdentifiantWifi_H
#define IdentifiantWifi_H
#include <string>

class IdentifiantWifi{

    private: 
        std::string ssid;
        std::string passwd;

    public :
        IdentifiantWifi();
        IdentifiantWifi(std::string ssid, std::string passwd);
        std::string getSSID();
        std::string getPasswd();

};

#endif