using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace UserManagement.Services;

public class EmailSender : IEmailSender
{
    
    private string? _host;
    private int _port;
    private bool _enableSsl;
    private string? _userName;
    private string? _password;
    
    public EmailSender(string? host, int port, bool enableSSL, string? userName, string? password) {
        this._host = host;
        this._port = port;
        this._enableSsl = enableSSL;
        this._userName = userName;
        this._password = password;
    }
    
    public Task SendEmailAsync(string email, string subject, string htmlMessage) {
        var client = new SmtpClient(_host, _port) {
            Credentials = new NetworkCredential(_userName, _password),
            EnableSsl = _enableSsl
        };
        return client.SendMailAsync(
            new MailMessage("grzegorzduraj2001@gmail.com", email, subject, htmlMessage) { IsBodyHtml = true }
        );
    }
}