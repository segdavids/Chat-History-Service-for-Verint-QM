using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FNB_Verint_QM_Service
{
    public class Test
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

        public class convdataobj
        {
            public string id { get; set; }
        }
    }
}
