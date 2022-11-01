using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FNB_Verint_QM_Service.Models;
using FNB_Verint_QM_Service.AppData;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using static FNB_Verint_QM_Service.Models.HC_Model;
using System.Web;
using System.IO;

namespace FNB_Verint_QM_Service
{
    internal class Program
    {
        
        static void Main(string[] args)
        {
           System.Threading.Tasks.Task.Run(() => FetchChats());
        }

        private static void FetchChats()
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["VerintDB"].ConnectionString);
            string host = String.Empty;
            string username = String.Empty;
            string password = String.Empty;
            string workingdirectory = String.Empty;
            int port = 0;
            //GET PASSWORD
            string get = $"select * from sftp";
            DataTable dt = BLL.GetRequest(get);
            if (dt.Rows.Count > 0)
            {
                string temppw = String.IsNullOrEmpty(dt.Rows[0]["password"].ToString()) ? "" : dt.Rows[0]["password"].ToString();
                password = temppw;// Library.decryptpass(temppw);
                username = String.IsNullOrEmpty(dt.Rows[0]["username"].ToString()) ? "" : dt.Rows[0]["username"].ToString();
                host = String.IsNullOrEmpty(dt.Rows[0]["host"].ToString()) ? "" : dt.Rows[0]["host"].ToString();
                port = Convert.ToInt32(String.IsNullOrEmpty(dt.Rows[0]["port"].ToString()) ? "0" : dt.Rows[0]["port"].ToString());
            }
            string hcurl;
            Verint.verint_interface retturnmeeesgae = new Verint.verint_interface { };
            try
            {
                string currtime = DateTime.Now.ToString("yyyy-MM-dd");
                string yesttime = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                string urlpart = "/conversations/getAllConversations?start_time=" + yesttime + "&end_time=" + currtime + "";
                var deserializer = new RestSharp.Serialization.Json.JsonDeserializer();
                BLL.Logger("Going to get Hybrid Chat URL from DB");
                string query = @"select hc_url from endpoints";// where url_name = 'getconversations'";
                DataTable querydt = BLL.GetRequest(query);
                if (querydt.Rows.Count > 0)
                {
                    hcurl = querydt.Rows[0]["hc_url"].ToString().Trim();
                    string getconvourl = hcurl + urlpart;
                    //GET CONVERSTATIONS
                    BLL.Logger("Fetching AllConversations from Hybrid Chat MongoDB");
                    var gethcconversations = new RestSharp.RestClient(getconvourl);
                    var getconvorequest = new RestSharp.RestRequest(getconvourl, RestSharp.Method.GET);
                    var hcconversationresponse = gethcconversations.Execute(getconvorequest);
                    var statcode = hcconversationresponse;
                    if (statcode.StatusCode == HttpStatusCode.OK)
                    {
                        if (statcode.Content.Length > 5)
                        {
                            string action = "Conversations Fetched successfully";
                            BLL.Logger(action);
                            var deserializer1 = new RestSharp.Serialization.Json.JsonDeserializer();
                            List<HC_Model.GetConversationResponseData> cooc = new List<HC_Model.GetConversationResponseData>();
                            cooc = deserializer1.Deserialize<List<HC_Model.GetConversationResponseData>>(hcconversationresponse);
                            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
                            BLL.Logger("looping through the conversation IDs");
                            int success_count = 0;
                            int fail_count = 0;
                            foreach (var conobj in cooc)
                            {
                                string convId = conobj.id.ToString();
                                //GET MESSAGES
                                string getmessageurl_part = $"https://cim.expertflow.com/conversation-manager/customer-topics/{convId}/past-events?activityType=ALL";
                                //string getmessageurl = hcurl + getmessageurl_part;
                                BLL.Logger("Going to fetch All Chats for Conversation_Id: " + convId + " from EF_HybridChat");
                                var gethcmessages = new RestSharp.RestClient(getmessageurl_part);
                                var getmessagesrequest = new RestSharp.RestRequest(getmessageurl_part, RestSharp.Method.GET);
                                var hcmessagesresponse = gethcmessages.Execute(getmessagesrequest);
                                int GetMessageStatcode = Convert.ToInt32(hcmessagesresponse.StatusCode);
                                if (GetMessageStatcode == 200)
                                {
                                    string info = "Chats for Conversations_Id: " + convId + " fetched successfully";
                                    BLL.Logger(info);
                                    var MessageDeserializer = new RestSharp.Serialization.Json.JsonDeserializer();
                                    var messageObj = MessageDeserializer.Deserialize<List<HC_Model.GetMessagesResponse>>(hcmessagesresponse);
                                    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
                                    //logerror("Fetched the Messages Successfully through the conversation IDs");
                                    BLL.Logger("Creating JSon object for Conversation_Id: " + convId + "");

                                    //CREAT LIST OF ACTOR CLASSES
                                    List<Verint.Actor> ActorList = new List<Verint.Actor>();
                                    //CREAT LIST OF ACTOR UTTERANCES
                                    List<Verint.Utterance> UtteranceList = new List<Verint.Utterance>();
                                    List<string> To = new List<string>();
                                    List<string> forempty = new List<string>();
                                    string receiver;

                                    //CREATE NEW ATTRIBUTE SUBCALSS INSTANCE
                                    Verint.Attributes attributeinst = new Verint.Attributes();
                                    attributeinst.sourceType = "Chat";
                                    attributeinst.sourceSubType = "Chat";

                                    //CALL NEW ROOT OBJECT FOR LLA INSTANCE
                                    Verint.verint_interface ObjClass = new Verint.verint_interface();
                                    ObjClass.id = convId;
                                    ObjClass.language = "en-us";
                                    ObjClass.sourceType = "Chat";//sourceType;
                                    ObjClass.project = "FNB Chat solution";
                                    //ObjClass.channel = looper.channel;
                                    ObjClass.startTime = conobj.creationTime;
                                    ObjClass.endTime = conobj.endTime;
                                    ObjClass.subject = "";
                                    ObjClass.direction = 1;
                                    ObjClass.threadId = conobj.channelSession;
                                    ObjClass.datasource = "ExpertFlow";
                                    ObjClass.parentId = convId;

                                    foreach (var message in messageObj)
                                    {
                                        if (message.type.ToLower() != "notification")
                                        {
                                            Verint.Actor actorsids = new Verint.Actor();
                                            //SETTING THE DYNAMIC PART OF ACTORS CLASS INSTANCE
                                            actorsids.id = message.data.header.sender.id;// actor.from.id.ToLower();
                                            bool containsItem = ActorList.Any(item => item.id == actorsids.id);
                                            if (containsItem == false)
                                            {
                                                actorsids.email = "";
                                                actorsids.accountId = message.data.header.sender.id.ToLower();
                                                string finalrole;
                                                switch (message.data.header.sender.type.ToLower())
                                                {
                                                    case "bot":
                                                        finalrole = "info";
                                                        break;
                                                    case "customer":
                                                        finalrole = "visitor";
                                                        break;
                                                    case "agent":
                                                        finalrole = "agent";
                                                        break;
                                                    case "supervisor":
                                                        finalrole = "agent";
                                                        break;
                                                    default:
                                                        finalrole = "";
                                                        break;
                                                }
                                                actorsids.role = finalrole;// actor.from.type.ToLower() == "bot" ? "info" : actor.from.type.ToLower();
                                                actorsids.displayName = finalrole == "agent" ? message.data.header.sender.participant.keycloakUser.username : finalrole == "customer" ? message.data.header.sender.participant.customer.firstName : finalrole == "bot" ? finalrole.ToString() : "";
                                                actorsids.timezone = "+02:00";
                                                actorsids.enterTime = message.timestamp;
                                                actorsids.leaveTime = conobj.endTime;
                                                //CONFIRM IF THE ACTOR DOES NOT ALREADY EXIST IN THE LIST OF ACTORS AND PUSH NEW ITEM INTO LIST
                                                ActorList.Add(actorsids);
                                            }
                                        }
                                        bool agenstexist = ActorList.Any(item => item.role.ToLower() == "agent");
                                        if (agenstexist == true)
                                        {
                                            foreach (var ChatItem in messageObj)
                                            {

                                                //SETTING THE DYNAMIC PART OF UTTERANCE CLASS INSTANCE
                                                //CREATE NEW UTTERANCE SUBCLASS
                                                Verint.Utterance utteranceinst = new Verint.Utterance();

                                                if (ChatItem.to.Count == 0)
                                                {
                                                    forempty = ActorList.Where(f => f.role.ToLower() == "agent").Select(n => n.id).ToList();
                                                }

                                                else
                                                {

                                                    receiver = ChatItem.to[0].id.ToLower().ToString();
                                                    //    To.Add(receiver);
                                                    bool containsItem2 = To.Any(item => item.ToString() == receiver.ToString());

                                                    if (containsItem2 == false)
                                                    {
                                                        To.Add(receiver);
                                                    }

                                                }
                                                utteranceinst.language = "en-us";
                                                utteranceinst.actor = ChatItem.from.id.ToLower().ToLower();

                                                utteranceinst.to = To.Count == 0 ? forempty : To; // ChatItem.to[0].id.ToLower();// To;
                                                utteranceinst.startTime = ChatItem.timestamp;
                                                utteranceinst.startTime = ChatItem.timestamp;
                                                utteranceinst.type = ChatItem.messageType;
                                                utteranceinst.value = ChatItem.text == null ? "This is an activity message" : System.Web.HttpUtility.UrlDecode(ChatItem.text);
                                                utteranceinst.raw_value = ChatItem.text == null ? "This is an activity message" : System.Web.HttpUtility.UrlDecode(ChatItem.text);
                                                // PUSH NEW ITEM INTO LIST
                                                UtteranceList.Add(utteranceinst);

                                                //SETTING THE DYNAMIC PART OF ROOT CLASS INSTANCE
                                                ObjClass.type = "EF-HybridChat" + ChatItem.messageType;
                                                ObjClass.actors = ActorList;
                                                ObjClass.attributes = attributeinst;
                                                ObjClass.utterances = UtteranceList;

                                                //NOW ADDING THE DATA TO RESPECTIVE OBJECT ARRAYS

                                            }
                                            //DELETE ANY EXISTING FILE IN THE FOLDER
                                            System.IO.DirectoryInfo di = new DirectoryInfo(@"C:\inetpub\wwwroot\Temp\");

                                            foreach (FileInfo file in di.EnumerateFiles())
                                            {
                                                file.Delete();
                                            }
                                            //CREATE A FILE OUT OF THE OBJECT BEFORE SENDING TO SFTP
                                            logerror("Creating JSon file - SFTP Transfer for Conversation_Id: " + convid + "");
                                            using (StreamWriter file = File.CreateText(@"C:\inetpub\wwwroot\Temp\" + convid + "_" + DateTime.Now.AddDays(-1).ToString("ddMMyyyy") + ".json"))
                                            {
                                                JsonSerializer serializer = new JsonSerializer();
                                                //serialize object directly into file stream
                                                serializer.Serialize(file, ObjClass);
                                            }

                                            //PUSHING TO SFTP FOLDER
                                            //logerror("JSon Created successfully, now sending file to SFTP client for Conversation_Id: " + convid + "");
                                            //using (var sftpclient = new SftpClient(host, port, username, password))
                                            //{
                                            //    string uploadFile = @"C:\inetpub\wwwroot\Temp\" + convid + "_" + DateTime.Now.AddDays(-1).ToString("ddMMyyyy") + ".json";
                                            //    sftpclient.Connect();
                                            //    if (sftpclient.IsConnected)
                                            //    {
                                            //        logerror("Connected to SFTP: " + convid + "");
                                            //        using (var fileStream = new FileStream(uploadFile, FileMode.Open))
                                            //        {

                                            //            sftpclient.BufferSize = 4 * 1024; // bypass Payload error large files
                                            //            sftpclient.UploadFile(fileStream, Path.GetFileName(uploadFile));
                                            //        }
                                            //        logerror("file sent to SFTP: " + convid + "");
                                            //        if (File.Exists(uploadFile))
                                            //        {
                                            //            // If file found, delete it    
                                            //            File.Delete(uploadFile);
                                            //        }
                                            //    }
                                            //    else
                                            //    {
                                            //        logerror("Connection to SFTP server failed, trying again..: " + convid + "");
                                            //    }
                                            //}





                                            //DELETING ANY REMNANT FILE IN TEMP FOLDER FOR JSON UPLOAD
                                            foreach (FileInfo file in di.EnumerateFiles())
                                            {
                                                file.Delete();
                                            }
                                            //NPW CALLING NTT API TO PUSH THE CHAT TRANSCRIPTS
                                            string LLA_url = "https://sydpvertxr01.iptel.lifeline.org.au/api/recording/textcapture/v1/ingestion";
                                            string test = "ed067050bbc1a63b285e970cf551dce5";
                                            // geo_json geoprop = new geo_json { type = "Feature", properties = "" };
                                            var keyId = "hmm11D1C";
                                            var keyStr = "nCYUvKyXqoc6dEboQCdmO8B94jVY8ySZrVBJWZLRS1s";

                                            var client = new RestSharp.RestClient(LLA_url);
                                            var request = new RestSharp.RestRequest("" + LLA_url + "", RestSharp.Method.POST);
                                            client.UseVwtAuthentication(keyId, keyStr);
                                            // request.AddHeader("UKtLFljf", "B6CEF06DE8BA59FA57ED4F76AC24F56DD6194DB589E414CB5B7E3812FF46944FFEA663CFCB79141DC5A2387A50B045D24360EC7F973D2E9D802B45B1161C21BF");
                                            request.RequestFormat = RestSharp.DataFormat.None;
                                            request.AddJsonBody(ObjClass);

                                            var response = client.Execute(request);
                                            int V_StatCode = Convert.ToInt32(response.StatusCode);
                                            if (V_StatCode == 201)
                                            {
                                                string V_info = "Successfully pushed Chat Transcript for " + convid + "";
                                                logerror(V_info);
                                                string insertquery = "insert into Report_Stat (ConversationId,ChatStartTime,ChatEndTime,ThreadId,status,Error_message,Date_reported) values('" + ObjClass.id + "','" + ObjClass.startTime + "','" + ObjClass.endTime + "','" + ObjClass.threadId + "'," + 1 + ",'N/A','" + DateTime.Now + "')";
                                                SqlCommand activated = new SqlCommand(insertquery, conn);
                                                activated.ExecuteNonQuery();
                                                success_count = success_count + 1;
                                                string s = Newtonsoft.Json.JsonConvert.SerializeObject(ObjClass);
                                                logerror(s);
                                            }
                                            else
                                            {
                                                string errorinstring = response.Content.ToString() == "" ? response.ErrorException.Message.ToString() : response.Content.ToString();
                                                errorinstring = errorinstring.Replace("'", "");
                                                errorinstring = errorinstring.Replace("\"", "");
                                                //string errormessagatrimmed = errormessagatrimmedx.Replace("com.verint.textcapture.model.exception.", "Verint Text Capture Model Exception:");
                                                // string checkquery = "select * from Report_Stat where ConversationId='" + ObjClass.id + "'";
                                                string checkquery = "IF EXISTS (select * from Report_Stat where ConversationId='" + ObjClass.id + "') BEGIN update Report_Stat set status=" + 0 + ",Error_message='" + errorinstring + "',Date_Updated='" + DateTime.Now + "' where ConversationId='" + ObjClass.id + "' END ELSE BEGIN insert into Report_Stat (ConversationId,ChatStartTime,ChatEndTime,ThreadId,status,Error_message,Date_reported) values('" + ObjClass.id + "','" + ObjClass.startTime + "','" + ObjClass.endTime + "','" + ObjClass.threadId + "'," + 0 + ",'" + errorinstring + "','" + DateTime.Now + "') END";

                                                SqlCommand activated = new SqlCommand(checkquery, conn);
                                                activated.ExecuteNonQuery();

                                                fail_count = fail_count + 1;
                                                var s = response.StatusCode.ToString() == "0" ? response.ErrorException.Message.ToString() : errorinstring;
                                                logerror(s);


                                            }
                                        }
                                        else
                                        {
                                            //NO AGENT IN THE ACTOR LIST SO DO NOTHING
                                            string noagenterror = "No agent exist in the conversation: " + convid + "";
                                            logerror(noagenterror);
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }


        }
    }
}
