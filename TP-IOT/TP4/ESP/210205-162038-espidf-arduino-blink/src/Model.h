#ifndef Model_h
#define Model_h

class StateApp
{
    public:
        bool *led_green;
        bool *led_red;
        int *photo;
        int *temp;
        void print();
};

#endif