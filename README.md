# VSCView
On-screen Controller Overlay for various controllers such as Valve Steam Controller & Sony PlayStation Controllers (DualShock 4 and DualSense Controller).

## Themes
* ﻿Sony DualSense
  * Default by [Al. Lopez (AL2009man)](https://github.com/AL2009man)
    - Color: White/Black
    - Color: Midnight Black 
    - Color: Cosmic Red
* ﻿Sony DualShock 4
  * Default by [Al. Lopez (AL2009man)](https://github.com/AL2009man)
  * Devil May Cry 5 by [Al. Lopez (AL2009man)](https://github.com/AL2009man)
  * PlayStation 20th Anniversary by [Al. Lopez (AL2009man)](https://github.com/AL2009man)
* ﻿Valve Steam Controller
  * Default by [Al. Lopez (AL2009man)](https://github.com/AL2009man)
    - Color: Black
    - Color: White
  * ahNOMinal 2 by [ahMEmon](https://www.twitch.tv/ahmemon)
  * Buh-Lack by [ahMEmon](https://www.twitch.tv/ahmemon)
  * Classic by Nielk1
  * Docteur Controller by [Docteur Controller](https://www.youtube.com/channel/UC1GoAgop-6tbftsU4qtpSOQ)
  * Devieus Glass by [Devieus](https://www.youtube.com/user/Devieus)
  * Diagnostic by [Al. Lopez (AL2009man)](https://github.com/AL2009man)
  * Grey Minimal by [CriticalComposer](https://www.youtube.com/c/criticalcomposer)
  * Line Art by [Ishmobile](https://www.youtube.com/channel/UCTAPOuAcWv9JGdiQsVVSxkQ)
  * Line Art Invert by Sora Firestone & [Ishmobile](https://www.youtube.com/channel/UCTAPOuAcWv9JGdiQsVVSxkQ)
  * Cat Paws by Nielk1
  * Black Stencil by [RambleTan](https://www.youtube.com/channel/UCYI8ifruvIqVtAY3joqlpfw)
  * Outline 2018 by [RambleTan](https://www.youtube.com/channel/UCYI8ifruvIqVtAY3joqlpfw)
  * Outline 2019 by [RambleTan](https://www.youtube.com/channel/UCYI8ifruvIqVtAY3joqlpfw)
  * Fire Sale by Nielk1, [Twenty-Six Twelve](https://www.reddit.com/user/Twenty-Six_Twelve/)
  * White Cerberus by [8BitCerberus](https://github.com/8BitCerberus/vscview-themes)
* ﻿Valve Steam Controller Chell
  * Default by Nielk1
* Microsoft Xbox Wireless Controller
  * Xbox Controller - Theme Prototype by [Al. Lopez (AL2009man)](https://github.com/AL2009man)
* ﻿General
  * Genshin Impact by Nielk1

## Element Properties
* *root*
  * name - string
  * height - number
  * width - number
  * version - theme structure version
  * children - array of elements
* Item
  * `"type": null` - AKA not supplied
  * x - number, position
  * y - number, position
  * rot - number, rotation degrees clockwise
  * winform.smoothing - enum, valid values:
    * "Default"
    * "HighSpeed"
    * "HighQuality"
    * "None"
    * "AntiAlias"
  * winform.interpolation - enum, valid values:
    * "Default"
    * "Low"
    * "High"
    * "Bilinear"
    * "Bicubic"
    * "NearestNeighbor"
    * "HighQualityBilinear"
    * "HighQualityBicubic"
  * children - array of elements
* GraphicalItem (everything in Item plus)
    * `"type": "image"`
    * image - string, filename of image, start with `\` to use a path relative to themes folder instead of current theme
    * center - boolean, draw image from center instead of top left
    * height - number
    * width - number
* ShowHide (everything in Item plus)
    * `"type": "showhide"`
    * input - Flee formula string, 0/1 active state
* Slider (everything in Item plus)
    * `"type": "slider"`
    * inputX - Flee formula string, X offset
    * inputY - Flee formula string, Y offset
    * inputR - Flee formula string, Rotation offset
* TrailPad (everything in Slider plus)
    * `"type": "trailpad"`
    * input - Flee formula string, 0/1 active state
    * image - string, filename of image, start with `\` to use a path relative to themes folder instead of current theme
    * length - number, length of trail
    * height - number
    * width - number
* PBar (everything in Item plus)
    * `"type": "pbar"`
    * image - string, if not set will render solid color, filename of image, start with `\` to use a path relative to themes folder instead of current theme
	* foreground - hex string, used if image not set, example `"FFFFFFFF"
	* background - hex string, used if image not set, example `"FFFFFFFF"
    * direction - enum, valid values:
      * "up"
      * "down"
      * "left"
      * "right"
    * mode - enum, valid values:
      * "" - default, same as no value
      * "stretch"
    * center - boolean, draw image from center instead of top left
    * height - number
    * width - number
    * input - Flee formula string
    * min - Flee formula string, default 0.0, yes this means it can be dynamic
    * max - Flee formula string, default 1.0, yes this means it can be dynamic
* PPie (everything in Item plus)
    * `"type": "ppie"`
    * image - string, if not set will render solid color, filename of image, start with `\` to use a path relative to themes folder instead of current theme
	* background - hex string, used if image not set, example `"FFFFFFFF"
    * center - boolean, draw image from center instead of top left
    * height - number
    * width - number
    * input - Flee formula string
	* ang - Flee formula string, initial angle degrees
    * min - Flee formula string, default 0.0, yes this means it can be dynamic
    * max - Flee formula string, default 1.0, yes this means it can be dynamic
* Basic3D1 (legacy component, no Flee expressions) (everything in Item plus)
    * `"type": "basic3d1"`
    * image - string, filename of image, start with `\` to use a path relative to themes folder instead of current theme
    * shadowl - string, filename of image, start with `\` to use a path relative to themes folder instead of current theme
    * shadowr - string, filename of image, start with `\` to use a path relative to themes folder instead of current theme
    * shadowu - string, filename of image, start with `\` to use a path relative to themes folder instead of current theme
    * shadowd - string, filename of image, start with `\` to use a path relative to themes folder instead of current theme
    * height - number
    * width - number
    * tilttranslatex - number
    * tilttranslatey - number
	* mode - enum, valid values:
	  * "accel"
	  * "gyro"

## Controller Parts
* ﻿DualSense Controller
  * `"quad_left"` = ControlDPad()
  * `"quad_right"` = ControlButtonQuad()
  * `"bumpers"` = ControlButtonPair()
  * `"bumpers2"` = ControlButtonPair()
  * `"triggers"` = ControlTriggerPair(HasStage2: false)
  * `"menu"` = ControlButtonPair()
  * `"home"` = ControlButton()
  * `"mute"` = ControlButton()
  * `"stick_left"` = ControlStick(HasClick: true)
  * `"stick_right"` = ControlStick(HasClick: true)
  * `"touch_center"` = ControlTouch(TouchCount: 2, HasClick: true)
  * `"motion"` = ControlMotion()
* ﻿DualShock 4 Controller
  * `"quad_left"` = ControlDPad()
  * `"quad_right"` = ControlButtonQuad()
  * `"bumpers"` = ControlButtonPair()
  * `"bumpers2"` = ControlButtonPair()
  * `"triggers"` = ControlTriggerPair(HasStage2: false)
  * `"menu"` = ControlButtonPair()
  * `"home"` = ControlButton()
  * `"stick_left"` = ControlStick(HasClick: true)
  * `"stick_right"` = ControlStick(HasClick: true)
  * `"touch_center"` = ControlTouch(TouchCount: 2, HasClick: true)
  * `"motion"` = ControlMotion()
* ﻿Steam Controller
  * `"quad_left"` = ControlDPad()
  * `"quad_right"` = ControlButtonQuad()
  * `"bumpers"` = ControlButtonPair()
  * `"triggers"` = ControlTriggerPair(HasStage2: true)
  * `"menu"` = ControlButtonPair()
  * `"grip"` = ControlButtonPair()
  * `"home"` = ControlButton()
  * `"stick_left"` = ControlStick(HasClick: true)
  * `"touch_left"` = ControlTouch(TouchCount: 1, HasClick: true)
  * `"touch_right"` = ControlTouch(TouchCount: 1, HasClick: true)
  * `"motion"` = ControlMotion()
* ﻿Steam Controller Chell
  * `"quad_left"` = ControlDPad()
  * `"quad_right"` = ControlButtonQuad()
  * `"bumpers"` = ControlButtonPair()
  * `"triggers"` = ControlTriggerPair(HasStage2: true)
  * `"menu"` = ControlButtonPair()
  * `"grip"` = ControlButtonPair()
  * `"home"` = ControlButton()
  * `"touch_left"` = ControlTouch(TouchCount: 1, HasClick: true)
  * `"touch_right"` = ControlTouch(TouchCount: 1, HasClick: true)
  * `"grid_center"` = ControlButtonGrid(2, 2)
  * `"motion"` = ControlMotion()

## Flee Variables
* ControlTrigger
  * `analog` - 0.0 to 1.0 value
  * `stage2` - Stage2 0/1
* ControlTriggerPair
  * `l:analog` - 0.0 to 1.0 value left trigger
  * `r:analog` - 0.0 to 1.0 value right trigger
  * `l:stage2` - Stage2 0/1 left trigger
  * `r:stage2` - Stage2 0/1 right trigger
* ControlDPad
  * `s` - South button `(D-Pad Down)` pressed 0/1 
  * `e` - East button `(D-Pad Right)` pressed 0/1 
  * `w` - West button `(D-Pad Left)` pressed 0/1
  * `n` - North button `(D-Pad Up)` pressed 0/1
* ControlButtonQuad
  * `s` - South button `(A for Steam Controller, ⨉ for PlayStation Controllers)` pressed 0/1
  * `e` - East button `(B for Steam Controller, ○ for PlayStation Controllers)` pressed 0/1
  * `w` - West button `(X for Steam Controller, □ for PlayStation Controllers)` pressed 0/1
  * `n` - North button `(Y for Steam Controller, △ for PlayStation Controllers)` pressed 0/1
* ControlButtonGrid (supports various sizes)
  * `width` - configured width of button grid
  * `height` - configured height of button grid
  * `0:1` - button x=0,y=1 pressed 0/1
* ControlButtonPair
  * `bumpers:l`- Left bumper button `(L1 for PlayStation Controllers)` pressed 0/1
  * `bumpers:r`- Right bumper button `(R1 for PlayStation Controllers)` pressed 0/1
  * `menu:l` - Select button `(SHARE/CREATE for PlayStation Controllers)` pressed 0/1
  * `menu:r` - Start button `(OPTIONS for PlayStation Controllers)` pressed 0/1
  * `grip:l` - P1 paddle button `(Left Grip for Steam Controller)` pressed 0/1
  * `grip:r` - P2 paddle button `(Right Grip for Steam Controller)` pressed 0/1
* ControlButton
  * `home` - Home button `(Guide Button for Steam Controller, Home button for PlayStation Controllers)` pressed 0/1
  * `mute` - Mute button `(Sony DualSense Controller-only)` pressed 0/1
* ControlStick
  * `x` - -1.0 to 1.0 value
  * `y` - -1.0 to 1.0 value
  * `click` - Stick clicked 0/1
* ControlTouch (supports multiple fingers)
  * `click` - Touch clicked 0/1
  * `0:touch` - finger #0 touching 0/1
  * `0:x` - -1.0 to 1.0 value for finger #0
  * `0:y` - -1.0 to 1.0 value for finger #0
* ControlMotion (subject to change in future versions)
  * `accelerometer:x` - AccelerometerX
  * `accelerometer:y` - AccelerometerY
  * `accelerometer:z` - AccelerometerZ
  * `angularVelocity:x` - AngularVelocityX
  * `angularVelocity:y` - AngularVelocityY
  * `angularVelocity:z` - AngularVelocityZ
  * `orientation:w` - OrientationW
  * `orientation:x` - OrientationX
  * `orientation:y` - OrientationY
  * `orientation:z` - OrientationZ

## Flee Functions
* `max(params float[])` - Return maximum number
* `min(params float[])` - Return minimum number
* `tobool(object)` - Convert almost anything to a bool, null implies false, needed in some cases because all variables are numbers by default
* `if(bool, object, object)` - If first paramater is true, return 2nd, else 3rd
* `math.function()` - any function from [.net's math library](https://docs.microsoft.com/en-us/dotnet/api/system.math?view=netframework-4.6.2)

## Examples
### value range change for 0/1 true/false
```json
{
  "input": "math.abs(stick_left:x) > 0.1"
}
```
### invert variable
(this is needed because all controller state variables are numbers)
```json
{
  "input": "not tobool(trigges:l:stage2)"
}
```
### invert variable (alterate)
```json
{
  "input": "not (trigges:l:stage2 > 0)"
}
```
### math library function call
```json
{
  "input": "math.max(triggers:l:analog, triggers:r:analog)"
}
```
### complex formula with function calls
```json
{
  "inputX": "(((quad_right:e * 15) + (quad_right:w * -15)) / max(1,quad_right:n + quad_right:e + quad_right:s + quad_right:w)) + (touch_right:0:touch * 100) + (touch_right:0:x * 55) + (max(quad_right:n, quad_right:e, quad_right:s, quad_right:w) * touch_right:0:touch * -25)",
  "inputY": "(((quad_right:s * 15) + (quad_right:n * -15)) / max(1,quad_right:n + quad_right:e + quad_right:s + quad_right:w)) + (touch_right:0:touch * -80) + (touch_right:0:y * 55) + (max(quad_right:n, quad_right:e, quad_right:s, quad_right:w) * touch_right:0:touch * 25)",
}
```
