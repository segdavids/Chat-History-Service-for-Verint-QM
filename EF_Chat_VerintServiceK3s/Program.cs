// See https://aka.ms/new-console-template for more information
using FNB_Verint_QM_Service.AppData;
using FNB_Verint_QM_Service;
using System.Data;
using System.Net;
using System;
using System.Configuration;
using System.Data.SqlClient;
using Verint.Platform.Security;

Console.WriteLine("Hello, World!");
BLL.FetchChats();
string[] msgs = { "TASK_STATE_CHANGED", "TASK_STATE_CHANGED" };
List<string> messagenames = new List<string>();
messagenames.AddRange(msgs);
SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["VerintDB"].ConnectionString);

string action = "Conversations Fetched successfully";
BLL.Logger(action);
var deserializer1 = new RestSharp.Serialization.Json.JsonDeserializer();
List<HC_Model.GetConversationResponseData> cooc = new List<HC_Model.GetConversationResponseData>();
// cooc = deserializer1.Deserialize<List<HC_Model.GetConversationResponseData>>(hcconversationresponse);
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
BLL.Logger("looping through the conversation IDs");
int success_count = 0;
int fail_count = 0;

string convId = "63510a7e2854e769ac310d74";
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
    List<FNB_Verint_QM_Service.Verint.Actor> ActorList = new ();
    //CREAT LIST OF ACTOR UTTERANCES
    List<FNB_Verint_QM_Service.Verint.Utterance> UtteranceList = new ();
    List<string> To = new List<string>();
    List<string> forempty = new List<string>();
    string receiver;


    //CALL NEW ROOT OBJECT FOR LLA INSTANCE
    FNB_Verint_QM_Service.Verint.verint_interface ObjClass = new FNB_Verint_QM_Service.Verint.verint_interface();
    ObjClass.id = convId;
    ObjClass.language = "en-us";
    ObjClass.sourceType = "Chat";//sourceType;
    ObjClass.project = "FNB Chat solution";
    //ObjClass.channel = looper.channel;
    ObjClass.startTime = "No Time";
    ObjClass.endTime = "No Time";
    ObjClass.subject = "";
    ObjClass.direction = 1;
    ObjClass.threadId = "No Time";
    ObjClass.datasource = "ExpertFlow";
    ObjClass.parentId = convId;

    foreach (var message in messageObj)
    {
        if (message.type.ToLower() != "notification")
        {
            FNB_Verint_QM_Service.Verint.Actor actorsids = new FNB_Verint_QM_Service.Verint.Actor();
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
                actorsids.leaveTime = message.timestamp;
                //CONFIRM IF THE ACTOR DOES NOT ALREADY EXIST IN THE LIST OF ACTORS AND PUSH NEW ITEM INTO LIST
                ActorList.Add(actorsids);
            }

            //SETTING THE DYNAMIC PART OF UTTERANCE CLASS INSTANCE
            //CREATE NEW UTTERANCE SUBCLASS
            FNB_Verint_QM_Service.Verint.Utterance utteranceinst = new FNB_Verint_QM_Service.Verint.Utterance();
            receiver = message.name.ToLower() == "customer_message" || message.name.ToLower() == "agent_message" ? message.data.header.sender.participant.customer._id.ToString() : message.data.header.sender.participant.id;
            To.Add(receiver);
            string sender = message.data.header.sender.type.ToLower() == "agent" ? message.data.header.sender.participant.keycloakUser.id : message.data.header.sender.type.ToLower() == "bot" ? message.data.header.sender.participant.id : message.data.header.sender.type.ToLower() == "customer" ? message.data.header.sender.participant.customer._id : "";
            string messagetype = message.data.body.type;

            utteranceinst.language = "en-us";
            utteranceinst.actor = sender;

            utteranceinst.to = To.Count == 0 ? forempty : To; // ChatItem.to[0].id.ToLower();// To;
            utteranceinst.startTime = message.timestamp;
            utteranceinst.type = messagetype;
            utteranceinst.value = message.data.body.markdownText == null ? "" : System.Web.HttpUtility.UrlDecode(message.data.body.markdownText);
            utteranceinst.raw_value = message.data.body.markdownText == null ? "" : System.Web.HttpUtility.UrlDecode(message.data.body.markdownText);
            // PUSH NEW ITEM INTO LIST
            UtteranceList.Add(utteranceinst);

        }
    }


    bool agenstexist = ActorList.Any(item => item.role.ToLower() == "agent");
    if (agenstexist == true)
    {

        double chatduration = (Convert.ToDateTime("2022-10-20T08:44:48.269+00:00") - Convert.ToDateTime("2022-10-20T06:44:48.269+00:00")).TotalMinutes;
        //CREATE NEW ATTRIBUTE SUBCALSS INSTANCE
        FNB_Verint_QM_Service.Verint.Attributes attributeinst = new()
        {
            sourceType = "platformChat",
            sourceSubType = "InternalChats",
            customfield24 = chatduration,
            customfield1 = "Queue",
            customfield2 = "",
            customfield3 = ""
        };

        //SETTING THE DYNAMIC PART OF ROOT CLASS INSTANCE
        ObjClass.type = "Conversation";
        ObjClass.actors = ActorList;
        ObjClass.attributes = attributeinst;
        ObjClass.utterances = UtteranceList;

        //NPW CALLING NTT API TO PUSH THE CHAT TRANSCRIPTS
        string VetintIngestor = ConfigurationManager.AppSettings["VetintIngestor"];// "https://sydpvertxr01.iptel.lifeline.org.au/api/recording/textcapture/v1/ingestion";
        string keyId = ConfigurationManager.AppSettings["keyId"];
        string keyStr = ConfigurationManager.AppSettings["keyStr"];
        var client = new RestSharp.RestClient(VetintIngestor);
        var request = new RestSharp.RestRequest("" + VetintIngestor + "", RestSharp.Method.POST);
        client.UseVwtAuthentication(keyId, keyStr);
        // request.AddHeader("UKtLFljf", "B6CEF06DE8BA59FA57ED4F76AC24F56DD6194DB589E414CB5B7E3812FF46944FFEA663CFCB79141DC5A2387A50B045D24360EC7F973D2E9D802B45B1161C21BF");
        request.RequestFormat = RestSharp.DataFormat.None;
        request.AddJsonBody(ObjClass);

        var response = client.Execute(request);
        int V_StatCode = Convert.ToInt32(response.StatusCode);
        if (V_StatCode == 201)
        {
            string V_info = "Successfully pushed Chat Transcript for " + convId + "";
            BLL.Logger(V_info);
            string insertquery = "insert into Report_Stat (ConversationId,ChatStartTime,ChatEndTime,ThreadId,status,Error_message,Date_reported) values('" + ObjClass.id + "','" + ObjClass.startTime + "','" + ObjClass.endTime + "','" + ObjClass.threadId + "'," + 1 + ",'N/A','" + DateTime.Now + "')";
            SqlCommand activated = new SqlCommand(insertquery, conn);
            activated.ExecuteNonQuery();
            success_count = success_count + 1;
            string s = Newtonsoft.Json.JsonConvert.SerializeObject(ObjClass);
            BLL.Logger(s);
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
            BLL.Logger(s);
        }
    }
    else
    {
        //NO AGENT IN THE ACTOR LIST SO DO NOTHING
        string noagenterror = "No agent exist in the conversation: " + convId + "";
        BLL.Logger(noagenterror);
    }
}


