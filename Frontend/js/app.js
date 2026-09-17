const API_BASE = "http://localhost:5253/api/v1";

const SESSION_KEY = "subastaya_session";

const ESTADOS = {
  ACTIVA: { label: "Activa", clase: "bg-success" },
  PROXIMA: { label: "Próxima", clase: "bg-secondary" },
  FINALIZADA: { label: "Finalizada", clase: "bg-danger" },
  DESIERTA: { label: "Desierta", clase: "bg-dark" },
};

function normalizarEstado(estado, inicio, fin) {
  const n = String(estado ?? "").toUpperCase().trim();
  if (ESTADOS[n]) return n;
  // Fallback: si el backend manda un valor raro (o null), se deduce de las fechas.
  const ahora = new Date();
  if (fin && new Date(fin) <= ahora) return "FINALIZADA";
  if (inicio && new Date(inicio) > ahora) return "PROXIMA";
  return "ACTIVA";
}

function normalizarSubasta(a) {
  return {
    id: a.id,
    titulo: a.title,
    descripcion: a.descripcion,
    urlImagen: a.urlImagen || `https://picsum.photos/seed/subasta${a.id}/600/400`,
    categoria: a.categoria || "General",
    vendedorId: a.sellerId ?? null,
    precioBase: a.basePrice,
    minIncremento: a.minIncrement,
    ofertaActual: a.ofertaActual ?? a.basePrice,
    cantidadPujas: a.cantidadPujas ?? 0,
    fechaInicio: a.startDate,
    fechaFin: a.endDate,
    estado: normalizarEstado(a.status, a.startDate, a.endDate),
  };
}

// Respaldo offline en el catálogo, controlado con una variable explícita.
// Los datos de ejemplo se regeneran en cada llamada para que contadores y
// estados sigan siendo coherentes con "ahora" mientras la API esté caída.
const USE_MOCK_FALLBACK = true;

function mockSubastas() {
  const ahora = new Date();
  const dias = (n) => new Date(ahora.getTime() + n * 86400000);
  const horas = (n) => new Date(ahora.getTime() + n * 3600000);
  return [
    { id: 1, titulo: "Notebook Gamer RTX 16GB", descripcion: "Demo sin conexión: subasta en curso.", urlImagen: "https://picsum.photos/seed/notebook/600/400", categoria: "Electrónica", precioBase: 40000, ofertaActual: 45000, cantidadPujas: 2, fechaInicio: horas(-2), fechaFin: dias(2), estado: "ACTIVA" },
    { id: 2, titulo: "Figura de colección edición limitada", descripcion: "Demo sin conexión: comienza mañana.", urlImagen: "https://picsum.photos/seed/figura/600/400", categoria: "Coleccionables", precioBase: 15000, ofertaActual: null, cantidadPujas: 0, fechaInicio: horas(24), fechaFin: dias(3), estado: "PROXIMA" },
    { id: 3, titulo: "Juego de living de roble", descripcion: "Demo sin conexión: finalizada con ganador.", urlImagen: "https://picsum.photos/seed/living/600/400", categoria: "Hogar", precioBase: 50000, ofertaActual: 60000, cantidadPujas: 1, fechaInicio: horas(-120), fechaFin: horas(-1), estado: "FINALIZADA" },
    { id: 4, titulo: "Monitor 27'' 144Hz", descripcion: "Demo sin conexión: desierta, sin pujas.", urlImagen: "https://picsum.photos/seed/monitor/600/400", categoria: "Electrónica", precioBase: 80000, ofertaActual: null, cantidadPujas: 0, fechaInicio: horas(-120), fechaFin: horas(-2), estado: "DESIERTA" },
  ];
}

document.addEventListener("DOMContentLoaded", () => {
  if (document.getElementById("grid-subastas")) initCatalogo();
  if (document.getElementById("form-login")) initLogin();
  if (document.getElementById("form-registro")) initRegistro();
  if (document.getElementById("billetera-app")) initBilletera();
  if (document.getElementById("estado-api")) verificarAPI();
  actualizarNav();
  mostrarAvisoSinConexion();
});

