const API_BASE = "http://localhost:5253/api/v1";

const SESSION_KEY = "subastaya_session";

const ESTADOS = {
  Activa: { label: "Activa", clase: "bg-success" },
  Proxima: { label: "Próxima", clase: "bg-secondary" },
  Finalizada: { label: "Finalizada", clase: "bg-danger" },
};

const MOCK_AUCTIONS = [
  {
    id: 1,
    titulo: "Reloj de bolsillo Longines 1920",
    descripcion: "Reloj de bolsillo suizo con esfera esmaltada.",
    urlImagen: "https://picsum.photos/seed/subasta1/600/400",
    categoria: "Antigüedades",
    precioBase: 45000,
    ofertaActual: 78000,
    cantidadPujas: 14,
    fechaInicio: "2026-08-01T10:00:00Z",
    fechaFin: "2026-09-15T20:00:00Z",
    estado: "Activa",
  },
  {
    id: 2,
    titulo: "Mesa ratona estilo Luis XV",
    descripcion: "Madera de nogal tallada a mano.",
    urlImagen: "https://picsum.photos/seed/subasta2/600/400",
    categoria: "Muebles",
    precioBase: 220000,
    ofertaActual: 275000,
    cantidadPujas: 9,
    fechaInicio: "2026-08-05T10:00:00Z",
    fechaFin: "2026-09-12T20:00:00Z",
    estado: "Activa",
  },
  {
    id: 3,
    titulo: "Colección de monedas argentinas",
    descripcion: "Lote de 40 piezas del período 1880-1950.",
    urlImagen: "https://picsum.photos/seed/subasta3/600/400",
    categoria: "Numismática",
    precioBase: 95000,
    ofertaActual: 132000,
    cantidadPujas: 21,
    fechaInicio: "2026-08-10T10:00:00Z",
    fechaFin: "2026-09-18T20:00:00Z",
    estado: "Activa",
  },
  {
    id: 4,
    titulo: "Lámpara de pie art déco",
    descripcion: "Bronce y vidrio opalino, década del 30.",
    urlImagen: "https://picsum.photos/seed/subasta4/600/400",
    categoria: "Iluminación",
    precioBase: 51000,
    ofertaActual: 51000,
    cantidadPujas: 2,
    fechaInicio: "2026-09-05T10:00:00Z",
    fechaFin: "2026-10-02T20:00:00Z",
    estado: "Proxima",
  },
  {
    id: 5,
    titulo: "Oleografía firmada, paisaje pampeano",
    descripcion: "Marcos en madera dorada, certificada.",
    urlImagen: "https://picsum.photos/seed/subasta5/600/400",
    categoria: "Arte",
    precioBase: 180000,
    ofertaActual: 198500,
    cantidadPujas: 11,
    fechaInicio: "2026-07-20T10:00:00Z",
    fechaFin: "2026-09-02T20:00:00Z",
    estado: "Finalizada",
  },
  {
    id: 6,
    titulo: "Juego de té inglés Royal Doulton",
    descripcion: "Porcelana, 12 piezas, perfecto estado.",
    urlImagen: "https://picsum.photos/seed/subasta6/600/400",
    categoria: "Porcelana",
    precioBase: 88000,
    ofertaActual: 143000,
    cantidadPujas: 17,
    fechaInicio: "2026-08-08T10:00:00Z",
    fechaFin: "2026-09-20T20:00:00Z",
    estado: "Activa",
  },
];

document.addEventListener("DOMContentLoaded", () => {
  if (document.getElementById("grid-subastas")) initCatalogo();
  if (document.getElementById("form-login")) initLogin();
  if (document.getElementById("billetera-app")) initBilletera();
  if (document.getElementById("estado-api")) verificarAPI();
  actualizarNav();
});

/* ============ Sesión ============ */
function getSession() {
  try {
    return JSON.parse(localStorage.getItem(SESSION_KEY));
  } catch {
    return null;
  }
}

