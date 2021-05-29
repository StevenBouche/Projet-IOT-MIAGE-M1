#ifndef GPSState_H
#define GPSState_H

class GPSState{

    private:
        double longitude, latitude;

    public:

        GPSState() {
            this->longitude = 0;
            this->latitude = 0;
        }

        double getLongitude(){
            return this->longitude;
        }

        void setLongitude(double longitude){
            this->longitude = longitude;
        }

        double getLatitude(){
            return this->latitude;
        }

        void setLatitude(double latitude){
            this->latitude = latitude;
        }

};

#endif