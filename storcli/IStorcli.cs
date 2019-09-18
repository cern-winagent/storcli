using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using plugin;
using storcli.Storcli;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using storcli.Exceptions;

namespace storcli
{
    [PluginAttribute(PluginName = "Storcli")]
    public class IStorcli : IInputPlugin
    {
        private Server Server;
        private Settings.Plugin settings;

        public string Execute(JObject set)
        {
            settings = set.ToObject<Settings.Plugin>();

            if (!File.Exists(settings.StorcliFilePath))
            {
                throw new WrongPathException(String.Format("Could not find Storcli: {0}", settings.StorcliFilePath));
            }
            try
            {
                Server = new Server();
                var version = getStorcliVersion();
                var showAll = getServerInfo();
                var callShow = getControllersInfo();
                var eallShow = getEnclosureDeviceInfo();
                var sallShowAll = getPhysicalDriveInfo();
                var rawData = new { showAll, callShow, eallShow, sallShowAll };
                var serverModel = new Models.Server()
                {
                    Name = Server.HostName,
                    OperatingSystem = Server.OperatingSystem,
                    Controllers = Server.Controllers.Select(c => new Models.Controller()
                    {
                        ControllerNumber = c.Id,
                        EnclosureDevices = c.EnclosureDevices.Select(ed => new Models.EnclosureDevice()
                        {
                            Eid = ed.EID,
                            NrPDs = ed.PDs,
                            NrSlots = ed.Slots,
                            Port = ed.Port,
                            PhysicalDrives = Server.PhysicalDrives.Where(pd => pd.ControllerId == c.Id && pd.EnclosureId == ed.EID).Select(pd => new Models.PhysicalDrive()
                            {
                                AssignmentType = pd.State,
                                MediaErrorCount = pd.MediaErrorCount,
                                PredictiveFailureCount = pd.PredictiveFailureCount,
                                ProductId = pd.Model,
                                SerialNumber = pd.SerialNumber,
                                Size = pd.Size,
                                Slot = pd.Slot,
                                VendorId = pd.ProducerId,
                                VirtualDriveDigitalGroup = pd.DigitalGroup == "-" ? null : (int?)int.Parse(pd.DigitalGroup)
                            }).ToList(),
                        }).ToList(),
                        Model = c.Name,
                        SerialNumber = c.SerialNumber,
                        Type = c.Type,
                        VirtualDrives = c.VirtualDrives.Select(vd => new Models.VirtualDrive()
                        {
                            DigitalGroup = vd.DigitalGroup,
                            Size = vd.Size,
                            State = vd.State,
                            Type = vd.Type,
                            VDNumber = vd.Id
                        }).ToList()
                    }).ToList(),
                    CurrentDateTime = DateTime.Now.ToUniversalTime(),
                    // TODO: Get this from an option
                    TimeIntervalInSeconds = 9999999,
                    UnprocessedOutput = JsonConvert.SerializeObject(rawData),
                    StorcliVersion = version
                };

                return JsonConvert.SerializeObject(serverModel);
            }
            catch (Exception)
            {
                throw new StorcliNotSupportedException("It looks like this machine does not support Storcli");
            }
        }

