using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QuestFlag.Passport.Domain.Entities;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Application.Features.Users.Queries;

/// <summary>Returns all active trusted devices for a user.</summary>
public record GetTrustedDevicesQuery(Guid UserId) : IRequest<IReadOnlyList<TrustedDevice>>;

public class GetTrustedDevicesQueryHandler : IRequestHandler<GetTrustedDevicesQuery, IReadOnlyList<TrustedDevice>>
{
    private readonly ITrustedDeviceRepository _deviceRepo;

    public GetTrustedDevicesQueryHandler(ITrustedDeviceRepository deviceRepo) => _deviceRepo = deviceRepo;

    public Task<IReadOnlyList<TrustedDevice>> Handle(GetTrustedDevicesQuery request, CancellationToken cancellationToken)
        => _deviceRepo.GetByUserIdAsync(request.UserId, cancellationToken);
}
