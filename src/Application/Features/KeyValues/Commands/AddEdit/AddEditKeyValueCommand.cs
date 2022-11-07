// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CleanArchitecture.Blazor.Application.Features.KeyValues.Caching;
using CleanArchitecture.Blazor.Application.Features.KeyValues.DTOs;

namespace CleanArchitecture.Blazor.Application.Features.KeyValues.Commands.AddEdit;

public class AddEditKeyValueCommand : IMapFrom<KeyValueDto>, IRequest<Result<int>>, ICacheInvalidator
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Value { get; set; }
    public string? Text { get; set; }
    public string? Description { get; set; }
    public TrackingState TrackingState { get; set; } = TrackingState.Unchanged;
    public string CacheKey => KeyValueCacheKey.GetAllCacheKey;
    public CancellationTokenSource? SharedExpiryTokenSource => KeyValueCacheKey.SharedExpiryTokenSource();
}

public class AddEditKeyValueCommandHandler : IRequestHandler<AddEditKeyValueCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AddEditKeyValueCommandHandler(
        IApplicationDbContext context,
         IMapper mapper
        )
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<Result<int>> Handle(AddEditKeyValueCommand request, CancellationToken cancellationToken)
    {
        var dto = _mapper.Map<KeyValueDto>(request);

        if (request.Id > 0)
        {
            var keyValue = await _context.KeyValues.FindAsync(new object[] { request.Id }, cancellationToken);
            _ = keyValue ?? throw new NotFoundException($"KeyValue Pair  {request.Id} Not Found.");
            keyValue = _mapper.Map(dto, keyValue);
            keyValue.AddDomainEvent(new UpdatedEvent<KeyValue>(keyValue));
            await _context.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(keyValue.Id);
        }
        else
        {
            var keyValue = _mapper.Map<KeyValue>(dto);
            keyValue.AddDomainEvent(new UpdatedEvent<KeyValue>(keyValue));
            _context.KeyValues.Add(keyValue);
            await _context.SaveChangesAsync(cancellationToken);
            return await Result<int>.SuccessAsync(keyValue.Id);
        }


    }
}