function setSession(session) {
  localStorage.setItem(SESSION_KEY, JSON.stringify(session));
}

function cerrarSesion() {
  localStorage.removeItem(SESSION_KEY);
  location.href = "index.html";
}

function actualizarNav() {
  const accion = document.getElementById("nav-accion");
  if (!accion) return;
  const sesion = getSession();
  if (sesion) {
    accion.innerHTML = `
      <a class="btn btn-outline-light btn-sm me-2" href="billetera.html"><i class="bi bi-wallet2 me-1"></i>Mi billetera</a>
      <span class="text-light me-2"><i class="bi bi-person-circle"></i> ${sesion.name}</span>
      <button class="btn btn-outline-light btn-sm" onclick="cerrarSesion()">Cerrar sesión</button>`;
  } else {
    accion.innerHTML =
      '<a class="btn btn-light btn-sm" href="acceder.html"><i class="bi bi-box-arrow-in-right me-1"></i>Acceder</a>';
  }
}

/* ============ Catálogo ============ */
async function fetchSubastas() {
  try {
    // Endpoint futuro (a cargo de la compañera); si no responde, usamos el catálogo de ejemplo.
    const res = await fetch(`${API_BASE}/subastas`);
    if (!res.ok) throw new Error("sin respuesta");
    return await res.json();
  } catch {
    return MOCK_AUCTIONS;
  }
}

function formatearPrecio(n) {
  return new Intl.NumberFormat("es-AR", {
    style: "currency",
    currency: "ARS",
    maximumFractionDigits: 0,
  }).format(n);
}

function textoContador(subasta) {
  const fin = new Date(subasta.fechaFin);
  const inicio = new Date(subasta.fechaInicio);
  const ahora = new Date();

  if (subasta.estado === "Finalizada" || fin <= ahora)
    return { texto: "Finalizada", clase: "finalizada" };

  if (inicio > ahora) {
    const dias = Math.ceil((inicio - ahora) / 86400000);
    return { texto: `Comienza en ${dias} día${dias === 1 ? "" : "s"}`, clase: "" };
  }

  const restante = Math.max(0, fin - ahora);
  const d = Math.floor(restante / 86400000);
  const h = Math.floor((restante % 86400000) / 3600000);
  const m = Math.floor((restante % 3600000) / 60000);
  const s = Math.floor((restante % 60000) / 1000);

  const dd = String(d).padStart(2, "0");
  const hh = String(h).padStart(2, "0");
  const mm = String(m).padStart(2, "0");
  const ss = String(s).padStart(2, "0");
  return { texto: `${dd}:${hh}:${mm}:${ss}`, clase: "" };
}

function cardSubasta(a) {
  const estado = ESTADOS[a.estado] ?? { label: a.estado, clase: "bg-secondary" };
  const contador = textoContador(a);
  return `
    <div class="col" data-id="${a.id}">
      <div class="card card-subasta h-100">
        <div class="img-wrap position-relative">
          <img src="${a.urlImagen}" alt="${a.titulo}" loading="lazy" />
          <span class="position-absolute top-0 start-0 m-2 badge est-badge ${estado.clase}">${estado.label}</span>
        </div>
        <div class="card-body d-flex flex-column">
          <span class="badge badge-categoria align-self-start mb-2">${a.categoria ?? "General"}</span>
          <h3 class="titulo-subasta">${a.titulo}</h3>
          <p class="text-muted small flex-grow-1">${a.descripcion ?? ""}</p>
          <div class="d-flex justify-content-between align-items-center mt-2">
            <span class="oferta-actual">${formatearPrecio(a.ofertaActual ?? a.precioBase)}</span>
            <span class="badge bg-light text-dark border">${a.cantidadPujas ?? 0} pujas</span>
          </div>
          <div class="d-flex justify-content-between align-items-center mt-3">
            <span class="small text-muted">Precio base ${formatearPrecio(a.precioBase)}</span>
            <span class="contador ${contador.clase}" data-fin="${a.fechaFin}"><i class="bi bi-hourglass-split me-1"></i>${contador.texto}</span>
          </div>
        </div>
        <div class="card-footer bg-white border-0">
          <button class="btn btn-verde w-100" type="button">Ver detalles</button>
        </div>
      </div>
    </div>`;
}

