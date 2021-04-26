#ifndef StateApp_H
#define StateApp_H
 
#define ULTRASONS_MAX = 300; 
#define ULTRASONS_MIN = 2; 

class StateApp
{
    public:
        double motorL;
        double motorR;
        int light;
        int temperature;
        double longitude;
        double latitude;
        long ultrasons;
        bool obstacleL;
        bool obstacleR;

        StateApp() {
            motorL = 0;
            motorR = 0;
            light = 0;
            temperature = 0;
            longitude = 0;
            latitude = 0;
            ultrasons = 0;
            obstacleL = false;
            obstacleR = false;
        }
 
        void print();
};
 
#endif