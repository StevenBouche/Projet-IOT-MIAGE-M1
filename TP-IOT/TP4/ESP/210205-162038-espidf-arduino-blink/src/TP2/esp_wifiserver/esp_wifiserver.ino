/*
 * Auteur : G.Menez
 */
#include <WiFi.h>
#include "classic_setup.h"

// Set timer 
unsigned long loop_period = 10L * 1000; /* =>  10000ms : 10 s */

// Instanciation of a Web server on port 80 
WiFiServer server(80);

/*---- Arduino IDE paradigm : setup+loop -----*/
void setup(){ 
  Serial.begin(9600);
  while (!Serial); // wait for a serial connection. Needed for native USB port only
  connect_wifi(); // Connexion Wifi  
  print_network_status();
  
  server.begin(); // Lancement du serveur
}

void loop() {
  // listen for incoming clients
  WiFiClient client = server.available();
  
  if (client) { // incoming client
    Serial.println("new client");
    // an http request ends with a blank line
    boolean currentLineIsBlank = true;
    
    while (client.connected()) {
      if (client.available()) {
	
        char c = client.read(); // Echo on the console
        Serial.write(c);
	
        // if you've gotten to a CRLF the http GET request has ended,
        // so you can send a reply
        if (c == '\n' && currentLineIsBlank) { 
          httpReply(client);
          break;
        }
        if (c == '\r') { // you're starting a new line
          currentLineIsBlank = true;
        } else if (c != '\r') { // you've gotten a character on the current line
          currentLineIsBlank = false;
        }
      }
    }
    
    // give the web browser time to receive the data
    delay(loop_period); // ms
    
    // close the connection :
    client.stop();
    Serial.println("client disconnected");
  }
}

/*--------------------------------*/
void httpReply(WiFiClient client) {
  // this method makes a simple HTTP GET reply 
  // the body syntax is HTML
  // => supposed to be displayed by a navigator
  client.println("HTTP/1.1 200 OK");
  client.println("Content-Type: text/html");
  client.println("Connection: close");  // the connection will be closed after 
                                        //completion of the response  
  client.println("Refresh: 5");         // refresh the page automatically every 5 sec
  
  client.println(); // Empty line between header and body
  
  client.println("<!DOCTYPE HTML>");
  client.println("<html>");    
  client.print("Hello, je tourne depuis : "); // Returns the ms passed since the ESP 
                                              // began running the current program. 
  client.print(millis()/1000); // On pourrait sans doute donner une info
  client.println("s <br />");  // plus pertinente ? temperature ? 
  client.println("</html>"); 
}
