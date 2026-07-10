/* ══════════════════════════════════════════════════════════════════════
   CEPLAN - Sistema de Gestión
   JavaScript - Interacciones del lado del cliente
   ══════════════════════════════════════════════════════════════════════ */

document.addEventListener('DOMContentLoaded', function () {

    // ── 1. Toggle Mostrar/Ocultar Contraseña ───────────────────────────
    initPasswordToggle();

    // ── 2. Toggle DNI / CE ─────────────────────────────────────────────
    initDocTypeToggle();

    // ── 3. Validación en tiempo real del Login ─────────────────────────
    initLoginValidation();

    // ── 4. Spinner de carga al enviar Login ────────────────────────────
    initLoginSpinner();

    // ── 5. Confirmación de Logout ──────────────────────────────────────
    initLogoutConfirmation();

    // ── 6. Sidebar mobile toggle ───────────────────────────────────────
    initSidebarToggle();

    // ── 7. Profile tabs ────────────────────────────────────────────────
    initProfileTabs();

    // ── 8. Toast de error del servidor ─────────────────────────────────
    // (eliminado: se usa TempData + showToast desde las vistas)

    // ── 9. Timeout de inactividad de sesión ────────────────────────────
    initSessionTimeout();
});

/* ════════════════════════════════════════════════════════════════════════
   FUNCIONES
   ════════════════════════════════════════════════════════════════════════ */

/**
 * 1. Mostrar/Ocultar contraseña con el icono del ojo.
 */
function initPasswordToggle() {
    const toggleBtn = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('inputPassword');
    const eyeIcon = document.getElementById('eyeIcon');

    if (!toggleBtn || !passwordInput || !eyeIcon) return;

    toggleBtn.addEventListener('click', function () {
        const isPassword = passwordInput.type === 'password';
        passwordInput.type = isPassword ? 'text' : 'password';

        // Cambiar icono
        eyeIcon.classList.toggle('bi-eye', !isPassword);
        eyeIcon.classList.toggle('bi-eye-slash', isPassword);

        // Focus de vuelta al input
        passwordInput.focus();
    });
}

/**
 * 2. Toggle entre tipo de documento DNI y CE.
 * Actualiza el campo hidden del formulario.
 */
function initDocTypeToggle() {
    const radios = document.querySelectorAll('input[name="TipoDocumento"]');
    const hiddenInput = document.getElementById('tipoDocumentoHidden');

    if (!radios.length || !hiddenInput) return;

    radios.forEach(function (radio) {
        radio.addEventListener('change', function () {
            hiddenInput.value = this.value;

            // Actualizar clases activas en labels
            document.querySelectorAll('.btn-doc-type').forEach(function (label) {
                label.classList.remove('active');
            });

            const activeLabel = document.querySelector('label[for="' + this.id + '"]');
            if (activeLabel) {
                activeLabel.classList.add('active');
            }
        });
    });
}

/**
 * 3. Validación en tiempo real de los campos del login.
 * Muestra feedback visual inmediato en los inputs.
 */
function initLoginValidation() {
    const inputUsuario = document.getElementById('inputUsuario');
    const inputPassword = document.getElementById('inputPassword');

    if (!inputUsuario || !inputPassword) return;

    function validateField(input) {
        const wrapper = input.closest('.input-group-custom');
        const validationSpan = wrapper?.parentElement?.nextElementSibling;

        if (!wrapper) return;

        if (input.value.trim() === '') {
            wrapper.classList.add('is-invalid');
            wrapper.classList.remove('is-valid');
        } else {
            wrapper.classList.remove('is-invalid');
            wrapper.classList.add('is-valid');
            if (validationSpan && validationSpan.classList.contains('field-validation-custom')) {
                validationSpan.textContent = '';
            }
        }
    }

    // Validar al perder foco
    inputUsuario.addEventListener('blur', function () {
        validateField(this);
    });

    inputPassword.addEventListener('blur', function () {
        validateField(this);
    });

    // Limpiar error mientras escribe
    [inputUsuario, inputPassword].forEach(function (input) {
        input.addEventListener('input', function () {
            const wrapper = this.closest('.input-group-custom');
            if (wrapper && this.value.trim() !== '') {
                wrapper.classList.remove('is-invalid');
            }
        });
    });
}

