# Table of Contents

[Commands](COMMANDS.md)  
[Variables](VARIABLES.md)  

# Commands

The following commands are available in your EasyLOU LUA scripts.  
Commands are not in alphabetic order. Rather, they are (somewhat) grouped by functionality.

- [FindItem](#FindItem)
- [FindPermanent](#FindPermanent)
- [Key](#Key)
- [Move](#Move)
- [Stop](#Stop)
- [ScanJournal](#ScanJournal)
- [Macro](#Macro)
- [Say](#Say)
- [SayCustom](#SayCustom)
- [ToggleWarPeace](#ToggleWarPeace)
- [TargetDynamic](#TargetDynamic)
- [TargetPermanent](#TargetPermanent)
- [TargetLoc](#TargetLoc)
- [LastTarget](#LastTarget)
- [TargetSelf](#TargetSelf)
- [AttackSelected](#AttackSelected)
- [UseSelected](#UseSelected)
- [ContextMenu](#ContextMenu)
- [Drag](#Drag)
- [Dropc](#Dropc)
- [Dropg](#Dropg)
- [FindMobile](#FindMobile)
- [FindGameObject](#FindGameObject)
- [SetUsername](#SetUsername)
- [SetPassword](#SetPassword)
- [Login](#Login)
- [SelectServer](#SelectServer)
- [CharacterSelect](#CharacterSelect)
- [FindPanel](#FindPanel)
- [FindButton](#FindButton)
- [ClickButton](#ClickButton)
- [GetTooltip](#GetTooltip)
- [SetInput](#SetInput)
- [SetTargetFrameRate](#SetTargetFrameRate)
- [SetVSyncCount](#SetVSyncCount)
- [SetMainCameraMask](#SetMainCameraMask)


## FindItem

Parameters:  

- Item: either a number or a string.
- ContainerID: a number, optional.

Useful for pretty much any item in game.
If Item is a number, FindItem will search for a dynamic object having the given ObjectID.
If Item is a string, FindItem will search for all the dynamic objects whose Name contains the given string.

If ContainerID is given, FindItem will only search within the container having the given ObjectId.

### Examples
  - FindItem("hatchet") *finds all hatchets and stores the result in FINDITEMID*
  - FindItem("hatchet",tonumber(BACKPACKID)) *finds all hatchet in your backpack and stores the result in FINDITEMID*

## FindPermanent

Parameters:  

- Permanent: either a number or a string.
- Distance: maximum search distance. If not specified, it defaults to 30.

Useful for trees, rocks, etc.
If Permanent is a number, FindPermanent will search for a permanent object having the given PermanentID.
If Permanent is a string, FindPermanent will search for all the permanent objects whose Name contains the given string.

### Examples
  - FindPermanent("tree") *finds all trees and stores the result in FINDITEMID*

## Key

Unused.

## Move

Parameters:  
- x: x coordinate.
- y: y coordinate.
- z: z coordinate.

or alternatively

- id: ObjectID.

Character will attempt to move at the given location x, y, and z, or if an ObjectID was provided, will attempt to move at the location of the given ObjectID.  
Please note that pathfinding will be used, but if there are obstacles such as doors the action will probably fail and character will stay at the original position.  
Likewise, if the location is too far from the current position than just a couple of screens, the action will probably fail and character will stay at the original position.  
A sleep() command may be desired after the call to Move() to allow the character time to walk to the location.

## Stop

Parameters:  
None.

If the character is moving, it will stop.

## ScanJournal

Parameters:  
- timestamp: timestamp in seconds.

The journal will be scanned, and it will return the first entry that immediately follows the given timestamp.  
If the timestamp provided is equal to 0, it will return the very first message of the journal.  

## Macro

Parameters:
- macro: a number.

It will execute the given macro.  
Macros dropped on the left bar are usually from 0-27.  
Primary action (usually bound to Q) is usually 27.  
Secondary action (usually bound to E) is usually 28.  

## Say

Parameters:
- text: a string.

Will say the given text.
This can also be used to trigger scripts commands, i.e. commands that starts with "/" such as "/wave".

## SayCustom

Parameters:
- text: a string.

Useful for context-menu commands such as "Stack Contents", "Release", etc. See 04-finditem-drag-split example file.  

## ToggleWarPeace

Parameters:
None.

Toggles between War and Peace.

## TargetDynamic

Parameters:
- id: the ObjectID to target.

Targets a dynamic object (i.e. an item, or a mobile).

### Examples
  - FindItem("bear") 
  - TargetDynamic(tonumber(FINDITEMID))

## TargetPermanent

Parameters:
- id: the PermanentID to target.

Targets a permanent object (i.e. a rock, or a tree).

### Examples
  - FindPermanent("tree") 
  - TargetPermanent(tonumber(FINDITEMID))

## TargetLoc

Parameters:
- x (number): x coordinate to target.
- y (number): y coordinate to target.
- z (number): z coordinate to target.
- objectId (number): not sure, may not be needed. it looks like an object with a given objectId can be targeted at the given coords.

Targets the given location.

## LastTarget

Parameters:
None.

Execute the LastTarget macro.

## TargetSelf

Parameters:
None.

Execute the TargetSelf macro.

## AttackSelected

Parameters:
- id: the ObjectID to attack.

Attack the given ObjectID.

## UseSelected

Parameters:
- id: the ObjectID to use.

Use the given ObjectID. It is the equivalent of Double Clicking the object.

### Examples

```lua
FindItem("cotton")
local cotton_id = ExtractFirstId(FINDITEMID)
UseSelected(cotton_id)
```

## ContextMenu

Parameters:
- id: the ObjectID to trigger the command on.
- command: the Context Menu command to trigger.

Triggers the given Context Menu command on the given object.

### Examples
   
 ```lua
ContextMenu(CHARID,"Dismount")
```

 ```lua
 ContextMenu(CHARID,"Power Hour")
```

 ```lua
 ContextMenu(FINDITEMID,"Split Stack")
```

 ```lua
 ContextMenu(FINDITEMCNTID,"Stack Contents")
```


## Drag

Parameters:
- id: the ObjectID to drag.

Drag the given ObjectID.

## Dropc

Parameters:
- id: the ContainerID where the dragged item should be dropped.

To be used in conjunction with Drag.  
Drops the currently dragged item into the given ContainerID.

### Example
   
  - Drag(itemId)
  - Dropc(tonumber(BACKPACKID))

## Dropg

Parameters:
- x (number): x coordinate.
- y (number): y coordinate.
- z (number): z coordinate.

To be used in conjunction with Drag.  
Drops the currently dragged item at the given location.

## FindMobile

Parameters:
- Mobile: can be a number or a string.
- Distance: maximum search distance. If not specified, it defaults to 30.

Useful for searching mobs, npcs, etc.  
If Mobile is a number, FindMobile will search for a mobile object having the given id.  
If Mobile is a string, FindMobile will search for all the mobile objects whose Name contains the given string.  

## FindGameObject

Parameters:
- Name: a string.

FindGameObject will search for all the Unity GameObjects whose Name contains the given string.

## SetUsername

Parameters:
- Username: a string.

Sets the Username textbox. This can be used at login.

## SetPassword

Parameters:
- Password: a string.

Sets the Password textbox. This can be used at login.

## Login

No parameters.

Triggers the login button. This can be used at login.

## SelectServer

Parameters:
- URL: a string.

Connects to the custom server with the given URL. This can be used at the server selection page.

## CharacterSelect

Parameters:
- Character: a number.

Selects the given character (0-3). This can be used at the character selection page.

## FindPanel

Parameters:
- Name: a string.

FindPanel will search for all the panels whose Name contains the given string.

### Example

FindPanel("Tracking") *result is stored in FINDPANELID*

## FindButton

Parameters:
- Panel: a string.
- Name: a string.
or
- Panel: a string.
- x: a GUI x coordinate.
- y: a GUI y coordinate.

If Name is provided, FindButton will search within the provided Panel all the buttons whose Name/Tooltip/Command contains the given Name string.  
If X and Y are provided, FindButton will search within the provided Panel all the buttons that are clickable at the given GUI coordinates X and Y.  

### Example

  - FindButton("CraftingWindow", "favorites")
  - ClickButton("CraftingWindow", FINDBUTTONNAME)


## ClickButton

Parameters:
- PanelName: a string.
- ButtonName: a string.

Trigger a click event on the button having the name specified and contained in the specified panel.  Mouse over the button needed, and the ButtonName will be stored in the MOUSEOVERUINAME variable

### Example

  - FindPanel("Tracking")
  - ClickButton(FINDPANELID, "0")

## GetTooltip

Parameters:
  - id: the ObjectID to examine

Gets the tooltip data of an object.  Each line of the tooltip is split with a '|' character and stored in the TOOLTIPTEXT variable.

### Example

  - FindItem("Chest");
  - GetTooltip(FINDITEMID);

  
## SetInput

Parameters:
- ContainerName: a string.
- InputName: a string.
- NewValue: a string.

Sets the value of the input having the name specified and contained in the specified panel.

## SetTargetFrameRate

Parameters:
- FrameRate: a number.

Set the given target frame rate.

## SetVSyncCount

Parameters:
- VSyncCount: a number.

Set the given target VSync count.

## SetMainCameraMask

Parameters:
- CameraMask: a number.

Set the given camera mask. 
If 0, will competely disable the rendering (CPU and GPU reduced usage).  
If -1, will re-enable the rendering (CPU and GPU high usage).  
Handy for multiboxing.  
