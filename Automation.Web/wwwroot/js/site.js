// Navigation and Section Switching
// Track loaded sections for AJAX lazy loading
const loadedSections = {
    settings: false,
    inbox: false,
    referred: false,
    search: false,
    newdocument: false,
    secretariat: false,
    forms: false
};

// Section URL mappings
const sectionUrls = {
    'setting': { key: 'settings', url: '/settingspage', container: 'settings-container' },
    'varedeh': { key: 'inbox', url: '/inbox', container: 'inbox-container' },
    'erjaee': { key: 'referred', url: '/referred', container: 'referred-container' },
    'search': { key: 'search', url: '/search', container: 'search-container' },
    'newdocument': { key: 'newdocument', url: '/newdocument', container: 'newdocument-container' },
    'dabirkhaneh': { key: 'secretariat', url: '/secretariat', container: 'secretariat-container' },
    'forms': { key: 'forms', url: '/formbuilder', container: 'forms-container' }
};

function switchSection(sectionId, element) {
    // Hide all sections
    document.querySelectorAll('.app-section').forEach(sec => sec.classList.remove('active'));
    // Remove active class from all ribbon links
    document.querySelectorAll('.ribbon-link').forEach(link => link.classList.remove('active'));

    // Show target section
    const targetSection = document.getElementById('sec-' + sectionId);
    if (targetSection) {
        targetSection.classList.add('active');

        // Load section content via AJAX if not loaded yet
        const sectionConfig = sectionUrls[sectionId];
        if (sectionConfig && !loadedSections[sectionConfig.key]) {
            loadSectionContent(sectionConfig.key, sectionConfig.url, sectionConfig.container);
        }
    }

    // Activate ribbon link
    if (element) element.classList.add('active');
}

// Load section content via AJAX
function loadSectionContent(key, url, containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;

    fetch(url)
        .then(response => {
            if (!response.ok) throw new Error('Network response was not ok');
            return response.text();
        })
        .then(html => {
            container.innerHTML = html;
            loadedSections[key] = true;

            // Execute any inline scripts in the loaded content
            const scripts = container.querySelectorAll('script');
            scripts.forEach(oldScript => {
                const newScript = document.createElement('script');
                // Copy all attributes
                Array.from(oldScript.attributes).forEach(attr => {
                    newScript.setAttribute(attr.name, attr.value);
                });
                // Copy the content
                newScript.textContent = oldScript.textContent;
                // Replace old script with new one to execute it
                oldScript.parentNode.replaceChild(newScript, oldScript);
            });
        })
        .catch(error => {
            console.error('Error loading section:', error);
            container.innerHTML = `
                <div class="alert alert-danger m-4">
                    <i class="fa-solid fa-exclamation-triangle me-2"></i>
                    خطا در بارگذاری محتوا
                </div>
            `;
        });
}

// Secretariat Logic
document.addEventListener('DOMContentLoaded', function() {
    const secretariatModal = document.getElementById('secretariatModal');
    if (secretariatModal) {
        var myModal = new bootstrap.Modal(secretariatModal);
        myModal.show();
    }
});

function selectSecretariat(name) {
    var myModalEl = document.getElementById('secretariatModal');
    if (myModalEl) {
        var modal = bootstrap.Modal.getInstance(myModalEl);
        if (modal) modal.hide();
    }
    const badge = document.getElementById('selectedBadge');
    if (badge) {
        badge.style.display = 'inline-block';
        badge.innerText = name;
    }
}

function showCriteria() {
    const rowCriteria = document.getElementById('row-criteria');
    if (rowCriteria) {
        rowCriteria.classList.remove('d-none-custom');
        rowCriteria.classList.add('d-flex-custom');
    }
}

function toggleFields() {
    const criteria = document.getElementById('searchCriteria');
    if (!criteria) return;
    
    const criteriaValue = criteria.value;
    // Hide all first
    const fields = ['subject', 'date', 'daterange', 'user', 'number'];
    fields.forEach(id => {
        const el = document.getElementById('field-' + id);
        if (el) {
            el.classList.remove('d-block');
            el.classList.add('d-none-custom');
        }
    });

    // Show selected
    if (criteriaValue) {
        const selectedField = document.getElementById('field-' + criteriaValue);
        if (selectedField) {
            selectedField.classList.remove('d-none-custom');
            selectedField.classList.add('d-block');
        }

        const btnRow = document.getElementById('row-search-btn');
        if (btnRow) {
            btnRow.classList.remove('d-none-custom');
            btnRow.classList.add('d-block');
        }
    }
}

// Filter Tabs Active State Logic
document.addEventListener('DOMContentLoaded', function() {
    const filterBtns = document.querySelectorAll('.filter-tabs .btn');
    filterBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            const activeBtn = document.querySelector('.filter-tabs .btn.active');
            if (activeBtn) activeBtn.classList.remove('active');
            btn.classList.add('active');
        });
    });
});

// Set Status Logic (Read/Unread)
function setStatus(element, status) {
    const dropdown = element.closest('.dropdown');
    if (!dropdown) return;
    
    const icon = dropdown.querySelector('.dropdown-toggle');
    if (!icon) return;

    if (status === 'read') {
        icon.className = 'fa-regular fa-envelope-open icon-view read dropdown-toggle';
        icon.setAttribute('title', 'مشاهده شده');
    } else {
        icon.className = 'fa-solid fa-envelope icon-view unread dropdown-toggle';
        icon.setAttribute('title', 'خوانده نشده');
    }
}

// Logic for Read/Unread Toggle
function setReadStatus(element, isRead) {
    const dropdownDiv = element.closest('.dropdown');
    if (!dropdownDiv) return;
    
    const icon = dropdownDiv.querySelector('.dropdown-toggle');
    if (!icon) return;

    if (isRead) {
        icon.className = 'fa-regular fa-envelope-open envelope-icon read dropdown-toggle';
    } else {
        icon.className = 'fa-solid fa-envelope envelope-icon unread dropdown-toggle';
    }
}

// Enable Tooltips
document.addEventListener('DOMContentLoaded', function() {
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
});

// Bootstrap 5 stacked modals: z-index fix
// When a second modal opens while another is already shown, raise its z-index
// and place the new backdrop between the two modals.
document.addEventListener('show.bs.modal', function (event) {
    const modal = event.target;
    const openCount = document.querySelectorAll('.modal.show').length;
    if (openCount > 0) {
        modal.style.zIndex = 1055 + (openCount * 10);
    }
});
document.addEventListener('shown.bs.modal', function (event) {
    const modal = event.target;
    const modalZ = parseInt(modal.style.zIndex) || 1055;
    // Adjust the most recently added backdrop to sit just below this modal
    const backdrops = document.querySelectorAll('.modal-backdrop');
    if (backdrops.length > 1) {
        backdrops[backdrops.length - 1].style.zIndex = modalZ - 1;
    }
});

// Theme toggle (default + alt)
(function () {
    const toggle = document.getElementById('theme-toggle');
    const altCss = document.getElementById('alt-theme-css');
    const root = document.documentElement;
    if (!altCss) return;

    const key = 'app-theme';
    const applyTheme = (theme) => {
        if (theme === 'alt') {
            altCss.disabled = false;
            root.setAttribute('data-app-theme', 'alt');
        } else {
            altCss.disabled = true;
            root.removeAttribute('data-app-theme');
        }
    };

    const saved = localStorage.getItem(key);
    applyTheme(saved);

    toggle?.addEventListener('click', () => {
        const next = (localStorage.getItem(key) === 'alt') ? 'default' : 'alt';
        localStorage.setItem(key, next);
        applyTheme(next);
    });
})();
