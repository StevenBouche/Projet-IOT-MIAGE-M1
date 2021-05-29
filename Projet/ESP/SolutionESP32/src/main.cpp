#include <Arduino.h>
#include "Wifi/IdentifiantWifi.h"
#include "Wifi/WifiESP.h"

#include "Models/StateApp.h"

//region variable_connection
//  WiFi
IdentifiantWifi ids[] = {
  IdentifiantWifi("Bbox-4DD70ADE", "551F54E2D72A27CA1EA44567F149E1"),
  IdentifiantWifi("iPhone de Steven", "iobwgn7obkf91")
};
WifiESP wifi;
//endregion variable_connection

//region variable_state_handler
StateApp state;
//endregion variable_state_handler

void setup() {

}

void loop() {
 
  delay(1000);
}