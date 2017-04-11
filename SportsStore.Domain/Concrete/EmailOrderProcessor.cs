using SportsStore.Domain.Abstract;
using SportsStore.Domain.Entities;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace SportsStore.Domain.Concrete
{
    public class EmailSettings
    {
        public string MailToAddress = "order@example.com";
        public string MailFromAddress = "sportsstore@example.com";
        public bool UseSsl = true;
        public string Username = "MySmtpUsername";
        public string Password = "MyStmpPassword";
        public string ServerName = "smtp.example.com";
        public int ServerPort = 587;
        public bool WriteAsFile = false;
        public string FileLocation = @"C:\sports_store_emails";
    }

    public class EmailOrderProcessor : IOderProcessor
    {
        private EmailSettings emailSettings;

        public EmailOrderProcessor(EmailSettings settings)
        {
            emailSettings = settings;
        }

        public void ProcessOrder(Cart cart, ShippingDetails shippingInfo)
        {
            using (var stmpClient = new SmtpClient())
            {
                stmpClient.EnableSsl = emailSettings.UseSsl;
                stmpClient.Host = emailSettings.ServerName;
                stmpClient.Port = emailSettings.ServerPort;
                stmpClient.UseDefaultCredentials = false;
                stmpClient.Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password);

                if (emailSettings.WriteAsFile)
                {
                    if(!Directory.Exists(emailSettings.FileLocation))
                    {
                        Directory.CreateDirectory(emailSettings.FileLocation);
                    }
                    stmpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    stmpClient.PickupDirectoryLocation = emailSettings.FileLocation;
                    stmpClient.EnableSsl = false;
                }

                StringBuilder body = new StringBuilder()
                    .AppendLine("A new order has been submitted")
                    .AppendLine("---")
                    .AppendLine("Items:");
                foreach (var line in cart.Lines)
                {
                    var subtotal = line.Product.Price * line.Quantity;
                    body.AppendLine($"{line.Quantity} x {line.Product.Price} (subtotal: {subtotal.ToString("c")}");
                }
                body.AppendLine($"Total order value: {cart.ComputeTotalValue().ToString("c")}")
                    .AppendLine("---")
                    .AppendLine("Ship to:")
                    .AppendLine(shippingInfo.Name)
                    .AppendLine(shippingInfo.Line1)
                    .AppendLine(shippingInfo.Line2)
                    .AppendLine(shippingInfo.Line3)
                    .AppendLine(shippingInfo.City)
                    .AppendLine(shippingInfo.State)
                    .AppendLine(shippingInfo.Country)
                    .AppendLine(shippingInfo.Zip)
                    .AppendLine("---")
                    .AppendLine($"Gift wrap: {(shippingInfo.GiftWrap ? "Yes" : "No")}");

                MailMessage mailMessage = new MailMessage(
                    emailSettings.MailFromAddress, // From
                    emailSettings.MailToAddress, // To
                    "New order submitted!", // Subject
                    body.ToString()); // Body

                if (emailSettings.WriteAsFile)
                {
                    mailMessage.BodyEncoding = Encoding.ASCII;
                }
                stmpClient.Send(mailMessage);
            }
        }
    }
}