using FluentValidation;
using QuestFlag.Infrastructure.Application.Features.Uploads.Commands;

namespace QuestFlag.Infrastructure.Application.Features.Uploads;

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator()
    {
        RuleFor(x => x.OriginalFileName).NotEmpty().WithMessage("Filename is required.");
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TenantSlug).NotEmpty();
        RuleFor(x => x.Category).NotEmpty().WithMessage("Category is required.");
        RuleFor(x => x.TaskName).NotEmpty().WithMessage("Task/App name is required.");
        RuleFor(x => x.FileStream).NotNull().WithMessage("File stream cannot be null.");
        RuleFor(x => x.SizeInBytes).GreaterThan(0).WithMessage("File size must be greater than 0.");
    }
}