function renderSubastas(lista) {
  const grid = document.getElementById("grid-subastas");
  const vacio = document.getElementById("sin-resultados");
  const cantidad = document.getElementById("cantidad-resultados");
  grid.innerHTML = lista.map(cardSubasta).join("");
  vacio.classList.toggle("d-none", lista.length > 0);
  if (cantidad) cantidad.textContent = `${lista.length} remate${lista.length === 1 ? "" : "s"}`;
  iniciarCountdowns(grid);
}

function aplicarFiltros() {
  const estado = document.getElementById("f-condicion").value;
  const categoria = document.getElementById("f-categoria").value;
  const min = Number(document.getElementById("f-min").value) || 0;
  const max = Number(document.getElementById("f-max").value) || Infinity;
  const orden = document.getElementById("f-orden").value;
  const buscar = document.getElementById("f-buscar").value.trim().toLowerCase();

  let lista = window.__subastas.filter((a) => {
    const okEstado = !estado || a.estado === estado;
    const okCat = !categoria || a.categoria === categoria;
    const precio = a.ofertaActual ?? a.precioBase;
    const okPrecio = precio >= min && precio <= max;
    const okBusqueda =
      !buscar || a.titulo.toLowerCase().includes(buscar) || (a.categoria ?? "").toLowerCase().includes(buscar);
    return okEstado && okCat && okPrecio && okBusqueda;
  });

  switch (orden) {
    case "precio-desc":
      lista.sort((a, b) => (b.ofertaActual ?? 0) - (a.ofertaActual ?? 0));
      break;
    case "precio-asc":
      lista.sort((a, b) => (a.ofertaActual ?? 0) - (b.ofertaActual ?? 0));
      break;
    case "final":
      lista.sort((a, b) => new Date(a.fechaFin) - new Date(b.fechaFin));
      break;
    default:
      lista.sort((a, b) => new Date(b.fechaInicio) - new Date(a.fechaInicio));
  }

  renderSubastas(lista);
}

function iniciarCountdowns(contenedor) {
  contenedor.querySelectorAll(".contador[data-fin]").forEach((el) => {
    if (el.dataset.timer) return;
    el.dataset.timer = "1";
    const card = el.closest(".col[data-id]");
    const subasta = window.__subastas.find((s) => String(s.id) === card.dataset.id);
    if (!subasta) return;

    const loop = () => {
      const c = textoContador(subasta);
      el.innerHTML = `<i class="bi bi-hourglass-split me-1"></i>${c.texto}`;
      if (c.clase) el.classList.add("finalizada");
    };
    loop();
    setInterval(loop, 1000);
  });
}

function cargarCategorias() {
  const select = document.getElementById("f-categoria");
  if (!select) return;
  const categorias = [...new Set(window.__subastas.map((a) => a.categoria).filter(Boolean))];
  categorias.forEach((c) => {
    const op = document.createElement("option");
    op.value = c;
    op.textContent = c;
    select.appendChild(op);
  });
}

async function initCatalogo() {
  window.__subastas = await fetchSubastas();
  cargarCategorias();
  aplicarFiltros();

  ["f-condicion", "f-categoria", "f-min", "f-max", "f-orden", "f-buscar"].forEach((id) => {
    document.getElementById(id).addEventListener("input", aplicarFiltros);
  });
}

function mensajeDeError(err) {
  if (
    err instanceof TypeError ||
    /failed to fetch|networkerror|load failed|fetch/i.test(err.message ?? "")
  ) {
    return "No se pudo conectar con el servidor. Revisá que la API esté corriendo (dotnet run --project SubastaYa) en http://localhost:5253";
  }
  return err.message || "Ocurrió un error inesperado.";
}

