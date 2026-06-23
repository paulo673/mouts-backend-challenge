using Ambev.DeveloperEvaluation.Application.Auth.AuthenticateUser;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Application.Users.CreateUser;
using Ambev.DeveloperEvaluation.Application.Users.DeleteUser;
using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using Ambev.DeveloperEvaluation.Common.Security;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

public class ApplicationModuleInitializer : IModuleInitializer
{
    public void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        builder.Services.AddTransient<IValidator<AuthenticateUserCommand>, AuthenticateUserValidator>();
        builder.Services.AddTransient<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
        builder.Services.AddTransient<IValidator<GetUserCommand>, GetUserValidator>();
        builder.Services.AddTransient<IValidator<DeleteUserCommand>, DeleteUserValidator>();
        builder.Services.AddTransient<IValidator<CreateSaleCommand>, CreateSaleCommandValidator>();
        builder.Services.AddTransient<IValidator<CancelSaleCommand>, CancelSaleValidator>();
        builder.Services.AddTransient<IValidator<CancelSaleItemCommand>, CancelSaleItemValidator>();
        builder.Services.AddTransient<IValidator<GetSaleQuery>, GetSaleValidator>();
        builder.Services.AddTransient<IValidator<GetSalesQuery>, GetSalesValidator>();
        builder.Services.AddTransient<IValidator<UpdateSaleCommand>, UpdateSaleCommandValidator>();
    }
}
