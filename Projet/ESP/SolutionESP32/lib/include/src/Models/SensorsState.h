#ifndef SensorsState_H
#define SensorsState_H
 
#define ULTRASONS_MAX = 300; 
#define ULTRASONS_MIN = 2; 

class SensorsState
{
    public:
        int light, temperature;
        bool obstacleL, obstacleR;
        long ultrasons;

        SensorsState() {
            light = 0;
            temperature = 0;
            ultrasons = 0;
            obstacleL = false;
            obstacleR = false;
        }

        int getLight(){
            return this->light;
        }

        void setLight(int ligth){
            this->light = ligth;
        }

        int getTemperature(){
            return this->temperature;
        }

        void setTemperature(int temperature){
            this->temperature = temperature;
        }

        bool getObstacleL(){
            return this->obstacleL;
        }

        void setObstacleL(bool o){
            this->obstacleL = o;
        }

        bool getObstacleR(){
            return this->obstacleR;
        }

        void setObstacleR(bool o){
            this->obstacleR = o;
        }

        long getUltrason(){
            return this->ultrasons;
        }

        void setUltrason(long o){
            this->ultrasons = o;
        }

};
 
#endif