//private static void FetchChats()
//{
//    SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["VerintDB"].ConnectionString);
//    //string host = String.Empty;
//    //string username = String.Empty;
//    //string password = String.Empty;
//    //string workingdirectory = String.Empty;
//    //int port = 0;
//    ////GET PASSWORD
//    //string get = $"select * from sftp";
//    //DataTable dt = BLL.GetRequest(get);
//    //if (dt.Rows.Count > 0)
//    //{
//    //    string temppw = String.IsNullOrEmpty(dt.Rows[0]["password"].ToString()) ? "" : dt.Rows[0]["password"].ToString();
//    //    password = temppw;// Library.decryptpass(temppw);
//    //    username = String.IsNullOrEmpty(dt.Rows[0]["username"].ToString()) ? "" : dt.Rows[0]["username"].ToString();
//    //    host = String.IsNullOrEmpty(dt.Rows[0]["host"].ToString()) ? "" : dt.Rows[0]["host"].ToString();
//    //    port = Convert.ToInt32(String.IsNullOrEmpty(dt.Rows[0]["port"].ToString()) ? "0" : dt.Rows[0]["port"].ToString());
//    //}
//    string hcurl;
//    Verint.verint_interface retturnmeeesgae = new Verint.verint_interface { };
//    try
//    {
//        string currtime = DateTime.Now.ToString("yyyy-MM-dd");
//        string yesttime = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
//        string urlpart = "/conversations/getAllConversations?start_time=" + yesttime + "&end_time=" + currtime + "";
//        var deserializer = new RestSharp.Serialization.Json.JsonDeserializer();
//        BLL.Logger("Going to get Hybrid Chat URL from DB");
//        string query = @"select hc_url from endpoints";// where url_name = 'getconversations'";
//        DataTable querydt = BLL.GetRequest(query);
//        if (querydt.Rows.Count > 0)
//        {
//            hcurl = querydt.Rows[0]["hc_url"].ToString().Trim();
//            string getconvourl = hcurl + urlpart;
//            //GET CONVERSTATIONS
//            BLL.Logger("Fetching AllConversations from Hybrid Chat MongoDB");
//            var gethcconversations = new RestSharp.RestClient(getconvourl);
//            var getconvorequest = new RestSharp.RestRequest(getconvourl, RestSharp.Method.GET);
//            var hcconversationresponse = gethcconversations.Execute(getconvorequest);
//            var statcode = hcconversationresponse;
//            if (statcode.StatusCode == HttpStatusCode.OK)
//            {
//                if (statcode.Content.Length > 5)
//                {
//                    string action = "Conversations Fetched successfully";
//                    BLL.Logger(action);
//                    var deserializer1 = new RestSharp.Serialization.Json.JsonDeserializer();
//                    List<HC_Model.GetConversationResponseData> cooc = new List<HC_Model.GetConversationResponseData>();
//                    cooc = deserializer1.Deserialize<List<HC_Model.GetConversationResponseData>>(hcconversationresponse);
//                    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
//                    BLL.Logger("looping through the conversation IDs");
//                    int success_count = 0;
//                    int fail_count = 0;
//                    foreach (var conobj in cooc)
//                    {
//                        string convId = conobj.id.ToString();
//                        //GET MESSAGES
//                        string getmessageurl_part = $"https://cim.expertflow.com/conversation-manager/customer-topics/{convId}/past-events?activityType=ALL";
//                        //string getmessageurl = hcurl + getmessageurl_part;
//                        BLL.Logger("Going to fetch All Chats for Conversation_Id: " + convId + " from EF_HybridChat");
//                        var gethcmessages = new RestSharp.RestClient(getmessageurl_part);
//                        var getmessagesrequest = new RestSharp.RestRequest(getmessageurl_part, RestSharp.Method.GET);
//                        var hcmessagesresponse = gethcmessages.Execute(getmessagesrequest);
//                        int GetMessageStatcode = Convert.ToInt32(hcmessagesresponse.StatusCode);
//                        if (GetMessageStatcode == 200)
//                        {
//                            string info = "Chats for Conversations_Id: " + convId + " fetched successfully";
//                            BLL.Logger(info);
//                            var MessageDeserializer = new RestSharp.Serialization.Json.JsonDeserializer();
//                            var messageObj = MessageDeserializer.Deserialize<List<HC_Model.GetMessagesResponse>>(hcmessagesresponse);
//                            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
//                            //logerror("Fetched the Messages Successfully through the conversation IDs");
//                            BLL.Logger("Creating JSon object for Conversation_Id: " + convId + "");

