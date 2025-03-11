using Microsoft.AspNetCore.Mvc;
using RealtimeMeetingAPI.Helpers;

namespace RealtimeMeetingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        [HttpGet]
        [Route("list")]
        public IActionResult GetListProduct()
        {
            var products = new List<object>()
            {
                new
                {
                    name = "iPhone",
                    description = "iPhone is the stylist phone ever",
                    price = 1000,
                    image = "iphone.jpg",
                },
                new
                {
                    name = "Pixel",
                    description = "Pixel is the stylist phone ever",
                    price = 800,
                    image = "pixel.jpg",
                },
                new
                {
                    name = "Laptop",
                    description = "Laptop is most productive development tool",
                    price = 2000,
                    image = "laptop.jpg",
                },
                new
                {
                    name = "Tablet",
                    description = "Tablet is the most useful device ever for meeting",
                    price = 1500,
                    image = "tablet.jpg",
                },
                new
                {
                    name = "Pendrive",
                    description = "Pendrive is the most useful for storage",
                    price = 100,
                    image = "pendrive.jpg",
                },
                new
                {
                    name = "Floppy Drive",
                    description = "Floppy Drive is the stylist phone ever",
                    price = 200,
                    image = "floppydisk.jpg",
                },
            };

            return StatusCode(StatusCodes.Status200OK, Response<List<object>>.Result(products, "Get list product successfully", StatusCodes.Status200OK));
        }
    }
}
