using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.GetSale.TestData;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.GetSale;

public class GetSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<GetSaleProfile>()).CreateMapper();
        _handler = new GetSaleHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given an existing sale id When getting sale Then returns mapped result with items")]
    public async Task Handle_ExistingSale_ReturnsMappedResult()
    {
        var query = GetSaleHandlerTestData.BuildQuery();
        var sale = GetSaleHandlerTestData.BuildSale();
        _saleRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(sale.Id);
        result.SaleNumber.Should().Be(sale.SaleNumber);
        result.CustomerName.Should().Be(sale.CustomerName);
        result.BranchName.Should().Be(sale.BranchName);
        result.TotalAmount.Should().Be(sale.TotalAmount);
        result.Items.Should().HaveCount(sale.Items.Count);
        result.Items[0].ProductId.Should().Be(GetSaleHandlerTestData.ProductAId);
        result.Items[0].Discount.Should().Be(0.10m);
    }

    [Fact(DisplayName = "Given a non-existing sale id When getting sale Then returns null")]
    public async Task Handle_NonExistingSale_ReturnsNull()
    {
        var query = GetSaleHandlerTestData.BuildQuery();
        _saleRepository.GetByIdAsync(query.Id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact(DisplayName = "Given an empty id When executing through validation pipeline Then throws validation exception")]
    public async Task Handle_EmptyId_ThroughPipeline_ThrowsValidationException()
    {
        var query = new GetSaleQuery(Guid.Empty);

        var act = () => ExecuteThroughValidationPipeline(query);

        await act.Should().ThrowAsync<ValidationException>();
        await _saleRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    private Task<GetSaleResult?> ExecuteThroughValidationPipeline(GetSaleQuery query)
    {
        var behavior = new ValidationBehavior<GetSaleQuery, GetSaleResult?>([new GetSaleValidator()]);

        return behavior.Handle(
            query,
            () => _handler.Handle(query, CancellationToken.None),
            CancellationToken.None);
    }
}
