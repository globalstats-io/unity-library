using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace GlobalstatsIO
{
    [Serializable]
    public class StatisticValues
    {
        public string key = null;
        public string value = "0";
        public string sorting = null;
        public string rank = "0";
        public string value_change = "0";
        public string rank_change = "0";
    }

    [Serializable]
    public class LinkData
    {
        public string url = null;
        public string pin = null;
    }

    [Serializable]
    public class LeaderboardValue
    {
        public string name = null;
        public string user_profile = null;
        public string user_icon = null;
        public string rank = "0";
        public string value = "0";
    }

    [Serializable]
    public class Leaderboard
    {
        public LeaderboardValue[] data;
    }

    public class GlobalstatsIOClient
    {
        [HideInInspector]
        public static string api_id = "";

        [HideInInspector]
        public static string api_secret = "";

        [Serializable]
        private class AccessToken
        {
            public string access_token = null;
            public string token_type = null;
            public string expires_in = null;
            public int created_at = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            public bool IsValid()
            {
                //Check if still valid, allow a 2 minute grace period
                return (created_at + int.Parse(expires_in) - 120) > (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            }
        }

        [Serializable]
        private class StatisticResponse
        {
            public string name = null;
            public string _id = null;

            [SerializeField]
            public List<StatisticValues> values = null;
        }

        private AccessToken api_access_token = null;

        private List<StatisticValues> statistic_values = new List<StatisticValues>();

        [HideInInspector]
        public string statistic_id = "";

        [HideInInspector]
        public string user_name = "";

        [HideInInspector]
        public LinkData link_data = null;

        private IEnumerator GetAccessToken()
        {
            if (api_id == "" || api_secret == "")
            {
                throw new Exception("Credentials missing");
            }

            string url = "https://api.globalstats.io/oauth/access_token";

            WWWForm form = new WWWForm();
            form.AddField("grant_type", "client_credentials");
            form.AddField("scope", "endpoint_client");
            form.AddField("client_id", api_id);
            form.AddField("client_secret", api_secret);

            UnityWebRequest www = UnityWebRequest.Post(url, form);
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            string responseBody = www.downloadHandler.text;

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogWarning("Error retrieving access token: " + www.error);
                Debug.Log("GlobalstatsIO API Response: " + responseBody);
                yield break;
            }
            else
            {
                api_access_token = JsonUtility.FromJson<AccessToken>(responseBody);
            }
        }

        public IEnumerator Share(string id = "", string name = "", Dictionary<string, string> values = null, Action<bool> callback = null)
        {
            bool update = false;

            if (api_access_token == null || !api_access_token.IsValid())
            {
                yield return GetAccessToken();
            }

            // If no id is supplied but we have one stored, reuse it.
            if (id == "" && statistic_id != "")
                id = statistic_id;

            string url = "https://api.globalstats.io/v1/statistics";
            if (id != "")
            {
                url = "https://api.globalstats.io/v1/statistics/" + id;
                update = true;
            }
            else
            {
                if (name == "")
                    name = "anonymous";
            }

            string json_payload;

            if (update == false)
            {
                json_payload = "{\"name\":\"" + name + "\", \"values\":{";
            }
            else
            {
                json_payload = "{\"values\":{";
            }

            bool semicolon = false;
            foreach (KeyValuePair<string, string> value in values)
            {
                if (semicolon)
                    json_payload += ",";
                json_payload += "\"" + value.Key + "\":\"" + value.Value + "\"";
                semicolon = true;
            }
            json_payload += "}}";

            byte[] pData = Encoding.UTF8.GetBytes(json_payload);
            UnityWebRequest www;

            if (update == false)
            {
                www = new UnityWebRequest(url, "POST")
                {
                    uploadHandler = new UploadHandlerRaw(pData)
                };
            }
            else
                www = UnityWebRequest.Put(url, pData);

            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Bearer " + api_access_token.access_token);
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            string responseBody = www.downloadHandler.text;

            StatisticResponse statistic = null;

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogWarning("Error submitting statistic: " + www.error);
                Debug.Log("GlobalstatsIO API Response: " + responseBody);
                callback(false);
            }
            else
            {
                statistic = JsonUtility.FromJson<StatisticResponse>(responseBody);
            }

            // ID is available only on create, not on update, so do not overwrite it
            if (statistic._id != null && statistic._id != "")
                statistic_id = statistic._id;

            user_name = statistic.name;

            //Store the returned data statically
            foreach (StatisticValues value in statistic.values)
            {
                bool updated_existing = false;
                for (int i = 0; i < statistic_values.Count; i++)
                {
                    if (statistic_values[i].key == value.key)
                    {
                        statistic_values[i] = value;
                        updated_existing = true;
                        break;
                    }
                }
                if (!updated_existing)
                {
                    statistic_values.Add(value);
                }
            }

            callback(true);
        }

        public StatisticValues GetStatistic(string key)
        {
            for (int i = 0; i < statistic_values.Count; i++)
            {
                if (statistic_values[i].key == key)
                {
                    return statistic_values[i];
                }
            }
            return null;
        }

        public IEnumerator LinkStatistic(string id = "", Action<bool> callback = null)
        {
            if (api_access_token == null || !api_access_token.IsValid())
            {
                yield return GetAccessToken();
            }

            // If no id is supplied but we have one stored, reuse it.
            if (id == "" && statistic_id != "")
                id = statistic_id;

            string url = "https://api.globalstats.io/v1/statisticlinks/" + id + "/request";

            string json_payload = "{}";
            byte[] pData = Encoding.UTF8.GetBytes(json_payload);

            UnityWebRequest www = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(pData),
                downloadHandler = new DownloadHandlerBuffer()
            };

            www.SetRequestHeader("Authorization", "Bearer " + api_access_token.access_token);
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            string responseBody = www.downloadHandler.text;

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogWarning("Error linking statistic: " + www.error);
                Debug.Log("GlobalstatsIO API Response: " + responseBody);
                callback(false);
            }

            link_data = JsonUtility.FromJson<LinkData>(responseBody);

            callback(true);
        }

        public IEnumerator GetLeaderboard(string gtd, int numberOfPlayers, Action<Leaderboard> callback)
        {
            numberOfPlayers = Mathf.Clamp(numberOfPlayers, 0, 100); // make sure numberOfPlayers is between 0 and 100

            if (api_access_token == null || !api_access_token.IsValid())
            {
                yield return GetAccessToken();
            }

            string url = "https://api.globalstats.io/v1/gtdleaderboard/" + gtd;

            string json_payload = "{\"limit\":" + numberOfPlayers + "\n}";
            byte[] pData = Encoding.UTF8.GetBytes(json_payload);

            UnityWebRequest www = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(pData),
                downloadHandler = new DownloadHandlerBuffer()
            };

            www.SetRequestHeader("Authorization", "Bearer " + api_access_token.access_token);
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            string responseBody = www.downloadHandler.text;

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogWarning("Error getting leaderboard: " + www.error);
                Debug.Log("GlobalstatsIO API Response: " + responseBody);
                callback(null);
            }

            Leaderboard leaderboard = JsonUtility.FromJson<Leaderboard>(responseBody);
            callback(leaderboard);
        }
    }
}