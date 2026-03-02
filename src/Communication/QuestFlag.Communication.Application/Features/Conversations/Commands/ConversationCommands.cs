using MediatR;

namespace QuestFlag.Communication.Application.Features.Conversations.Commands;

public record ProcessInboundWebhookCommand(string Recipient, string Content, Guid TenantId) : IRequest<Unit>;
public record CloseConversationCommand(Guid ConversationId) : IRequest<Unit>;