/**
 * 4. Spinner de carga durante la autenticación.
 * Muestra un loader y deshabilita el botón al enviar el formulario.
 */
function initLoginSpinner() {
    const form = document.getElementById('loginForm');
    const btnLogin = document.getElementById('btnLogin');

    if (!form || !btnLogin) return;

    form.addEventListener('submit', function (e) {
        // Validación básica antes de mostrar spinner
        const usuario = document.getElementById('inputUsuario');
        const password = document.getElementById('inputPassword');

        if (usuario && usuario.value.trim() === '') {
            e.preventDefault();
            const wrapper = usuario.closest('.input-group-custom');
            if (wrapper) wrapper.classList.add('is-invalid');
            usuario.focus();
            return;
        }

        if (password && password.value.trim() === '') {
            e.preventDefault();
            const wrapper = password.closest('.input-group-custom');
            if (wrapper) wrapper.classList.add('is-invalid');
            password.focus();
            return;
        }

        // Mostrar spinner en el botón
        const btnText = btnLogin.querySelector('.btn-login-text');
        const btnSpinner = btnLogin.querySelector('.btn-login-spinner');

        if (btnText) btnText.classList.add('d-none');
        if (btnSpinner) btnSpinner.classList.remove('d-none');

        btnLogin.disabled = true;
    });
}

/**
 * 5. Confirmación antes de cerrar sesión.
 * Muestra un modal Bootstrap pidiendo confirmación.
 */
function initLogoutConfirmation() {
    const logoutBtn = document.getElementById('logoutBtn');
    const confirmBtn = document.getElementById('confirmLogoutBtn');
    const logoutForm = document.getElementById('logoutForm');
    const modalElement = document.getElementById('logoutModal');

    if (!logoutBtn || !confirmBtn || !logoutForm || !modalElement) return;

    const logoutModal = new bootstrap.Modal(modalElement);

    logoutBtn.addEventListener('click', function (e) {
        e.preventDefault();
        logoutModal.show();
    });

    confirmBtn.addEventListener('click', function () {
        logoutModal.hide();
        logoutForm.submit();
    });
}

/**
 * 6. Toggle del sidebar en dispositivos móviles.
 */
function initSidebarToggle() {
    const toggleBtn = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('dashboardSidebar');
    const overlay = document.getElementById('sidebarOverlay');

    if (!toggleBtn || !sidebar) return;

    toggleBtn.addEventListener('click', function () {
        sidebar.classList.toggle('show');
        if (overlay) overlay.classList.toggle('active');
    });

    if (overlay) {
        overlay.addEventListener('click', function () {
            sidebar.classList.remove('show');
            overlay.classList.remove('active');
        });
    }
}

/**
 * 7. Navegación entre tabs del perfil.
 */
function initProfileTabs() {
    const tabs = document.querySelectorAll('.profile-tab');

    if (!tabs.length) return;

    tabs.forEach(function (tab) {
        tab.addEventListener('click', function () {
            const targetTab = this.getAttribute('data-tab');

            // Desactivar todos los tabs y paneles
            tabs.forEach(function (t) {
                t.classList.remove('active');
            });

            document.querySelectorAll('.tab-pane').forEach(function (pane) {
                pane.classList.remove('active');
            });

            // Activar el tab y panel seleccionado
            this.classList.add('active');
            var targetPane = document.getElementById('tab-' + targetTab);
            if (targetPane) {
                targetPane.classList.add('active');
            }
        });
    });
}

/**
 * Utilidad: Crear y mostrar un toast Bootstrap.
 * @param {string} message - Mensaje a mostrar.
 * @param {string} type - Tipo: 'success', 'danger', 'warning', 'info'.
 */
