using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using FacekBookFriends;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace FaceBookFriends
{
    public partial class _Default : System.Web.UI.Page
    {
        string facebook_urlAuthorize_base = "https://graph.facebook.com/oauth/authorize";
        string facebook_urlGetToken_base = "https://graph.facebook.com/oauth/access_token";
        string facebook_AppID = ConfigurationManager.AppSettings["facebook:AppID"];
        string facebook_AppSecret = ConfigurationManager.AppSettings["facebook:AppSecret"];

        MySQL mySQL;
        public static Friend owner;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request["code"] != null)
                {
                    mySQL = new MySQL();
                    
                    string authorizationCode = Request["code"];
                    string access_token = Facebook_GetAccessToken(authorizationCode);
                    if (access_token == "")
                    {
                        Response.Write("Could not get access_token");
                        return;
                    }


                    Facebook_GetMyself(access_token);

                    Facebook_ListFriends(access_token);
                }
            }
        }

         private void Facebook_AuthorizeApplication()
        {
            //In this function we ask the user to authorize our Facebook application using an authorization request url

            //the authorization request url need to be appended
            //1) Our AppID
            //2) The permission scope (here publish_stream)
            //3) A Url that Facebook can redirect the users browser to then Facebook is finish asking the user whether permission can be granted

            string scope = "publish_stream"; //see: http://developers.facebook.com/docs/authentication/permissions/ for extended permissions
            string urlAuthorize = facebook_urlAuthorize_base;
            urlAuthorize += "?client_id=" + facebook_AppID;
            urlAuthorize += "&redirect_uri=" + Facebook_GetRedirectUri();
            urlAuthorize += "&scope=" + scope;

            //redirect the users browser to Facebook to ask the user to authorize our Facebook application
            Response.Redirect(urlAuthorize, true); //this cannot be done using WebRequest since facebook may need to show dialogs in the users browser
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            
            Facebook_AuthorizeApplication();            
        }



        private string Facebook_GetAccessToken(string pAuthorizationCode)
        {
            //In this function we use the authorization code from before to obtain an access_token.
            //The access_token can be used to request resources from a specific user within the authorized permission scope

            //The access_token request url need to be appended
            //1) Our AppID
            //2) Our AppSecret
            //3) The exact same redirect_uri that was used before then authorized
            //4) The authorization code we just got from Facebook

            string urlGetAccessToken = facebook_urlGetToken_base;
            urlGetAccessToken += "?client_id=" + facebook_AppID;
            urlGetAccessToken += "&client_secret=" + facebook_AppSecret;
            urlGetAccessToken += "&redirect_uri=" + Facebook_GetRedirectUri();
            urlGetAccessToken += "&code=" + pAuthorizationCode;

            string responseData = RequestResponse(urlGetAccessToken); //we write RequestResponse a little later
            if (responseData == "")
            {
                return "";
            }
            NameValueCollection qs = HttpUtility.ParseQueryString(responseData);
            string access_token = qs["access_token"] == null ? "" : qs["access_token"];

            return access_token;
            //(The access_token is valid only from within the site domain specified for our Facebook application)
        }

        private string Facebook_GetRedirectUri()
        {
            string urlCurrentPage = Request.Url.AbsoluteUri.IndexOf('?') == -1 ? Request.Url.AbsoluteUri : Request.Url.AbsoluteUri.Substring(0, Request.Url.AbsoluteUri.IndexOf('?'));
            NameValueCollection nvc = new NameValueCollection();
            foreach (string key in Request.QueryString) { if (key != "code") { nvc.Add(key, Request.QueryString[key]); } }
            string qs = "";
            foreach (string key in nvc)
            {
                qs += qs == "" ? "?" : "&";
                qs += key + "=" + nvc[key];
            }
            string redirect_uri = urlCurrentPage + qs; //urlCallback have to be exactly the same each time it is used (that's why the code key is removed)

            return redirect_uri;
        }

        private string HttpPost(string pUrl, string pPostData)
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(pUrl);
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(pPostData);
            Stream requestWriter = webRequest.GetRequestStream(); //GetRequestStream
            requestWriter.Write(bytes, 0, bytes.Length);
            requestWriter.Close();

            Stream responseStream = null;
            StreamReader responseReader = null;
            string responseData = "";
            try
            {
                WebResponse webResponse = webRequest.GetResponse();
                responseStream = webResponse.GetResponseStream();
                responseReader = new StreamReader(responseStream);
                responseData = responseReader.ReadToEnd();
            }
            catch (Exception exc)
            {
                throw new Exception("could not post : " + exc.Message);
            }
            finally
            {
                if (responseStream != null)
                {
                    responseStream.Close();
                    responseReader.Close();
                }
            }

            return responseData;
        }

        private string RequestResponse(string pUrl)
        {
            HttpWebRequest webRequest = System.Net.WebRequest.Create(pUrl) as HttpWebRequest;
            webRequest.Method = "GET";
            webRequest.ServicePoint.Expect100Continue = false;
            webRequest.Timeout = 20000;

            Stream responseStream = null;
            StreamReader responseReader = null;
            string responseData = "";
            try
            {
                WebResponse webResponse = webRequest.GetResponse();
                responseStream = webResponse.GetResponseStream();
                responseReader = new StreamReader(responseStream);
                responseData = responseReader.ReadToEnd();
            }
            catch (Exception exc)
            {
                Response.Write("<br /><br />ERROR : " + exc.Message);
            }
            finally
            {
                if (responseStream != null)
                {
                    responseStream.Close();
                    responseReader.Close();
                }
            }

            return responseData;
        }

        private void Facebook_GetMyself(string pAccessToken)
        {
            string username = "me";
            string urlGetFriends = "https://graph.facebook.com/" + username + "?fields=id,name&access_token=" + pAccessToken;

            string jsonFriends = RequestResponse(urlGetFriends);
            if (jsonFriends == "")
            {
                Response.Write("<br /><br />urlGetFriends have problems");
                return;
            }

            owner = JsonConvert.DeserializeObject<Friend>(jsonFriends);
            if(!mySQL.FriendExists(owner))
                mySQL.InsertFriend(owner);
            
        }

        private void Facebook_ListFriends(string pAccessToken)
        {
            string username = "me";
            string dataType = "friends";
            string urlGetFriends = "https://graph.facebook.com/" + username + "/" + dataType + "?access_token=" + pAccessToken;
            string jsonFriends = RequestResponse(urlGetFriends);
            if (jsonFriends == "")
            {
                Response.Write("<br /><br />urlGetFriends have problems");
                return;
            }
            Friends fbFriendsList = JsonConvert.DeserializeObject<Friends>(jsonFriends); //we write the Friends class later

            foreach (Friend friend in fbFriendsList.data) //we write the Friend class later
            {                
                if (!mySQL.FriendShipExists(friend))
                    mySQL.InsertFriendShip(friend);          
            }

            Friends dbFriendsList = 
                new Friends( mySQL.SelectFromUser(String.Format("select a.id, a.name from facebookuser a join facebookfriend b on a.id = b.friend where b.owner = '{0}'", owner.id)));

            foreach (Friend f in dbFriendsList.GetUniqueFriends(fbFriendsList))
            {
                mySQL.DeleteFriendShip(f);
            }

            Repeater1.DataSource = mySQL.GetHistory();
            Repeater1.DataBind(); 
            
        }

    }
}