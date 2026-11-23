// ===== Sistema de Gestión de Tickets =====

// Mensajes de la validación del navegador (jQuery Validation) en español
if (window.jQuery && jQuery.validator) {
    jQuery.extend(jQuery.validator.messages, {
        required: "Este campo es obligatorio.",
        email: "Ingrese un correo válido.",
        number: "Ingrese un número válido.",
        digits: "Ingrese solo dígitos.",
        date: "Ingrese una fecha válida.",
        maxlength: jQuery.validator.format("Máximo {0} caracteres."),
        minlength: jQuery.validator.format("Mínimo {0} caracteres."),
        range: jQuery.validator.format("Ingrese un valor entre {0} y {1}."),
        max: jQuery.validator.format("Ingrese un valor menor o igual a {0}."),
        min: jQuery.validator.format("Ingrese un valor mayor o igual a {0}.")
    });
}

// Ventana modal: los enlaces con data-modal="Título" cargan la página del
// formulario (Create/Edit/Details/Delete generados por scaffolding) dentro del modal.
(() => {
    const elemento = document.getElementById("modalFormulario");
    if (!elemento) return;

    const modal = new bootstrap.Modal(elemento);
    const titulo = document.getElementById("modalTitulo");
    const cuerpo = document.getElementById("modalCuerpo");
    const cabeceras = { "X-Requested-With": "XMLHttpRequest" };

    function mostrar(html) {
        cuerpo.innerHTML = html;

        // "Cancelar" / "Cerrar" cierran el modal en lugar de navegar
        cuerpo.querySelectorAll("[data-cancelar]").forEach(boton =>
            boton.addEventListener("click", e => { e.preventDefault(); modal.hide(); }));

        const formulario = cuerpo.querySelector("form");
        if (!formulario) return;

        if (window.jQuery && jQuery.validator) {
            jQuery(formulario).removeData("validator").removeData("unobtrusiveValidation");
            jQuery.validator.unobtrusive.parse(formulario);
        }
        formulario.addEventListener("submit", enviar);
        setTimeout(() => formulario.querySelector("input:not([type=hidden]), select, textarea")?.focus(), 250);
    }

    async function enviar(e) {
        e.preventDefault();
        const formulario = e.currentTarget;
        if (window.jQuery && jQuery(formulario).valid && !jQuery(formulario).valid()) return;

        const boton = formulario.querySelector("[type=submit]");
        if (boton) boton.disabled = true;
        try {
            const respuesta = await fetch(formulario.action, {
                method: "POST",
                body: new FormData(formulario),
                headers: cabeceras
            });
            // Guardado correcto: el controlador redirige al listado con el mensaje de confirmación
            if (respuesta.redirected) {
                window.location.href = respuesta.url;
                return;
            }
            // Errores de validación del servidor: se vuelve a mostrar el formulario con los mensajes
            mostrar(await respuesta.text());
        } catch {
            cuerpo.insertAdjacentHTML("afterbegin",
                '<div class="alert alert-danger">No se pudo conectar con el servidor.</div>');
            if (boton) boton.disabled = false;
        }
    }

    document.addEventListener("click", async e => {
        const enlace = e.target.closest("a[data-modal]");
        if (!enlace) return;
        e.preventDefault();

        titulo.textContent = enlace.dataset.modal;
        cuerpo.innerHTML = '<div class="text-center text-muted py-4">Cargando…</div>';
        modal.show();
        try {
            const respuesta = await fetch(enlace.href, { headers: cabeceras });
            mostrar(await respuesta.text());
        } catch {
            cuerpo.innerHTML = '<div class="alert alert-danger">No se pudo cargar la información.</div>';
        }
    });
})();
