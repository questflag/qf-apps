using MediatR;
using QuestFlag.Communication.Shared.DTOs;

namespace QuestFlag.Communication.Application.Features.Messages.Queries;

public record GetMessageStatusQuery(string TransactionId) : IRequest<MessageStatusDto?>;
