# BodySonicReciever

This software is an interface that allows you to control Synesthesia Wear ver.0 from a Windows PC.
Receives an OSC message from the LAN and bypasses the message to the connected USB-BT module.

## Setup
### Control from same machine
<img width="805" alt="スクリーンショット 2020-04-27 20 51 54" src="https://user-images.githubusercontent.com/3236256/80369258-f0064180-88c8-11ea-80a8-3ff903dc1ae5.png">

### Control from another machine 
<img width="677" alt="スクリーンショット 2020-04-27 21 01 25" src="https://user-images.githubusercontent.com/3236256/80370074-4f188600-88ca-11ea-961f-3402821a99e8.png">

## OSC command format
First, You can know OSC command format in this page.

https://qiita.com/kodai100/items/5b614fed5f3e17b7f8f6

Generaly OSC works with following command format.
~~~
/[address pattern] [property]
~~~

BodySonicReciever allows you to send commands in the following format.

~~~
/(Group ID)-(Node ID)-(Sound ID)-(Strength)-(LED ID) [empty]
~~~

That is, BodySonicReciever uses only the address pattern for receiving commands and does not use any arguments. What an implementation!

If you want to send a command with sound ID 12 and LED ID 60 to the module 5-3, you will get the following command.

~~~
/5-3-12-0-60 [empty]
~~~

 Strength is usually set to 0. If the value of 1 is set here, the vibration intensity is halved, and each time it is incremented, the vibration intensity is halved. The OSC receiver is open at port 12345, connect the USB module and press the OK button to start the OSC receiver.
