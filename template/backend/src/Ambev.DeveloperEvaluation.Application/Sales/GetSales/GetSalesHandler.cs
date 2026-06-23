using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

public class GetSalesHandler : IRequestHandler<GetSalesQuery, GetSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public GetSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<GetSalesResult> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var (sales, totalCount) = await _saleRepository.GetPagedAsync(request.Page, request.Size, request.Order, cancellationToken);

        return new GetSalesResult
        {
            Items = _mapper.Map<List<GetSalesSaleResult>>(sales),
            TotalCount = totalCount,
            CurrentPage = request.Page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.Size)
        };
    }
}
