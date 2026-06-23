using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.GetSales.TestData;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.GetSales;

public class GetSalesHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetSalesHandler _handler;

    public GetSalesHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<GetSalesProfile>()).CreateMapper();
        _handler = new GetSalesHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given sales exist When getting paged Then returns items and pagination metadata")]
    public async Task Handle_SalesExist_ReturnsItemsAndPaginationMetadata()
    {
        var query = GetSalesHandlerTestData.BuildQuery(page: 2, size: 10);
        var sales = GetSalesHandlerTestData.BuildSales(10);
        _saleRepository
            .GetPagedAsync(2, 10, null, Arg.Any<CancellationToken>())
            .Returns((sales, 25));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.CurrentPage.Should().Be(2);
        result.TotalPages.Should().Be(3);
    }

    [Fact(DisplayName = "Given page size and order When getting paged Then forwards arguments to repository")]
    public async Task Handle_ForwardsArgumentsToRepository()
    {
        var query = GetSalesHandlerTestData.BuildQuery(page: 2, size: 10, order: "saleDate desc");
        _saleRepository
            .GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Sale>(), 0));

        await _handler.Handle(query, CancellationToken.None);

        await _saleRepository.Received(1).GetPagedAsync(2, 10, "saleDate desc", Arg.Any<CancellationToken>());
    }

    [Theory(DisplayName = "Given invalid pagination When executing through validation pipeline Then throws validation exception")]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task Handle_InvalidPagination_ThroughPipeline_ThrowsValidationException(int page, int size)
    {
        var query = GetSalesHandlerTestData.BuildQuery(page, size);

        var act = () => ExecuteThroughValidationPipeline(query);

        await act.Should().ThrowAsync<ValidationException>();
        await _saleRepository.DidNotReceive()
            .GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    private Task<GetSalesResult> ExecuteThroughValidationPipeline(GetSalesQuery query)
    {
        var behavior = new ValidationBehavior<GetSalesQuery, GetSalesResult>([new GetSalesValidator()]);

        return behavior.Handle(
            query,
            () => _handler.Handle(query, CancellationToken.None),
            CancellationToken.None);
    }
}