async function mostrarAvisoSinConexion() {
  const aviso = document.getElementById("alerta-sin-conexion");
  if (!aviso) return;
  if (!new URLSearchParams(location.search).get("sin-conexion")) return;

  // Solo se muestra el aviso si la API sigue caída. Si responde, se limpia el
  // parámetro y el usuario entra con normalidad a su sesión.
  const ctrl = new AbortController();
  const tiempo = setTimeout(() => ctrl.abort(), 4000);
  try {
    const res = await fetch(`${API_BASE}/users/1/wallets`, { signal: ctrl.signal });
    if (res.ok) {
      history.replaceState(null, "", location.pathname);
      return;
    }
  } catch {
    // sigue abajo: se muestra el aviso
  } finally {
    clearTimeout(tiempo);
  }
  aviso.classList.remove("d-none");
}

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
  const sesion = getSession();

  // Links privados (Mis actividades, Mi billetera, Crear subasta): solo con sesión.
  document.querySelectorAll("[data-solo-login]").forEach((el) => {
    el.classList.toggle("d-none", !sesion);
  });

  if (!accion) return;
  if (sesion) {
    accion.innerHTML = `
      <span class="chip-usuario me-2"><i class="bi bi-person-circle me-1"></i>${sesion.name}</span>
      <button class="btn btn-outline-light btn-sm" onclick="cerrarSesion()"><i class="bi bi-box-arrow-right me-1"></i>Cerrar sesión</button>`;
  } else {
    accion.innerHTML =
      '<a class="btn btn-light btn-sm" href="acceder.html"><i class="bi bi-box-arrow-in-right me-1"></i>Acceder</a>';
  }
}

