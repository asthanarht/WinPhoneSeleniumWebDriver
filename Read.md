**Project Description**


**!! How can I use it?**
Start with reading about [url:Selenium WebDriver|http://www.seleniumhq.org/].

Selenium provides a common framework for automating Web applications, with unified API and bindings implemented for many languages. It is one of possible implementations of the [url:W3C Working Draft of WebDriver|http://www.w3.org/TR/webdriver/], so, potentially, our product will work with any of them.

Selenium supports multiple web browsers and operating systems by abstracting communication between the framework and real browsers via different WebDrivers. There are implementations for Firefox, Internet Explorer, Google Chrome, Android devices, iOS, etc. We provide yet another WebDriver, for Windows Phone platform.

We tested it with both dev-unlocked physical phones, and with Windows Phone emulator, on Windows 8/Windows Server. However, once you deploy it to the phone/emulator and obtain TCP/IP address and port - you can automate it from any platform.

!! How can I use it, part II. I know how to use Selenium!

# Obtain and install [url:Windows Phone 8 SDK|http://www.microsoft.com/en-us/download/details.aspx?id=35471]
# You will need ISETool.exe which is a part of Windows Phone SDK and installed as its part
# If you want to automate Windows Phone emulator, not a physical device - then you'll need Windows Phone 8 emulator image, which can be obtained from Windows Phone SDK, or a Windows Phone device.
# Download our driver and unpack it anywhere on your local disk
# Deploy WindowsPhoneDriver.xap to the phone! If you use emulator, just run StartWindowsPhoneDriver.cmd and it will do everything for you, and prints the IP address/port to the console (for example, "http://157.59.109.235:8080/"). It will also set system environment variable REMOTEWEBDRIVERIP.
# Use it from your code with RemoteWebDriver!

**C#:**

RemoteWebDriver driver = RemoteWebDriver(
  new Uri("http://157.59.109.235:8080"), 
  DesiredCapabilities.InternetExplorer());

driver.Navigate().GoToUrl("https://winphonewebdriver.codeplex.com/");



**Powershell:**

Add-Type -Path C:\Projects\UITests\WebDriver.dll
$driver = New-Object OpenQA.Selenium.Remote.RemoteWebDriver( `
  "http://157.59.109.235:8080", `
  [OpenQA.Selenium.Remote.DesiredCapabilities]::InternetExplorer())

$navigate = $driver.Navigate()
$navigate.GoToUrl("https://winphonewebdriver.codeplex.com/")
"Last edited by {0}" -f $driver.FindElementById("wikiEditByLink").Text | Write-Host


**!! Architecture**
This project provides support for Selenium on Windows Phone platform.
Project consist of:
* The driver itself represented as a normal WindowsPhone application (*.xap file)
* Driver deployment tool (DriverDeploymentTool.exe) which tool made for automated deployment and configuration of the driver.

Selenium is a framework build in spirit of client-server model, in this approach the WebDriver is a server (listens for connections) and created tests are clients that connects to the server and sending commands.

In fact, communication protocol between driver and client tests is specified as  [url:JsonWireProtocol|https://code.google.com/p/selenium/wiki/JsonWireProtocol]

**!! Distribution**
* WindowsPhoneDriver.xap - Windows 8 package to be deployed to the target device
* DriverDeployTool.exe - deployment tool (see below), to be run on a Windows machine (Running it is not a requirement but convenience. You can deploy XAP manually, and provide Phone's IP address to RemoteWebDriver. DriverDeployTool simply automate it for you.)
* WindowsPhoneDriver.xml - sample config for DriverDeployTool
* XapDeployDll.dll - required by DriverDeployTool
* StartWindowsPhoneDriver.cmd - sample script to call DriverDeployTool to setup emulator end-to-end

!! Driver Deployment Tool
The tool itself provides 3 functions:
* deploying *.xap file into the emulator/device
* configuring driver via configuration file uploaded through ISETool.exe
* retrieving WP8 emulator/device ip address and returning it as a environmental variable.

**Parameters:**

{{
/isetool      - Path to the ISETool.exe 
/targetdevice - 'device' of 'emulator'
/xapfile      - Path to the *.xap application file
/config       - Name of the config file
/setenv       - Name of the environmental variable that will contain IP address associated with the device/emulator
}}

Configuration file

Currently configuration file provides only one option, it's a port number on which the driver should listen.

Example configuration file is given below:
{{
<?xml version="1.0" encoding="utf-8"?>
<DriverConfig>
  <Port>8080</Port>
</DriverConfig>
}}

**!! Development notes**
If you want to add another functionality, keep that in mind that, interaction with WebBrowser control can be only done in the main thread.

**!! Limitations**
* No support for touching automation
* No support for multiple windows
* No support for frames

All functions that are not implemented will return appropriate error message specified as in JsonWireProtocol document.

**!! Third party dependencies**
The following third-party products were used to create Windows Phone WebDriver:
* [url:Selenium|http://www.seleniumhq.org/], Apache 2.0 license
* [url:Newtonsoft.Json.dll|https://json.codeplex.com/], The MIT License (MIT)
