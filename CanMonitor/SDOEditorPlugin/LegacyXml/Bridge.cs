/*
    This file is part of libEDSsharp.

    libEDSsharp is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    libEDSsharp is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with libEDSsharp.  If not, see <http://www.gnu.org/licenses/>.

    Copyright(c) 2016 - 2019 Robin Cornelius <robin.cornelius@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using libEDSsharp;
using CanMonitorLegacyXml;

namespace SDOEditorPlugin.LegacyXml
{
    internal class Bridge
    {
        public EDSsharp convert(Device dev)
        {
            EDSsharp eds = new EDSsharp();

            foreach (CanMonitorLegacyXml.CANopenObject coo in dev.CANopenObjectList.CANopenObject)
            {
                ODentry entry = new ODentry();
                entry.Index = Convert.ToUInt16(coo.Index, 16);
                entry.parameter_name = coo.Name;

                if (coo.AccessType != null)
                {
                    string at = coo.AccessType;

                    Regex reg = new Regex(@"^cons$");
                    at = reg.Replace(at, "const");

                    entry.accesstype = (EDSsharp.AccessType)Enum.Parse(typeof(EDSsharp.AccessType), at);
                }

                if (coo.DataType != null)
                {
                    byte datatype = Convert.ToByte(coo.DataType, 16);
                    entry.datatype = (DataType)datatype;
                }
                else
                {
                    //CANopenNode Project XML did not correctly set DataTypes for record sets

                    if (entry.Index == 0x1018)
                        entry.datatype = DataType.IDENTITY;

                    if (entry.Index >= 0x1200 && entry.Index < 0x1400) //check me is this the correct range??
                        entry.datatype = DataType.SDO_PARAMETER;


                    if (entry.Index >= 0x1400 && entry.Index < 0x1600) //check me is this the correct range??
                        entry.datatype = DataType.PDO_COMMUNICATION_PARAMETER;


                    if (entry.Index >= 0x1600 && entry.Index < 0x1800) //check me is this the correct range??
                        entry.datatype = DataType.PDO_MAPPING;


                    if (entry.Index >= 0x1800 && entry.Index < 0x1a00) //check me is this the correct range??
                        entry.datatype = DataType.PDO_COMMUNICATION_PARAMETER;


                    if (entry.Index >= 0x1a00 && entry.Index < 0x1c00) //check me is this the correct range??
                        entry.datatype = DataType.PDO_MAPPING;


                }

                //Fix for old CanOpen XML;
                if (coo.ObjectType == "REC")
                {
                    coo.ObjectType = "RECORD";
                }

                ObjectType oType;
                if(Enum.TryParse(coo.ObjectType, out oType))
                {
                    //TDO Parsing failed, we should alert the user
                }
                entry.objecttype = oType;

                entry.defaultvalue = coo.DefaultValue;
                entry.HighLimit = coo.HighValue;
                entry.LowLimit = coo.LowValue;
                //entry.nosubindexes = Convert.ToInt16(coo.SubNumber);

                if (coo.PDOmapping != null)
                    entry.PDOtype = (PDOMappingType)Enum.Parse(typeof(PDOMappingType), coo.PDOmapping);
                else
                    entry.PDOtype = PDOMappingType.no;

                entry.prop.CO_flagsPDO = coo.TPDOdetectCOS == "true";
                entry.prop.CO_disabled = coo.Disabled == "true";

                if (coo.Description != null)
                    entry.Description = coo.Description.Text; //FIXME URL/LANG

                if (coo.Label != null)
                    entry.Label = coo.Label.Text; //FIXME LANG

                if (coo.MemoryType != null)
                {
                    entry.prop.CO_storageGroup = coo.MemoryType;
                    eds.CO_storageGroups.Add(coo.MemoryType);
                }

                eds.ods.Add(entry.Index, entry);

                if (entry.Index == 0x1000 || entry.Index == 0x1001 || entry.Index == 0x1018)
                {
                    eds.md.objectlist.Add(eds.md.objectlist.Count + 1, entry.Index);
                }
                else
                if (entry.Index >= 0x2000 && entry.Index < 0x6000)
                {
                    eds.mo.objectlist.Add(eds.mo.objectlist.Count + 1, entry.Index);
                }
                else
                {
                    eds.oo.objectlist.Add(eds.oo.objectlist.Count + 1, entry.Index);
                }


                foreach (CanMonitorLegacyXml.CANopenSubObject coosub in coo.CANopenSubObject)
                {

                    ODentry subentry = new ODentry();

                    subentry.parameter_name = coosub.Name;
                    subentry.Index = entry.Index;

                    if (coosub.AccessType != null)
                        subentry.accesstype = (EDSsharp.AccessType)Enum.Parse(typeof(EDSsharp.AccessType), coosub.AccessType);

                    if (coosub.DataType != null)
                    {
                        byte datatype = Convert.ToByte(coosub.DataType, 16);
                        subentry.datatype = (DataType)datatype;
                    }

                    if (coosub.Description != null)
                        subentry.Description = coosub.Description.Text; //FIXME URL/LANG

                    subentry.defaultvalue = coosub.DefaultValue;
                    subentry.HighLimit = coosub.HighValue;
                    subentry.LowLimit = coosub.LowValue;

                    byte subindex = Convert.ToByte(coosub.SubIndex, 16);

                    if (coosub.PDOmapping != null)
                        subentry.PDOtype = (PDOMappingType)Enum.Parse(typeof(PDOMappingType), coosub.PDOmapping);

                    if (entry.objecttype == ObjectType.ARRAY)
                    {
                        subentry.PDOtype = entry.PDOtype;
                    }

                    subentry.prop.CO_storageGroup = entry.prop.CO_storageGroup;
                    subentry.parent = entry;

                    subentry.objecttype = ObjectType.VAR;

                    if (coosub.TPDOdetectCOS != null)
                    {
                        subentry.prop.CO_flagsPDO = coosub.TPDOdetectCOS == "true";
                    }
                    else
                    {
                        if (coo.TPDOdetectCOS != null)
                            subentry.prop.CO_flagsPDO = coo.TPDOdetectCOS == "true";
                    }


                    entry.subobjects.Add(subindex, subentry);

                }
            }

            eds.du.Dummy0001 = dev.Other.DummyUsage.Dummy[0].Entry == "Dummy0001=1";
            eds.du.Dummy0002 = dev.Other.DummyUsage.Dummy[1].Entry == "Dummy0002=1";
            eds.du.Dummy0003 = dev.Other.DummyUsage.Dummy[2].Entry == "Dummy0003=1";
            eds.du.Dummy0004 = dev.Other.DummyUsage.Dummy[3].Entry == "Dummy0004=1";
            eds.du.Dummy0005 = dev.Other.DummyUsage.Dummy[4].Entry == "Dummy0005=1";
            eds.du.Dummy0006 = dev.Other.DummyUsage.Dummy[5].Entry == "Dummy0006=1";
            eds.du.Dummy0007 = dev.Other.DummyUsage.Dummy[6].Entry == "Dummy0007=1";

            foreach (CanMonitorLegacyXml.SupportedBaudRate baud in dev.Other.Baudrate.SupportedBaudRate)
            {
                if (baud.Value == "10 Kbps")
                    eds.di.BaudRate_10 = true;
                if (baud.Value == "20 Kbps")
                    eds.di.BaudRate_20 = true;
                if (baud.Value == "50 Kbps")
                    eds.di.BaudRate_50 = true;
                if (baud.Value == "125 Kbps")
                    eds.di.BaudRate_125 = true;
                if (baud.Value == "250 Kbps")
                    eds.di.BaudRate_250 = true;
                if (baud.Value == "500 Kbps")
                    eds.di.BaudRate_500 = true;
                if (baud.Value == "800 Kbps")
                    eds.di.BaudRate_800 = true;
                if (baud.Value == "1000 Kbps")
                    eds.di.BaudRate_1000 = true;

            }

            Dictionary<string, string> keypairs = new Dictionary<string, string>();

            if (dev.Other.Capabilities != null)
            {
                if (dev.Other.Capabilities.CharacteristicsList != null)
                {
                    foreach (CanMonitorLegacyXml.Characteristic c in dev.Other.Capabilities.CharacteristicsList.Characteristic)
                    {
                        try
                        {
                            keypairs.Add(c.CharacteristicName.Label.Text, c.CharacteristicContent.Label.Text);
                        }
                        catch (Exception)
                        {
                            // Warnings.warning_list.Add("Parsing characteristics failed " + e.ToString());
                        }
                    }
                }
            }


            bool boolout;
            byte byteout;

            if (keypairs.ContainsKey("SimpleBootUpSlave") && bool.TryParse(keypairs["SimpleBootUpSlave"], out boolout))
                eds.di.SimpleBootUpSlave = boolout;
            if (keypairs.ContainsKey("SimpleBootUpMaster") && bool.TryParse(keypairs["SimpleBootUpMaster"], out boolout))
                eds.di.SimpleBootUpMaster = boolout;
            if (keypairs.ContainsKey("DynamicChannelsSupported") && bool.TryParse(keypairs["DynamicChannelsSupported"], out boolout))
                eds.di.DynamicChannelsSupported = boolout;
            if (keypairs.ContainsKey("CompactPDO") && byte.TryParse(keypairs["CompactPDO"], out byteout))
                eds.di.CompactPDO = byteout;
            if (keypairs.ContainsKey("GroupMessaging") && bool.TryParse(keypairs["GroupMessaging"], out boolout))
                eds.di.GroupMessaging = boolout;
            if (keypairs.ContainsKey("LSS_Supported") && bool.TryParse(keypairs["LSS_Supported"], out boolout))
                eds.di.LSS_Supported = boolout;
            if (keypairs.ContainsKey("LSS_Master") && bool.TryParse(keypairs["LSS_Master"], out boolout))
                eds.di.LSS_Master = boolout;
            else if (keypairs.ContainsKey("LSS_Type") && keypairs["LSS_Type"] != null && keypairs["LSS_Type"].ToString() == "Client")
                eds.di.LSS_Master = true;
            if (keypairs.ContainsKey("Granularity") && byte.TryParse(keypairs["Granularity"], out byteout))
                eds.di.Granularity = byteout;

            eds.di.ProductName = dev.Other.DeviceIdentity.ProductName;
            eds.di.ProductNumber = dev.Other.DeviceIdentity.ProductNumber;

            if (dev.Other.DeviceIdentity.ProductText != null && dev.Other.DeviceIdentity.ProductText.Description != null & dev.Other.DeviceIdentity.ProductText.Description.Text != null)
                eds.fi.Description = dev.Other.DeviceIdentity.ProductText.Description.Text;

            eds.di.VendorName = dev.Other.DeviceIdentity.VendorName;
            eds.di.VendorNumber = dev.Other.DeviceIdentity.VendorNumber;

            if (dev.Other.DeviceIdentity.ConcreteNoideId != null)
            {
                eds.dc.NodeID = Convert.ToByte(dev.Other.DeviceIdentity.ConcreteNoideId);
            }
            else
            {
                eds.dc.NodeID = 0;
            }

            string dtcombined;

            eds.fi.FileName = dev.Other.File.FileName;
            eds.edsfilename = dev.Other.File.EdsFile;
            //fixme dcffilename

            dtcombined = string.Format("{0} {1}", dev.Other.File.FileCreationTime, dev.Other.File.FileCreationDate);
            try
            {
                eds.fi.CreationDateTime = DateTime.ParseExact(dtcombined, "h:mmtt MM-dd-yyyy", CultureInfo.InvariantCulture);
                eds.fi.CreationDate = eds.fi.CreationDateTime.ToString("MM-dd-yyyy");
                eds.fi.CreationTime = eds.fi.CreationDateTime.ToString("h:mmtt");

            }
            catch (Exception) { }

            eds.fi.CreatedBy = dev.Other.File.FileCreator;
            eds.fi.exportFolder = dev.Other.File.ExportFolder;

            dtcombined = string.Format("{0} {1}", dev.Other.File.FileModificationTime, dev.Other.File.FileModificationDate);
            try
            {
                eds.fi.ModificationDateTime = DateTime.ParseExact(dtcombined, "h:mmtt MM-dd-yyyy", CultureInfo.InvariantCulture);
                eds.fi.ModificationDate = eds.fi.ModificationDateTime.ToString("MM-dd-yyyy");
                eds.fi.ModificationTime = eds.fi.ModificationDateTime.ToString("h:mmtt");
            }
            catch (Exception) { }




            eds.fi.ModifiedBy = dev.Other.File.FileModifedBy;


            dev.Other.Capabilities = dev.Other.Capabilities;

            eds.fi.FileVersion = dev.Other.File.FileVersion;

            eds.fi.FileRevision = dev.Other.File.FileRevision;

            eds.fi.EDSVersion = "4.0";

            //FIX me any other appropriate defaults for eds here??

            eds.UpdatePDOcount();

            return eds;
        }



    }

}
