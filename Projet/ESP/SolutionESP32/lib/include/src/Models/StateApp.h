#ifndef StateApp_H
#define StateApp_H

#include "SensorsState.h"
#include "MotorsState.h"
#include "GPSState.h"

class StateApp
{
    public:
        MotorsState *motorsState;
        SensorsState *sensorsState;
        GPSState *gpsState;

        StateApp() {
            this->sensorsState = new SensorsState();
            this->motorsState = new MotorsState();
            this->gpsState = new GPSState();
        }

        

};
 
#endif