/* ============ Catálogo ============ */
async function fetchSubastas() {
  try {
    // Listado real desde la API.
    const res = await fetch(`${API_BASE}/auctions`);
    if (!res.ok) throw new Error("sin respuesta");
    const data = await res.json();
    return data.map(normalizarSubasta);
  } catch {
    // Respaldo solo si la variable lo habilita; el aviso oculta el modo demo en la UI.
    if (!USE_MOCK_FALLBACK) return [];
    const aviso = document.getElementById("aviso-demo");
    if (aviso) aviso.classList.remove("d-none");
    return mockSubastas().map(normalizarSubasta);
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

  if (subasta.estado === "DESIERTA")
    return { texto: "Desierta", clase: "finalizada" };

  if (subasta.estado === "FINALIZADA" || fin <= ahora)
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

function cardSubasta(a, i) {
  const estado = ESTADOS[a.estado] ?? { label: a.estado, clase: "bg-secondary" };
  const contador = textoContador(a);
  return `
    <div class="col" data-id="${a.id}">
      <div class="card card-subasta h-100" style="animation-delay:${Math.min(i, 8) * 70}ms">
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
          <a class="btn btn-verde w-100" href="subasta.html?id=${a.id}">Ver sala de pujas</a>
        </div>
      </div>
    </div>`;
}

const PASO_CATALOGO = 5;

function renderSubastas() {
  const lista = window.__filtradas.slice(0, window.__visibles);
  const grid = document.getElementById("grid-subastas");
  const vacio = document.getElementById("sin-resultados");
  const cantidad = document.getElementById("cantidad-resultados");
  const verMasWrap = document.getElementById("ver-mas-wrap");
  grid.innerHTML = lista.map((a, i) => cardSubasta(a, i)).join("");
  vacio.classList.toggle("d-none", window.__filtradas.length > 0);
  if (cantidad) {
    const total = window.__filtradas.length;
    if (window.__modoCatalogo === "destacadas") {
      cantidad.textContent = `${total} subasta${total === 1 ? "" : "s"} destacada${total === 1 ? "" : "s"}`;
    } else {
      cantidad.textContent = `${total} remate${total === 1 ? "" : "s"}${total > PASO_CATALOGO ? ` (mostrando ${Math.min(window.__visibles, total)})` : ""}`;
    }
  }
  if (verMasWrap) {
    const inactivo =
      window.__modoCatalogo === "destacadas" || window.__visibles >= window.__filtradas.length;
    verMasWrap.classList.toggle("d-none", inactivo);
  }
  iniciarCountdowns(grid);
}

function mostrarMas() {
  window.__visibles += PASO_CATALOGO;
  renderSubastas();
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

  window.__filtradas = lista;
  window.__visibles = PASO_CATALOGO;
  renderSubastas();
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
  const esCatalogoCompleto = Boolean(document.getElementById("f-condicion"));

  if (esCatalogoCompleto) {
    window.__modoCatalogo = "completo";
    cargarCategorias();
    aplicarFiltros();

    ["f-condicion", "f-categoria", "f-min", "f-max", "f-orden", "f-buscar"].forEach((id) => {
      const el = document.getElementById(id);
      if (el) el.addEventListener("input", aplicarFiltros);
    });

    const btnVerMas = document.getElementById("btn-ver-mas");
    if (btnVerMas) btnVerMas.addEventListener("click", mostrarMas);
    return;
  }

  // Index: solo 5 destacadas (activas/próximas por orden de cierre).
  window.__modoCatalogo = "destacadas";
  const destacadas = window.__subastas
    .filter((a) => a.estado === "ACTIVA" || a.estado === "PROXIMA")
    .sort((a, b) => new Date(a.fechaFin) - new Date(b.fechaFin))
    .slice(0, PASO_CATALOGO);
  window.__filtradas = destacadas;
  window.__visibles = PASO_CATALOGO;
  renderSubastas();
}

function esErrorDeConexion(err) {
  return (
    err instanceof TypeError ||
    /failed to fetch|networkerror|load failed|fetch/i.test(err.message ?? "")
  );
}

function mensajeDeError(err) {
  if (esErrorDeConexion(err)) {
    return "No se pudo conectar con el servidor. Revisá que la API esté corriendo (dotnet run --project SubastaYa) en http://localhost:5253";
  }
  return err.message || "Ocurrió un error inesperado.";
}

// Si la API no está disponible y la cuenta tiene sesión abierta, se cierra la
// sesión y se redirige al login con un aviso.
function cerrarSesionPorConexion() {
  localStorage.removeItem(SESSION_KEY);
  location.href = "acceder.html?sin-conexion=1";
}

async function verificarAPI() {
  const el = document.getElementById("estado-api");
  if (!el) return;
  const ctrl = new AbortController();
  const tiempo = setTimeout(() => ctrl.abort(), 4000);
  try {
    const res = await fetch(`${API_BASE}/users/1/wallets`, { signal: ctrl.signal });
    el.classList.remove("barra-conexion-checking");
    el.classList.toggle("barra-conexion-ok", res.ok);
    el.classList.toggle("barra-conexion-error", !res.ok);
  } catch {
    el.classList.remove("barra-conexion-checking");
    el.classList.add("barra-conexion-error");
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

/* ============ Registro ============ */
function initRegistro() {
  const form = document.getElementById("form-registro");
  const alerta = document.getElementById("alert-registro");

  const confirmar = document.getElementById("password-confirm");
  confirmar.addEventListener("input", () => {
    const coincide = confirmar.value === document.getElementById("password").value;
    confirmar.setCustomValidity(coincide ? "" : "no-coinciden");
  });

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    alerta.classList.add("d-none");

    if (!form.checkValidity()) {
      form.classList.add("was-validated");
      return;
    }

    const btns = document.getElementById("btn-registrarse");
    const btnOriginal = btns.innerHTML;
    btns.disabled = true;
    btns.innerHTML =
      '<span class="spinner-border spinner-border-sm me-2"></span>Creando cuenta...';

    const name = document.getElementById("nombre").value.trim();
    const email = document.getElementById("email").value.trim();
    const password = document.getElementById("password").value;

    try {
      const res = await fetch(`${API_BASE}/users`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, name, password }),
      });

      if (!res.ok) {
        const body = await res.json().catch(() => ({}));
        throw new Error(body.error ?? "No se pudo crear la cuenta.");
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
};

const INFO_MOVIMIENTO = {
  DEPOSITO: { texto: "Depósito", clase: "bg-success" },
  RETIRO: { texto: "Retiro", clase: "bg-danger" },
  RETENCION: { texto: "Retención", clase: "bg-secondary" },
  LIBERACION: { texto: "Liberación", clase: "bg-warning text-dark" },
  PAGO: { texto: "Pago de subasta", clase: "bg-danger" },
  COBRO: { texto: "Cobro de subasta", clase: "bg-success" },
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
    if (esErrorDeConexion(err)) return cerrarSesionPorConexion();
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
    if (esErrorDeConexion(err)) return cerrarSesionPorConexion();
    mostrarMensaje(mensajeDeError(err), true);
  } finally {
    loader.classList.add("d-none");
  }
}

function filaMovimiento(t) {
  const info = INFO_MOVIMIENTO[t.type] ?? { texto: t.type ?? "—", clase: "bg-secondary" };
  const esEntrada = t.type === "DEPOSITO" || t.type === "LIBERACION" || t.type === "COBRO";
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
    if (esErrorDeConexion(err)) return cerrarSesionPorConexion();
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