using KTT.Comm.Data;
using KTT.Config;
using KTT.Logging;
using System;
using System.Collections.Generic;
using zkemkeeper;
using System.Linq;

namespace KTT.Comm
{
    public enum EStatus
    {
        Administrators = 1,
        Users = 2,
        FingerpringTemplate = 3,
        Password = 4,
        AdminTransactLog = 5,
        GenLogs = 6
    }

    class AttComm
    {
        //Create Standalone SDK class dynamicly.
        private CZKEMClass _axCZKEM1;
        private bool _bIsConnected;//the boolean value identifies whether the device is connected
        //In fact,when you are using the tcp/ip communication,this parameter will be ignored,that is any integer will all right.Here we use 1.
        private int _iMachineNumber;//the serial number of the device.After connecting the device ,this value will be changed.

        public AttComm()
        {
            _bIsConnected = false;
        }

        //If your device supports the TCP/IP communications, you can refer to this.
        //when you are using the tcp/ip communication,you can distinguish different devices by their IP address.
        public void Connect()
        {
            int idwErrorCode = 0;
            _axCZKEM1 = new CZKEMClass();
            _bIsConnected = _axCZKEM1.Connect_Net(ApplicationSettings.ATT_IP, ApplicationSettings.ATT_Port);
            if (_bIsConnected == true)
            {
                _iMachineNumber = 1;
                if (!_axCZKEM1.RegEvent(_iMachineNumber, 65535))//Here you can register the realtime events that you want to be triggered(the parameters 65535 means registering all)
                {
                    _axCZKEM1.GetLastError(ref idwErrorCode);
                    Disconnect();
                    throw new Exception("Unable to register to evetns, ErrorCode=" + idwErrorCode.ToString());
                }
            }
            else
            {
                _axCZKEM1.GetLastError(ref idwErrorCode);
                throw new Exception("Unable to connect the device, ErrorCode=" + idwErrorCode.ToString());

            }
        }

        public void Disconnect()
        {
            _axCZKEM1.Disconnect();
            _bIsConnected = false;
        }

        //Download the attendance records from the device(For both Black&White and TFT screen devices).
        internal IList<TimeReportEntry> ReadTimeReports(DateTime from)
        {
            if (_bIsConnected == false)
            {
                throw new Exception("Please connect the att device first");
            }

            if (!_axCZKEM1.EnableDevice(_iMachineNumber, false))//disable the device
            {
                throw new Exception("Unable to disable device");
            }
            IList<TimeReportEntry> trrList;
            try
            {
                trrList = ReadTimeReportsInternal(from);
                Logger.LoggerInstance.log.Debug($"Retrived {trrList.Count} records from Date {from}");
            }
            catch(Exception ex)
            {
                throw;
            }
            finally
            {
                if (!_axCZKEM1.EnableDevice(_iMachineNumber, true))//enable the device
                {
                    throw new Exception("Unable to enable device");
                }
            }

            if (trrList.Count == 0)
                throw new Exception("No records to sync found in att");

            return trrList;
        }

        private IList<TimeReportEntry> ReadTimeReportsInternal(DateTime from)
        {

            List<TimeReportEntry> res = new List<TimeReportEntry>();
            
            if (!_axCZKEM1.ReadGeneralLogData(_iMachineNumber))
            {
                int errCode = 0;
                _axCZKEM1.GetLastError(ref errCode);

                if (errCode != 0)
                    throw new Exception("Reading data from terminal failed, ErrorCode: " + errCode.ToString());
                return new List<TimeReportEntry>();
            }
            TimeReportEntry? report;
            while ((report = NextTimeReport(from)) != null && report.HasValue)//get records from the memory
                res.Add(report.Value);
            return res;
        }

        private TimeReportEntry? NextTimeReport(DateTime from)
        {
            int temp1 = 0, temp2 = 0;
            int enrollNumber = 0;
            int verifyMode = 0;
            int inOutMode = 0;
            int year = 0;
            int month = 0;
            int day = 0;
            int hour = 0;
            int minute = 0;
            int second = 0;
            int workcode = 0;

            while (_axCZKEM1.GetGeneralLogData(_iMachineNumber, ref temp1, ref enrollNumber, ref temp2, ref verifyMode, ref inOutMode, ref year, ref month, ref day, ref hour, ref minute))//get records from the memory
            {
                DateTime timeReport = new DateTime(year, month, day, hour, minute, second);

                if (DateTime.Compare(timeReport, from) > 0)
                {
                    return new TimeReportEntry()
                    {
                        EnrollNumber = enrollNumber,
                        VerifyMode = verifyMode,
                        InOutMode = inOutMode == 1,
                        TimeReport = timeReport,
                        WorkCode = workcode

                    };
                }
            }
            return null;
        }

        private Dictionary<int, string> GetUsersFromAtt()
        {
            string name = "", password = "";
            int enrollno = 0, privilage = 0;
            bool enabled = false;

            Dictionary<int, string> res = new Dictionary<int, string>();
            if (_axCZKEM1.ReadAllUserID(_iMachineNumber))
            {
                while (_axCZKEM1.GetAllUserInfo(_iMachineNumber, ref enrollno, ref name, ref password, ref privilage, ref enabled))
                {
                    if (!res.ContainsKey(enrollno))
                    {
                        res[enrollno] = name;
                    }
                }
            }

            return res;
        }

        public int GetDeviceStatus(EStatus statusRequest)
        {
            if (_bIsConnected == false)
            {
                throw new Exception("Please connect the att device first");
            }

            if (!_axCZKEM1.EnableDevice(_iMachineNumber, false))//disable the device
            {
                throw new Exception("Unable to disable device");
            }

            int status = 0;
            if (!_axCZKEM1.GetDeviceStatus(_iMachineNumber, (int)statusRequest, ref status))
            {
                int errCode = 0;
                _axCZKEM1.GetLastError(ref errCode);

                if (errCode != 0)
                    throw new Exception("Reading data from terminal failed, ErrorCode: " + errCode.ToString());
            }

            if (!_axCZKEM1.EnableDevice(_iMachineNumber, true))//enable the device
            {
                throw new Exception("Unable to enable device");
            }

            return status;
        }
    }
}
