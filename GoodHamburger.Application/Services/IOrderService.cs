using GoodHamburger.Application.Contracts;

namespace GoodHamburger.Application.Services;

public interface IOrderService
{
    Task<IReadOnlyCollection<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateAsync(IReadOnlyCollection<int> itemIds, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateAsync(int id, IReadOnlyCollection<int> itemIds, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
