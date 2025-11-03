# Cross-platform controller compatibility matrix for C# Raylib + SDL2 game engine (Windows, Linux, macOS, Steam Deck).

### Tested With
- Raylib-cs 5.0
- SDL2-CS 2.30
- Windows 11 (23H2), macOS Sequoia, Ubuntu 24.04
- Controllers: Xbox 360, Xbox One S, PlayStation 3/4, 8BitDo SN30 Pro, SNES USB, Dual Arcade HID

### Notice
Everything is tested as-is / out of the box. I did not want to do special patches or tricks to get it to work. Reason? Most gamers are not tweakers. They just like the "it just works" experience

## Windows 11

|     | RAY Buttons | RAY Rumble | RAY DETECTED AS                  | SDL Buttons | SDL Rumble | SDL DETECTED AS |
| --- | --- |------------|----------------------------------| --- |------------|-----------------|
| XBOX 360 | Works no middle button | NO         | XBOX  <br>XBOX Controller        | Works | Works      | Xbox            |
| XBOX one | Works no middle button | NO         | XBOX  <br>XBOX Controller        | Works | Works      | Xbox            |
| 8bitdo | Works but A B reversed | NO         | Other<br><br>8bitdo SN30 Pro     | Works | NO         | Other           |
| SNES | Works but A B reversed | NVT        | Other  <br>USB Gamepad           | Works (Special binding) | NVT        | Other           |
| Play3 | Windows fail | \-         | \-                               | \-  | \-         | \-              |
| Play4 | Works 100% | NO         | Other<br><br>Wireless controller | Works | Works      | Playstation     |

## M4 Macbook Pro

|     | RAY Buttons | RAY Rumble | RAY DETECTED AS                                                                            | SDL Buttons | SDL Rumble    | SDL DETECTED AS             |
| --- | --- |------------|--------------------------------------------------------------------------------------------| --- |---------------|-----------------------------|
| XBOX 360 | NO LeftBumper & RightBumper? | NO         | Other  <br>Controller                                                                      | Works\* Registered twice… | Works         | Xbox                        |
| XBOX one | Only on wireless | NO         | Other USB  <br>Controller  <br>Wireless  <br>Xbox wireless controller                      | Works\* Registered twice with USB… | Only wireless | Xbox                        |
| 8bitdo | Works but A B reversed | NO         | Other<br><br>8bitdo SN30 Pro                                                               | Works also the middle button? | NO            | Other                       |
| SNES | NO  | NO         | Other  <br>USB GamePad                                                                     | Works (Special binding) | NVT           | Other                       |
| Play3 | NO (USB) | NO         | Playstation  <br>Playstation(R)3 Controller                                                | Button layout messed up (USB) | Works No Left | Playstation & Other (Twice) |
| Play4 | Works | NO         | Other (USB)  <br>Wireless controller  <br>Playstation  <br>DUALSHOCK 4 Wireless Controller | Works | Works         | Playstation                 |

## Linux (Ubuntu)

|     | RAY Buttons | RAY Rumble | RAY DETECTED AS                                               | SDL Buttons | SDL Rumble | SDL DETECTED AS |
| --- | --- |------------|---------------------------------------------------------------| --- |------------|-----------------|
| XBOX 360 | Works | NO         | Xbox  <br>Microsoft X-Box 360 pad                             | Works | No         | Xbox            |
| XBOX one | Works | NO         | Xbox  <br>Microsoft X-Box One S pad                           | Works | No         | Xbox            |
| 8bitdo | Fails in linux | \-         | \-                                                            | \-  | \-         | \-              |
| SNES | Works  <br>Left right bumper. A, B reversed | NO         | Other<br><br>USB Gamepad                                      | Works | NVT        | Other           |
| Play3 | Buttons messed up | NO         | Playstation  <br>Playstation(R)3 Controller                   | Buttons Messed up | NO         | Playstation     |
| Play4 | Works | No         | Other  <br>Sony interactive Entertainment Wireless Controller | Works | No         | Playstation     |

## SteamDeck

- Ray Buttons work
- Ray rumble did not work
- Detected as Steam!
- SDL Buttons work
- SDL Rumble don't work
- Detected as Xbox

### Arcade cabinet

- Ray Butttons did not work
- Detected as other (Xi-Mi dual)
- SDL buttons works until 10 buttons (Implementation issue)
- Detected as Other

[I Tested Every Controller for Game Dev (YouTube)](https://youtube.com/@MeatcorpsOfficial)