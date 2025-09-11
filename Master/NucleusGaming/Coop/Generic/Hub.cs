using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;

namespace Nucleus.Gaming.Coop.Generic
{
    public class Hub
    {
        private bool updateAvailable = false;

        private static bool connected;
        public static bool Connected
        {
            get => connected;
            set
            {
                connected = value;
            }
        }

        public bool IsUpdateAvailable(bool fetch)
        {
            if (!connected)
            {
                return false;
            }

            if (fetch)
            {
                updateAvailable = CheckUpdateAvailable();
            }

            return updateAvailable;
        }

        public _Maintainer Maintainer = new _Maintainer();
        public _Handler Handler = new _Handler();

        public class _Maintainer
        {
            public string Name = "";
            public string Id = "";
        }

        public class _Handler
        {
            public int Version = -1;
            public string Id = "";
        }

        public bool CheckUpdateAvailable()
        {
            if (Handler.Version < 0)
            {
                return false;
            }

            string id = Handler.Id;
            int newVersion = -1;

            string resp = Get("http://localhost:3000//api/v1/" + "handler/" + id);

            if (resp == null)
            {
                return false;
            }
            else if (resp == "{}")
            {
                return false;
            }

            JObject jObject = JsonConvert.DeserializeObject(resp) as JObject;

            if (jObject == null)
            {
                return false;
            }

            JArray array = jObject["Handlers"] as JArray;

            if (array == null)
            {
                return false;
            }
            else if (array.Count != 1)
            {
                return false;
            }

            newVersion = int.TryParse(array[0]["currentVersion"].ToString(), out int _v) ? _v : -1;

            return newVersion > Handler.Version;
        }

        public List<string> GetGameGenres(string handlerId)
        {
            List<string> genres = new List<string>();

            if (handlerId == null)
            {
                return genres;
            }

            string resp = Get("http://localhost:3000//api/v1/" + "handler/" + handlerId);

            if (resp == null)
            {
                return genres;
            }
            else if (resp == "{}")
            {
                return genres;
            }

            JObject jObject = null;
            try
            {
                jObject = JsonConvert.DeserializeObject(resp) as JObject;
            }
            catch
            {
                return genres;
            }

            if (jObject == null)
            {
                return genres;
            }

            JArray array = jObject["Handlers"] as JArray;

            if (array == null)
            {
                return genres;
            }
            else if (array.Count != 1)
            {
                return genres;
            }

            if (array[0]["gameGenres"] != null)
            {

                JToken jGenres = array["gameGenres"] as JToken;
                
                for (int s = 0; s < jGenres.Count(); s++)
                {
                    genres.Add(s.ToString());
                }

                return genres;
            }

            return null;
        }

        public string Get(string uri)
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.DefaultConnectionLimit = 9999;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                request.Timeout = 2500;//lower values make the timeout too short for some "big" handlers (GTAIV)             
                request.Method = "Get";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
