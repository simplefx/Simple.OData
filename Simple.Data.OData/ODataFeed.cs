namespace Simple.Data.OData
{
    public class ODataFeed
    {
        public string Url { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public bool IntegratedSecurity { get; set; }

        public ODataFeed(string url)
            : this(url, null, null, null, false)
        {
            
        }

        public ODataFeed(string url, string user, string password, string domain = null)
            : this(url, user, password, domain, false)
        {

        }

        public ODataFeed(string url, bool integratedSecrity)
            : this(url, null, null, null, integratedSecrity)
        {

        }

        public ODataFeed(string url, string user, string password, string domain, bool integratedSecrity)
        {
            this.Url = url;
            this.User = user;
            this.Password = password;
            this.Domain = domain;
            this.IntegratedSecurity = integratedSecrity;
        }
    }
}