async function verificarAPI() {
  const el = document.getElementById("estado-api");
  if (!el) return;
  const ctrl = new AbortController();
  const tiempo = setTimeout(() => ctrl.abort(), 4000);
  try {
    const res = await fetch(`${API_BASE}/users/1/wallets`, { signal: ctrl.signal });
    el.innerHTML = `<i class="bi bi-check-circle me-1"></i>Servidor conectado (HTTP ${res.status})`;
    el.className = "text-center small fw-semibold text-success";
  } catch {
    el.innerHTML =
      '<i class="bi bi-x-circle me-1"></i>Sin conexión con el servidor. ¿Está corriendo la API?';
    el.className = "text-center small fw-semibold text-danger";
  } finally {
    clearTimeout(tiempo);
  }
}

/* ============ Login ============ */
function initLogin() {
  const form = document.getElementById("form-login");
  const alerta = document.getElementById("alert-login");

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    alerta.classList.add("d-none");

    if (!form.checkValidity()) {
      form.classList.add("was-validated");
      return;
    }

    const btns = document.getElementById("btn-ingresar");
    const btnOriginal = btns.innerHTML;
    btns.disabled = true;
    btns.innerHTML =
      '<span class="spinner-border spinner-border-sm me-2"></span>Ingresando...';

    const email = document.getElementById("email").value.trim();
    const password = document.getElementById("password").value;

    try {
      const res = await fetch(`${API_BASE}/auth/sessions`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
      });

      if (!res.ok) {
        const body = await res.json().catch(() => ({}));
        throw new Error(body.error ?? "No se pudo iniciar sesión");
      }

      const user = await res.json();
      setSession({ userId: user.id, name: user.name, email: user.email });
      location.href = "index.html";
    } catch (err) {
      alerta.textContent = mensajeDeError(err);
      alerta.classList.remove("d-none");
    } finally {
      btns.disabled = false;
      btns.innerHTML = btnOriginal;
    }
  });
}

/* ============ Billetera ============ */
const AYUDA_MOVIMIENTO = {
  DEPOSITO: "Suma dinero a tu saldo disponible (recarga).",
  RETIRO: "Retira dinero de tu saldo disponible.",
  RETENCION: "Congela dinero para garantizar una oferta.",
  LIBERACION: "Devuelve dinero congelado a tu saldo disponible.",
};

const INFO_MOVIMIENTO = {
  DEPOSITO: { texto: "Depósito", clase: "bg-success" },
  RETIRO: { texto: "Retiro", clase: "bg-danger" },
  RETENCION: { texto: "Retención", clase: "bg-secondary" },
  LIBERACION: { texto: "Liberación", clase: "bg-warning text-dark" },
};

async function initBilletera() {
  const sesion = getSession();
  if (!sesion) {
    location.href = "acceder.html";
    return;
  }
  window.__walletUserId = sesion.userId;
  const nombre = document.getElementById("lbl-usuario");
  if (nombre) nombre.textContent = sesion.name;

  const tipo = document.getElementById("mov-tipo");
  if (tipo) {
    tipo.addEventListener("change", mostrarAyudaMovimiento);
    mostrarAyudaMovimiento();
    document.getElementById("form-movimiento").addEventListener("submit", (event) => {
      event.preventDefault();
      hacerMovimiento();
    });
  }

  await cargarBilletera();
  await cargarMovimientos();
}

async function cargarBilletera() {
  document.getElementById("loader-billetera").classList.remove("d-none");
  document.getElementById("billetera-cards").classList.add("d-none");
  try {
    const res = await fetch(`${API_BASE}/users/${window.__walletUserId}/wallets`);
    if (!res.ok) throw new Error("No se pudo obtener tu billetera.");
    const b = await res.json();
    document.getElementById("tot-disponible").textContent = formatearPrecio(b.availableBalance);
    document.getElementById("tot-congelado").textContent = formatearPrecio(b.heldBalance);
    document.getElementById("tot-total").textContent = formatearPrecio(b.totalBalance);
  } catch (err) {
    mostrarMensaje(mensajeDeError(err), true);
  } finally {
    document.getElementById("loader-billetera").classList.add("d-none");
    document.getElementById("billetera-cards").classList.remove("d-none");
  }
}

