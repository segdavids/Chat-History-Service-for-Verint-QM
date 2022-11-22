using Microsoft.SqlServer.Server;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Runtime.Remoting.Channels;
//using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace FNB_Verint_QM_Service
{
    public class HC_Model
    {
        public class GetConversationResponseData
        {
            public string id { get; set; }
            public string customer { get; set; }
            public List<string> participants { get; set; }
            public string state { get; set; }
            public string channelSession { get; set; }
            public string creationTime { get; set; }
            public string endTime { get; set; }
            public convdataobj conversationData { get; set; }
            public string botId { get; set; }
            public string lastUsedChannelSession { get; set; }
        }

        /// <summary>
        /// GET ALL CONVERSATION OBJECT 
        /// </summary>


        public class convdataobj
        {
            public string id { get; set; }
        }

       //RESPOJNSE TO GET MESAAGE

            /// <summary>
            /// ROOT RESPONSE FOR GET MESSAGES
            /// </summary>
            public class GetMessagesResponse
            {
                public string id { get; set; }
                public string name { get; set; }
                public string type { get; set; }
                public string timestamp { get; set; }
                public string conversationId { get; set; }
                public Data data { get; set; }
            }


            // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
            public class AdditionalAttribute
            {
                public string key { get; set; }
                public string type { get; set; }
                public Value value { get; set; }
            }

            public class AgentParticipant
            {
                public string id { get; set; }
                public string type { get; set; }
                public Participant participant { get; set; }
                public object token { get; set; }
                public string conversationId { get; set; }
                public string role { get; set; }
                public object userCredentials { get; set; }
                public string state { get; set; }
            }

            public class Attribute
            {
                public string value { get; set; }
                public string key { get; set; }
                public string type { get; set; }
            }

            public class Body
            {
                public string type { get; set; }
                public string markdownText { get; set; }
            }

            public class BrowserDeviceInfo
            {
                public string browserId { get; set; }
                public string browserIdExpiryTime { get; set; }
                public string browserName { get; set; }
                public string deviceType { get; set; }
            }

            public class Channel
            {
                public string id { get; set; }
                public string name { get; set; }
                public string serviceIdentifier { get; set; }
                public bool defaultOutbound { get; set; }
                public Tenant tenant { get; set; }
                public ChannelConfig channelConfig { get; set; }
                public ChannelConnector channelConnector { get; set; }
                public ChannelType channelType { get; set; }
            }

            public class ChannelConfig
            {
                public string id { get; set; }
                public string channelMode { get; set; }
                public string conversationBot { get; set; }
                public int responseSla { get; set; }
                public int customerActivityTimeout { get; set; }
                public CustomerIdentificationCriteria customerIdentificationCriteria { get; set; }
                public RoutingPolicy routingPolicy { get; set; }
                public string botId { get; set; }
            }

            public class ChannelConnector
            {
                public string id { get; set; }
                public string name { get; set; }
                public ChannelProviderInterface channelProviderInterface { get; set; }
                public List<object> channelProviderConfigs { get; set; }
                public Tenant tenant { get; set; }
            }

            public class ChannelData
            {
                public string channelCustomerIdentifier { get; set; }
                public string serviceIdentifier { get; set; }
                public int requestPriority { get; set; }
                public List<AdditionalAttribute> additionalAttributes { get; set; }
            }

            public class ChannelProviderInterface
            {
                public string id { get; set; }
                public string name { get; set; }
                public List<SupportedChannelType> supportedChannelTypes { get; set; }
                public string providerWebhook { get; set; }
                public List<object> channelProviderConfigSchema { get; set; }
            }

            public class ChannelSession
            {
                public string participantType { get; set; }
                public string id { get; set; }
                public Channel channel { get; set; }
                public Customer customer { get; set; }
                public List<object> customerSuggestions { get; set; }
                public ChannelData channelData { get; set; }
                public object latestIntent { get; set; }
                public CustomerPresence customerPresence { get; set; }
                public bool isActive { get; set; }
                public string conversationId { get; set; }
                public State state { get; set; }
                public bool active { get; set; }
            }

            public class ChannelType
            {
                public string id { get; set; }
                public string name { get; set; }
                public string channelLogo { get; set; }
                public bool isInteractive { get; set; }
                public string mediaRoutingDomain { get; set; }
            }

            public class Customer
            {
                public string _id { get; set; }
                public string firstName { get; set; }
                public List<object> phoneNumber { get; set; }
                public bool isAnonymous { get; set; }
                public int __v { get; set; }
                public List<string> web { get; set; }
            }

            public class CustomerIdentificationCriteria
            {
                public object value { get; set; }
            }

            public class CustomerPresence
            {
                public object value { get; set; }
            }

            public class Data
            {
                public string id { get; set; }
                public object type { get; set; }
                public string name { get; set; }
                public string uri { get; set; }
                public string participantType { get; set; }
                public Channel channel { get; set; }
                public Customer customer { get; set; }
                public List<object> customerSuggestions { get; set; }
                public ChannelData channelData { get; set; }
                public object latestIntent { get; set; }
                public CustomerPresence customerPresence { get; set; }
                public bool? isActive { get; set; }
                public string conversationId { get; set; }
                public State state { get; set; }
                public bool? active { get; set; }
                public Header header { get; set; }
                public Body body { get; set; }
                public object data { get; set; }
                public ChannelSession channelSession { get; set; }
                public Mrd mrd { get; set; }
                public object queue { get; set; }
                public int? priority { get; set; }
                public string assignedTo { get; set; }
                public long? enqueueTime { get; set; }
                public long? answerTime { get; set; }
                public int? handleTime { get; set; }
                public Task task { get; set; }
                public AgentParticipant agentParticipant { get; set; }
                public string reason { get; set; }
            }

            public class Entities
            {
            }

            public class FormData
            {
                public double id { get; set; }
                public double formId { get; set; }
                public string filledBy { get; set; }
                public List<Attribute> attributes { get; set; }
                public DateTime createdOn { get; set; }
            }

            public class Header
            {
                public Sender sender { get; set; }
                public ChannelData channelData { get; set; }
                public Language language { get; set; }
                public object timestamp { get; set; }
                public SecurityInfo securityInfo { get; set; }
                public List<object> stamps { get; set; }
                public object intent { get; set; }
                public Entities entities { get; set; }
                public ChannelSession channelSession { get; set; }
                public object replyToMessageId { get; set; }
                public object providerMessageId { get; set; }
            }

            public class KeycloakUser
            {
                public string id { get; set; }
                public string firstName { get; set; }
                public string lastName { get; set; }
                public string username { get; set; }
                public PermittedResources permittedResources { get; set; }
                public List<string> roles { get; set; }
                public string realm { get; set; }
            }

            public class Language
            {
            }

            public class Locale
            {
                public string timezone { get; set; }
                public string language { get; set; }
                public string country { get; set; }
            }

            public class Metadata
            {
                public string requestedBy { get; set; }
                public object note { get; set; }
            }

            public class Mrd
            {
                public string id { get; set; }
                public string name { get; set; }
                public string description { get; set; }
                public bool interruptible { get; set; }
                public int maxRequests { get; set; }
            }

            public class Participant
            {
                public string participantType { get; set; }
                public string id { get; set; }
                public string type { get; set; }
                public string name { get; set; }
                public string uri { get; set; }
                public Channel channel { get; set; }
                public Customer customer { get; set; }
                public List<object> customerSuggestions { get; set; }
                public ChannelData channelData { get; set; }
                public object latestIntent { get; set; }
                public CustomerPresence customerPresence { get; set; }
                public bool? isActive { get; set; }
                public string conversationId { get; set; }
                public State state { get; set; }
                public bool? active { get; set; }
                public KeycloakUser keycloakUser { get; set; }
                public List<object> associatedRoutingAttributes { get; set; }
            }

            public class PermittedResources
            {
                public List<Resource> Resources { get; set; }
            }

            public class Resource
            {
                public List<string> scopes { get; set; }
                public string rsid { get; set; }
                public string rsname { get; set; }
            }


            public class RoutingPolicy
            {
                public string agentSelectionPolicy { get; set; }
                public bool routeToLastAgent { get; set; }
                public string routingMode { get; set; }
                public string routingObjectId { get; set; }
                public int agentRequestTtl { get; set; }
            }

            public class SecurityInfo
            {
            }

            public class Sender
            {
                public string type { get; set; }
                public string role { get; set; }
                public Participant participant { get; set; }
                public string id { get; set; }
                public object joiningTime { get; set; }
                public object token { get; set; }
                public string conversationId { get; set; }
                public bool isActive { get; set; }
                public UserCredentials userCredentials { get; set; }
                public string state { get; set; }
                public object stateChangedOn { get; set; }
            }

            public class State
            {
                public string name { get; set; }
                public string reasonCode { get; set; }
            }

            public class SupportedChannelType
            {
                public string id { get; set; }
                public string name { get; set; }
                public string channelLogo { get; set; }
                public bool isInteractive { get; set; }
                public string mediaRoutingDomain { get; set; }
            }

            public class Task
            {
                public string id { get; set; }
                public ChannelSession channelSession { get; set; }
                public Mrd mrd { get; set; }
                public string queue { get; set; }
                public int priority { get; set; }
                public State state { get; set; }
                public Type type { get; set; }
                public object assignedTo { get; set; }
                public object enqueueTime { get; set; }
                public object answerTime { get; set; }
                public int handleTime { get; set; }
            }

            public class Tenant
            {
                public string id { get; set; }
                public object name { get; set; }
            }

            public class Type
            {
                public string direction { get; set; }
                public string mode { get; set; }
                public Metadata metadata { get; set; }
            }

            public class UserCredentials
            {
            }

            public class Value
            {
                public BrowserDeviceInfo browserDeviceInfo { get; set; }
                public string queue { get; set; }
                public Locale locale { get; set; }
                public FormData formData { get; set; }
            }




        


    }
    
}
