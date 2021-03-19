/* 
 * Auteur : G.Menez
 * Fichier : esp_asyncweb.ino 
 * Adaptation de :
 => https://raw.githubusercontent.com/RuiSantosdotme/ESP32-Course/master/code/WiFi_Web_Server_DHT/WiFi_Web_Server_DHT.ino
 => https://randomnerdtutorials.com/esp32-dht11-dht22-temperature-humidity-web-server-arduino-ide/
*/

// Import required libraries
#include <WiFi.h>
#include "ESPAsyncWebServer.h"
#include "classic_setup.h"
#include "sensors.h"
#include "OneWire.h"
#include "DallasTemperature.h"

extern const char page_html[];

/* ---- Set timer ---- */
unsigned long loop_period = 10L * 1000; /* =>  10000ms : 10 s */

/* ---- LED ---- */
const int LEDpin = 19; // LED will use GPIO pin 19
// Ces variables permettent d'avoir une representation
// interne au programme du statut "electrique" de l'objet.
// Car on ne peut pas interroge une GPIO pour lui demander !
String LEDState = "off";

/* ---- Light ----*/
const int LightPin = A5; // Read analog input on ADC1_CHANNEL_5 (GPIO 33)

/* ---- TEMP ---- */
OneWire oneWire(23); // Pour utiliser une entite oneWire sur le port 23
DallasTemperature tempSensor(&oneWire) ; // Cette entite est utilisee par le capteur de temperature

// Create AsyncWebServer object on port 80
AsyncWebServer server(80);

/*---- Arduino IDE paradigm : setup+loop -----*/
void setup(){
  Serial.begin(9600);
  while (!Serial); // wait for a serial connection. Needed for native USB port only
  
  connect_wifi(); // Connexion Wifi  
  print_network_status();
  
  // Initialize the LED 
  setup_led(LEDpin, OUTPUT, LOW);
  
  // Init temperature sensor 
  tempSensor.begin();
  
  server.begin(); // Lancement du serveur

  // Route for root / web page
  auto root_handler = server.on("/", HTTP_GET, [](AsyncWebServerRequest *request){
    request->send_P(200, "text/html", page_html, processor);
  });
  server.on("/temperature", HTTP_GET, [](AsyncWebServerRequest *request){
    request->send_P(200, "text/plain", get_temperature(tempSensor).c_str());
  });
  server.on("/light", HTTP_GET, [](AsyncWebServerRequest *request){
    request->send_P(200, "text/plain", get_light(LightPin).c_str());
  });

  // Start server
  server.begin();
}
 
void loop(){  
}

// Replaces placeholder with sensors values
String processor(const String& var){
  //Serial.println(var);
  if(var == "TEMPERATURE"){
    return get_temperature(tempSensor);
  }
  else if(var == "LIGHT"){
    return get_light(LightPin);
  }
  return String();
}

/**========== HTML ==========**/
// C++11 standard introduced  raw string literal :
// Raw string literals look like  : R"token(text)token"
const char page_html[] PROGMEM = R"rawliteral(
<!DOCTYPE HTML><html>
<head>
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <link rel="stylesheet" href="https://use.fontawesome.com/releases/v5.7.2/css/all.css" integrity="sha384-fnmOCqbTlWIlj8LyTjo7mOUStjsKC4pOpQbqyi7RrhN7udi9RwhKkMHpvLbHG9Sr" crossorigin="anonymous">
  <style>
    html {
     font-family: Arial;
     display: inline-block;
     margin: 0px auto;
     text-align: center;
    }
    h2 { font-size: 3.0rem; }
    p { font-size: 3.0rem; }
    .units { font-size: 1.2rem; }
    .sensors-labels{
      font-size: 1.5rem;
      vertical-align:middle;
      padding-bottom: 15px;
    }
  </style>
</head>
<body>
  <h2>ESP32</h2>
  <p>
    <i class="fas fa-thermometer-half" style="color:#059e8a;"></i> 
    <span class="sensors-labels">Temperature</span> 
    <span id="temperature">%TEMPERATURE%</span>
    <sup class="units">&deg;C</sup>
  </p>
  <p>
    <i class="far fa-lightbulb" style="color:#00add6;"></i> 
    <span class="sensors-labels">Light</span>
    <span id="light">%LIGHT%</span>
    <sup class="units">Lumen</sup>
  </p>
<script>
setInterval(function ( ) {
  var xhttp = new XMLHttpRequest();
  xhttp.onreadystatechange = function() {
    if (this.readyState == 4 && this.status == 200) {
      document.getElementById("temperature").innerHTML = this.responseText;
    }
  };
  xhttp.open("GET", "/temperature", true);
  xhttp.send();
}, 10000 ) ;
setInterval(function ( ) {
  var xhttp = new XMLHttpRequest();
  xhttp.onreadystatechange = function() {
    if (this.readyState == 4 && this.status == 200) {
      document.getElementById("light").innerHTML = this.responseText;
    }
  };
  xhttp.open("GET", "/light", true);
  xhttp.send();
}, 10000 ) ;
</script>
</body>
</html>)rawliteral";
