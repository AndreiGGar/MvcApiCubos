using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using MvcApiCubos.Filters;
using MvcApiCubos.Models;
using MvcApiCubos.Services;
using System.ComponentModel;

namespace MvcApiCubos.Controllers
{
    public class UsuariosController : Controller
    {
        private ServiceApiCubos service;
        private ServiceStorageBlobs storageBlob;
        private string container = "usuarios";

        public UsuariosController(ServiceApiCubos service, ServiceStorageBlobs storageBlob)
        {
            this.service = service;
            this.storageBlob = storageBlob;
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> Perfil()
        {
            string token = HttpContext.Session.GetString("TOKEN");
            Usuario usuario = await this.service.GetPerfilUsuarioAsync(token);
            string blob = usuario.Imagen;
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
                usuario.Imagen = uri.ToString();
            }
            return View(usuario);
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> Compras()
        {
            string token = HttpContext.Session.GetString("TOKEN");
            List<Compra> compras = await this.service.GetComprasUsuarioAsync(token);
            return View(compras);
        }
    }
}
