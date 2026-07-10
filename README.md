# Sistema de Gestión de Usuarios y Autenticación - CEPLAN

Aplicación web funcional desarrollada en **ASP.NET Core 8.0 MVC** y **SQL Server**, diseñada siguiendo fielmente los lineamientos UX/UI de Figma e implementando patrones de arquitectura limpia y ciberseguridad avanzada.

## 🚀 Características y Decisiones Técnicas

### 1. Arquitectura y Código Limpio
*   **Separación de Responsabilidades (SRP):** La lógica de negocio relacionada con la autenticación, control de fuerza bruta y bloqueos temporales se ha encapsulado en el servicio `AuthService`, liberando al controlador `AccountController` de la lógica de negocio y dejándolo únicamente como gestor de peticiones HTTP.
*   **Patrón Repository:** Desacoplamiento del acceso a datos mediante la interfaz `IUsuarioRepository` facilitando la mantenibilidad y la inyección de dependencias.
*   **Base de Datos Tipada:** Se migraron los campos de fecha (`FechaNacimiento` y `FechaContratacion`) a tipos de datos temporales nativos de SQL (`DATE`) mapeados con `DateOnly?` en C#, garantizando la integridad referencial y permitiendo consultas de rango eficientes en base de datos.
*   **DevOps ready:** Se estructuró la raíz con un archivo de solución unificado (`PruebaAspNet.sln`) para compilar y ejecutar suites de pruebas de forma integrada.

### 2. Ciberseguridad Aplicada
*   **Mitigación de Enumeración de Usuarios:** Mensajes de error unificados ante fallos de credenciales o cuentas inexistentes. El sistema no revela si una cuenta de DNI existe o no en la base de datos.
*   **Protección contra Fuerza Bruta (Brute-Force Protection):** Bloqueo automático por 15 minutos en base de datos tras 5 intentos fallidos consecutivos.
*   **Rate Limiting:** Regulación de tráfico en el endpoint de login mediante directiva de Rate Limiting fija por dirección IP (máximo 10 peticiones por minuto) para mitigar ataques DoS distribuidos.
*   **Seguridad en Logs (Enmascaramiento de PII):** Los identificadores sensibles (DNI) se registran en los logs del servidor de forma enmascarada (ej. `07***79`) cumpliendo con regulaciones de protección de datos personales.
*   **Cookies de Sesión Seguras:** Cookies configuradas como `HttpOnly` (previene robo por XSS), `SecurePolicy.Always` (solo HTTPS) y `SameSite = Lax` (mitigación CSRF).
*   **Integridad de Recursos Externos (SRI):** Uso de atributos `integrity` y `crossorigin` en los scripts y estilos cargados por CDN pública.

### 3. Funcionalidades del Cliente (JavaScript)
*   **Timeout de Inactividad Sincronizado:** Implementación de un detector de inactividad de 30 minutos en JS que muestra una advertencia interactiva al usuario 30 segundos antes de expirar. Si el usuario no responde, realiza un logout asíncrono seguro en el backend y lo redirige al login con una alerta visual.
*   **Interacciones Fieles a Figma:** Mostrar/ocultar contraseña, validación de inputs en tiempo real, spinner de carga en el botón de submit, y navegación entre pestañas de información.

---

## 🛠️ Tecnologías y Herramientas Utilizadas
*   **Backend:** ASP.NET Core 8.0 MVC / C#
*   **Base de Datos:** SQL Server / EF Core 8.0
*   **Frontend:** Bootstrap 5.3.3, Bootstrap Icons, CSS Custom Variables, Vanilla JavaScript
*   **Seguridad:** BCrypt.Net-Next (Work Factor 12)
*   **Pruebas Unitarias:** xUnit, Moq, FluentAssertions
