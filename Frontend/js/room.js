/* ============ Sala de pujas en vivo (short-polling) ============ */
(() => {
  const params = new URLSearchParams(location.search);
  const auctionId = parseInt(params.get("id") || "0", 10);

  const POLL_MS = 3000;                 // short-polling: refresca oferta + historial
  const ANTI_SNIPING_WINDOW_S = 60;     // replica appsettings (Auction:AntiSnipingWindowSeconds)

  let subasta = null;
  let historial = [];
  let detenido = false;

  const loader = document.getElementById("room-loader");
  const contenido = document.getElementById("room-contenido");
  const noEncontrada = document.getElementById("room-noencontrada");
  const alerta = document.getElementById("alerta-room");
  const form = document.getElementById("form-puja");
  const montoEl = document.getElementById("room-monto");
  const btnPujar = document.getElementById("btn-pujar");

  if (!auctionId || !form) return;

  const sesion = () => getSession();

  function mostrarAlerta(texto, esError) {
    alerta.textContent = texto;
    alerta.className = `alert ${esError ? "alert-danger" : "alert-success"}`;
    alerta.classList.remove("d-none");
    clearTimeout(window.__timerRoom);
    window.__timerRoom = setTimeout(() => alerta.classList.add("d-none"), 6000);
  }

  function mostrarToast(texto) {
    const toastEl = document.getElementById("toast-puja");
    if (!toastEl) return;
    const cuerpo = document.getElementById("toast-puja-body");
    if (cuerpo) cuerpo.textContent = texto;
    clearTimeout(window.__timerToast);
    window.__timerToast = setTimeout(() => {
      if (typeof bootstrap !== "undefined") bootstrap.Toast.getOrCreateInstance(toastEl).hide();
    }, 5000);
    if (typeof bootstrap !== "undefined") bootstrap.Toast.getOrCreateInstance(toastEl).show();
  }

  const esTerminal = (a) =>
    a.estado === "FINALIZADA" || a.estado === "DESIERTA" || new Date(a.fechaFin) <= new Date();

  const restanteMs = () => (subasta ? new Date(subasta.fechaFin) - new Date() : 0);
  const sugerencia = () =>
    subasta ? (subasta.ofertaActual ?? subasta.precioBase) + subasta.minIncremento : 0;

  function miDatoPersonal() {
    const user = sesion();
    const mias = historial.filter((b) => b.isMine);
    const lider = historial.find((b) => b.isLeader);
    const estoyLiderando = mias.length > 0 && lider && lider.isMine;
    return { user, puje: mias.length > 0, estoyLiderando };
  }

  function renderEstadoPersonal() {
    const bloque = document.getElementById("room-lider");
    const aviso = document.getElementById("room-antisniping");
    const requiereLogin = document.getElementById("room-requiere-login");
    const esVendedor = document.getElementById("room-es-vendedor");

    const { user, puje, estoyLiderando } = miDatoPersonal();
    const soyElVendedor = Boolean(user && subasta.vendedorId && user.userId === subasta.vendedorId);

    requiereLogin.classList.toggle("d-none", Boolean(user) || subasta.estado !== "ACTIVA");
    esVendedor.classList.toggle("d-none", !(soyElVendedor && subasta.estado === "ACTIVA"));
    montoEl.disabled = !user || soyElVendedor;
    btnPujar.disabled = !user || soyElVendedor || subasta.estado !== "ACTIVA";
    if (!user) bloque.classList.add("d-none");

    if (subasta.estado !== "ACTIVA") {
      bloque.classList.add("d-none");
      aviso.classList.add("d-none");
      return;
    }

    if (soyElVendedor) {
      bloque.classList.add("d-none");
      aviso.classList.add("d-none");
      return;
    }

    const restanteS = Math.max(0, Math.floor(restanteMs() / 1000));
    const enVentanaSniping = restanteS > 0 && restanteS <= ANTI_SNIPING_WINDOW_S;
    aviso.classList.toggle("d-none", !enVentanaSniping);
    if (enVentanaSniping)
      aviso.querySelector("strong").textContent =
        `Quedan ${restanteS}s. Si pujás ahora, el cierre se extiende 2 minutos.`;

    if (!user) return;
    if (!puje) {
      bloque.className = "badge-lider d-block badge bg-light text-dark border text-start";
      bloque.innerHTML = '<i class="bi bi-person me-1"></i>Tu puja: <strong>todavía no participás</strong>.';
      return;
    }

    if (estoyLiderando) {
      bloque.className = "badge-lider d-block badge bg-success text-white text-start";
      bloque.innerHTML = '<i class="bi bi-trophy me-1"></i><strong>Liderando:</strong> tu oferta es la más alta en este momento.';
    } else {
      bloque.className = "badge-lider d-block badge bg-warning text-dark text-start";
      bloque.innerHTML = '<i class="bi bi-arrow-up me-1"></i><strong>Fuiste superado:</strong> otra oferta quedó por encima de la tuya.';
    }
  }

  function renderSala() {
    if (!subasta) return;

    document.title = `${subasta.titulo} · Sala de pujas · SubastaYa`;
    document.getElementById("room-crumb").textContent = subasta.titulo;
    document.getElementById("room-titulo").textContent = subasta.titulo;
    document.getElementById("room-descripcion").textContent = subasta.descripcion || "Sin descripción.";
    document.getElementById("room-imagen").src = subasta.urlImagen || `https://picsum.photos/seed/subasta${subasta.id}/800/500`;
    document.getElementById("room-imagen").alt = subasta.titulo;

    const estado = ESTADOS[subasta.estado] ?? { label: subasta.estado, clase: "bg-secondary" };
    const badgeEstado = document.getElementById("room-estado");
    badgeEstado.textContent = estado.label;
    badgeEstado.className = `badge est-badge ${estado.clase}`;
    document.getElementById("room-categoria").textContent = subasta.categoria ?? "General";
    document.getElementById("room-oferta").textContent = formatearPrecio(subasta.ofertaActual ?? subasta.precioBase);
    document.getElementById("room-pujas").textContent = subasta.cantidadPujas ?? 0;
    document.getElementById("room-base").textContent = formatearPrecio(subasta.precioBase);
    document.getElementById("room-incremento").textContent = formatearPrecio(subasta.minIncremento);

    const fmt = new Intl.DateTimeFormat("es-AR", { day: "2-digit", month: "short", hour: "2-digit", minute: "2-digit" });
    document.getElementById("room-fechas").textContent = `Inicia ${fmt.format(new Date(subasta.fechaInicio))} · Cierra ${fmt.format(new Date(subasta.fechaFin))}`;

    const contador = document.getElementById("room-contador");
    contador.dataset.fin = subasta.fechaFin;
    const pront = textoContador(subasta);
    contador.textContent = pront.texto;
    contador.classList.remove("urgente", "finalizada");
    if (pront.clase) contador.classList.add(pront.clase);

    const sug = sugerencia();
    montoEl.min = sug;
    document.getElementById("room-sugerencia").querySelector("strong").textContent =
      `${formatearPrecio(sug)} (oferta actual + incremento mínimo)`;
    if (Number(montoEl.value) < sug) montoEl.value = sug;

    // Formulario habilitado solo en ACTIVA y negociable.
    const activa = subasta.estado === "ACTIVA" && !esTerminal(subasta);
    btnPujar.disabled = !activa;
    montoEl.readOnly = !activa;

    renderHistorial();
    renderEstadoPersonal();
  }

  function renderHistorial() {
    const lista = document.getElementById("room-historial");
    const sinPujas = document.getElementById("room-sin-pujas");
    sinPujas.classList.toggle("d-none", historial.length > 0);
    lista.innerHTML = historial.map((b) => {
      const lider = b.isLeader
        ? ' <span class="badge bg-success">Liderando</span>'
        : "";
      const mia = b.isMine ? ' <span class="badge bg-light text-dark border">(vos)</span>' : "";
      const fecha = new Date(b.bidDate).toLocaleTimeString("es-AR", { hour: "2-digit", minute: "2-digit", second: "2-digit" });
      return `
        <li class="list-group-item d-flex justify-content-between align-items-center px-0">
          <div>
            <i class="bi bi-hammer me-2 text-success"></i>
            <strong>${formatearPrecio(b.amount)}</strong>
            ${lider}${mia}
          </div>
          <small class="text-muted">${b.buyerLabel} · ${fecha}</small>
        </li>`;
    }).join("");
  }

  async function refrescarSala(primera = false) {
    if (detenido) return;
    const user = sesion();
    const qs = user ? `?userId=${user.userId}` : "";
    try {
      const [auctionRes, bidsRes] = await Promise.all([
        fetch(`${API_BASE}/auctions/${auctionId}`),
        fetch(`${API_BASE}/auctions/${auctionId}/bids${qs}`),
      ]);

      if (auctionRes.status === 404) {
        mostrarNoEncontrada();
        return;
      }
      if (!auctionRes.ok) throw new Error("error de API");

      subasta = normalizarSubasta(await auctionRes.json());
      historial = bidsRes.ok ? await bidsRes.json() : [];

      loader.classList.add("d-none");
      contenido.classList.remove("d-none");

      renderSala();

      if (esTerminal(subasta)) detener();
    } catch (err) {
      if (!primera) return; // si ya hay datos, no romper la sala por un poll fallido
      loader.classList.add("d-none");
      mostrarAlerta(mensajeDeError(err), true);
    }
  }

  function mostrarNoEncontrada() {
    loader.classList.add("d-none");
    contenido.classList.add("d-none");
    noEncontrada.classList.remove("d-none");
    detener();
  }

  function detener() {
    detenido = true;
    clearInterval(window.__timerRoomPoll);
    clearInterval(window.__timerRoomCount);
  }

  // Countdown (tick por segundo)
  const contadorEl = document.getElementById("room-contador");
  window.__timerRoomCount = setInterval(() => {
    if (!subasta || detenido) {
      if (subasta) renderEstadoPersonal();
      return;
    }
    const c = textoContador(subasta);
    contadorEl.textContent = c.texto;
    contadorEl.classList.remove("urgente", "finalizada");
    if (c.clase) contadorEl.classList.add(c.clase);
    renderEstadoPersonal();
  }, 1000);

  // Short-polling de oferta + historial
  window.__timerRoomPoll = setInterval(() => refrescarSala(), POLL_MS);

  // Puja
  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    const user = sesion();
    if (!user) return mostrarAlerta("Iniciá sesión para poder pujar.", true);

    const monto = Number(montoEl.value);
    const sug = sugerencia();
    if (!monto || monto < sug)
      return mostrarAlerta(`Tu oferta debe ser al menos ${formatearPrecio(sug)}.`, true);

    btnPujar.disabled = true;
    const original = btnPujar.innerHTML;
    btnPujar.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Pujando...';

    try {
      const res = await fetch(`${API_BASE}/bids`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ auctionId, amount: monto, buyerId: user.userId }),
      });
      if (!res.ok) {
        const body = await res.json().catch(() => ({}));
        throw new Error(body.error ?? "No se pudo registrar la puja.");
      }

      const bid = await res.json().catch(() => null);
      montoEl.value = monto + subasta.minIncremento;

      // Éxito: toast de confirmación. Si la puja cayó en la ventana anti-sniping
      // se avisa la extensión del cierre y el nuevo horario.
      if (bid && bid.seExtendio) {
        const nuevaFin = new Date(bid.nuevaFechaFin);
        const fechaOk = !Number.isNaN(nuevaFin.getTime());
        const hh = fechaOk ? String(nuevaFin.getHours()).padStart(2, "0") : "--";
        const mm = fechaOk ? String(nuevaFin.getMinutes()).padStart(2, "0") : "--";
        mostrarToast(`⏰ Tu puja extendió el cierre 2 minutos (nuevo cierre ${hh}:${mm})`);
      } else {
        mostrarToast("Puja registrada. Actualizando la sala...");
      }
      await refrescarSala();
    } catch (err) {
      mostrarAlerta(mensajeDeError(err), true);
    } finally {
      btnPujar.disabled = false;
      btnPujar.innerHTML = original;
    }
  });

  refrescarSala(true);
})();