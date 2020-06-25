# RemoteControl
Tool for controlling Windows remotely.

Content
- Server: Windows service for receiving commands via TCP. For now only Ping, SendMessage and Shutdown are implemented. Creates a log file on users desktop.
- ClientApp: Android app to send the commands via TCP. Includes a console view for debugging.
- CommandAndControl: shared components, e.g. network settings and commands.
