/* ============ Creación de subasta desde el front ============ */
(() => {
  const form = document.getElementById("form-crear");
  if (!form) return;

  const alerta = document.getElementById("alerta-crear");
  const btn = document.getElementById("btn-crear");
  const selectCat = document.getElementById("cs-categoria");

  function mostrarAlerta(texto, esError) {
    alerta.textContent = texto;
    alerta.className = `alert ${esError ? "alert-danger" : "alert-success"}`;
    alerta.classList.remove("d-none");
    clearTimeout(window.__timerCrear);
    window.__timerCrear = setTimeout(() => alerta.classList.add("d-none"), 6000);
  }

  // Las fechas se manejan en hora local del navegador: lo que escribís es lo que se
// muestra y se compara contra tu propio reloj. El backend guarda en UTC interno.
  function aLocal(ms) {
    const d = new Date(ms);
    const pad = (n) => String(n).padStart(2, "0");
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }

  // Parsea un value de <input type="datetime-local"> (YYYY-MM-DDTHH:mm) como hora
  // local, sin depender del parseo de strings del navegador.
  function parseLocalDateTime(valor) {
    const [fecha, hora] = String(valor ?? "").split("T");
    if (!fecha || !hora) return new Date(NaN);
    const [y, m, d] = fecha.split("-").map(Number);
    const [hh, mm] = hora.split(":").map(Number);
    return new Date(y || 0, (m || 1) - 1, d || 1, hh || 0, mm || 0, 0, 0);
  }

  // Hora actual (local) para el cartel junto a los campos de fecha.
  function horaActual() {
    const pad = (n) => String(n).padStart(2, "0");
    const d = new Date();
    return `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }

  function relojLocal() {
    const el = document.getElementById("cs-ahora");
    if (el) el.textContent = horaActual();
  }
  relojLocal();
  setInterval(relojLocal, 1000);

  // Vista previa: muestra las fechas que se van a guardar, en hora local.
  function actualizarPreview() {
    const el = document.getElementById("cs-preview");
    if (!el) return;
    const ini = parseLocalDateTime(document.getElementById("cs-inicio").value);
    const fin = parseLocalDateTime(document.getElementById("cs-fin").value);
    if (Number.isNaN(ini.getTime()) || Number.isNaN(fin.getTime()) || !(fin > ini)) {
      el.textContent = "";
      return;
    }
    const fmt = new Intl.DateTimeFormat("es-AR", { day: "2-digit", month: "short", hour: "2-digit", minute: "2-digit" });
    el.textContent = `Publicarás: inicia ${fmt.format(ini)} · cierra ${fmt.format(fin)}`;
  }

  ["cs-inicio", "cs-fin"].forEach((id) => {
    const n = document.getElementById(id);
    if (n) n.addEventListener("input", actualizarPreview);
  });

  function iniciar() {
    const sesion = getSession();
    if (!sesion) {
      location.href = "acceder.html";
      return;
    }

    // Fechas por defecto (hora local): arranca ahora y cierra 10 minutos después (mismo día).
    const ahora = Date.now();
    document.getElementById("cs-inicio").value = aLocal(ahora);
    document.getElementById("cs-fin").value = aLocal(ahora + 10 * 60000);

    cargarCategorias();
    actualizarPreview();
  }

  async function cargarCategorias() {
    try {
      const res = await fetch(`${API_BASE}/categories`);
      if (!res.ok) throw new Error("No se pudieron cargar las categorías.");
      const cats = await res.json();
      selectCat.innerHTML = '<option value="" selected disabled>Elegí una categoría</option>' +
        cats.map((c) => `<option value="${c.id}">${c.name}</option>`).join("");
    } catch (err) {
      if (esErrorDeConexion(err)) return cerrarSesionPorConexion();
      selectCat.innerHTML = '<option value="" selected disabled>Sin categorías</option>';
      mostrarAlerta(mensajeDeError(err), true);
    }
  }

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    mostrarAlerta("", false);
    alerta.classList.add("d-none");

    if (!form.checkValidity()) {
      form.classList.add("was-validated");
      return;
    }

    const sesion = getSession();
    const basePrice = Number(document.getElementById("cs-base").value);
    const minIncrement = Number(document.getElementById("cs-incremento").value);
    const inicio = parseLocalDateTime(document.getElementById("cs-inicio").value);
    const fin = parseLocalDateTime(document.getElementById("cs-fin").value);

    if (minIncrement > basePrice)
      return mostrarAlerta("El incremento mínimo no puede superar el precio base.", true);
    if (Number.isNaN(inicio.getTime()) || Number.isNaN(fin.getTime()))
      return mostrarAlerta("Ingresá fechas y horas válidas.", true);
    if (fin <= Date.now())
      return mostrarAlerta(`La fecha de cierre ya pasó (ahora son las ${horaActual()}). Elegí un cierre futuro.`, true);
    if (!(fin > inicio))
      return mostrarAlerta("La fecha de cierre debe ser posterior a la fecha de inicio.", true);

    const payload = {
      sellerId: sesion.userId,
      categoryId: Number(selectCat.value),
      title: document.getElementById("cs-titulo").value.trim(),
      descripcion: document.getElementById("cs-descripcion").value.trim(),
      urlImagen: document.getElementById("cs-imagen").value.trim(),
      basePrice,
      minIncrement,
      startDate: inicio.toISOString(),
      endDate: fin.toISOString(),
    };
    if (!payload.urlImagen)
      payload.urlImagen = `https://picsum.photos/seed/subasta${Date.now()}/600/400`;

    btn.disabled = true;
    const original = btn.innerHTML;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Publicando...';

    try {
      const res = await fetch(`${API_BASE}/auctions`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });
      if (!res.ok) {
        const body = await res.json().catch(() => ({}));
        const detalle =
          body.error ??
          (body.errors ? Object.values(body.errors).flat()[0] : null) ??
          "No se pudo publicar la subasta.";
        throw new Error(detalle);
      }
      const creada = await res.json();
      location.href = `subasta.html?id=${creada.id}`;
    } catch (err) {
      if (esErrorDeConexion(err)) return cerrarSesionPorConexion();
      mostrarAlerta(mensajeDeError(err), true);
    } finally {
      btn.disabled = false;
      btn.innerHTML = original;
    }
  });

  iniciar();
})();