//                            //CREAT LIST OF ACTOR CLASSES
//                            List<Verint.Actor> ActorList = new List<Verint.Actor>();
//                            //CREAT LIST OF ACTOR UTTERANCES
//                            List<Verint.Utterance> UtteranceList = new List<Verint.Utterance>();
//                            List<string> To = new List<string>();
//                            List<string> forempty = new List<string>();
//                            string receiver;


//                            //CALL NEW ROOT OBJECT FOR LLA INSTANCE
//                            Verint.verint_interface ObjClass = new Verint.verint_interface();
//                            ObjClass.id = convId;
//                            ObjClass.language = "en-us";
//                            ObjClass.sourceType = "Chat";//sourceType;
//                            ObjClass.project = "FNB Chat solution";
//                            //ObjClass.channel = looper.channel;
//                            ObjClass.startTime = conobj.creationTime;
//                            ObjClass.endTime = conobj.endTime;
//                            ObjClass.subject = "";
//                            ObjClass.direction = 1;
//                            ObjClass.threadId = conobj.channelSession;
//                            ObjClass.datasource = "ExpertFlow";
//                            ObjClass.parentId = convId;

//                            foreach (var message in messageObj)
//                            {
//                                if (message.type.ToLower() != "notification")
//                                {
//                                    Verint.Actor actorsids = new Verint.Actor();
//                                    //SETTING THE DYNAMIC PART OF ACTORS CLASS INSTANCE
//                                    actorsids.id = message.data.header.sender.id;// actor.from.id.ToLower();
//                                    bool containsItem = ActorList.Any(item => item.id == actorsids.id);
//                                    if (containsItem == false)
//                                    {
//                                        actorsids.email = "";
//                                        actorsids.accountId = message.data.header.sender.id.ToLower();
//                                        string finalrole;
//                                        switch (message.data.header.sender.type.ToLower())
//                                        {
//                                            case "bot":
//                                                finalrole = "info";
//                                                break;
//                                            case "customer":
//                                                finalrole = "visitor";
//                                                break;
//                                            case "agent":
//                                                finalrole = "agent";
//                                                break;
//                                            case "supervisor":
//                                                finalrole = "agent";
//                                                break;
//                                            default:
//                                                finalrole = "";
//                                                break;
//                                        }
//                                        actorsids.role = finalrole;// actor.from.type.ToLower() == "bot" ? "info" : actor.from.type.ToLower();
//                                        actorsids.displayName = finalrole == "agent" ? message.data.header.sender.participant.keycloakUser.username : finalrole == "customer" ? message.data.header.sender.participant.customer.firstName : finalrole == "bot" ? finalrole.ToString() : "";
//                                        actorsids.timezone = "+02:00";
//                                        actorsids.enterTime = message.timestamp;
//                                        actorsids.leaveTime = conobj.endTime;
//                                        //CONFIRM IF THE ACTOR DOES NOT ALREADY EXIST IN THE LIST OF ACTORS AND PUSH NEW ITEM INTO LIST
//                                        ActorList.Add(actorsids);
//                                    }

