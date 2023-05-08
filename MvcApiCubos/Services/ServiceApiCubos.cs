using MvcApiCubos.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace MvcApiCubos.Services
{
    public class ServiceApiCubos
    {
        private MediaTypeWithQualityHeaderValue Header;
        private string UrlApiCubos;

        public ServiceApiCubos(IConfiguration configuration)
        {
            this.UrlApiCubos = configuration.GetValue<string>("ApiUrls:ApiCubos");
            this.Header = new MediaTypeWithQualityHeaderValue("application/json");
        }

        public async Task<string> GetTokenAsync(string username, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Auth/Login";
                client.BaseAddress = new Uri(this.UrlApiCubos);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                LoginModel model = new LoginModel
                {
                    UserName = username,
                    Password = password
                };
                string jsonModel = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(jsonModel, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(data);
                    string token = jsonObject.GetValue("response").ToString();
                    return token;
                }
                else
                {
                    return null;
                }
            }
        }

        private async Task<T> CallApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApiCubos);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        private async Task<T> CallApiAsync<T>(string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApiCubos);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        public async Task<List<Cubo>> GetCubosAsync()
        {
            string request = "api/Cubos";
            List<Cubo> cubos = await this.CallApiAsync<List<Cubo>>(request);
            return cubos;
        }

        public async Task<List<Cubo>> GetCubosMarcaAsync(string marca)
        {
            string request = "api/Cubos/" + marca;
            List<Cubo> cubos = await this.CallApiAsync<List<Cubo>>(request);
            return cubos;
        }

        public async Task NuevoUsuarioAsync(string nombre, string email, string pass, string imagen)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Cubos/NuevoUsuario";
                client.BaseAddress = new Uri(this.UrlApiCubos);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                Usuario usuario = new Usuario();
                usuario.Nombre = nombre;
                usuario.Email = email;
                usuario.Pass = pass;
                usuario.Imagen = imagen;
                string json = JsonConvert.SerializeObject(usuario);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
            }
        }

        public async Task NuevoCuboAsync(string nombre, string marca, string imagen, int precio)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Cubos/NuevoCubo";
                client.BaseAddress = new Uri(this.UrlApiCubos);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                Cubo cubo = new Cubo();
                cubo.Nombre = nombre;
                cubo.Marca = marca;
                cubo.Imagen = imagen;
                cubo.Precio = precio;
                string json = JsonConvert.SerializeObject(cubo);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
            }
        }

        public async Task<Usuario> GetPerfilUsuarioAsync(string token)
        {
            string request = "api/Cubos/PerfilUsuario";
            Usuario usuario = await this.CallApiAsync<Usuario>(request, token);
            return usuario;
        }

        public async Task<List<Compra>> GetComprasUsuarioAsync(string token)
        {
            string request = "api/Cubos/ComprasUsuario";
            List<Compra> compras = await this.CallApiAsync<List<Compra>>(request, token);
            return compras;
        }

        public async Task NuevaCompraAsync(string token, int idcubo, int idusuario)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "api/Cubos/NuevaCompra";
                client.BaseAddress = new Uri(this.UrlApiCubos);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.Header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                Compra compra = new Compra();
                compra.IdCubo = idcubo;
                compra.IdUsuario = idusuario;
                string json = JsonConvert.SerializeObject(compra);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
            }
        }

        public async Task<List<Cubo>> GetCubosCarritoAsync(List<int> ids)
        {
            string request = "api/Cubos";
            List<Cubo> cubos = await this.CallApiAsync<List<Cubo>>(request);
            var query = cubos.Where(cubo => ids.Contains(cubo.IdCubo));
            return query.ToList();
        }

        public async Task<Usuario> GetUsuarioLoginAsync(string username)
        {
            string request = "api/Cubos/GetUsuario/" + username;
            Usuario usuario = await this.CallApiAsync<Usuario>(request);
            return usuario;
        }
    }
}
