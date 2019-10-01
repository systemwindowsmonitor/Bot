namespace Diet_Center_Bot
{
    internal class User
    {
        public string id { get; set; }
        public string account_name { get; set; }
        public string ip { get; set; }
        public string Region { get; set; }

        public User(string id, string name, string ip, string region)
        {
            this.account_name = name;
            this.id = id;
            this.ip = ip;
            this.Region = region;
        }

    }
}