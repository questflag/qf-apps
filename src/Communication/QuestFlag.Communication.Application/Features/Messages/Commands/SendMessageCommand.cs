using MediatR;
using QuestFlag.Communication.Shared.DTOs;

namespace QuestFlag.Communication.Application.Features.Messages.Commands;

public record SendMessageCommand(SendMessageDto Message) : IRequest<string>; // Returns transaction ID
