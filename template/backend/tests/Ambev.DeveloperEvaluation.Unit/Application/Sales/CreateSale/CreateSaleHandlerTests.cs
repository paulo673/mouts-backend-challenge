using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.CreateSale.TestData;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.CreateSale;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPublisher _publisher;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _publisher = Substitute.For<IPublisher>();
        var logger = Substitute.For<ILogger<CreateSaleHandler>>();
        _handler = new CreateSaleHandler(_saleRepository, _publisher, logger);
    }

    [Fact(DisplayName = "Given valid sale data When creating sale Then returns success response")]
    public async Task Handle_ValidCommand_ReturnsSuccessResponse()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        ConfigureRepositoryToReturnPersistedSale();

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.SaleNumber.Should().Be(command.SaleNumber);
        result.TotalAmount.Should().Be(30m);
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given valid sale data When creating sale Then publishes SaleCreated event for the persisted sale")]
    public async Task Handle_ValidCommand_PublishesSaleCreatedEvent()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        ConfigureRepositoryToReturnPersistedSale();

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        await _publisher.Received(1).Publish(
            Arg.Is<SaleCreatedNotification>(n =>
                n.DomainEvent.Sale.Id == result.Id &&
                n.DomainEvent.Sale.SaleNumber == command.SaleNumber),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given a sale that fails domain validation When creating sale Then no SaleCreated event is published")]
    public async Task Handle_DomainFailure_DoesNotPublishSaleCreatedEvent()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateCommandWithDuplicateProductLinesOverLimit();

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<DomainException>();
        await _publisher.DidNotReceive().Publish(Arg.Any<SaleCreatedNotification>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid sale data When executing through validation pipeline Then throws validation exception")]
    public async Task Handle_InvalidCommand_ThroughPipeline_ThrowsValidationException()
    {
        // Given
        var command = new CreateSaleCommand();

        // When
        var act = () => ExecuteThroughValidationPipeline(command);

        // Then
        await act.Should().ThrowAsync<ValidationException>();
        await _saleRepository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given duplicated product lines When creating sale Then aggregates quantities before discounting")]
    public async Task Handle_DuplicateProductLines_AggregatesQuantitiesBeforeDiscounting()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateCommandWithDuplicateProductLines();
        ConfigureRepositoryToReturnPersistedSale();

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.TotalAmount.Should().Be(45m);
        await _saleRepository.Received(1).CreateAsync(
            Arg.Is<Sale>(sale =>
                sale.Items.Count == 1 &&
                sale.Items[0].ProductId == CreateSaleHandlerTestData.ProductAId &&
                sale.Items[0].Quantity == 5 &&
                sale.Items[0].Discount == 0.10m),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given sale with mixed item quantities When creating sale Then total amount is sum of discounted lines")]
    public async Task Handle_MixedQuantities_CalculatesTotalAmountCorrectly()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateCommandWithMixedQuantityItems();
        ConfigureRepositoryToReturnPersistedSale();

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.TotalAmount.Should().Be(350m);
    }

    [Fact(DisplayName = "Given duplicated product lines over the item limit When creating sale Then throws domain exception")]
    public async Task Handle_DuplicateProductLinesOverLimit_ThrowsDomainException()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateCommandWithDuplicateProductLinesOverLimit();

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Cannot sell more than 20 identical items.");
        await _saleRepository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given duplicated product lines with different prices When creating sale Then throws domain exception")]
    public async Task Handle_DuplicateProductLinesWithDifferentPrices_ThrowsDomainException()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateCommandWithDuplicateProductDifferentPrice();

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Unit price must be consistent for the same product.");
        await _saleRepository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Theory(DisplayName = "Given quantity thresholds When creating sale Then discount tier is applied correctly")]
    [InlineData(3, 30)]
    [InlineData(4, 36)]
    [InlineData(10, 80)]
    [InlineData(20, 160)]
    public async Task Handle_QuantityThresholds_ApplyExpectedDiscounts(int quantity, decimal expectedTotal)
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateCommandWithExactQuantity(quantity);
        ConfigureRepositoryToReturnPersistedSale();

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.TotalAmount.Should().Be(expectedTotal);
    }

    [Fact(DisplayName = "Given item with missing product id When executing through validation pipeline Then throws validation exception")]
    public async Task Handle_InvalidItem_ThroughPipeline_ThrowsValidationException()
    {
        // Given
        var command = CreateSaleHandlerTestData.BuildCommand(
            CreateSaleHandlerTestData.BuildItem(Guid.Empty, "Product A", 1, 10m));

        // When
        var act = () => ExecuteThroughValidationPipeline(command);

        // Then
        await act.Should().ThrowAsync<ValidationException>();
        await _saleRepository.DidNotReceive().CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    private void ConfigureRepositoryToReturnPersistedSale()
    {
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var sale = callInfo.ArgAt<Sale>(0);
                if (sale.Id == Guid.Empty)
                    sale.Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
                return sale;
            });
    }

    private Task<CreateSaleResult> ExecuteThroughValidationPipeline(CreateSaleCommand command)
    {
        var behavior = new ValidationBehavior<CreateSaleCommand, CreateSaleResult>(
            [new CreateSaleCommandValidator()]);

        return behavior.Handle(
            command,
            () => _handler.Handle(command, CancellationToken.None),
            CancellationToken.None);
    }
}
