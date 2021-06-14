#ifndef MQTTConfig_h
#define MQTTConfig_h

#include <Arduino.h>

class MQTTConfig{

    private:
        const char* host;
        String id, username, passwd;
        uint16_t port;
        

    public:

        bool secure;
        
        MQTTConfig(const char* host, uint16_t port, String id, String username, String passwd, bool secure){
            this->host = host;
            this->port = port;
            this->id = id;
            this->username = username;
            this->passwd = passwd;
            this->secure = secure;
        }

        void printConfig(){
            String s = "MQTT Configuration : \n";
            s += "\tHost : " + String(this->host) + "\n";
            s += "\tPort : " + String(this->port) + "\n";
            s += "\tId : " + String(this->id) + "\n";
            s += "\tUsername : " + String(this->username) + "\n";
            s += "\tPassword : " + String(this->passwd) + "\n";
            Serial.println(s);
        }
    
        const char* getHost(){
            return this->host;
        }

        void setHost(const char* host){
            this->host = host;
        }

        uint16_t getPort(){
            return this->port;
        }

        void setPort(uint16_t port){
            this->port = port;
        }

        String getID(){
            return this->id;
        }

        void setID(String id){
            this->id = id;
        }

        String getUsername(){
            return this->username;
        }

        void setUsername(String username){
            this->username = username;
        }

        String getPasswd(){
            return this->passwd;
        }

        void setPasswd(String passwd){
            this->passwd = passwd;
        }
};

#endif