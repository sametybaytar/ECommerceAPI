﻿using ECommerceAPI.Application.Repositories;
using ECommerceAPI.Application.RequestParameters;
using ECommerceAPI.Application.Services;
using ECommerceAPI.Application.ViewModels.Products;
using ECommerceAPI.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace ECommerceAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductReadRepository _productReadRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IProductWriteRepository _productWriteRepository;
        private readonly IFileService _fileService;

        public ProductsController(
            IProductWriteRepository productWriteRepository,
            IProductReadRepository productReadRepository,
            IWebHostEnvironment webHostEnvironment,
            IFileService fileService)
        {
            _productWriteRepository = productWriteRepository;
            _productReadRepository = productReadRepository;
            _webHostEnvironment = webHostEnvironment;
            _fileService = fileService;
        }
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] Pagination pagination)
        {
            var totalCount = _productReadRepository.GetAll(false).Count();
            var products = _productReadRepository.GetAll(false).Select(p => new
            {
                p.Id,
                p.Name,
                p.Stock,
                p.Price,
                p.CreatedDate,
                p.UpdatedDate

            }).Skip(pagination.Page * pagination.Size).Take(pagination.Size).ToList();

            return Ok(new {totalCount, products });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(string id)
        {
            Product product = await _productReadRepository.GetByIdAsync(id);
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create(VM_Create_Product model)
        {
            
            Product product = new(){ Name = model.Name, Stock = model.Stock, Price = model.Price };
            await _productWriteRepository.AddAsync(product);
            await _productWriteRepository.SaveAsync();
            return StatusCode((int)HttpStatusCode.Created);
        }

        [HttpPut]
        public async Task<IActionResult> Update(VM_Update_Product model)
        {
            Product product = await _productReadRepository.GetByIdAsync(model.Id.ToString());
            product.Name = model.Name;
            product.Price = model.Price;
            product.Stock = model.Stock;
            await _productWriteRepository.SaveAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id) {

            var asd = await _productWriteRepository.RemoveAsync(id);
            await _productWriteRepository.SaveAsync();
            return Ok();
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Upload()
        {
            await _fileService.UpdateAsync("resource/products/images", Request.Form.Files);

            //string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "resource/products/images");

            //if (!Directory.Exists(uploadPath))
            //    Directory.CreateDirectory(uploadPath);

            //Random r = new();
            //foreach (IFormFile file in Request.Form.Files)
            //{
            //    string fullPath = Path.Combine(uploadPath, $"{r.Next()}{Path.GetExtension(file.FileName)}");

            //    using FileStream fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024, useAsync: false);
            //    await file.CopyToAsync(fileStream);
            //    await fileStream.FlushAsync();
            //}
            return Ok();
        }


    }
}
