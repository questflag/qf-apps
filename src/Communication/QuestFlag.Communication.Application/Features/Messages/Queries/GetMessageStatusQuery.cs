using MediatR;
using QuestFlag.Communication.Application.Common.DTOs;

namespace QuestFlag.Communication.Application.Features.Messages.Queries;

public record GetMessageStatusQuery(string TransactionId) : IRequest<MessageStatusDto?>;
