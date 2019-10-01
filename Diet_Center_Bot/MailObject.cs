using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrowserHistoryServer.Data
{
     public class MailObject
    {
        public string Server { get; set; }
        public int SMPT_port { get; set; }
        public string From_email { get; set; }
        public string To_email { get; set; }
        public string From_email_login { get; set; }
        public string From_email_password { get; set; }
        public string Name { get; set; }
        public string Topic { get; set; }
        public string Text { get; set; }
        public MailObject()
        {

        }
        MailObject(string Server, int SMPT_port, string From_email, string To_email, string From_email_login, string From_email_password, string Name, string Topic, string Text)
        {
            this.Server = Server;
            this.SMPT_port = SMPT_port;
            this.From_email = From_email;
            this.To_email = To_email;
            this.From_email_login = From_email_login;
            this.From_email_password = From_email_password;
            this.Name = Name;
            this.Topic = Topic;
            this.Text = Text;
        }
    }
}