async function cargarMovimientos() {
  const tbody = document.getElementById("tabla-movimientos");
  const vacio = document.getElementById("sin-movimientos");
  const loader = document.getElementById("loader-movimientos");
  tbody.innerHTML = "";
  loader.classList.remove("d-none");
  try {
    const res = await fetch(`${API_BASE}/users/${window.__walletUserId}/wallets/transactions`);
    if (!res.ok) throw new Error("No se pudieron obtener los movimientos.");
    const lista = await res.json();
    vacio.classList.toggle("d-none", lista.length > 0);
    tbody.innerHTML = lista.map(filaMovimiento).join("");
  } catch (err) {
    mostrarMensaje(mensajeDeError(err), true);
  } finally {
    loader.classList.add("d-none");
  }
}

function filaMovimiento(t) {
  const info = INFO_MOVIMIENTO[t.type] ?? { texto: t.type ?? "—", clase: "bg-secondary" };
  const esEntrada = t.type === "DEPOSITO" || t.type === "LIBERACION";
  const fecha = new Date(t.date).toLocaleString("es-AR", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
  return `
    <tr>
      <td><span class="badge ${info.clase}">${info.texto}</span></td>
      <td class="${esEntrada ? "text-success fw-semibold" : "text-danger fw-semibold"}">${esEntrada ? "+" : "−"}${formatearPrecio(t.amount)}</td>
      <td class="text-muted small">${fecha}</td>
      <td class="text-muted small">${t.auctionId ? `Subasta #${t.auctionId}` : "—"}</td>
    </tr>`;
}

async function hacerMovimiento() {
  const tipo = document.getElementById("mov-tipo").value;
  const montoEl = document.getElementById("mov-monto");
  const monto = Number(montoEl.value);
  const btn = document.getElementById("btn-movimiento");

  if (!tipo) return mostrarMensaje("Elegí un tipo de movimiento.", true);
  if (!monto || monto <= 0) return mostrarMensaje("Ingresá un monto mayor a 0.", true);

  btn.disabled = true;
  btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Procesando...';
  try {
    const res = await fetch(`${API_BASE}/users/${window.__walletUserId}/wallets/transactions`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ type: tipo, amount: monto, auctionId: null }),
    });
    if (!res.ok) {
      const body = await res.json().catch(() => ({}));
      throw new Error(body.error ?? "No se pudo realizar el movimiento.");
    }
    montoEl.value = "";
    mostrarMensaje(`Movimiento realizado con éxito.`, false);
    await cargarBilletera();
    await cargarMovimientos();
  } catch (err) {
    mostrarMensaje(mensajeDeError(err), true);
  } finally {
    btn.disabled = false;
    btn.innerHTML = '<i class="bi bi-arrow-right-circle me-1"></i>Ejecutar movimiento';
  }
}

function mostrarAyudaMovimiento() {
  const tipo = document.getElementById("mov-tipo");
  const ayuda = document.getElementById("ayuda-movimiento");
  if (ayuda) ayuda.textContent = AYUDA_MOVIMIENTO[tipo.value] ?? "";
}

function mostrarMensaje(texto, esError) {
  const alerta = document.getElementById("alerta-billetera");
  if (!alerta) return;
  alerta.textContent = texto;
  alerta.className = `alert mt-4 ${esError ? "alert-danger" : "alert-success"}`;
  alerta.classList.remove("d-none");
  clearTimeout(window.__timerMsg);
  window.__timerMsg = setTimeout(() => alerta.classList.add("d-none"), 6000);
}