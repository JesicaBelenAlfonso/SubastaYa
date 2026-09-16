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

  function aLocal(ms) {
    const d = new Date(ms);
    const pad = (n) => String(n).padStart(2, "0");
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }

  function iniciar() {
    const sesion = getSession();
    if (!sesion) {
      location.href = "acceder.html";
      return;
    }

    // Fechas por defecto: arranca ahora y cierra en 48 h.
    const ahora = Date.now();
    document.getElementById("cs-inicio").value = aLocal(ahora);
    document.getElementById("cs-fin").value = aLocal(ahora + 48 * 3600000);

    cargarCategorias();
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
    const inicio = new Date(document.getElementById("cs-inicio").value);
    const fin = new Date(document.getElementById("cs-fin").value);

    if (minIncrement > basePrice)
      return mostrarAlerta("El incremento mínimo no puede superar el precio base.", true);
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
        throw new Error(body.error ?? "No se pudo publicar la subasta.");
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