function showToast(message, type) {
    var container = document.getElementById('toastContainer');
    if (!container) return;

    var iconMap = {
        'success': 'bi-check-circle-fill',
        'danger': 'bi-exclamation-triangle-fill',
        'warning': 'bi-exclamation-circle-fill',
        'info': 'bi-info-circle-fill'
    };

    var bgMap = {
        'success': '#E6F3ED',
        'danger': '#FDEAEA',
        'warning': '#FFF8E6',
        'info': '#DFEDFF'
    };

    var colorMap = {
        'success': '#005E35',
        'danger': '#D20000',
        'warning': '#8B6914',
        'info': '#0156AC'
    };

    var toastHtml =
        '<div class="toast align-items-center border-0 show" role="alert" aria-live="assertive" aria-atomic="true" ' +
        'style="background: ' + bgMap[type] + '; color: ' + colorMap[type] + ';">' +
        '  <div class="d-flex">' +
        '    <div class="toast-body d-flex align-items-center gap-2">' +
        '      <i class="bi ' + iconMap[type] + '"></i>' +
        '      <span>' + escapeHtml(message) + '</span>' +
        '    </div>' +
        '    <button type="button" class="btn-close btn-close-sm me-2 m-auto" data-bs-dismiss="toast" aria-label="Cerrar"></button>' +
        '  </div>' +
        '</div>';

    var wrapper = document.createElement('div');
    wrapper.innerHTML = toastHtml;
    var toastElement = wrapper.firstElementChild;
    container.appendChild(toastElement);

    // Auto-dismiss después de 5 segundos
    setTimeout(function () {
        if (toastElement.parentNode) {
            toastElement.style.opacity = '0';
            toastElement.style.transition = 'opacity 0.3s ease';
            setTimeout(function () {
                if (toastElement.parentNode) {
                    toastElement.parentNode.removeChild(toastElement);
                }
            }, 300);
        }
    }, 5000);
}

/**
 * Utilidad: Escapar HTML para prevenir XSS.
 */
