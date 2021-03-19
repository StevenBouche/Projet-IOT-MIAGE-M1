#include <WiFi.h>
#include "classic_setup.h"

/* This sketch sends data via HTTP GET requests to host/url 
   and returns the website in html format which is printed on the
   console */ 

IPAddress server(216,58,205,195);  // Google

/*---- Arduino IDE paradigm : setup+loop -----*/
void setup(){
  Serial.begin(9600);
  while (!Serial); // wait for a serial connection
  connect_wifi();//connect wifi 
  print_network_status();
}

void loop() {
  WiFiClient client;

  char host[100] = "httpbin.org";
  const int httpPort = 80;
  
  // Now create a URI for the GET request : this url contains the
  // information we want to send to the server => GET style !!
  String url = "/ip";
  /* GET avec params 
   * url += "?param1=";
     url += v1;
     url += "?param2=";
     url += v2;
  */
  //strcpy(host,"www.google.fr");
  //url = "/search?q=esp32";
  
  Serial.print("connecting to ");
  Serial.println(host); // Use WiFiClient class to create TCP connections
  if (!client.connect(host, httpPort)) {
    Serial.println("connection failed");
    return;
  }
  Serial.print("Requesting URL : ");
  Serial.println(url); 

  // Now create HTTP request
  String req = String("GET ");
  req +=  url + " HTTP/1.1\r\n";
  req += "Host: " + String(host) + "\r\n";
  req += "Connection: close\r\n";
  req += "\r\n";  // empty line : separator header/body        

  // Send request to host throught client socket
  client.print(req); 
  
  unsigned long timeout = millis();
  // https://www.arduino.cc/en/Reference/WiFiClientAvailable
  while (client.available() == 0) { // no answer => timeout mechanism !
    if (millis() - timeout > 5000) {
      Serial.println(">>> Client Timeout !");
      client.stop();
      return;
    }
  }
  // Read all the lines of the reply from server and print them to Serial
  while (client.available()) { //Returns the number of bytes available for reading
    String line = client.readStringUntil('\r'); 
    Serial.print(line); // echo to console

    // en version car/car
    //char c = client.read(); 
    //Serial.print(c);    
  }
  Serial.println();
  Serial.println("closing connection");

  delay(10000); //ms
}
