📦 Track&Order API:

Backend REST para la plataforma Track&Order, sistema multiplataforma de gestión de pedidos, entregas y facturación.

🛠️ Tecnologías:

| Componente    | Tecnología                     |
|---------------|--------------------------------|
| Lenguaje      | C# (.NET 8)                    |
| Framework     | ASP.NET Core Web API           |
| Base de datos | MySQL                          |
| ORM           | Entity Framework Core (Pomelo) |
| Seguridad     | JWT Bearer                     |
| Documentación | Swagger / OpenAPI              |

🚀 Inicio rápido:

git clone https://github.com/<tuusuario>/TrackAndOrderAPI.git
cd TrackAndOrderAPI

Configura tu conexión en `appsettings.json`:
{
  "ConnectionStrings": {
    "SmartWarehouseDB": "server=localhost;database=trackorder_db;user=root;password=TU_PASSWORD;"
  }
}

Ejecuta con `dotnet run` y abre Swagger en `http://localhost:5294/swagger`.

🔐 Autenticación:

La API usa JWT Bearer. Obtén tu token con:

POST /api/Usuarios/login
{ "email": "admin@example.com", "password": "tu_password" }

Incluye el token en cada petición: `Authorization: Bearer <token>`

📡 Endpoints principales:

| Recurso                | Ruta base          |
|------------------------|--------------------|
| Usuarios               | `/api/Usuarios`    |
| Productos              | `/api/Productos`   |
| Pedidos                | `/api/Pedidos`     |
| Facturas               | `/api/Facturas`    |
| Albaranes              | `/api/Albaranes`   |
| Rutas de entrega       | `/api/Rutas`       |
| Ubicaciones repartidor | `/api/Ubicaciones` |

👥 Roles:

`admin` · `empleado` · `repartidor` · `cliente`

Los roles `admin` y `empleado` acceden desde la app de escritorio (C#). Los roles `repartidor` y `cliente` desde la app Android (Kotlin).

🧠 Autor:

Proyecto desarrollado por Kevin Guerra para el TFG de Desarrollo de Aplicaciones Multiplataforma (DAM).
