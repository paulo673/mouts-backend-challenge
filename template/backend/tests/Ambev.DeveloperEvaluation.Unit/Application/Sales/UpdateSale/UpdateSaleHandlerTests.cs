using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.UpdateSale.TestData;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.UpdateSale;

public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPublisher _publisher;
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _publisher = Substitute.For<IPublisher>();
        var logger = Substitute.For<ILogger<UpdateSaleHandler>>();
        _handler = new UpdateSaleHandler(_saleRepository, _publisher, logger);
    }

    [Fact(DisplayName = "Given valid sale data When updating sale Then returns success response")]
    public async Task Handle_ValidCommand_ReturnsSuccessResponse()
    {
        // Given
        var command = UpdateSaleHandlerTestData.GenerateValidCommand();
        ConfigureRepositoryWithExistingSale();

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result!.Id.Should().Be(UpdateSaleHandlerTestData.SaleId);
        result.SaleNumber.Should().Be(command.SaleNumber);
        result.TotalAmount.Should().Be(30m);
        result.UpdatedAt.Should().NotBeNull();
        await _saleRepository.Received(1).UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given valid sale data When updating sale Then replaces previous items and recalculates total")]
    public async Task Handle_ValidCommand_ReplacesItemsAndRecalculatesTotal()
    {
        // Given
        var command = UpdateSaleHandlerTestData.GenerateValidCommand();
        ConfigureRepositoryWithExistingSale();

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        await _saleRepository.Received(1).UpdateAsync(
            Arg.Is<Sale>(sale =>
                sale.Items.Count == 1 &&
                sale.Items[0].ProductId == UpdateSaleHandlerTestData.ProductAId &&
                sale.Items[0].Quantity == 3 &&
                sale.CustomerName == command.CustomerName &&
                sale.SaleNumber == command.SaleNumber),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given valid sale data When updating sale Then publishes SaleModified event for the persisted sale")]
    public async Task Handle_ValidCommand_PublishesSaleModifiedEvent()
    {
        // Given
        var command = UpdateSaleHandlerTestData.GenerateValidCommand();
        ConfigureRepositoryWithExistingSale();

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        await _publisher.Received(1).Publish(
            Arg.Is<SaleModifiedNotification>(n =>
                n.DomainEvent.Sale.Id == result!.Id &&
                n.DomainEvent.Sale.SaleNumber == command.SaleNumber),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a non-existent sale When updating sale Then returns null and does not persist")]
    public async Task Handle_SaleNotFound_ReturnsNullWithoutPersisting()
    {
        // Given
        var command = UpdateSaleHandlerTestData.GenerateValidCommand();
        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().BeNull();
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().Publish(Arg.Any<SaleModifiedNotification>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a sale that fails domain validation When updating sale Then no SaleModified event is published")]
    public async Task Handle_DomainFailure_DoesNotPublishSaleModifiedEvent()
    {
        // Given
        var command = UpdateSaleHandlerTestData.GenerateCommandWithDuplicateProductLinesOverLimit();
        ConfigureRepositoryWithExistingSale();

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<DomainException>();
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().Publish(Arg.Any<SaleModifiedNotification>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid sale data When executing through validation pipeline Then throws validation exception")]
    public async Task Handle_InvalidCommand_ThroughPipeline_ThrowsValidationException()
    {
        // Given
        var command = new UpdateSaleCommand();

        // When
        var act = () => ExecuteThroughValidationPipeline(command);

        // Then
        await act.Should().ThrowAsync<ValidationException>();
        await _saleRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given command with empty id When executing through validation pipeline Then throws validation exception")]
    public async Task Handle_EmptyId_ThroughPipeline_ThrowsValidationException()
    {
        // Given
        var command = UpdateSaleHandlerTestData.GenerateValidCommand();
        command.Id = Guid.Empty;

        // When
        var act = () => ExecuteThroughValidationPipeline(command);

        // Then
        await act.Should().ThrowAsync<ValidationException>();
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given duplicated product lines When updating sale Then aggregates quantities before discounting")]
    public async Task Handle_DuplicateProductLines_AggregatesQuantitiesBeforeDiscounting()
    {
        // Given
        var command = UpdateSaleHandlerTestData.GenerateCommandWithDuplicateProductLines();
        ConfigureRepositoryWithExistingSale();

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result!.TotalAmount.Should().Be(45m);
        await _saleRepository.Received(1).UpdateAsync(
            Arg.Is<Sale>(sale =>
                sale.Items.Count == 1 &&
                sale.Items[0].ProductId == UpdateSaleHandlerTestData.ProductAId &&
                sale.Items[0].Quantity == 5 &&
                sale.Items[0].Discount == 0.10m),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given sale with mixed item quantities When updating sale Then total amount is sum of discounted lines")]
    public async Task Handle_MixedQuantities_CalculatesTotalAmountCorrectly()
    {
        // Given
        var command = UpdateSaleHandlerTestData.GenerateCommandWithMixedQuantityItems();
        ConfigureRepositoryWithExistingSale();

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result!.TotalAmount.Should().Be(350m);
    }

    [Fact(DisplayName = "Given duplicated product lines over the item limit When updating sale Then throws domain exception")]
    public async Task Handle_DuplicateProductLinesOverLimit_ThrowsDomainException()
    {
        // Given
        var command = UpdateSaleHandlerTestData.GenerateCommandWithDuplicateProductLinesOverLimit();
        ConfigureRepositoryWithExistingSale();

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Cannot sell more than 20 identical items.");
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given duplicated product lines with different prices When updating sale Then throws domain exception")]
    public async Task Handle_DuplicateProductLinesWithDifferentPrices_ThrowsDomainException()
    {
        // Given
        var command = UpdateSaleHandlerTestData.GenerateCommandWithDuplicateProductDifferentPrice();
        ConfigureRepositoryWithExistingSale();

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Unit price must be consistent for the same product.");
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Theory(DisplayName = "Given quantity thresholds When updating sale Then discount tier is applied correctly")]
    [InlineData(3, 30)]
    [InlineData(4, 36)]
    [InlineData(10, 80)]
    [InlineData(20, 160)]
    public async Task Handle_QuantityThresholds_ApplyExpectedDiscounts(int quantity, decimal expectedTotal)
    {
        // Given
        var command = UpdateSaleHandlerTestData.GenerateCommandWithExactQuantity(quantity);
        ConfigureRepositoryWithExistingSale();

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result!.TotalAmount.Should().Be(expectedTotal);
    }

    private void ConfigureRepositoryWithExistingSale()
    {
        _saleRepository.GetByIdAsync(UpdateSaleHandlerTestData.SaleId, Arg.Any<CancellationToken>())
            .Returns(_ => UpdateSaleHandlerTestData.GenerateExistingSale());

        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<Sale>(0));
    }

    private Task<UpdateSaleResult?> ExecuteThroughValidationPipeline(UpdateSaleCommand command)
    {
        var behavior = new ValidationBehavior<UpdateSaleCommand, UpdateSaleResult?>(
            [new UpdateSaleCommandValidator()]);

        return behavior.Handle(
            command,
            () => _handler.Handle(command, CancellationToken.None),
            CancellationToken.None);
    }
}
