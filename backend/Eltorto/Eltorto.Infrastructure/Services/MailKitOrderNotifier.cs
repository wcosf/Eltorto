using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Eltorto.Infrastructure.Services;

public sealed class MailKitOrderNotifier : IOrderNotifier
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MailKitOrderNotifier> _logger;

    public MailKitOrderNotifier(IConfiguration configuration, ILogger<MailKitOrderNotifier> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOrderCreatedAsync(OrderDto order, string toEmail, IReadOnlyList<string> imageUrls, CancellationToken cancellationToken = default)
    {
        var host = _configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogInformation("[EMAIL] SMTP is not configured, skipping notification for order {OrderId}", order.Id);
            return;
        }

        var port = int.TryParse(_configuration["Smtp:Port"], out var parsedPort) ? parsedPort : 587;
        var userName = _configuration["Smtp:UserName"];
        var password = _configuration["Smtp:Password"];
        var from = string.IsNullOrWhiteSpace(_configuration["Smtp:From"])
            ? "no-reply@eltorto.ru"
            : _configuration["Smtp:From"]!;

        var body = OrderEmailHtmlBuilder.BuildOrderCreatedHtml(order, imageUrls);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Eltorto", from));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = $"Новый заказ №{order.Id}";
        message.Body = new TextPart("html") { Text = body };

        using var client = new MailKit.Net.Smtp.SmtpClient { Timeout = 15_000 };

        var socketOptions = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

        await client.ConnectAsync(host, port, socketOptions, cancellationToken);

        if (!string.IsNullOrEmpty(userName))
        {
            await client.AuthenticateAsync(userName, password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("[EMAIL] Order notification sent for order {OrderId} to {ToEmail}", order.Id, toEmail);
    }
}
