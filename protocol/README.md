# P2P Chat - Protocol

This folder contains the protocol definition and design documents.

## Client
- Register
	- REGISTER\nusername\npassword (max 16 characters)
- Login
	- LOGIN\nusername\npassword
- Logout
	- LOGOUT 
- Send HELLO every 60 seconds over UDP
	- HELLO\nusername
- Search for username
	- SEARCH\nusername
- Chat
	- Request: CHAT
	- Response: OK / REJECT | BUSY
	- TEXT\nusername\nMESSAGE (message max 325 characters)
- Group
	- GROUP_CREATE\nusername1\nusername2\nusername3 (max 100 user)
	- GROUP_SEARCH\ngroupid
	- GROUP_TEXT\ngroupid\nusername\nMESSAGE (message max 325 characters)
	
## Registry
- Register
	- All the usernames should be unique
- Login
	- Store IP address of the user
- Logout
	- Remove
- Listen to a UDP socket for heartbeat (remove after 200 seconds)
- Search 
	- Return IP address of the given user (if online) 
	- OFFLINE
	- NOT_FOUND
- Group
	- GROUP_CREATE
		- GROUP_CREATE
		- GROUP_MEMBER_NOT_FOUND\nusername1\nusername2\nusername3
	- GROUP_SEARCH
		- GROUP_SEARCH\nusername1\nIP1\nusername2\nIP2
		- GROUP_NOT_FOUND