function escapeHtml(text) {
    var div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

/**
 * 9. Timeout de inactividad de sesión (3 minutos).
 * Muestra aviso 30 segundos antes de expirar. Si el usuario no responde, cierra sesión.
 */
function initSessionTimeout() {
    // Solo funciona en páginas autenticadas (donde exista el botón de logout)
    var logoutForm = document.getElementById('logoutForm');
    if (!logoutForm) return;

    var TIMEOUT_MINUTES = 30;
    var WARNING_SECONDS = 30;
    var INACTIVITY_MS = TIMEOUT_MINUTES * 60 * 1000;
    var WARNING_MS = WARNING_SECONDS * 1000;

    var lastActivity = Date.now();
    var warningTimer = null;
    var logoutTimer = null;
    var warningShown = false;
    var keepAliveUrl = '/Account/KeepAlive';

    // Resetear timer en cualquier actividad del usuario
    function resetActivity() {
        if (warningShown) return; // No resetear si ya se mostró el aviso
        lastActivity = Date.now();
        clearTimers();
        scheduleWarning();
    }

    function clearTimers() {
        if (warningTimer) { clearTimeout(warningTimer); warningTimer = null; }
        if (logoutTimer) { clearTimeout(logoutTimer); logoutTimer = null; }
    }

    function scheduleWarning() {
        var delay = INACTIVITY_MS - WARNING_MS;
        warningTimer = setTimeout(showWarning, delay);
    }

    function showWarning() {
        warningShown = true;
        var remaining = WARNING_SECONDS;

        // Crear modal de aviso de inactividad
        var modalHtml =
            '<div class="modal fade" id="sessionWarningModal" tabindex="-1" aria-hidden="true" data-bs-backdrop="static">' +
            '  <div class="modal-dialog modal-dialog-centered modal-sm">' +
            '    <div class="modal-content">' +
            '      <div class="modal-header" style="border-bottom: 1px solid var(--color-border);">' +
            '        <h5 class="modal-title" style="font-family: var(--font-primary); font-weight: 600; font-size: 16px;">Sesión por expirar</h5>' +
            '      </div>' +
            '      <div class="modal-body" style="font-family: var(--font-primary); font-size: 14px; color: var(--color-text-secondary); padding: 20px;">' +
            '        <p>Tu sesión se cerrará por inactividad en <strong id="sessionCountdown">' + remaining + '</strong> segundos.</p>' +
            '        <p class="mb-0">¿Deseas continuar con la sesión?</p>' +
            '      </div>' +
            '      <div class="modal-footer" style="border-top: 1px solid var(--color-border); padding: 12px 20px;">' +
            '        <button type="button" class="btn btn-outline-secondary btn-sm" id="sessionLogoutBtn">Cerrar sesión</button>' +
            '        <button type="button" class="btn btn-primary btn-sm" id="sessionContinueBtn" style="background: var(--color-primary-blue); border-color: var(--color-primary-blue);">Continuar</button>' +
            '      </div>' +
            '    </div>' +
            '  </div>' +
            '</div>';

        var wrapper = document.createElement('div');
        wrapper.innerHTML = modalHtml;
        document.body.appendChild(wrapper.firstElementChild);

        var modalElement = document.getElementById('sessionWarningModal');
        var modal = new bootstrap.Modal(modalElement);
        modal.show();

        // Countdown
        var countdownEl = document.getElementById('sessionCountdown');
        var countdownInterval = setInterval(function () {
            remaining--;
            if (countdownEl) countdownEl.textContent = remaining;
            if (remaining <= 0) {
                clearInterval(countdownInterval);
                doAutoLogout();
            }
        }, 1000);

        // Botón continuar
        document.getElementById('sessionContinueBtn').addEventListener('click', function () {
            clearInterval(countdownInterval);
            modal.hide();
            modalElement.remove();
            warningShown = false;
            lastActivity = Date.now();

            // Enviar keep-alive al servidor
            sendKeepAlive();

            // Reiniciar timer
            scheduleWarning();
        });

        // Botón cerrar sesión
        document.getElementById('sessionLogoutBtn').addEventListener('click', function () {
            clearInterval(countdownInterval);
            doAutoLogout();
        });
    }

    function doAutoLogout() {
        // Enviar formulario de logout
        var formData = new FormData(logoutForm);
        fetch(logoutForm.action, {
            method: 'POST',
            body: formData
        }).then(function () {
            // Mostrar notificación antes de redirigir
            showSessionExpiredNotification();
        }).catch(function () {
            window.location.href = '/Account/Login';
        });
    }

    function showSessionExpiredNotification() {
        // Crear notificación toast que se desvanezca sola
        var notificationHtml =
            '<div class="session-expired-notification" id="sessionExpiredNotification">' +
            '  <div class="session-expired-content">' +
            '    <i class="bi bi-clock-history"></i>' +
            '    <span>Sesión cerrada por inactividad</span>' +
            '  </div>' +
            '</div>';

        var container = document.createElement('div');
        container.innerHTML = notificationHtml;
        document.body.appendChild(container.firstElementChild);

        var notification = document.getElementById('sessionExpiredNotification');

        // Redirigir después de 2 segundos
        setTimeout(function () {
            window.location.href = '/Account/Login';
        }, 2000);
    }

    function sendKeepAlive() {
        var token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (!token) return;

        var formData = new FormData();
        formData.append('__RequestVerificationToken', token.value);

        fetch(keepAliveUrl, {
            method: 'POST',
            body: formData
        }).catch(function () {
            // Silenciar errores de keep-alive
        });
    }

    // Eventos de actividad del usuario
    var events = ['mousedown', 'mousemove', 'keydown', 'scroll', 'touchstart'];
    events.forEach(function (event) {
        document.addEventListener(event, resetActivity, { passive: true });
    });

    // Iniciar timer
    scheduleWarning();
}
