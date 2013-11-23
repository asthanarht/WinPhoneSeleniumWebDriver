Windows Phone Selenium WebDriver

1. Copyright and License

  Copyright Microsoft Corporation, Inc.
  All Rights Reserved

  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 
  THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
  INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 
  
  See the Apache 2 License for the specific language governing permissions and limitations under the License.

2. About

  This project provides support for Selenium on Windows Phone platform.
  Project consist of:
    - The driver itself represented as a normal WindowsPhone application (*.xap file)
    - Driver deployment tool (DriverDeploymentTool.exe) which tool made for automated
      Deployment and configuration of the driver.

  Selenium is a framework build in sprit of client-server model, in this approach
  the WebDriver is a server (listens for connections) and created tests are 
  clients that connects to the server and sending commands.

  In fact, communication protocol between driver and client tests is specified as
  JsonWireProtocol (https://code.google.com/p/selenium/wiki/JsonWireProtocol)

3. Usage

    3.1 Requirements
      - Isolated storage explorer tool (ISETool.exe) from WinPhone SDK
      - Windows Phone 8 emulator image in your hyper-v configuration
        it can be obtained from Windows Phone SDK and/or WP8 device 
        (We tested with both phone emulator, and with vanilla physical devices only. 
        We did not test against dev-unlocked physical devices.)

    3.2 Distribution
      You'll need the following binaries to start the driver:
        - WindowsPhoneDriver_Release_x86.xap - Windows 8 package to be deployed to the target device
        - DriverDeployTool.exe - deployment tool (see 3.3), to be run on a Windows machine
          (Running it is not a requirement but convenience. You can deploy XAP manually, and provide Phone's 
          IP address to RemoteWebDriver. DriverDeployTool simply automate it for you.)
        - WindowsPhoneDriver.xml - sample config for DriverDeployTool
        - XapDeployDll.dll - required by DriverDeployTool
        - StartWindowsPhoneDriver.cmd - sample script to call DriverDeployTool to setup emulator end-to-end

    3.3 Driver deployment tool

      The tool itself provides 3 functions:
        - deploying *.xap file into the emulator/device
        - configuring driver via configuration file uploaded through
          ISETool.exe
        - retrieving WP8 emulator/device ip address and returning it
          as a environmental variable.

      Parameters:

          /isetool      - Path to the ISETool.exe 
          /targetdevice - 'device' of 'emulator'
          /xapfile      - Path to the *.xap application file
          /config       - Name of the config file
          /setenv       - Name of the environmental variable that
                          will contain IP address associated
                          with the device/emulator

     3.4 Configuration file
       
       Currently configuration file provides only one option, it's a number of port
       on which the driver should listen.

       Example configuration file is given below:
         <?xml version="1.0" encoding="utf-8"?>
         <DriverConfig>
           <Port>8080</Port>
         </DriverConfig>

       When DriverDeployTool.exe will finishes it work, you can simply start your tests using Selenium
       WebDriver interface for example:

         RemoteWebDriver driver = RemoteWebDriver(new Uri("http://157.59.109.235:8080"), DesiredCapabilities.InternetExplorer());
         driver.Navigate().GoToUrl("https://winphonewebdriver.codeplex.com/");
         ...

4. Development notes

   If you want to add another functionalities, keep that in mind that, interaction with WebBrowser control
   can be only done in the main thread.

5. Limitations

   - No support for touching automations
   - No support for multiple windows
   - No support for frames

   All functions that are not implemented will return appropriate error message specified as in
   JsonWireProtocol document.

5. Third party dependencies
    The following third-party products were used to create Windows Phone WebDriver:
    - Selenium (http://www.seleniumhq.org/, Apache 2.0 license)
    - Newtonsoft.Json.dll (https://json.codeplex.com/, The MIT License (MIT))
 