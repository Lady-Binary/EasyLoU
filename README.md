[![Build status](https://ci.appveyor.com/api/projects/status/slacd3eo77qmy1fp/branch/master?svg=true)](https://ci.appveyor.com/project/Lady-Binary/easylou/branch/master)

# Easy LOU

![EasyLOU Screenshot](Screenshot.png?raw=true "EasyLOU Screenshot")

## So what is EasyLOU?

EasyLOU is a totally free tool inspired by EasyUO, the popular premiere macroing tool for Ultima Online. 

## Why EasyLOU and not EasyLOA?

For two reasons:  
- The name EasyLOA was already taken by https://github.com/MythikGN/EasyLoA :)
- As a tribute to the [Legends of Ultima](https://www.legendsofultima.online/) community server.    

## What can I do with EasyLOU?

With EasyLOU you can write [Lua 5.2](https://www.lua.org/manual/5.2/) scripts that will make your Legends of Ultima character perform pretty much anything you want them to.

## How do I get started?

- Read the [Instructions](#how-does-it-work) below in order to learn how to launch EasyLOU and connect it to Aria
- Take a look at the scripts provided with the [Examples/](Examples/) folder
- Take a look at the available [Commands](COMMANDS.md) and [Variables](VARIABLES.md)
- Learn some [Lua](https://www.lua.org/manual/5.2/)
- Enjoy!

## Is EasyLOU compatible with the official Legends of Aria servers?

The community server Legends of Ultima is the reason why this tool exists: we enjoy this community server and have decided to write this tool and name EasyLOU after it.  
EasyLOU has only been tested with the Legends of Ultima Community Server only as of today.  
EasyLOU may or may not work with official Legends of Aria servers run by Citadel Studios.  

## Why is EasyLOU not working with the new version of Legends of Aria?

Some Legends of Aria client updates may break the compatibility with EasyLOU.  
When you download the latest EasyLOU release archive, double check the release notes: there is usually an indication of which version of Legends of Aria client the release is compatible with.  
Upon the release of a new Legends of Aria Client, it may take some time before we update EasyLOU to be compatible with the latest Legends of Aria client.  
Volunteers are always welcome :)

## How does it work?

- Download the latest build from the Releases tab here on GitHub
- Extract it and place its content in the folder where the file Assembly-CSharp.dll resides (usually C:\Program Files\Legends of Aria Launcher\Legends of Aria\Legends of Aria_Data\Managed)
- Launch EasyLOU.exe
- Once EasyLOU is launched, launch your Legends of Aria Client
- Wait for your Legends of Aria Client login page to show up
- On EasyLOU click on the Connect To Client Icon ![icon](EasyLOU/icons/uo.ico?raw=true "Connect to Client Icon"); the mouse cursor should turn into a beautiful UO icon
- Click on the Legends of Aria Client you want to connect to
- When asked, click OK to inject
- Load a script
- Enjoy!

*Note*: On Windows 10, upon launcing EasyLOU.exe you might get a "Microsoft Defender SmartScreen prevented an unrecognised app from starting." error message. If you know what you're doing, open the properties menu of EasyLOU.exe, and check the "Unblock" click apply, click OK, and relaunch EasyLOU.exe.  

## Yes, ok, but how does it *really* work?

EasyLOU.exe injects LOU.dll into the Legends of Aria client process, using a well known technique for injecting assemblies into Mono embedded applications, commonly Unity Engine based games.  
EasyLOU.exe acts as the GUI while LOU.dll acts as the commands engine, and they communicate via two shared memory maps:  
- A ClientCommand memory map, where EasyLOU.exe queues all the commands that have to be processed and executed by the LoA Client
- A ClientStatus memory map, where LOU.dll updates the status of the LoA Client and the answers to various commands by populating a bunch of variables  
Credits for various components and implementations can be found at the bottom of this page.  

## Can I run multiple scripts in parallel?

Yes, but it has not been extensively tested yet.

## Can I multibox?

Yes.  
You need to open one instance of EasyLOU, lunch a client, and conect EasyLOU to it.  
Then you can open another instance of EasyLOU, lunch another client, and connect EasyLOU to it.  
Rinse, and repeat.  

## How can I build EasyLOU?

EasyLOU has been compiled with Visual Studio Community 2017 and .NETFramework Version v4.7.2.  
In order to build it, you need to own a copy of the Legends of Aria client, and you need to copy into the LOU\libs\ folder the following libraries which you can take from the C:\Program Files\Legends of Aria Launcher\Legends of Aria\Legends of Aria_Data\Managed folder (or whatever path you have installed your client into):

Assembly-CSharp-firstpass.dll  
Assembly-CSharp.dll  
CoreUtil.dll  
MessageCore.dll  
protobuf-net.dll  
UnityEngine.CoreModule.dll  
UnityEngine.InputLegacyModule.dll  
UnityEngine.InputModule.dll  
UnityEngine.PhysicsModule.dll  
UnityEngine.UI.dll  

## How can I contribute?

Please Star this repository, Fork it, and engage.  
If you are a developer, GitHub Issues and GitHub PRs are always welcome.  
If you have scripts you want to share with the community, please feel free to reach out, or just create a GitHub Issue and attach the script to it.  

We are hoping to create a new community around this tool, so any form of contribution is more than welcome!

# IMPORTANT DISCLAIMER

By using EasyLOU you may be breaching the Terms and Conditions of Citadel Studios, Legends of Aria, Legends of Ultima, or whatever community server you are playing on or service you are using.

***USE AT YOUR OWN RISK***

# IMPORTANT RECOMMENDATIONS

ONLY download EasyLOU from its official repository: NEVER accept a copy of EasyLOU from other people.  
ONLY run scripts that you can understand: NEVER accept scripts from people you do not know and trust.  

Keep in mind, there is always a possibility that a malicious version of EasyLOU or a malicious script will steal your LOU/LOA or Steam credentials or cause other damage. You assume the risk and full responsibility for all of your actions.  

Also: don't be evil.

***USE AT YOUR OWN RISK***

# CREDITS

Legends of Aria is copyright of [Citadel Studios](https://citadelstudios.net/).  
LadyBinary is a tribute to Lord Binary, who was very active in the UO hacking scene (see for example [UO_RICE](https://github.com/necr0potenc3/UO_RICE)).  
EasyLOU is of course inspired by the great [EasyUO](http://www.easyuo.com/).  
The LOU part of EasyLOU is a tribute to [Legends Of Ultima Online](https://www.legendsofultima.online/), whose passionate staff have dedicated so much effort in putting together a wonderful product based off of [Legends of Aria](https://www.legendsofaria.com/).  
The Lua engine is based on [MoonSharp](https://github.com/moonsharp-devs/moonsharp/), commit 4e748a7 plus minor enhancements.  
The Text editor is based on [ICSharpCode.TextEditorEx](https://github.com/StefH/ICSharpCode.TextEditorEx), commit 1934da7 plus minor enhancements.  
The Mono Injection code is based on [SharpMonoInjector](https://github.com/warbler/SharpMonoInjector), commit 73566c1.  

# CONTACTS

You can contact me at ladybinary@protonmail.com

License
-------

This project is licensed under a 3-clause BSD license, which can be found in the [LICENSE](LICENSE) file.  
