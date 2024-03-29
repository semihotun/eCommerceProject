﻿using AdminIdentityService.Domain.AggregateModels;
using AdminIdentityService.Domain.Result;
using AdminIdentityService.Insfrastructure.Utilities.Caching.Redis;
using AdminIdentityService.Persistence.GenericRepository;
using AdminIdentityService.Persistence.UnitOfWork;
using MediatR;
namespace AdminIdentityService.Application.Handlers.AdminRoles.Commands.AdminRoleRequests;

public record AdminRoleCommand(string[] RolePath) : IRequest<Result>;

public class AdminRoleCommandHandler(IRepository<AdminRole> adminRoleRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<AdminRoleCommand, Result>
{
    private readonly IRepository<AdminRole> _adminRoleRepository = adminRoleRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<Result> Handle(AdminRoleCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.BeginTransaction<Result>(async () =>
        {
            var newProducts = request.RolePath
                       .Where(newAdminRole => !_adminRoleRepository.Any(existingAdminRole => existingAdminRole.Role == newAdminRole))
                       .Select(x => new AdminRole(x));
            if (newProducts != null)
            {
                await _adminRoleRepository.AddRangeAsync(newProducts);
            }
            await _cacheService.RemovePatternAsync("AdminIdentityService:AdminRoles");
            return new SuccessResult();
        });
    }
}