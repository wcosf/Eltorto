using Eltorto.Application.DTOs;

namespace Eltorto.Application.Interfaces.Services;

public interface IReviewNotifier
{
    Task SendReviewCreatedAsync(TestimonialDto testimonial, string toEmail, CancellationToken cancellationToken = default);
}