        String getStorcliVersion()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = settings.StorcliFilePath,
                    Arguments = "version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                string pattern = "Ver ([0-9.]+)";
                Match match = Regex.Match(line, pattern);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            return "";
        }
        String getServerInfo()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = settings.StorcliFilePath,
                    Arguments = "show all",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            String output = "";

            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                output += line;
                if (line.Contains("Number of Controllers"))
                {
                    Server.NrControllers = int.Parse(line.Split(new string[] { "= " }, StringSplitOptions.None)[1]);
                }
                else if (line.Contains("Host Name"))
                {
                    Server.HostName = line.Split(new string[] { "= " }, StringSplitOptions.None)[1];
                }
                else if (line.Contains("Operating System"))
                {
                    Server.OperatingSystem = line.Split(new string[] { "= " }, StringSplitOptions.None)[1];
                }
            }
            return output;
        }

        String getControllersInfo()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = settings.StorcliFilePath,
                    Arguments = "\\call show",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            String output = "";

            proc.Start();
            List<Controller> controllers = new List<Controller>();
            Controller currentController = null;
            int sizePosition = 0; int sizeLength = 0;
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                output += line;
                if (line.Contains("Controller ="))
                {
                    if (currentController != null)
                    {
                        controllers.Add(currentController);
                    }
                    currentController = new Controller(int.Parse(line.Split(new string[] { "= " }, StringSplitOptions.None)[1]));
                }
                else if (line.Contains("Product Name"))
                {
                    currentController.Name = line.Split(new string[] { "= " }, StringSplitOptions.None)[1];
                    if (currentController.Name[currentController.Name.Length - 1] == 'i')
                    {
                        currentController.Type = "internal";
                    }
                    else
                    {
                        currentController.Type = "external";
                    }
                }
                else if (line.Contains("Serial Number"))
                {
                    currentController.SerialNumber = line.Split(new string[] { "= " }, StringSplitOptions.None)[1];
                }
                else if (line.Contains("Drive Groups "))
                {
                    currentController.NrDriveGroups = int.Parse(line.Split(new string[] { "= " }, StringSplitOptions.None)[1]);
                }
                else if (line.Contains("Virtual Drives "))
                {
                    currentController.NrVirtualDrives = int.Parse(line.Split(new string[] { "= " }, StringSplitOptions.None)[1]);
                }
                else if (line.Contains("Physical Drives "))
                {
                    currentController.NrPhysicalDrives = int.Parse(line.Split(new string[] { "= " }, StringSplitOptions.None)[1]);
                }
                else if (line.Contains("DG/VD"))
                {
                    var VDInfo = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < VDInfo.Length; i++)
                    {
                        string pattern = @"(\s+)Size";
                        Match match = Regex.Match(line, pattern);
                        if (match.Success)
                        {
                            sizePosition = match.Index;
                            sizeLength = match.Length;
                        }
                        //if (VDInfo[i]=="Size")
                        //{
                        //    sizePosition = i;
                        //}
                    }
                }
                if (currentController != null && currentController.NrVirtualDrives != 0 && line.Contains("RAID"))
                {
                    var VDInfo = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    var IDDG = VDInfo[0].Split('/');
                    var size = line.Substring(sizePosition, sizeLength).Trim();
                    currentController.VirtualDrives.Add(new VirtualDrive(int.Parse(IDDG[1]), int.Parse(IDDG[0]), VDInfo[1], size, VDInfo[2]));
                }
            }
            controllers.Add(currentController);
            Server.Controllers = controllers;
            return output;
        }

        String getEnclosureDeviceInfo()
        {
            var resultOutput = new List<String>();
            foreach (var controller in Server.Controllers)
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = settings.StorcliFilePath,
                        Arguments = $"\\c{controller.Id} \\eall show",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                var output = "";
                proc.Start();
                int cnt = 0;
                int eidPos = 0;
                int slotsPos = 0;
                int PDPos = 0;
                int portPos = 0;
                int portStartPos = 0;
                int portLength = 0;
                while (!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput.ReadLine();
                    output += line;
                    if (line.Contains("EID") && cnt == 0)
                    {
                        cnt++;

                        var info = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < info.Length; i++)
                        {
                            if (info[i] == "EID") { eidPos = i; }
                            else if (info[i] == "Slots") { slotsPos = i; }
                            else if (info[i] == "PD") { PDPos = i; }
                            else if (info[i].Contains("Port")) { portPos = i; }
                        }
                        string pattern = @"Port#(\s+)";
                        Match result = Regex.Match(line, pattern);
                        if (result.Success)
                        {
                            portStartPos = result.Index;
                            portLength = result.Length;
                        }

                    }
                    else if (cnt > 0 && line.Contains("-------------------"))
                    {
                        cnt++;
                    }
                    else if (cnt == 2)
                    {
                        var info = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                        int nrPD = int.Parse(info[PDPos]);
                        if (nrPD > 0)
                        {
                            if (controller.Type == "internal")
                            {
                                controller.EnclosureDevices.Add(new EnclosureDevice(int.Parse(info[eidPos]), int.Parse(info[slotsPos]), nrPD, "Internal"));
                            }
                            else
                            {
                                controller.EnclosureDevices.Add(new EnclosureDevice(int.Parse(info[eidPos]), int.Parse(info[slotsPos]), nrPD, line.Substring(portStartPos, portLength).Trim()));
                            }
                        }
                    }

                }
                resultOutput.Add(output);
            }
            return JsonConvert.SerializeObject(resultOutput);

        }

        String getPhysicalDriveInfo()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = settings.StorcliFilePath,
                    Arguments = $"\\call \\eall \\sall show all",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            String output = "";

            proc.Start();
            PhysicalDrive currentDrive = null;
            List<PhysicalDrive> drives = new List<PhysicalDrive>();

            string sn_pattern = @"SN\s=\s+([0-9A-Z]+)\s*$";

            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                output += line;
                var sn_match = Regex.Match(line, sn_pattern);
                if (line.Contains("Drive /c") && line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length == 3)
                {
                    if (currentDrive != null)
                    {
                        drives.Add(currentDrive);
                    }
                    var driveInfo = (line.Split(' ')[1]).Split('/');

                    int ctrl = int.Parse(driveInfo[1].Remove(0, 1));
                    int enclosure = int.Parse(driveInfo[2].Remove(0, 1));
                    int slot = int.Parse(driveInfo[3].Remove(0, 1));
                    currentDrive = new PhysicalDrive(slot, enclosure, ctrl);
                }
                if (currentDrive != null)
                {
                    if (line.Contains($"{currentDrive.EnclosureId}:{currentDrive.Slot}"))
                    {
                        var driveInfo = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                        //string pattern = @"(\s)Size";
                        //Match match = Regex.Match(line, pattern);
                        //if (match.Success)
                        //{
                        //    currentDrive.Size = line.Substring(match.Index, match.Length).Trim();
                        //}
                        currentDrive.State = driveInfo[2];
                        currentDrive.DigitalGroup = driveInfo[3];
                        currentDrive.Size = driveInfo[4] + " " + driveInfo[5];
                        currentDrive.Model = driveInfo[11];

                    }
                    if (line.Contains("Media Error Count "))
                    {
                        currentDrive.MediaErrorCount = int.Parse(line.Split(new string[] { "= " }, StringSplitOptions.None)[1]);
                    }
                    else if (line.Contains("Predictive Failure Count "))
                    {
                        currentDrive.PredictiveFailureCount = int.Parse(line.Split(new string[] { "= " }, StringSplitOptions.None)[1]);
                    }
                    else if (sn_match.Success)
                    {
                        currentDrive.SerialNumber = sn_match.Groups[1].Value;
                    }
                    else if (line.Contains("Manufacturer Id"))
                    {
                        currentDrive.ProducerId = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[3];
                    }
                }
            }
            drives.Add(currentDrive);
            Server.PhysicalDrives = drives;
            return output;
        }
    }
}
