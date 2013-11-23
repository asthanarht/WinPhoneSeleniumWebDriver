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

using Microsoft.Phone.Tools;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace DriverDeployTool
{
    public enum ConfigOption
    {
        XapFile,
        IseTool,
        Config,
        Nothing,
        TargetDevice,
        SetEnv,
        Help
    }

    public class DeploymentAutomator
    {
        public string ConfigPath {get; private set;}
        public string XapFilePath {get; private set;}
        public string TargetDevice { get; private set; }
        public string XapDeployCmdPath {get; private set;}
        public string ISEToolPath {get; private set;}
        public string LastError {get; private set;}
        public string EnvVarName { get; private set; }
        public string RemoteUrl { get; private set; }

        private string help;
        private string ipAddresses = string.Empty;
        private string tempConfigDir;
        private string port = string.Empty;
        private string[] args;
        private Guid productID;
        private const string configFileName = "WebDriverConfig.xml";
        private ConfigOption[] requiredOptions = new ConfigOption[] {
            ConfigOption.XapFile,
            ConfigOption.TargetDevice,
            ConfigOption.Config,
            ConfigOption.SetEnv
        };

        public DeploymentAutomator(string[] args)
        {
            this.args = args;
            //Assuming path, if option /isetool is specyfied
            //this value will be overriden
            this.ISEToolPath = "ISETool.exe";
            this.LastError = string.Empty;
            SetHelp();
        }

        private void SetHelp()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Windows Phone Selenium driver deployment tool");
            sb.AppendLine("Args:");
            sb.AppendLine("/xapfile <xapfile-path> xap file to deploy path");
            sb.AppendLine("/config <config-path> path config file");
            sb.AppendLine("/isetool <isetool-path> path to ISETool.exe");
            sb.AppendLine("/targetdevice <emulator|device> destination of deployment");
            sb.AppendLine("/setenv <env-name> name of enviromenat variable, after");
            sb.AppendLine("\tall it will contain URL for Selenium's RemoteWebDriver class contructor");

            this.help = sb.ToString();
        }

        public void Run()
        {
            ParseArgs();
            Deploy();
        }

        private void Deploy()
        {
            MoveConfigToTempDir();
            EnsurePortDefinition();
            GetXapGUID();
            DeployXap();
            GetDeviceIP();
            WriteConfig();
            FindValidIp();
        }

        private void FindValidIp()
        {
            string validIp = string.Empty;
            try
            {
                validIp = CheckAllAddresses();
            }
            catch
            {
                throw new Exception("Target dosen't have any interface allowing connection.");
            }
            
            this.RemoteUrl = string.Format("http://{0}:{1}/", validIp, this.port);
            Environment.SetEnvironmentVariable(this.EnvVarName, this.RemoteUrl, EnvironmentVariableTarget.User);
        }

        private void GetXapGUID()
        {
            Guid? guid = null;
            Version manifestVersion = null;
            bool isNative = false;

            try
            {
                Utils.ReadWMAppManifestXaml(this.XapFilePath, out guid, out manifestVersion, out isNative);
                this.productID = guid.GetValueOrDefault();
            }
            catch
            {
                throw new Exception("Cannot read ProductID from XAP file.");
            }
        }

        private void DeployXap()
        {
            string[] par = null;

            try
            {
                var Target = ETypeOfDevice.tdDefaultEmulator;
                int n = 0;

                if (this.TargetDevice == "emulator")
                {
                    Target = ETypeOfDevice.tdDefaultEmulator;
                }
                else if (this.TargetDevice == "device")
                {
                    Target = ETypeOfDevice.tdDevice;
                }
                else
                {
                    try
                    {
                        n = int.Parse(TargetDevice);
                        Target = ETypeOfDevice.tdIndexedDevice;
                    }
                    catch
                    {
                        throw new Exception("Cannot parse indexed device");
                    }
                }

                if (Target == ETypeOfDevice.tdIndexedDevice)
                {
                    XapDeployUtil.DoXapDeployCommand(
                        EXapDeployCmd.cmdInstallLaunch,
                        this.XapFilePath,
                        new TargetedDevice(ETypeOfDevice.tdIndexedDevice, n),
                        out par);
                }
                else
                {
                    XapDeployUtil.DoXapDeployCommand(
                        EXapDeployCmd.cmdInstallLaunch,
                        this.XapFilePath,
                        new TargetedDevice(Target),
                        out par);
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Cannot deploy app: {0}", e.Message));
            }
        }

        private void WriteConfig()
        {
            string target = string.Empty;

            if (this.TargetDevice == "emulator")
            {
                //this option names are specyfic for ISETool.exe
                target = "xd";
            }

            if (this.TargetDevice == "device")
            {
                target = "de";
            }
            //TODO: indexed device

            string calle = string.Format("rs {0} {1} \"{2}\"",
                target,
                this.productID,
                this.tempConfigDir
                );

            try
            {
                if (File.Exists(Path.Combine(this.tempConfigDir, configFileName)) == false)
                {
                    throw new Exception(string.Format("Config directory {0} doesn't contain config file named {1}.", this.tempConfigDir, configFileName));
                }

                Process process = new Process();

                process.StartInfo = new ProcessStartInfo(this.ISEToolPath, calle)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception(string.Format("Problems while writing config (ISETool.exe output : {0})", output));
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Cannot send config file. {0}", e.Message));
            }
            finally
            {
                Directory.Delete(tempConfigDir, true);
            }
        }

        private void EnsurePortDefinition()
        {
            string content = File.ReadAllText(Path.Combine(this.tempConfigDir, configFileName));
            var doc = new XmlDocument();

            try
            {
                doc.LoadXml(content);

                XmlNode port = doc.SelectSingleNode("/DriverConfig/Port");

                if (port == null)
                {
                    throw new Exception("<Port> is not defined in <DriverConfig>");
                }

                this.port = port.InnerText;

            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// This is due to way in which ISETool.exe works, it requires directory as a parameter, not particular file
        /// </summary>
        /// <returns>Temp dir. path.</returns>
        private void MoveConfigToTempDir()
        {
            string tempDir = CreateTempDir();
            string fullFilePath = Path.Combine(new string[] { tempDir, configFileName });

            File.Copy(this.ConfigPath, fullFilePath);

            this.tempConfigDir = tempDir;
        }

        private string CreateTempDir()
        {
            Guid name = Guid.NewGuid();
            string tempPath = System.IO.Path.GetTempPath();
            string path = Path.Combine(new string[] {tempPath, name.ToString()});

            Directory.CreateDirectory(path);

            return path;
        }

        private void GetDeviceIP()
        {
            /*
             * Use IESTool.exe to pull file with internal data from phone
             */
            string target = string.Empty;
            string tempDir = string.Format("{0}{1}", Path.GetTempPath(), Guid.NewGuid());
            string configFileName = "WindowsPhoneConfig.xml";
            string configFilePath = string.Format(@"{0}\IsolatedStore\{1}", tempDir, configFileName);

            if (this.TargetDevice == "emulator")
            {
                target = "xd";
            }

            if (this.TargetDevice == "device")
            {
                target = "de";
            }
            //TODO: indexed device

            string calle = string.Format("ts {0} {1} {2}",
                target,
                this.productID,
                tempDir
                );

            Directory.CreateDirectory(tempDir);
            while (File.Exists(configFilePath) == false)
            {
                try
                {
                    Process process = new Process();

                    process.StartInfo = new ProcessStartInfo(this.ISEToolPath, calle)
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();

                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception(string.Format("Problems while getting IP address (ISETool.exe output : {0})", output));
                        
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("Cannot send config file. Check ISETool path. {0}", e.Message));
                }
            }

            try
            {
                this.ipAddresses = File.ReadAllText(configFilePath);

                Directory.Delete(tempDir, true);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Problems while reading IP addresse from device. {0}", e.Message));
            }
        }

        private string CheckAllAddresses()
        {
            var doc = new XmlDocument();

            doc.LoadXml(this.ipAddresses);

            XmlNodeList ips = doc.SelectNodes("/List/Ip");

            foreach (XmlNode ip in ips)
            {
                if (Connect(ip.InnerText) == true)
                {
                    return ip.InnerText;
                }
            }

            throw new Exception();
        }

        private bool Connect(string ip)
        {
            string url = string.Format("http://{0}:{1}/status", ip, this.port);

            try
            {
                var request = HttpWebRequest.Create(url);
                using (var response = request.GetResponse())
                {
                    using (var receiveStream = response.GetResponseStream())
                    {
                        using (var readStream = new StreamReader(receiveStream))
                        {
                            var content = readStream.ReadToEnd();

                            if (content != string.Empty)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private void EnsureTargetDevice(string dev)
        { 
            //TODO: change it to enum
            if (dev != "device" && dev != "emulator")
            { 
                //maybe enumarated device?
                try
                {
                    int.Parse(dev);
                }
                catch
                {
                    throw new Exception(string.Format("Unknown device {0}", dev));
                }
            }
        }

        private void EnsureOptions(ConfigOption[] optionsToEnsure)
        {
            if (this.args.Contains("/help", StringComparer.OrdinalIgnoreCase) == true)
            {
                throw new Exception(this.help);
            }

            foreach (ConfigOption option in optionsToEnsure)
            {
                string check = string.Format("/{0}", option.ToString());

                if (this.args.Contains(check, StringComparer.OrdinalIgnoreCase) == false)
                {
                    throw new Exception(string.Format("Option {0} not specified, check /help for more information.", check));
                }
            }
        }

        private void ParseArgs()
        {
            EnsureOptions(requiredOptions);

            ConfigOption ret = ConfigOption.Nothing;

            foreach (string arg in this.args)
            {
                if (ret == ConfigOption.Nothing)
                {
                    ret = ParseOption(arg);
                }
                else
                {
                    switch (ret)
                    {
                        //TODO: change it to hashtable
                        case ConfigOption.Config:
                            this.ConfigPath = arg;
                            break;
                        case ConfigOption.IseTool:
                            this.ISEToolPath = arg;
                            break;
                        case ConfigOption.XapFile:
                            this.XapFilePath = arg;
                            break;
                        case ConfigOption.TargetDevice:
                            EnsureTargetDevice(arg);
                            this.TargetDevice = arg;
                            break;
                        case ConfigOption.SetEnv:
                            this.EnvVarName = arg;
                            break;
                    }
                    ret = ConfigOption.Nothing;
                }
            }

            if (ret != ConfigOption.Nothing)
            {
                throw new Exception(string.Format("No value for option {0}", ret.ToString()));
            }
        }

        private ConfigOption ParseOption(string opt)
        { 
            //Remove slash

            if (opt[0] != '/')
            {
                throw new Exception(string.Format("Wrong option format, should be /<option-name> for {0}.", opt));
            }

            opt = opt.Substring(1);

            if (opt.Length == 0)
            {
                throw new Exception(string.Format("Empty option passed '/'", opt));
            }

            try
            {
                ConfigOption ret = (ConfigOption)Enum.Parse(typeof(ConfigOption), opt, true);
                return ret;
            }
            catch
            {
                throw new Exception(string.Format("Cannot parse option: {0}", opt));
            }
        }

        private bool IsIt(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }
    }
}
