# Sistema de Gestión de Tickets (MesaAyuda)

Aplicación web en **C# — ASP.NET Core MVC (.NET 10)** con **Entity Framework Core 10** y
**PostgreSQL 17**, construida con **scaffolding** y ejecutada con **Docker Compose**.

## Base de datos: 5 entidades × 5 atributos

| Tabla          | Atributos (además del identificador)                                   |
|----------------|------------------------------------------------------------------------|
| `categorias`   | nombre, descripcion, prioridad, tiempo_respuesta_horas, activa         |
| `usuarios`     | cedula, nombre, correo, telefono, departamento                         |
| `tecnicos`     | nombre, correo, telefono, especialidad, activo                         |
| `tickets`      | asunto, descripcion, fecha_apertura, usuario_id (FK), categoria_id (FK)|
| `seguimientos` | ticket_id (FK), tecnico_id (FK), fecha, estado, comentario             |

```
usuarios ──< tickets >── categorias
               │
               └──< seguimientos >── tecnicos
```

El **estado** y el **técnico asignado** de un ticket se calculan con su último seguimiento
(como el saldo de una cuenta en el ejemplo MiBanco), no se guardan en una columna.

## Scaffolding utilizado

1. **Modelos desde la base de datos** (Database First):

   ```bash
   dotnet ef dbcontext scaffold "Host=localhost;Port=5434;Database=gestion_tickets;Username=postgres;Password=<contraseña>" \
     Npgsql.EntityFrameworkCore.PostgreSQL --project src/GestionTickets \
     --output-dir Models --context-dir Data --context GestionTicketsContext \
     --data-annotations --no-onconfiguring --force
   ```

2. **Controladores y vistas CRUD** desde los modelos (uno por entidad):

   ```bash
   dotnet aspnet-codegenerator controller -name TicketsController -m Ticket \
     -dc GestionTickets.Data.GestionTicketsContext --relativeFolderPath Controllers \
     --useDefaultLayout --referenceScriptLibraries
   ```

Después se personalizaron: interfaz en español, ventanas modales para agregar/editar
(como MiBanco), listas desplegables legibles y reglas de negocio. Las validaciones están en
`Models/Metadatos.cs` (clases `partial`), así que regenerar los modelos no las borra.

## Ejecutar con Docker

```bash
copy .env.example .env        # y escribir POSTGRES_PASSWORD
docker compose up -d --build
```

Abrir http://localhost:8081 (estado: http://localhost:8081/health).
La primera vez, PostgreSQL ejecuta `Database/01_crear_tablas.sql` y `Database/02_datos_prueba.sql`.

Los pasos completos están en `INSTRUCCIONES_DESPLIEGUE.txt`; la especificación, en
`specs/gestion-tickets/`.
