using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FNB_Verint_QM_Service
{
    internal class Verint
    {
        public class verint_interface
        {
            public string id { get; set; }
            public string language { get; set; }
            public string type { get; set; }
            public string sourceType { get; set; }
            public string project { get; set; }
            public string channel { get; set; }
            public string startTime { get; set; }
            public string endTime { get; set; }
            public string subject { get; set; }
            public int direction { get; set; }
            public string threadId { get; set; }
            public string datasource { get; set; }
            public string parentId { get; set; }
            public List<Actor> actors { get; set; }
            public Attributes attributes { get; set; }
            public List<Utterance> utterances { get; set; }

        }

        public class Actor
        {
            public string id { get; set; }
            public string email { get; set; }
            public string accountId { get; set; }
            public string role { get; set; }
            public string displayName { get; set; }
            public string timezone { get; set; }
            public string enterTime { get; set; }
            public string leaveTime { get; set; }
        }

        public class Attributes
        {
            public string sourceType { get; set; }
            public string sourceSubType { get; set; }
            public int customfield24 { get; set; } //This is the duration of the chat e.g 100
            public string customfield1 { get; set; } //This is the skill of the agent default value is "Queue"
            public string customfield2 { get; set; } //This is the customer device verification status e.g untrusted
            public string customfield3 { get; set; } //Wrap up reason

        }

        public class Utterance
        {
            public string language { get; set; }
            public string actor { get; set; }
            public List<string> to { get; set; }
            public string startTime { get; set; }
            public string type { get; set; }
            public string value { get; set; }
            public string raw_value { get; set; }
        }
    }
}
