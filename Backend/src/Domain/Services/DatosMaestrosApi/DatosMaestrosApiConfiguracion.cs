namespace IERIC.SumariosIERIC.Infrastructure.Services.DatosMaestrosApi
{
    public sealed class DatosMaestrosApiConfiguracion
    {
        public string BaseUrl { get; set; } = string.Empty;

        public string Usuario { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool Publica { get; set; } = true;

        public int MinutosDuracionToken { get; set; } = 120;
    }
}