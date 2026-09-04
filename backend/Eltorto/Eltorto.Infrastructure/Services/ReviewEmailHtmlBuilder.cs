using System.Globalization;
using System.Net;
using System.Text;
using Eltorto.Application.DTOs;

namespace Eltorto.Infrastructure.Services;

public static class ReviewEmailHtmlBuilder
{
    public static string BuildReviewCreatedHtml(TestimonialDto t)
    {
        string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

        var builder = new StringBuilder();
        builder.Append("<!DOCTYPE html><html lang=\"ru\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><style>");
        builder.Append("@media only screen and (max-width:600px){.review-table{width:100%!important}.review-table td,.review-table tr{display:block;width:100%!important;box-sizing:border-box}.review-table td.review-label{padding-bottom:0!important}.review-table td.review-value{padding-top:2px!important}.review-wrapper{padding:12px!important}}");
        builder.Append("</style></head>");
        builder.Append("<body style=\"margin:0;padding:0;background-color:#f5f3ee;font-family:Arial,Helvetica,sans-serif;\">");
        builder.Append("<div class=\"review-wrapper\" style=\"width:100%;max-width:600px;margin:0 auto;padding:16px;box-sizing:border-box;\">");
        builder.Append("<h2 style=\"color:#4b2e1e;margin:0 0 16px;\">Новый отзыв ждёт модерации</h2>");
        builder.Append("<table class=\"review-table\" style=\"width:100%;table-layout:fixed;border-collapse:collapse;background:#ffffff;border-radius:8px;overflow:hidden;border:1px solid #eee;\">");
        AppendRow(builder, "Автор", Encode(t.Author));
        AppendRow(builder, "Дата", t.Date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
        AppendRow(builder, "Оценка", FormatRating(t.Rating));
        AppendRow(builder, "Отзыв", Encode(t.Text));
        builder.Append("</table>");
        builder.Append("</div></body></html>");

        return builder.ToString();
    }

    private static string FormatRating(int? rating)
    {
        if (!rating.HasValue)
        {
            return "—";
        }

        var stars = new string('★', rating.Value) + new string('☆', 5 - rating.Value);
        return $"<span style=\"color:#f5b301;font-size:18px;\">{stars}</span>";
    }

    private static void AppendRow(StringBuilder builder, string label, string value)
    {
        var display = string.IsNullOrWhiteSpace(value) ? "—" : value;
        builder.Append("<tr><td class=\"review-label\" style=\"width:30%;padding:10px 14px;border-bottom:1px solid #eee;color:#6b6b6b;background:#faf8f4;vertical-align:top;word-break:break-word;\">")
            .Append(label)
            .Append("</td><td class=\"review-value\" style=\"padding:10px 14px;border-bottom:1px solid #eee;color:#333;vertical-align:top;word-break:break-word;\">")
            .Append(display)
            .Append("</td></tr>");
    }
}
