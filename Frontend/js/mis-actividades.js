/* ============ Mis actividades (vendedor / pujador) ============ */
(() => {
  const app = document.getElementById("actividades-app");
  if (!app) return;

  const alerta = document.getElementById("alerta-actividades");
  const loader = document.getElementById("loader-actividades");
  const contenido = document.getElementById("actividades-contenido");

  function mostrarAlerta(texto, esError) {
    alerta.textContent = texto;
    alerta.className = `alert ${esError ? "alert-danger" : "alert-success"}`;
    alerta.classList.remove("d-none");
    clearTimeout(window.__timerAct);
    window.__timerAct = setTimeout(() => alerta.classList.add("d-none"), 6000);
  }

  function cardActividad(item) {
    const a = normalizarSubasta(item.auction);
    const estado = ESTADOS[a.estado] ?? { label: a.estado, clase: "bg-secondary" };
    const lider = item.role === "PUJADOR" && item.liderando
      ? '<span class="badge bg-success"><i class="bi bi-trophy me-1"></i>Liderando</span>'
      : item.role === "PUJADOR" && !item.liderando
        ? '<span class="badge bg-warning text-dark"><i class="bi bi-arrow-up me-1"></i>Superado</span>'
        : "";
    return `
      <div class="col-md-6">
        <div class="card card-subasta h-100">
          <div class="card-body d-flex flex-column">
            <div class="d-flex align-items-center gap-2 mb-2">
              <span class="badge est-badge ${estado.clase}">${estado.label}</span>
              <span class="badge badge-categoria">${a.categoria ?? "General"}</span>
              ${lider}
            </div>
            <h3 class="titulo-subasta">${a.titulo}</h3>
            <p class="text-muted small flex-grow-1">${a.descripcion ?? ""}</p>
            <div class="d-flex justify-content-between align-items-center mt-2">
              <span class="oferta-actual">${formatearPrecio(a.ofertaActual ?? a.precioBase)}</span>
              <span class="badge bg-light text-dark border">${a.cantidadPujas ?? 0} pujas</span>
            </div>
            <div class="d-flex justify-content-between align-items-center mt-3">
              <span class="small text-muted">Precio base ${formatearPrecio(a.precioBase)}</span>
              <span class="contador">${textoContador(a).texto}</span>
            </div>
          </div>
          <div class="card-footer bg-white border-0">
            <a class="btn btn-verde w-100" href="subasta.html?id=${a.id}">Abrir sala de pujas</a>
          </div>
        </div>
      </div>`;
  }

  async function cargar() {
    const sesion = getSession();
    if (!sesion) {
      location.href = "acceder.html";
      return;
    }

    loader.classList.remove("d-none");
    contenido.classList.add("d-none");
    try {
      const res = await fetch(`${API_BASE}/users/${sesion.userId}/activities`);
      if (res.status === 404) throw new Error("El usuario no existe o fue eliminado.");
      if (!res.ok) throw new Error("No se pudieron cargar tus actividades.");
      const lista = await res.json();

      const vendedor = lista.filter((i) => i.role === "VENDEDOR");
      const pujador = lista.filter((i) => i.role === "PUJADOR");

      document.getElementById("grid-vendedor").innerHTML = vendedor.map(cardActividad).join("");
      document.getElementById("grid-pujador").innerHTML = pujador.map(cardActividad).join("");
      document.getElementById("vacio-vendedor").classList.toggle("d-none", vendedor.length > 0);
      document.getElementById("vacio-pujador").classList.toggle("d-none", pujador.length > 0);

      contenido.classList.remove("d-none");
    } catch (err) {
      if (esErrorDeConexion(err)) return cerrarSesionPorConexion();
      mostrarAlerta(mensajeDeError(err), true);
    } finally {
      loader.classList.add("d-none");
    }
  }

  cargar();
})();