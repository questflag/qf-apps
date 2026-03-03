namespace QuestFlag.Communication.Client.DTOs;

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
