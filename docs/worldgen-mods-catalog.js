(function () {
  const meta = JSON.parse(document.getElementById("catalog-meta").textContent);
  const state = {
    mods: [],
    banned: new Set(),
    required: new Set(),
    text: "",
    sort: "downloads-desc",
    minDownloads: 0,
    hasImages: false,
    worldgenOnly: false,
    page: 1,
    pageSize: 25,
    filtered: [],
  };

  const els = {
    results: document.getElementById("results"),
    shownCount: document.getElementById("shown-count"),
    totalCount: document.getElementById("total-count"),
    pageLabel: document.getElementById("page-label"),
    pageStatus: document.getElementById("page-status"),
    banCategories: document.getElementById("ban-categories"),
    requireCategories: document.getElementById("require-categories"),
    textFilter: document.getElementById("text-filter"),
    sortBy: document.getElementById("sort-by"),
    pageSize: document.getElementById("page-size"),
    minDownloads: document.getElementById("min-downloads"),
    hasImages: document.getElementById("has-images"),
    worldgenOnly: document.getElementById("worldgen-only"),
    prevPage: document.getElementById("prev-page"),
    nextPage: document.getElementById("next-page"),
    resetFilters: document.getElementById("reset-filters"),
    banLibraries: document.getElementById("ban-libraries"),
  };

  const swipers = [];

  function escapeHtml(value) {
    return String(value)
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;");
  }

  function formatDownloads(value) {
    return Number(value || 0).toLocaleString();
  }

  function destroySwipers() {
    while (swipers.length) {
      const swiper = swipers.pop();
      try {
        swiper.destroy(true, true);
      } catch (_) {
        /* ignore */
      }
    }
  }

  function matchesText(mod, query) {
    if (!query) return true;
    const haystack = `${mod.name} ${mod.slug} ${mod.description}`.toLowerCase();
    return haystack.includes(query);
  }

  function applyFilters() {
    const query = state.text.trim().toLowerCase();

    let list = state.mods.filter((mod) => {
      const categories = mod.categories || [];

      if (state.banned.size && categories.some((cat) => state.banned.has(cat))) {
        return false;
      }
      if (state.required.size) {
        for (const required of state.required) {
          if (!categories.includes(required)) return false;
        }
      }
      if (mod.downloads < state.minDownloads) return false;
      if (state.hasImages && !(mod.images && mod.images.length)) return false;
      if (state.worldgenOnly && categories.length !== 1) return false;
      return matchesText(mod, query);
    });

    list.sort((a, b) => {
      switch (state.sort) {
        case "downloads-asc":
          return a.downloads - b.downloads;
        case "name-asc":
          return a.name.localeCompare(b.name);
        case "name-desc":
          return b.name.localeCompare(a.name);
        case "images-desc":
          return (b.images?.length || 0) - (a.images?.length || 0);
        default:
          return b.downloads - a.downloads;
      }
    });

    state.filtered = list;
    const maxPage = Math.max(1, Math.ceil(list.length / state.pageSize));
    if (state.page > maxPage) state.page = maxPage;
  }

  function renderCategoryList(container, mode) {
    container.innerHTML = "";
    for (const [category, count] of Object.entries(meta.category_counts)) {
      const id = `${mode}-${category}`;
      const activeSet = mode === "ban" ? state.banned : state.required;
      const label = document.createElement("label");
      label.className = "category-item";
      if (activeSet.has(category)) {
        label.classList.add(mode === "ban" ? "banned" : "required");
      }

      const checkbox = document.createElement("input");
      checkbox.type = "checkbox";
      checkbox.id = id;
      checkbox.checked = activeSet.has(category);
      checkbox.addEventListener("change", () => {
        if (checkbox.checked) activeSet.add(category);
        else activeSet.delete(category);
        state.page = 1;
        render();
      });

      const text = document.createElement("span");
      text.textContent = category;

      const countEl = document.createElement("span");
      countEl.className = "count";
      countEl.textContent = String(count);

      label.append(checkbox, text, countEl);
      container.append(label);
    }
  }

  function renderModCard(mod, index) {
    const categories = (mod.categories || [])
      .map((cat) => {
        const classes = ["tag"];
        if (state.banned.has(cat)) classes.push("banned");
        return `<span class="${classes.join(" ")}">${escapeHtml(cat)}</span>`;
      })
      .join("");

    let gallery = '<div class="no-image">No Modrinth images</div>';
    if (mod.images && mod.images.length) {
      const slides = mod.images
        .map(
          (url, imageIndex) => `
            <div class="swiper-slide">
              <img
                class="lazyload"
                data-src="${escapeHtml(url)}"
                alt="${escapeHtml(mod.name)} screenshot ${imageIndex + 1}"
                src="data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw=="
                loading="lazy"
              >
            </div>`
        )
        .join("");
      gallery = `
        <div class="swiper mod-swiper" id="swiper-${index}">
          <div class="swiper-wrapper">${slides}</div>
          <div class="swiper-pagination"></div>
          <div class="swiper-button-prev"></div>
          <div class="swiper-button-next"></div>
        </div>`;
    }

    return `
      <article class="mod-card">
        <h3><a href="${escapeHtml(mod.link)}" target="_blank" rel="noopener">${escapeHtml(mod.name)}</a></h3>
        <div><a href="${escapeHtml(mod.link)}" target="_blank" rel="noopener">${escapeHtml(mod.link)}</a></div>
        <div class="tags">${categories}</div>
        ${gallery}
        <p class="desc">${escapeHtml(mod.description || "No description provided.")}</p>
        <p class="meta">Downloads: ${formatDownloads(mod.downloads)} · Images: ${mod.images?.length || 0}</p>
      </article>`;
  }

  function initSwipers() {
    document.querySelectorAll(".mod-swiper").forEach((element) => {
      const swiper = new Swiper(element, {
        loop: element.querySelectorAll(".swiper-slide").length > 1,
        lazy: true,
        preloadImages: false,
        watchSlidesProgress: true,
        pagination: {
          el: element.querySelector(".swiper-pagination"),
          clickable: true,
        },
        navigation: {
          nextEl: element.querySelector(".swiper-button-next"),
          prevEl: element.querySelector(".swiper-button-prev"),
        },
        on: {
          slideChange() {
            if (window.lazySizes) window.lazySizes.loader.checkElems();
          },
        },
      });
      swipers.push(swiper);
    });

    if (window.lazySizes) window.lazySizes.loader.checkElems();
  }

  function renderResults() {
    destroySwipers();

    const start = (state.page - 1) * state.pageSize;
    const pageItems = state.filtered.slice(start, start + state.pageSize);
    const maxPage = Math.max(1, Math.ceil(state.filtered.length / state.pageSize));

    els.shownCount.textContent = String(state.filtered.length);
    els.totalCount.textContent = String(state.mods.length);
    els.pageLabel.textContent = String(state.page);
    els.pageStatus.textContent = `Page ${state.page} of ${maxPage}`;
    els.prevPage.disabled = state.page <= 1;
    els.nextPage.disabled = state.page >= maxPage;

    if (!pageItems.length) {
      els.results.innerHTML =
        '<div class="empty">No mods match the current filters. Try clearing banned categories or relaxing search.</div>';
      return;
    }

    els.results.innerHTML = pageItems
      .map((mod, index) => renderModCard(mod, `${state.page}-${index}`))
      .join("");

    initSwipers();
  }

  function render() {
    applyFilters();
    renderCategoryList(els.banCategories, "ban");
    renderCategoryList(els.requireCategories, "require");
    renderResults();
  }

  function resetFilters() {
    state.banned.clear();
    state.required.clear();
    state.text = "";
    state.sort = "downloads-desc";
    state.minDownloads = 0;
    state.hasImages = false;
    state.worldgenOnly = false;
    state.page = 1;
    state.pageSize = 25;

    els.textFilter.value = "";
    els.sortBy.value = state.sort;
    els.pageSize.value = String(state.pageSize);
    els.minDownloads.value = "0";
    els.hasImages.checked = false;
    els.worldgenOnly.checked = false;
    render();
  }

  function bindEvents() {
    els.textFilter.addEventListener("input", (event) => {
      state.text = event.target.value;
      state.page = 1;
      render();
    });
    els.sortBy.addEventListener("change", (event) => {
      state.sort = event.target.value;
      render();
    });
    els.pageSize.addEventListener("change", (event) => {
      state.pageSize = Number(event.target.value) || 25;
      state.page = 1;
      render();
    });
    els.minDownloads.addEventListener("input", (event) => {
      state.minDownloads = Number(event.target.value) || 0;
      state.page = 1;
      render();
    });
    els.hasImages.addEventListener("change", (event) => {
      state.hasImages = event.target.checked;
      state.page = 1;
      render();
    });
    els.worldgenOnly.addEventListener("change", (event) => {
      state.worldgenOnly = event.target.checked;
      state.page = 1;
      render();
    });
    els.prevPage.addEventListener("click", () => {
      if (state.page > 1) {
        state.page -= 1;
        renderResults();
        window.scrollTo({ top: 0, behavior: "smooth" });
      }
    });
    els.nextPage.addEventListener("click", () => {
      const maxPage = Math.max(1, Math.ceil(state.filtered.length / state.pageSize));
      if (state.page < maxPage) {
        state.page += 1;
        renderResults();
        window.scrollTo({ top: 0, behavior: "smooth" });
      }
    });
    els.resetFilters.addEventListener("click", resetFilters);
    els.banLibraries.addEventListener("click", () => {
      state.banned.add("library");
      state.page = 1;
      render();
    });
  }

  async function loadCatalog() {
    const response = await fetch("worldgen-mods-neoforge-1.21.1.json");
    if (!response.ok) {
      throw new Error(`Failed to load catalog JSON (${response.status})`);
    }
    state.mods = await response.json();
    bindEvents();
    render();
  }

  loadCatalog().catch((error) => {
    els.results.innerHTML = `<div class="empty">Could not load catalog data. Serve this folder over HTTP, e.g. <code>python3 -m http.server --directory docs</code>.<br><br>${escapeHtml(error.message)}</div>`;
  });
})();
