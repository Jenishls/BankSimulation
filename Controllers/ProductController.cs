using BankingConsole.Models.Product;
using BankingConsole.Models.ProductCreation;
using BankingConsole.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankingConsole.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetAll(
        CancellationToken cancellationToken)
    {
        var products = await _productService.GetProductsAsync(
            cancellationToken);

        return Ok(products);
    }

    [HttpGet("{productId:guid}")]
    public async Task<ActionResult<Product>> GetById(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(
            productId,
            cancellationToken);

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(
        [FromBody] ProductCreationData creationData,
        CancellationToken cancellationToken)
    {
        var product = await _productService.CreateProductAsync(
            creationData,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { productId = product.ProductId },
            product);
    }

    [HttpPut("{productId:guid}")]
    public async Task<ActionResult<Product>> Update(
        Guid productId,
        [FromBody] ProductCreationData updateData,
        CancellationToken cancellationToken)
    {
        var product = await _productService.UpdateProductAsync(
            productId,
            updateData,
            cancellationToken);

        return Ok(product);
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> Delete(
        Guid productId,
        CancellationToken cancellationToken)
    {
        await _productService.DeleteProductAsync(
            productId,
            cancellationToken);

        return NoContent();
    }
}
