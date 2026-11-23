using System.Globalization;
using DotNetEnv;
using GestionTickets.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;

// Carga el archivo .env (si existe) sin pisar variables de entorno ya definidas.
// Así la contraseña de la base de datos nunca está en el código.
Env.NoClobber().TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(opciones =>
{
    // Las propiedades de navegación (Ticket.Usuario, etc.) no llegan en el formulario:
    // solo se exige lo que tenga [Required] en los metadatos de Models/Metadatos.
    opciones.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    MensajesEnEspanol(opciones.ModelBindingMessageProvider);
});

// Entity Framework Core + PostgreSQL. El contexto fue generado por scaffolding
// (dotnet ef dbcontext scaffold) a partir de la base de datos.
builder.Services.AddDbContext<GestionTicketsContext>(opciones =>
    opciones.UseNpgsql(ConfiguracionBaseDatos.CadenaConexion(builder.Configuration)));

// En Docker las claves de Data Protection se guardan en un volumen
var rutaClaves = builder.Configuration["DATA_PROTECTION_PATH"];
if (!string.IsNullOrWhiteSpace(rutaClaves))
{
    builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(rutaClaves));
}

// GET /health  →  "Healthy" cuando la base de datos responde
builder.Services.AddHealthChecks().AddDbContextCheck<GestionTicketsContext>("postgresql");

var app = builder.Build();

await app.EsperarBaseDatosAsync();

// Cultura invariante: las fechas de los formularios llegan como yyyy-MM-dd
var cultura = CultureInfo.InvariantCulture;
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = [cultura],
    SupportedUICultures = [cultura]
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStatusCodePagesWithReExecute("/Home/Estado/{0}");
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapHealthChecks("/health");

// Ruta MVC: /Controlador/Acción/Id  (por defecto: Home/Index)
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static void MensajesEnEspanol(DefaultModelBindingMessageProvider m)
{
    m.SetValueMustNotBeNullAccessor(_ => "Este campo es obligatorio.");
    m.SetMissingBindRequiredValueAccessor(campo => $"Falta el valor de {campo}.");
    m.SetMissingKeyOrValueAccessor(() => "Este campo es obligatorio.");
    m.SetMissingRequestBodyRequiredValueAccessor(() => "La solicitud no tiene datos.");
    m.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"El valor '{valor}' no es válido para {campo}.");
    m.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"El valor '{valor}' no es válido.");
    m.SetUnknownValueIsInvalidAccessor(campo => $"El valor ingresado no es válido para {campo}.");
    m.SetNonPropertyUnknownValueIsInvalidAccessor(() => "El valor ingresado no es válido.");
    m.SetValueIsInvalidAccessor(valor => $"El valor '{valor}' no es válido.");
    m.SetValueMustBeANumberAccessor(campo => $"{campo} debe ser un número.");
    m.SetNonPropertyValueMustBeANumberAccessor(() => "El valor debe ser un número.");
}
