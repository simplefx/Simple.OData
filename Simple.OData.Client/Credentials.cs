namespace Simple.OData.Client
{
    public class Credentials
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public bool IntegratedSecurity { get; set; }

        public Credentials(string user, string password, string domain, bool integratedSecrity)
        {
            this.User = user;
            this.Password = password;
            this.Domain = domain;
            this.IntegratedSecurity = integratedSecrity;
        }
    }
}