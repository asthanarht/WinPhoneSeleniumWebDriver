//---------------------------------------------------------------------------------------------------------------------------------------------------
// Copyright Microsoft Corporation, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 
// See the Apache 2 License for the specific language governing permissions and limitations under the License.
//---------------------------------------------------------------------------------------------------------------------------------------------------

/*
 * Tool for deploying Windowd Phone Selenium driver
 * with specyfied configuration file
 * Steps this is app is going to take:
 *  1) Obtain GUID of given XAP
 *  2) deploy app to specyfied target (emulator, device)
 *  3) write configuration file to targets isolated storage
 *  4) Pull back XML with all IP addresses of associated with device
 *  
 * Dependencies (all of them can be found in the Windows Phone SDK pack):
 *  1) XapDeployCmd.dll
 *  2) ISETool.exe
 *  
 * Args:
 *  /xap <xapfile>
 *  /conf <conf-dir>
 *  /xapdeploycmd <path> (optional, assuming defined in %PATH%)
 *  /isetool <path> (optional, assuming defined in %PATH%)
 *  /targetdevice <emulator|device>
 *  /setenv (optional)
 */
using System;

namespace DriverDeployTool
{
    class Program
    {
        static int Main(string[] args)
        {
            DeploymentAutomator autmator = new DeploymentAutomator(args);

            try
            {
                autmator.Run();
                Console.WriteLine(autmator.RemoteUrl);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 2;
            }

            return 0;
        }
    }
}
