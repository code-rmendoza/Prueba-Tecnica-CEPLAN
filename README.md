# Sistema de Gestión de Usuarios y Autenticación - CEPLAN

Aplicación web funcional desarrollada en **ASP.NET Core 8.0 MVC** y **SQL Server**, diseñada siguiendo fielmente los lineamientos UX/UI de Figma e implementando patrones de arquitectura limpia y ciberseguridad avanzada.

---

## 🔑 Credenciales de Acceso para Pruebas

Para validar el inicio de sesión y la visualización de los perfiles dinámicos según el rol, puedes utilizar los siguientes datos cargados en la base de datos de pruebas:

| Documento (Usuario) | Contraseña | Rol / Nombre | Avatar cargado dinámicamente |
| :--- | :--- | :--- | :--- |
| **07079879** | `Admin123!` | Administradora (July Camila Mendoza Quispe) | Imagen por defecto |
| **46844596** | `User123!` | Operadora (Adriana Osorio Montes) | Imagen por defecto |

---

## 🛠️ Puesta en Marcha y Configuración Local

Sigue estos sencillos pasos para compilar, configurar y ejecutar el proyecto en tu entorno local. Tienes dos opciones de despliegue:

### Opción A: Despliegue Rápido (Recomendado - Docker)
Esta opción es ideal para la evaluación ya que no requiere instalar SQL Server ni SSMS localmente.
1.  **Levantar Base de Datos:** Abre una terminal en la raíz del proyecto y ejecuta:
    ```bash
    docker compose up -d
    ```
2.  **Ejecutar la Aplicación:**
    ```bash
    dotnet run --project PruebaAspNet/PruebaAspNet.csproj
    ```
    *(Nota: Al arrancar por primera vez, Entity Framework creará la base de datos en el contenedor y sembrará los datos de prueba automáticamente).*
3.  **Acceder:** Abre tu navegador e ingresa a: **`http://localhost:5000`**

---

### Opción B: Configuración Tradicional (Local)
1.  **Requisitos Previos:** Tener el SDK de .NET 8.0 y una instancia activa de SQL Server.
2.  **Configuración de Conexión:** Abre [appsettings.json](PruebaAspNet/appsettings.json) y actualiza la cadena de conexión `DefaultConnection` para que apunte a tu servidor de SQL Server local (ej. `(localdb)\\mssqllocaldb` o `localhost\\SQLEXPRESS`).
3.  **Ejecución de Script:** Abre SSMS u otra herramienta de administración y ejecuta el script [database_script.sql](PruebaAspNet/Scripts/database_script.sql).
4.  **Ejecutar la Aplicación:** En la raíz del proyecto, ejecuta:
    ```bash
    dotnet run --project PruebaAspNet/PruebaAspNet.csproj
    ```

---

### 🧪 Suite de Pruebas Unitarias
El proyecto cuenta con una suite de pruebas unitarias automatizadas que validan la lógica de negocio en servicios y controladores. Para ejecutarlas:
```bash
dotnet test
```

---

## 🚀 Características y Decisiones Técnicas

### 1. Parámetros de Seguridad Dinámicos
*   **Configuración centralizada (`appsettings.json`):** Los límites de intentos de login, minutos de bloqueo y expiración de sesión se leen dinámicamente desde el archivo de configuración.
*   **Entornos Flexibles de Prueba:** En `appsettings.Development.json` los parámetros están configurados a **2 minutos de sesión, 15 segundos de aviso y 3 intentos fallidos** para facilitar una evaluación rápida e interactiva de todos los flujos sin esperas.
*   **Toggle de Fidelidad vs. Ciberseguridad (`MensajesInlineFigma`):** Un interruptor de configuración permite cambiar entre mensajes unificados seguros (mitigación de enumeración de usuarios) y mensajes inline diferenciados (*"Usuario incorrecto"* / *"Contraseña incorrecta"*) solicitados en el diseño de Figma.

### 2. Flujos Funcionales Completos (Figma)
*   **Activación de Cuenta:** Formulario funcional de validación de documento contra la base de datos. Muestra un saludo dinámico y pantalla de éxito animada (*"¡Bienvenida, July!"*) si la cuenta existe.
*   **Recuperación de Contraseña:** Formulario funcional que valida DNI/CE y Correo Electrónico. Simula el envío seguro de instrucciones a través de logs detallados del servidor.
*   **Soporte Técnico desde Bloqueo:** Cuando la cuenta es bloqueada temporalmente, el usuario dispone de un formulario interactivo para enviar un reporte a soporte técnico con confirmación visual (Toast) y registro de tickets en logs.

### 3. Experiencia de Perfil Enriquecida
*   **Tabs Dinámicos por Rol:** Las pestañas de *Responsabilidades* e *Historial* en el perfil muestran información estructurada de acuerdo al rol del usuario autenticado (Administrador de Recursos vs. Operador), usando layouts interactivos, tarjetas de tareas y tablas de auditoría.

### 4. Arquitectura y Ciberseguridad
*   **Separación de Responsabilidades (SRP):** Encapsulación de lógica en `AuthService` y exposición limpia en `AccountController`.
*   **Patrón Repository:** Desacoplamiento de acceso a datos mediante la interfaz `IUsuarioRepository`.
*   **Protección contra Fuerza Bruta (Brute-Force):** Control de intentos fallidos persistido en la tabla `Usuarios` para evitar evasión reiniciando navegador.
*   **Rate Limiting:** Regulación por IP en login (máx. 10 peticiones/min) ante ataques DoS.
*   **Protección de Datos (Enmascaramiento):** Enmascaramiento automático de PII (ej. DNI `07***79`) en logs.
*   **Cookies de Sesión Seguras:** Cookies HTTPOnly, SecurePolicy Always y SameSite Lax.

---

## 🛠️ Tecnologías y Herramientas Utilizadas
*   **Backend:** ASP.NET Core 8.0 MVC / C#
*   **Base de Datos:** SQL Server 2022 / EF Core 8.0
*   **Frontend:** Bootstrap 5.3.3, Bootstrap Icons, CSS Custom Variables, Vanilla JavaScript
*   **Seguridad:** BCrypt.Net-Next (Work Factor 12)
*   **Pruebas Unitarias:** xUnit, Moq, FluentAssertions
*   **Contenedores:** Docker / Docker Compose
