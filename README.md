# SharpMod
A .NET port of MikMod, a soundtrack player library. After 8 years without updates !

# Project Description

A Soundtracker Module music player in C# (100% Managed, without interops), At the beginning, a simple port in C# of the famous Mikmod Library. Today, it has some improvements. Three versions are avaible : NAudio Output, Silverlight and XNA (Windows Phone 7, Windows and Xbox360).The code is shared by links between projects from a trunk "Core" Project. Audio renderers are saved in each project files (Windows, Silverlight and XNA)

## SilverLight Screenshot :

Not Avaiable... Silverlight Version will not be maintained.

## Winforms Screenshot :

![alt text](https://github.com/Mdwf-droid/SharpMod/blob/master/SharpMod.Images/WinformDemo.png?raw=true)

## Command line, with track view and position

![alt text](https://github.com/Mdwf-droid/SharpMod/blob/master/SharpMod.Images/ConsoleDemo.png?raw=true)

# Project's Goal

This library allow you to add soundtracker music replay to your .NET applications. It's designed for framework 3.5 and 4.0.
Actually, Protracker, FastTracker ans ScreamTracker formats are implemented.


5 Lines of code to do this :

SongModule myMod = ModuleLoader.Instance.LoadModule("SongModule.Mod|S3M|XM");
ModulePlayer player= new ModulePlayer(myMod);

// Silverlight Driver
SharpMod.SoundRenderer.SilverlightDriver drv = new SharpMod.SoundRenderer.SilverlightDriver(MyMediaElement);
// Or NAudio Driver
SharpMod.SoundRenderer.NAudioWaveChannelDriver drv = new SharpMod.SoundRenderer.NAudioWaveChannelDriver(NAudioWaveChannelDriver.Output.WaveOut);
// Or XNA Driver
SharpMod.SoundRenderer.XnaSoundRenderer drv = new XnaSoundRenderer(new DynamicSoundEffectInstance(48000, AudioChannels.Stereo));

player.RegisterRenderer(drv);
player.Start();



# Where can I found a Song Module ???

On you're Amiga old floppies or.... 

The Mod Archive http://www.modarchive.org
Amiga Music Preservation : http://amp.dascene.net/home.php
and many other ways :o)

# Performance

 - The replay routine take between 1% and 18% Cpu depending of module played on my PM1.5Ghz centrino laptop.
 - Better on my E6300 1.8Ghz desktop.
 - Seems to have minor problems on my wife's N270 1.6 netbook (have to test again). Have tested, it's the FFT and visualizers who are too expensive.
 - Seems to play fine with the WP7 Emulator. I Need some help to test on real WP7 Hardware.
 - Take a maximum of 5% on my new I5-2537 Alienware M11X on complex Module (32 chans with lots of effects and big samples)

# Known issues

The Naudio version will hang up on 64 bits systems (error while loading the NAudio assembly). I'll try to fix it with a modified version of NAudio.

# History

29/05/2011.. first XNA's renderer work
- Thx to the DynamicSoundEffectInstance class... the first attempt to play Mod on WP7 is successful

23/07/2011 All Xna Platforms are are ok
-See the source code, release will be coming soon.

07/11/2011 Welcome to a new developer
- Since the november 4th, Waldog78 will contribute to the project with me.I Hope that we can deliver a new release as soon as possible, including the full XNA binaries and samples for all platfroms and major fixes of the issues.
- The VS2008 solutions will be obsolete, all code will be maintened under VS2010. The target Platforms are .NET 3.5, Silverlight 4 and XNA 4.0

The XNA sound renderer is present in the svn source code. Not yet avaiable as stable release.

14/09/2012 SharpMod is still alive

 - A Major release will be coming soon, stay tuned

- Enjoy the new SharpMod Logo :)


# Donate

Perhaps later... :)



# Future
- Maintain this piece of code ?
- try to solve bugs
- make a Mono sound renderer (Have some ideas with PortAudio)... perhaps with .NET Core...
- port to .NET Core

Have fun !
