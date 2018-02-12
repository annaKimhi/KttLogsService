using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using zkemkeeper;

namespace KttLogService
{
   internal class AttDataHandler
    {
        //Create Standalone SDK class dynamicly.
        private CZKEMClass _axCZKEM1 = new CZKEMClass();
        private bool _bIsConnected = false;//the boolean value identifies whether the device is connected
        private int _iMachineNumber = 1;//the serial number of the device.After connecting the device ,this value will be changed.
        private Uri _kttUri;
        private string _attServerIp;
        private int _attServerPort;

        public AttDataHandler(string kttUri, string attServerIp, int attServerPort)
        {
            _kttUri = new Uri(kttUri + "/att_sync.php");
            _attServerIp = attServerIp;
            _attServerPort = attServerPort;


        }

        //If your device supports the TCP/IP communications, you can refer to this.
        //when you are using the tcp/ip communication,you can distinguish different devices by their IP address.
        public void ConnectAttMachine()
        {
            int idwErrorCode = 0;
            _bIsConnected = _axCZKEM1.Connect_Net(_attServerIp, _attServerPort);
            if (_bIsConnected == true)
            {
                _iMachineNumber = 1;//In fact,when you are using the tcp/ip communication,this parameter will be ignored,that is any integer will all right.Here we use 1.
                _axCZKEM1.RegEvent(_iMachineNumber, 65535);//Here you can register the realtime events that you want to be triggered(the parameters 65535 means registering all)
            }
            else
            {
                _axCZKEM1.GetLastError(ref idwErrorCode);
                throw new Exception("Unable to connect the device,ErrorCode=" + idwErrorCode.ToString());
            }
        }

        public void DisconnectAttMachine()
        {
            _axCZKEM1.Disconnect();
            _bIsConnected = false;
        }

        //Download the attendance records from the device(For both Black&White and TFT screen devices).
        internal void SyncData(DateTime from, out string lastSync, out int count)
        {
            if (_bIsConnected == false)
            {
                throw new Exception("Please connect the att device first");
            }

            _axCZKEM1.EnableDevice(_iMachineNumber, false);//disable the device
            List<TimeReportRepository> trrList = GetDataFromAtt(from);
            Logger.LoggerInstance.log.Debug($"Retrived {trrList.Count} records from Date {from}");
            int i = 1;
            foreach (TimeReportRepository item in trrList)
            {
                string mode = item.InOutMode ? "Out" : "In";
                Logger.LoggerInstance.log.Info($"Record {i} EnrollNumber {item.EnrollNumber} TimeReport {item.TimeReport}  WorkCode {item.WorkCode} action {mode}");
                i++;
            }
            _axCZKEM1.EnableDevice(_iMachineNumber, true);//enable the device

            if (trrList.Count == 0)
            {
                throw new Exception("No records to sync found in att");
            }
            SendDataToKtt(from, trrList, out lastSync);
            count = trrList.Count;
        }

        private void SendDataToKtt(DateTime from, List<TimeReportRepository> trrList, out string lastSync)
        {
            //date fromat must by 'yyyy-MM-dd'
            //example for a correct format for trrList in json format:
            //[{"key":"data","value":"[{\"timestamp\":\"2017-01-02 01:02:03\",\"att_id\":140, \"date\":\"2017-09-11\", \"time\":\"08:30\", \"in_out\": false},{\"timestamp\":\"2017-01-02 01:02:03\",\"att_id\":141, \"date\":\"2017-09-21\", \"time\":\"12:30\", \"in_out\": true}]","description":""}]
            using (WebClient wc = new WebClient())
            {
                var reqparm = new System.Collections.Specialized.NameValueCollection();
                reqparm.Add("from", from.ToString(@"yyyy-MM-dd HH:mm:ss"));
                reqparm.Add("to", DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss"));

                JArray trrArray = new JArray(
                    trrList.Select(r => new JObject {
                        { "timestamp", from },
                        { "att_id", r.EnrollNumber},
                        { "date", r.TimeReport.ToString("yyyy-MM-dd") },
                        { "time", r.TimeReport.ToString("HH:mm") },
                        { "in_out", r.InOutMode }
                    })
                );


                reqparm.Add("data", trrArray.ToString());
                byte[] responsebytes = wc.UploadValues(_kttUri, "POST", reqparm);
                string responsebody = Encoding.UTF8.GetString(responsebytes);
                JObject json;
                try
                {
                    json = JObject.Parse(responsebody);
                }
                catch (Exception)
                {

                    throw new InvalidCastException(responsebody.ToString());
                }


                lastSync = json["lastSync"].ToString();
                bool success;
                if (!Boolean.TryParse(json["success"].ToString(), out success))
                    throw new InvalidOperationException("Unknown response from KTT server");

                if (!success)
                {
                    throw new InvalidOperationException(json["errors"].ToString());
                }
                Logger.LoggerInstance.log.Info("Successfuly updated");

            }
        }

        private void wc_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            Console.WriteLine(e.Result.ToString());
            throw new NotImplementedException();
        }

        private List<TimeReportRepository> GetDataFromAtt(DateTime from)
        {


            List<TimeReportRepository> res = new List<TimeReportRepository>();
            int errCode = 0;

            if (_axCZKEM1.ReadGeneralLogData(_iMachineNumber))
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

                    if (DateTime.Compare(timeReport, from) >= 0)// && enrollNumber ==140)// DEBUG revital*/
                    {
                        res.Add(new TimeReportRepository()
                        {
                            EnrollNumber = enrollNumber,
                            VerifyMode = verifyMode,
                            InOutMode = inOutMode == 1,
                            TimeReport = timeReport,
                            WorkCode = workcode

                        });
                    }
                }
            }
            else
            {
                _axCZKEM1.GetLastError(ref errCode);

                if (errCode != 0)
                {
                    throw new Exception("Reading data from terminal failed, ErrorCode: " + errCode.ToString());
                }
                else
                {
                    throw new Exception("No data returned from terminal!");
                }
            }
            return res;
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
    }
}