//                                    //SETTING THE DYNAMIC PART OF UTTERANCE CLASS INSTANCE
//                                    //CREATE NEW UTTERANCE SUBCLASS
//                                    Verint.Utterance utteranceinst = new Verint.Utterance();
//                                    receiver = message.name.ToLower() == "customer_message" || message.name.ToLower() == "agent_message" ? message.data.header.sender.participant.customer._id.ToString() : message.data.header.sender.participant.id;
//                                    To.Add(receiver);
//                                    string sender = message.data.header.sender.type.ToLower() == "agent" ? message.data.header.sender.participant.keycloakUser.id : message.data.header.sender.type.ToLower() == "bot" ? message.data.header.sender.participant.id : message.data.header.sender.type.ToLower() == "customer" ? message.data.header.sender.participant.customer._id : "";
//                                    string messagetype = message.data.body.type;

//                                    utteranceinst.language = "en-us";
//                                    utteranceinst.actor = sender;

//                                    utteranceinst.to = To.Count == 0 ? forempty : To; // ChatItem.to[0].id.ToLower();// To;
//                                    utteranceinst.startTime = message.timestamp;
//                                    utteranceinst.type = messagetype;
//                                    utteranceinst.value = message.data.body.markdownText == null ? "" : System.Web.HttpUtility.UrlDecode(message.data.body.markdownText);
//                                    utteranceinst.raw_value = message.data.body.markdownText == null ? "" : System.Web.HttpUtility.UrlDecode(message.data.body.markdownText);
//                                    // PUSH NEW ITEM INTO LIST
//                                    UtteranceList.Add(utteranceinst);

//                                }
//                            }


//                            bool agenstexist = ActorList.Any(item => item.role.ToLower() == "agent");
//                            if (agenstexist == true)
//                            {

//                                double chatduration = (Convert.ToDateTime(conobj.creationTime) - Convert.ToDateTime(conobj.endTime)).TotalMinutes;
//                                //CREATE NEW ATTRIBUTE SUBCALSS INSTANCE
//                                Verint.Attributes attributeinst = new Verint.Attributes();
//                                attributeinst.sourceType = "platformChat";
//                                attributeinst.sourceSubType = "InternalChats";
//                                attributeinst.customfield24 = chatduration;
//                                attributeinst.customfield1 = "Queue";
//                                attributeinst.customfield2 = "";
//                                attributeinst.customfield3 = "";

