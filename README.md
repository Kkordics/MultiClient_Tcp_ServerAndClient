# MultiClient_Tcp_ServerAndClient
It is a basic threaded tcp server and client.

## Features
 - Can handle multiple clients
 - Handles connection lost
 - It can be strong base 

## Tts operation in short
When a client connects to the server it makes a new thread for reading data. In the first connection a new thread make and this thread do the sending for all client. When a client disconnects it own thread will be end.
