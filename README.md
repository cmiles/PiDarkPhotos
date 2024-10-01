## Pi Dark Photos

This is a small .NET Core (C#) program designed to be run on a Raspberry Pi that takes x number of photos per day with the extra functionality of triggering
 a light setup via GPIO pin. This program is for use in areas like crawlspaces and tanks where the environment usually dark.

### Program Requirements

 - A Raspberry Pi where .NET Core can run and an attached camera that responds to libcamera-still. This program has only been confirmed to run on a Raspberry Pi 3 A+ with an  [RPi IR-CUT Camera](https://www.waveshare.com/rpi-ir-cut-camera.htm)
 - Lights that can be triggered by a GPIO pin. Tested with the 3.3V lights from the [RPi IR-CUT Camera](https://www.waveshare.com/rpi-ir-cut-camera.htm) package - separated from the lights and powered thru/triggered by an [Adafruit MOSFET Driver](https://www.adafruit.com/product/5648)


### Setup Notes

Suggested setup on the Pi - this is here for convenience and was last updated September 2024. As OS and Pi details change some/most/all of this might become irrelevant, or even harmful, and this by no means covers all the details needed to run a Pi securely and appropriately. The setup below is pretty basic, but use at your own risk!
 - I haven't had any issues with older packages in my own installs, but certainly if the Pi is going to be dedicated to PiSlicedDayPhotos I would suggest running 'sudo apt-get update' and 'sudo apt-get upgrade' before installing the program.
 - Move the PiDarkPhotosProject folder to your home directory:
 - Change the permissions for the program to be executable:
	```
	chmod +x PiDarkPhotosProject/PiDarkPhotos/PiDarkPhotos
	```
 - Run the program as a service: Edit pidarkphotos.service replacing [Your Directory Here] with your home directory, copy it to /etc/systemd/system/, start and follow the service to check for any errors:
	```
	nano PiDarkPhotosProject/PiDarkPhotos/pidarkphotos.service
	sudo cp PiDarkPhotosProject/PiDarkPhotos/pidarkphotos.service /etc/systemd/system/
	sudo systemctl daemon-reload
 	sudo systemctl enable pidarkphotos --now
	journalctl -u pidarkphotos -f
	```

I like to disable the LEDs on the Pi - [How To Easily Disable Status LEDs On RaspberryTips](https://raspberrytips.com/disable-leds-on-raspberry-pi/)
  ```
  sudo nano /boot/firmware/config.txt
  ```
  Add the following lines to the end of the file:
  ```
  #Disable Power LED (Red)
  dtparam=pwr_led_activelow=off
  #Disable Activity LED (Green)
  dtparam=act_led_trigger=none
  dtparam=act_led_activelow=off
  #Disable LAN LEDs
  dtparam=eth_led0=14
  dtparam=eth_led1=14
  # Disable the ACT LED
  ```

My preference is for Automatic/Unattended Upgrades - do this long enough and something unexpected will break, but I would rather stay up to date and have something break sooner rather than later. [Secure your Raspberry Pi by enabling automatic software updates – Sean Carney](https://www.seancarney.ca/2021/02/06/secure-your-raspberry-pi-by-enabling-automatic-software-updates/) and [UnattendedUpgrades - Debian Wiki](https://wiki.debian.org/UnattendedUpgrades)
	```
	sudo apt-get update
	sudo apt-get install unattended-upgrades
	sudo dpkg-reconfigure --priority=low unattended-upgrades
	```

If you've worked in years gone by with the Pi Camera and C# you might know the very useful [techyian/MMALSharp: C# wrapper to Broadcom's MMAL with an API to the Raspberry Pi camera](https://github.com/techyian/MMALSharp) - unfortunately without choosing an older version of Raspberry Pi OS that library no longer works. The Pi has moved on to [libcamera](https://libcamera.org/). I didn't find a C# wrapper for libcamera and since I didn't need to do anything other than write stills to the Pi's storage calling libcamera-still 'command line style' seemed to be the best option.

I didn't find a single great place for libcamera-still documentation - frustrating until I figured out that (beyond 'getting started' content) running 'libcamera-still --help' was really the best single source of information.

### Backstory

I have been using several Raspberry Pis and my [Pi Sliced-Day Photos](https://software.pointlesswaymarks.com/Posts/Software/pi-sliced-day-photos/pi-sliced-day-photos.html) project ([code on GitHub](https://github.com/cmiles/PiSlicedDayPhotos)) for about a year to docment the landscape around our house. Overall the Pis have been reasonably low cost and reasonable low hassle! Recently because of an issue with our Alternative Septic System I wanted to have photos from inside our holding tank - so inspired by the Pi Sliced-Day Photo project I wrote this project. 

### Other Projects

Fundamentally this project is just taking photographs with the Raspberry Pi which is not hard to do and you can find other great free projects and code to take stills, timelapses and more! One of my favorites is [GitHub - thomasjacquin's allsky: A Raspberry Pi operated Wireless Allsky Camera](https://github.com/thomasjacquin/allsky) - I hope to build on of these in the future... Also see [Roll Your Own All-Sky, Raspberry Pi Camera - IEEE Spectrum](https://spectrum.ieee.org/all-sky-camera) and the [Hacker News discussion](https://news.ycombinator.com/item?id=37850485).


### Tools and Libraries

This program would not be possible without the amazing resources available for creating Free software! Used in this project:

**Tools:**
  - [Visual Studio IDE](https://visualstudio.microsoft.com/), [.NET Core (Linux, macOS, and Windows)](https://dotnet.microsoft.com/download/dotnet-core)
  - [ReSharper: The Visual Studio Extension for .NET Developers by JetBrains](https://www.jetbrains.com/resharper/)
  - [GitHub Copilot · Your AI pair programmer · GitHub](https://github.com/features/copilot)
  - [AutoHotkey](https://www.autohotkey.com/)
  - [Compact-Log-Format-Viewer: A cross platform tool to read & query JSON aka CLEF log files created by Serilog](https://github.com/warrenbuckley/Compact-Log-Format-Viewer)
  - [Fork - a fast and friendly git client for Mac and Windows](https://git-fork.com/)
  - [LINQPad - The .NET Programmer's Playground](https://www.linqpad.net/)
  - [Notepad++](https://notepad-plus-plus.org/)

**Core Technologies:**
  - [dotnet/core: Home repository for .NET Core](https://github.com/dotnet/core)

**Libraries:**
  - [GitInfo | Git and SemVer Info from MSBuild, C# and VB](https://www.clarius.org/GitInfo/). MIT License.
  - [SvenGroot/Ookii.CommandLine: Ookii.CommandLine is a powerful and flexible command line argument parsing library for .Net applications, supporting PowerShell-like and POSIX-like conventions, as well as subcommands.](https://github.com/SvenGroot/Ookii.CommandLine?tab=readme-ov-file)
  - [serilog/serilog: Simple .NET logging with fully-structured events](https://github.com/serilog/serilog). Easy full featured logging. Apache-2.0 License.
   - [RehanSaeed/Serilog.Exceptions: Log exception details and custom properties that are not output in Exception.ToString().](https://github.com/RehanSaeed/Serilog.Exceptions) MIT License.
   - [serilog/serilog-formatting-compact: Compact JSON event format for Serilog](https://github.com/serilog/serilog-formatting-compact). Apache-2.0 License.
   - [serilog/serilog-sinks-console: Write log events to System.Console as text or JSON, with ANSI theme support](https://github.com/serilog/serilog-sinks-console). Apache-2.0 License.
  - [toptensoftware/RichTextKit: Rich text rendering for SkiaSharp](https://github.com/toptensoftware/richtextkit). Apache-2.0 License.
  - [mono/SkiaSharp: SkiaSharp is a cross-platform 2D graphics API for .NET platforms based on Google's Skia Graphics Library. It provides a comprehensive 2D API that can be used across mobile, server and desktop models to render images.](https://github.com/mono/SkiaSharp). MIT License.
  - [NUnit.org](https://nunit.org/). [NUnit License](https://docs.nunit.org/articles/nunit/license.html)
  - [thomasgalliker/ObjectDumper: ObjectDumper is a utility which aims to serialize C# objects to string for debugging and logging purposes.](https://github.com/thomasgalliker/ObjectDumper). Apache-2.0 License.
  - [Codeuctivity/SkiaSharp.Compare: Adds compare features on top of SkiaSharp](https://github.com/Codeuctivity/SkiaSharp.Compare). Apache-2.0 License.
