using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.CancelSale.TestData;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.CancelSale;

public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPublisher _publisher;
    private readonly CancelSaleHandler _handler;

    public CancelSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _publisher = Substitute.For<IPublisher>();
        var logger = Substitute.For<ILogger<CancelSaleHandler>>();
        _handler = new CancelSaleHandler(_saleRepository, _publisher, logger);
    }

    [Fact(DisplayName = "Given existing active sale When cancelling sale Then cancels and persists sale")]
    public async Task Handle_ExistingActiveSale_CancelsAndPersistsSale()
    {
        var command = CancelSaleHandlerTestData.GenerateValidCommand();
        ConfigureRepositoryWithExistingSale();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(CancelSaleHandlerTestData.SaleId);
        result.IsCancelled.Should().BeTrue();
        result.TotalAmount.Should().Be(0m);
        result.UpdatedAt.Should().NotBeNull();

        await _saleRepository.Received(1).UpdateAsync(
            Arg.Is<Sale>(sale =>
                sale.Id == command.Id &&
                sale.IsCancelled &&
                sale.TotalAmount == 0m &&
                sale.Items.All(item => item.IsCancelled)),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given existing active sale When cancelling sale Then publishes SaleCancelled event")]
    public async Task Handle_ExistingActiveSale_PublishesSaleCancelledEvent()
    {
        var command = CancelSaleHandlerTestData.GenerateValidCommand();
        ConfigureRepositoryWithExistingSale();

        var result = await _handler.Handle(command, CancellationToken.None);

        await _publisher.Received(1).Publish(
            Arg.Is<SaleCancelledNotification>(n =>
                n.DomainEvent.Sale.Id == result.Id &&
                n.DomainEvent.Sale.IsCancelled),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existent sale When cancelling sale Then throws key not found exception")]
    public async Task Handle_SaleNotFound_ThrowsKeyNotFoundException()
    {
        var command = CancelSaleHandlerTestData.GenerateValidCommand();
        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Sale with ID {command.Id} not found");
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().Publish(Arg.Any<SaleCancelledNotification>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given cancelled sale When cancelling sale Then throws domain exception")]
    public async Task Handle_AlreadyCancelledSale_ThrowsDomainException()
    {
        var command = CancelSaleHandlerTestData.GenerateValidCommand();
        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>())
            .Returns(_ => CancelSaleHandlerTestData.GenerateCancelledSale());

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Sale is already cancelled.");
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().Publish(Arg.Any<SaleCancelledNotification>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid command When executing through validation pipeline Then throws validation exception")]
    public async Task Handle_InvalidCommand_ThroughPipeline_ThrowsValidationException()
    {
        var command = new CancelSaleCommand();

        var act = () => ExecuteThroughValidationPipeline(command);

        await act.Should().ThrowAsync<ValidationException>();
        await _saleRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    private void ConfigureRepositoryWithExistingSale()
    {
        _saleRepository.GetByIdAsync(CancelSaleHandlerTestData.SaleId, Arg.Any<CancellationToken>())
            .Returns(_ => CancelSaleHandlerTestData.GenerateExistingSale());

        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<Sale>(0));
    }

    private Task<CancelSaleResult> ExecuteThroughValidationPipeline(CancelSaleCommand command)
    {
        var behavior = new ValidationBehavior<CancelSaleCommand, CancelSaleResult>(
            [new CancelSaleValidator()]);

        return behavior.Handle(
            command,
            () => _handler.Handle(command, CancellationToken.None),
            CancellationToken.None);
    }
}
