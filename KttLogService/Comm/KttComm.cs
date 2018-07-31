using KTT.Config;
using KTT.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using KTT.Comm.Data;
using System.Linq;
using System.Collections.Specialized;

namespace KTT.Comm
{
    class KttComm
    {
        private static Uri NormilizeUri(Uri uri)
        {
            if (!uri.OriginalString.EndsWith("/") && !uri.OriginalString.EndsWith("\\"))
            {
                uri = new Uri(uri.OriginalString + '/');
            }
            return uri;
        }

        public static DateTime LastSynced()
        {
            Uri uri = new Uri(NormilizeUri(ApplicationSettings.KTT_URI), "att_last_sync.php");
            using (WebClient wc = new WebClient())
            {
                byte[] responsebytes = wc.DownloadData(uri);
                string responsebody = Encoding.UTF8.GetString(responsebytes);
                JObject json;
                try
                {
                    json = JObject.Parse(responsebody);
                }
                catch (Exception ex)
                {
                    Logger.LoggerInstance.log.Error("failed to retreive last sync time stamp", ex);
                    throw new InvalidCastException(responsebody.ToString());
                }

                bool success = json["success"].ToObject<bool>();
                if (!success)
                {
                    Exception err = new Exception(json["errors"].ToString());
                    Logger.LoggerInstance.log.Error("failed to retreive last sync time stamp", err);
                    throw err;
                }

                
                if (!json.ContainsKey("lastSync"))
                    return new DateTime(1969, 31, 1, 23, 58, 00);
                JToken token = json["lastSync"];
                if (token == null)
                    return new DateTime(1969, 31, 1, 23, 58, 00);
                return token.Value<DateTime>();
            }
        }

        public static DateTime UpdateReports(DateTime timeStamp, IEnumerable<TimeReportEntry> trrList)
        {
            DateTime from = trrList.Min(e => e.TimeReport);
            DateTime to = trrList.Max(e => e.TimeReport);

            NameValueCollection reqparm = new NameValueCollection();
            reqparm.Add("from", from.ToString(@"yyyy-MM-dd HH:mm:ss"));
            reqparm.Add("to", to.ToString(@"yyyy-MM-dd HH:mm:ss"));

            JArray trrArray = new JArray(
                    trrList.Select(r => new JObject {
                        { "timestamp", timeStamp },
                        { "att_id", r.EnrollNumber},
                        { "date", r.TimeReport.ToString("yyyy-MM-dd") },
                        { "time", r.TimeReport.ToString("HH:mm:ss") },
                        { "in_out", r.InOutMode }
                    })
                );

            reqparm.Add("data", trrArray.ToString());

            Uri uri = new Uri(NormilizeUri(ApplicationSettings.KTT_URI), "att_sync.php");

            using (WebClient wc = new WebClient())
            {   
                byte[] responsebytes = wc.UploadValues(uri, "POST", reqparm);
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

                bool success = json["success"].ToObject<bool>();
                if (!success)
                {
                    Exception err = new Exception(json["errors"].ToString());
                    Logger.LoggerInstance.log.Error("failed to push time reports", err);
                    throw err;
                }

                JToken token = json["lastSync"];
                if (token == null || !token.HasValues)
                {
                    Logger.LoggerInstance.log.Error("server did not return last time sync");
                    throw new Exception("server did not return last time sync");
                }

                return token.Value<DateTime>();
            }
        }
    }
}
