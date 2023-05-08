using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using MvcApiCubos.Extensions;
using MvcApiCubos.Filters;
using MvcApiCubos.Models;
using MvcApiCubos.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace MvcApiCubos.Controllers
{
    public class HomeController : Controller
    {
        private ServiceApiCubos service;
        private ServiceStorageBlobs storageBlob;
        private string container = "cubos";
        private string container2 = "usuarios";

        public HomeController(ServiceApiCubos service, ServiceStorageBlobs storageBlob)
        {
            this.service = service;
            this.storageBlob = storageBlob;
        }

        public async Task<IActionResult> Index()
        {
            List<Cubo> cubos = await this.service.GetCubosAsync();
            foreach (Cubo cubo in cubos)
            {
                string blob = cubo.Imagen;
                if (blob != null)
                {
                    BlobContainerClient blobContainerClient = await this.storageBlob.GetContainerAsync(container);
                    BlobClient blobClient = blobContainerClient.GetBlobClient(blob);

                    BlobSasBuilder sasBuilder = new BlobSasBuilder()
                    {
                        BlobContainerName = this.container,
                        BlobName = blob,
                        Resource = "b",
                        StartsOn = DateTimeOffset.UtcNow,
                        ExpiresOn = DateTime.UtcNow.AddHours(1),
                    };

                    sasBuilder.SetPermissions(BlobSasPermissions.Read);
                    var uri = blobClient.GenerateSasUri(sasBuilder);
                    cubo.Imagen = uri.ToString();
                }
            }
            return View(cubos);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string marca)
        {
            List<Cubo> cubos = await this.service.GetCubosMarcaAsync(marca);
            return View(cubos);
        }

        public IActionResult NuevoCubo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NuevoCubo(Cubo cubo, IFormFile file)
        {
            string blobName = file.FileName;
            cubo.Imagen = blobName;
            using (Stream stream = file.OpenReadStream())
            {
                await this.storageBlob.UploadBlobAsync(this.container, blobName, stream);
            }
            await this.service.NuevoCuboAsync(cubo.Nombre, cubo.Marca, cubo.Imagen, cubo.Precio);
            return RedirectToAction("Index");
        }

        public IActionResult NuevoUsuario()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> NuevoUsuario(Usuario usuario, IFormFile file)
        {
            string blobName = file.FileName;
            usuario.Imagen = blobName;
            using (Stream stream = file.OpenReadStream())
            {
                await this.storageBlob.UploadBlobAsync(this.container2, blobName, stream);
            }
            await this.service.NuevoUsuarioAsync(usuario.Nombre, usuario.Email, usuario.Pass, usuario.Imagen);
            return RedirectToAction("Login", "Managed");
        }

        public async Task<IActionResult> Comprar(int? idcubo)
        {
            if (idcubo != null)
            {
                List<int> cart;

                if (HttpContext.Session.GetObject<List<int>>("CART") == null)
                {
                    cart = new List<int>();
                }
                else
                {
                    cart = HttpContext.Session.GetObject<List<int>>("CART");
                }
                cart.Add(idcubo.Value);
                HttpContext.Session.SetObject("CART", cart);
            }
            string previousUrl = Request.Headers["Referer"].ToString();
            return Redirect(previousUrl);
        }

        public async Task<IActionResult> Carrito(int? idcubo)
        {
            List<int> cart = HttpContext.Session.GetObject<List<int>>("CART");
            if (cart == null)
            {
                ViewData["MESSAGE"] = "Actualmente no tienes ningún producto en el carrito";
                return View();
            }
            else
            {
                if (idcubo != null)
                {
                    cart.Remove(idcubo.Value);

                    if (cart.Count == 0)
                    {
                        HttpContext.Session.Remove("CART");
                        ViewData["MESSAGE"] = "Actualmente no tienes ningún producto en el carrito";
                    }
                    else
                    {
                        HttpContext.Session.SetObject("CART", cart);
                    }
                }

                List<Cubo> cubos = await this.service.GetCubosCarritoAsync(cart);
                return View(cubos);
            }
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> Compra()
        {
            List<int> cart = HttpContext.Session.GetObject<List<int>>("CART");
            List<Cubo> cubos = await this.service.GetCubosCarritoAsync(cart);

            Compra compra = new Compra();
            foreach (Cubo cubo in cubos)
            {
                compra.IdCubo = cubo.IdCubo;
                compra.IdUsuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                await this.service.NuevaCompraAsync(HttpContext.Session.GetString("TOKEN"), compra.IdCubo, compra.IdUsuario);
            }
            HttpContext.Session.Remove("CART");
            return RedirectToAction("Compras", "Usuarios");
        }
    }
}