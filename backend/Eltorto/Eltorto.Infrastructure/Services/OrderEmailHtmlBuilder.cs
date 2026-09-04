using System.Globalization;
using System.Net;
using System.Text;
using Eltorto.Application.DTOs;

namespace Eltorto.Infrastructure.Services;

public static class OrderEmailHtmlBuilder
{
    public static string BuildOrderCreatedHtml(OrderDto order, IReadOnlyList<string> imageUrls)
    {
        string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

        var builder = new StringBuilder();
        builder.Append("<!DOCTYPE html><html lang=\"ru\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><style>");
        builder.Append("@media only screen and (max-width:600px){.order-table{width:100%!important}.order-table td,.order-table tr{display:block;width:100%!important;box-sizing:border-box}.order-photos img{width:100%!important;height:auto!important;max-width:100%!important;display:block;margin:0 0 8px!important}.order-table td.order-label{padding-bottom:0!important}.order-table td.order-value{padding-top:2px!important}.order-wrapper{padding:12px!important}}");
        builder.Append("</style></head>");
        builder.Append("<body style=\"margin:0;padding:0;background-color:#f5f3ee;font-family:Arial,Helvetica,sans-serif;\">");
        builder.Append("<div class=\"order-wrapper\" style=\"width:100%;max-width:600px;margin:0 auto;padding:16px;box-sizing:border-box;\">");
        builder.Append("<h2 style=\"color:#4b2e1e;margin:0 0 16px;\">Новый заказ №").Append(order.Id).Append("</h2>");

        if (imageUrls.Count > 0)
        {
            builder.Append("<div class=\"order-photos\" style=\"margin-bottom:16px;\">");
            foreach (var url in imageUrls)
            {
                builder.Append("<img src=\"").Append(Encode(url))
                    .Append("\" alt=\"Фото заказа\" style=\"max-width:100%;height:auto;width:220px;border-radius:8px;margin:0 8px 8px 0;\">");
            }
            builder.Append("</div>");
        }

        builder.Append("<table class=\"order-table\" style=\"width:100%;table-layout:fixed;border-collapse:collapse;background:#ffffff;border-radius:8px;overflow:hidden;border:1px solid #eee;\">");
        AppendRow(builder, "Имя", Encode(order.CustomerName));
        AppendRow(builder, "Телефон", Encode(order.CustomerPhone));
        AppendRow(builder, "Email", Encode(order.CustomerEmail));
        AppendRow(builder, "Торт", !string.IsNullOrWhiteSpace(order.CakeName) ? Encode(order.CakeName) : "Свой дизайн");
        AppendRow(builder, "Описание дизайна", Encode(order.CustomCakeDescription));
        AppendRow(builder, "Начинка", Encode(order.FillingName));
        AppendRow(builder, "Вес, кг", order.Weight.HasValue ? order.Weight.Value.ToString("0.##", CultureInfo.InvariantCulture) : "—");
        AppendRow(builder, "Дата доставки", order.DeliveryDate.HasValue ? order.DeliveryDate.Value.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture) : "—");
        AppendRow(builder, "Адрес доставки", Encode(order.DeliveryAddress));
        AppendRow(builder, "Комментарий", Encode(order.Comment));
        AppendRow(builder, "Статус", Encode(order.Status));
        builder.Append("</table>");
        builder.Append("</div></body></html>");

        return builder.ToString();
    }

    private static void AppendRow(StringBuilder builder, string label, string? value)
    {
        var display = string.IsNullOrWhiteSpace(value) ? "—" : value;
        builder.Append("<tr><td class=\"order-label\" style=\"width:30%;padding:10px 14px;border-bottom:1px solid #eee;color:#6b6b6b;background:#faf8f4;vertical-align:top;word-break:break-word;\">")
            .Append(label)
            .Append("</td><td class=\"order-value\" style=\"padding:10px 14px;border-bottom:1px solid #eee;color:#333;vertical-align:top;word-break:break-word;\">")
            .Append(display)
            .Append("</td></tr>");
    }
}