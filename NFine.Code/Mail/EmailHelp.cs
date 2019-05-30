using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace NFine.Code.Mail
{
    public class EmailHelp
    {
        public int end_Email(string reciver, string content)
        {
            try
            {
                var emailAcount = ConfigurationManager.AppSettings["EmailAcount"];
                var emailPassword = ConfigurationManager.AppSettings["EmailPassword"];
                if (string.IsNullOrEmpty(emailAcount) || string.IsNullOrEmpty(emailPassword))
                {
                    return 0;
                }
                MailMessage message = new MailMessage();
                //设置发件人,发件人需要与设置的邮件发送服务器的邮箱一致
                MailAddress fromAddr = new MailAddress(emailAcount);
                message.From = fromAddr;
                //设置收件人,可添加多个,添加方法与下面的一样
                message.To.Add(reciver);
                //设置抄送人
                // message.CC.Add("qwe123@163.com");
                //设置邮件标题
                message.Subject = "密码找回";
                //设置邮件内容
                message.Body = content;
                //设置邮件发送服务器,服务器根据你使用的邮箱而不同,可以到相应的 邮箱管理后台查看,下面是QQ的
                SmtpClient client = new SmtpClient("smtp.qq.com", 25);
                //设置发送人的邮箱账号和密码
                client.Credentials = new NetworkCredential(emailAcount, emailPassword);
                //启用ssl,也就是安全发送
                client.EnableSsl = true;
                //发送邮件
                client.Send(message);
                return 1;
            }
            catch (Exception exception)
            {
                Log4NetHelper log=new Log4NetHelper();
                log.Error(exception.Message,exception);
                return 2;
                throw;
            }
      
        }

        public int SendMailUse(string strto,string body)
        {
            string host = "smtp.163.com";// 邮件服务器smtp.163.com表示网易邮箱服务器   
            var userName = ConfigurationManager.AppSettings["EmailAcount"];
            var password = ConfigurationManager.AppSettings["EmailPassword"];
            //string userName = "15764226619@163.com";// 发送端账号   
            //string password = "password";// 发送端密码(这个客户端重置后的密码)
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                return 0;
            }

            SmtpClient client = new SmtpClient();
            client.DeliveryMethod = SmtpDeliveryMethod.Network;//指定电子邮件发送方式    
            client.Host = host;//邮件服务器
            client.UseDefaultCredentials = true;
            client.Credentials = new System.Net.NetworkCredential(userName, password);//用户名、密码

            string strfrom = userName;
          
            //string strcc = "2605625733@qq.com";//抄送


            string subject = "找回密码";//邮件的主题             
         

            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
            msg.From = new MailAddress(strfrom, "RXXX");
            msg.To.Add(strto);
            //msg.CC.Add(strcc);

            msg.Subject = subject;//邮件标题   
            msg.Body = body;//邮件内容   
            msg.BodyEncoding = System.Text.Encoding.UTF8;//邮件内容编码   
            msg.IsBodyHtml = true;//是否是HTML邮件   
            msg.Priority = MailPriority.High;//邮件优先级   


            try
            {
                client.Send(msg);
                return 1;
            }
            catch (System.Net.Mail.SmtpException exception)
            {
                Log4NetHelper log = new Log4NetHelper();
                log.Error(exception.Message, exception);
                return 2;
            }
        }
    }
}