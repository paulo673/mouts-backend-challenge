using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.CancelSaleItem.TestData;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        var logger = Substitute.For<ILogger<CancelSaleItemHandler>>();
        _handler = new CancelSaleItemHandler(_saleRepository, logger);
    }

    [Fact(DisplayName = "Given existing sale and active item When cancelling item Then cancels and persists sale")]
    public async Task Handle_ExistingSaleAndActiveItem_CancelsAndPersistsSale()
    {
        var command = CancelSaleItemHandlerTestData.GenerateValidCommand();
        ConfigureRepositoryWithExistingSale();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.SaleId.Should().Be(CancelSaleItemHandlerTestData.SaleId);
        result.ItemId.Should().Be(CancelSaleItemHandlerTestData.ItemAId);
        result.IsCancelled.Should().BeTrue();
        result.TotalAmount.Should().Be(20m);
        result.UpdatedAt.Should().NotBeNull();

        await _saleRepository.Received(1).UpdateAsync(
            Arg.Is<Sale>(sale =>
                sale.Id == command.SaleId &&
                sale.TotalAmount == 20m &&
                sale.Items.Single(item => item.Id == command.ItemId).IsCancelled),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existent sale When cancelling item Then throws key not found exception")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFoundException()
    {
        var command = CancelSaleItemHandlerTestData.GenerateValidCommand();
        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Sale with ID {command.SaleId} not found");
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given existing sale and non-existent item When cancelling item Then throws domain exception")]
    public async Task Handle_ItemNotFound_ThrowsDomainException()
    {
        var command = CancelSaleItemHandlerTestData.GenerateCommandWithUnknownItem();
        ConfigureRepositoryWithExistingSale();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Sale item not found.");
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given existing sale and cancelled item When cancelling item Then throws domain exception")]
    public async Task Handle_ItemAlreadyCancelled_ThrowsDomainException()
    {
        var command = CancelSaleItemHandlerTestData.GenerateValidCommand();
        _saleRepository.GetByIdAsync(command.SaleId, Arg.Any<CancellationToken>())
            .Returns(_ => CancelSaleItemHandlerTestData.GenerateSaleWithCancelledItem());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Sale item is already cancelled.");
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid sale id When executing through validation pipeline Then throws validation exception")]
    public async Task Handle_InvalidSaleId_ThroughPipeline_ThrowsValidationException()
    {
        var command = CancelSaleItemHandlerTestData.GenerateCommandWithInvalidSaleId();

        var act = () => ExecuteThroughValidationPipeline(command);

        await act.Should().ThrowAsync<ValidationException>();
        await _saleRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid item id When executing through validation pipeline Then throws validation exception")]
    public async Task Handle_InvalidItemId_ThroughPipeline_ThrowsValidationException()
    {
        var command = CancelSaleItemHandlerTestData.GenerateCommandWithInvalidItemId();

        var act = () => ExecuteThroughValidationPipeline(command);

        await act.Should().ThrowAsync<ValidationException>();
        await _saleRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    private void ConfigureRepositoryWithExistingSale()
    {
        _saleRepository.GetByIdAsync(CancelSaleItemHandlerTestData.SaleId, Arg.Any<CancellationToken>())
            .Returns(_ => CancelSaleItemHandlerTestData.GenerateExistingSale());

        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<Sale>(0));
    }

    private Task<CancelSaleItemResult> ExecuteThroughValidationPipeline(CancelSaleItemCommand command)
    {
        var behavior = new ValidationBehavior<CancelSaleItemCommand, CancelSaleItemResult>(
            [new CancelSaleItemValidator()]);

        return behavior.Handle(
            command,
            () => _handler.Handle(command, CancellationToken.None),
            CancellationToken.None);
    }
}
