# PartY
PartY is a Unity C# networking solution targeted towards party games/large scale matchmaking with small scale gameplay. This solution aims to avoid any third party connections so that there are no fees or limitations to the host.

This project is a work in progress and will be updated for a bit until it is completed.

## How to use

Host:
In the release folder there is a PartY.unitypackage and a HostBuild.zip
Download the HostBuild.zip, extract it. From here you should just run the exe file and it should just work. (Note, if you want external connections to work you will need to port forward)

Clients:
Import the PartY.unitypackage downloaded from the releases page here.
Check out the demo scenes to see base setup of client side gameplay and matchmaking.

## Developing Note
When developing, remember that both the host and client solutions must be updated to support one another and that the host must be running for the clients to connect!
