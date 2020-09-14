using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace WebsysServer
{
    [DataContract]
    internal class CfgJsonDto
    {
        [DataMember(Name ="logLevel")]
        public int LogLevel { get; set; }
        [DataMember(Name ="httpServerPort")]
        public int HttpServerPort { get; set; }
        [DataMember(Name ="httpServerUrl")]
        public string HttpServerUrl { get; set; }
        [DataMember(Name ="webServerIP")]
        public string WebServerIP { get; set; }
        [DataMember(Name ="assDir")]
        public string[] AssDir { get; set; }
        [DataMember(Name = "reqTimeOut")]
        public int ReqTimeOut { get; set; }
        [DataMember(Name = "assList")]
        public List<ActiveObject> AssList { get; set; }
    }
}
