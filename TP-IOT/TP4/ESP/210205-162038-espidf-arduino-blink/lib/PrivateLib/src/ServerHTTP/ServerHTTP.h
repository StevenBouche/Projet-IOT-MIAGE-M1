#ifndef ServerHTTP_h
#define ServerHTTP_h

#endif

#include <Arduino.h>
#include <AsyncTCP.h>
#include "ESPAsyncWebServer.h"


void init_HTTP_server(AsyncWebServer* server, int* tempInit, int* lumInit);