//                                //SETTING THE DYNAMIC PART OF ROOT CLASS INSTANCE
//                                ObjClass.type = "Conversation";
//                                ObjClass.actors = ActorList;
//                                ObjClass.attributes = attributeinst;
//                                ObjClass.utterances = UtteranceList;

//                                //NPW CALLING NTT API TO PUSH THE CHAT TRANSCRIPTS
//                                string VetintIngestor = ConfigurationManager.AppSettings["VetintIngestor"];// "https://sydpvertxr01.iptel.lifeline.org.au/api/recording/textcapture/v1/ingestion";
//                                string keyId = ConfigurationManager.AppSettings["keyId"];
//                                string keyStr = ConfigurationManager.AppSettings["keyStr"];
//                                var client = new RestSharp.RestClient(VetintIngestor);
//                                var request = new RestSharp.RestRequest("" + VetintIngestor + "", RestSharp.Method.POST);
//                                client.UseVwtAuthentication(keyId, keyStr);
//                                // request.AddHeader("UKtLFljf", "B6CEF06DE8BA59FA57ED4F76AC24F56DD6194DB589E414CB5B7E3812FF46944FFEA663CFCB79141DC5A2387A50B045D24360EC7F973D2E9D802B45B1161C21BF");
//                                request.RequestFormat = RestSharp.DataFormat.None;
//                                request.AddJsonBody(ObjClass);

//                                var response = client.Execute(request);
//                                int V_StatCode = Convert.ToInt32(response.StatusCode);
//                                if (V_StatCode == 201)
//                                {
//                                    string V_info = "Successfully pushed Chat Transcript for " + convId + "";
//                                    BLL.Logger(V_info);
//                                    string insertquery = "insert into Report_Stat (ConversationId,ChatStartTime,ChatEndTime,ThreadId,status,Error_message,Date_reported) values('" + ObjClass.id + "','" + ObjClass.startTime + "','" + ObjClass.endTime + "','" + ObjClass.threadId + "'," + 1 + ",'N/A','" + DateTime.Now + "')";
//                                    SqlCommand activated = new SqlCommand(insertquery, conn);
//                                    activated.ExecuteNonQuery();
//                                    success_count = success_count + 1;
//                                    string s = Newtonsoft.Json.JsonConvert.SerializeObject(ObjClass);
//                                    BLL.Logger(s);
//                                }
//                                else
//                                {
//                                    string errorinstring = response.Content.ToString() == "" ? response.ErrorException.Message.ToString() : response.Content.ToString();
//                                    errorinstring = errorinstring.Replace("'", "");
//                                    errorinstring = errorinstring.Replace("\"", "");
//                                    //string errormessagatrimmed = errormessagatrimmedx.Replace("com.verint.textcapture.model.exception.", "Verint Text Capture Model Exception:");
//                                    // string checkquery = "select * from Report_Stat where ConversationId='" + ObjClass.id + "'";
//                                    string checkquery = "IF EXISTS (select * from Report_Stat where ConversationId='" + ObjClass.id + "') BEGIN update Report_Stat set status=" + 0 + ",Error_message='" + errorinstring + "',Date_Updated='" + DateTime.Now + "' where ConversationId='" + ObjClass.id + "' END ELSE BEGIN insert into Report_Stat (ConversationId,ChatStartTime,ChatEndTime,ThreadId,status,Error_message,Date_reported) values('" + ObjClass.id + "','" + ObjClass.startTime + "','" + ObjClass.endTime + "','" + ObjClass.threadId + "'," + 0 + ",'" + errorinstring + "','" + DateTime.Now + "') END";

//                                    SqlCommand activated = new SqlCommand(checkquery, conn);
//                                    activated.ExecuteNonQuery();

//                                    fail_count = fail_count + 1;
//                                    var s = response.StatusCode.ToString() == "0" ? response.ErrorException.Message.ToString() : errorinstring;
//                                    BLL.Logger(s);
//                                }
//                            }
//                            else
//                            {
//                                //NO AGENT IN THE ACTOR LIST SO DO NOTHING
//                                string noagenterror = "No agent exist in the conversation: " + convId + "";
//                                BLL.Logger(noagenterror);
//                            }
//                        }
//                    }
//                }
//            }
//            else
//            {
//                BLL.Logger($"Conversations were not successfully fetched: {statcode.StatusCode}");
//            }
//        }
//    }
//    catch (Exception e)
//    {
//        BLL.Logger(e.ToString());
//    }
//}

