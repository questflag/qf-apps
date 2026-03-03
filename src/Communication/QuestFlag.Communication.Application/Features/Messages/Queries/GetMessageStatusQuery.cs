using MediatR;
using QuestFlag.Communication.Domain.DTOs;

namespace QuestFlag.Communication.Application.Features.Messages.Queries;

public record GetMessageStatusQuery(string TransactionId) : IRequest<MessageStatusDto?>;
