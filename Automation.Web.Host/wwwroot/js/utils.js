/**
 * Office Automation System - JavaScript Utilities
 * Centralized utilities for notifications, API calls, and common functions
 */

(function(window) {
    'use strict';

    // ===================================
    // 1. NOTIFICATION SYSTEM (Toast)
    // ===================================
    
    const NotifySystem = {
        container: null,
        
        init() {
            if (!this.container) {
                this.container = document.createElement('div');
                this.container.className = 'glass-toast-container';
                document.body.appendChild(this.container);
            }
        },
        
        show(message, type = 'info', duration = 4000) {
            this.init();
            
            const toast = document.createElement('div');
            toast.className = `glass-toast glass-toast--${type}`;
            
            const icon = this.getIcon(type);
            toast.innerHTML = `
                <i class="${icon}"></i>
                <span>${message}</span>
            `;
            
            this.container.appendChild(toast);
            
            // Auto remove
            setTimeout(() => {
                toast.style.opacity = '0';
                toast.style.transform = 'translateX(-20px)';
                setTimeout(() => toast.remove(), 300);
            }, duration);
        },
        
        getIcon(type) {
            const icons = {
                success: 'fa-solid fa-circle-check',
                error: 'fa-solid fa-circle-xmark',
                warning: 'fa-solid fa-triangle-exclamation',
                info: 'fa-solid fa-circle-info'
            };
            return icons[type] || icons.info;
        },
        
        success(message, duration) { this.show(message, 'success', duration); },
        error(message, duration) { this.show(message, 'error', duration); },
        warning(message, duration) { this.show(message, 'warning', duration); },
        info(message, duration) { this.show(message, 'info', duration); }
    };

    // ===================================
    // 2. API HELPER
    // ===================================
    
    const ApiHelper = {
        baseHeaders: {
            'Content-Type': 'application/json'
        },
        
        async request(url, options = {}) {
            try {
                const response = await fetch(url, {
                    ...options,
                    headers: {
                        ...this.baseHeaders,
                        ...options.headers
                    }
                });
                
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                
                const data = await response.json();
                return { success: true, data };
            } catch (error) {
                console.error('API Error:', error);
                NotifySystem.error('خطا در ارتباط با سرور');
                return { success: false, error: error.message };
            }
        },
        
        async get(url) {
            return this.request(url, { method: 'GET' });
        },
        
        async post(url, data) {
            return this.request(url, {
                method: 'POST',
                body: JSON.stringify(data)
            });
        },
        
        async put(url, data) {
            return this.request(url, {
                method: 'PUT',
                body: JSON.stringify(data)
            });
        },
        
        async delete(url) {
            return this.request(url, { method: 'DELETE' });
        }
    };

    // ===================================
    // 3. LOADING UTILITIES
    // ===================================
    
    const LoadingHelper = {
        showInElement(element, text = 'در حال بارگذاری...') {
            const originalContent = element.innerHTML;
            element.dataset.originalContent = originalContent;
            element.innerHTML = `
                <div class="text-center py-4">
                    <div class="spinner-border text-primary mb-2" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <div class="text-muted">${text}</div>
                </div>
            `;
        },
        
        hideInElement(element) {
            if (element.dataset.originalContent) {
                element.innerHTML = element.dataset.originalContent;
                delete element.dataset.originalContent;
            }
        },
        
        showButton(button) {
            button.disabled = true;
            button.dataset.originalText = button.innerHTML;
            button.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>صبر کنید...';
        },
        
        hideButton(button) {
            button.disabled = false;
            if (button.dataset.originalText) {
                button.innerHTML = button.dataset.originalText;
                delete button.dataset.originalText;
            }
        }
    };

    // ===================================
    // 4. MODAL UTILITIES
    // ===================================
    
    const ModalHelper = {
        open(modalId) {
            const modal = document.getElementById(modalId);
            if (modal) {
                modal.style.display = 'flex';
                modal.classList.add('show');
                document.body.style.overflow = 'hidden';
            }
        },
        
        close(modalId) {
            const modal = document.getElementById(modalId);
            if (modal) {
                modal.style.display = 'none';
                modal.classList.remove('show');
                document.body.style.overflow = '';
            }
        },
        
        setupCloseOnOverlay(modalId) {
            const modal = document.getElementById(modalId);
            if (modal) {
                modal.addEventListener('click', (e) => {
                    if (e.target.id === modalId) {
                        this.close(modalId);
                    }
                });
            }
        }
    };

    // ===================================
    // 5. FORM UTILITIES
    // ===================================
    
    const FormHelper = {
        getValues(formId) {
            const form = document.getElementById(formId);
            if (!form) return {};
            
            const formData = new FormData(form);
            const data = {};
            
            for (let [key, value] of formData.entries()) {
                data[key] = value;
            }
            
            return data;
        },
        
        setValues(formId, data) {
            const form = document.getElementById(formId);
            if (!form) return;
            
            Object.keys(data).forEach(key => {
                const input = form.elements[key];
                if (input) {
                    if (input.type === 'checkbox') {
                        input.checked = !!data[key];
                    } else {
                        input.value = data[key];
                    }
                }
            });
        },
        
        reset(formId) {
            const form = document.getElementById(formId);
            if (form) form.reset();
        }
    };

    // ===================================
    // 6. DATE UTILITIES (Persian)
    // ===================================
    
    const DateHelper = {
        toGregorian(persianDate) {
            // Simple implementation - can be enhanced with a proper library
            return persianDate; // Return as-is for now
        },
        
        toPersian(gregorianDate) {
            // Simple implementation using Intl
            const date = new Date(gregorianDate);
            return date.toLocaleDateString('fa-IR');
        },
        
        formatDate(dateString) {
            if (!dateString) return '-';
            const date = new Date(dateString);
            return date.toLocaleDateString('fa-IR');
        }
    };

    // ===================================
    // 7. TAB UTILITIES
    // ===================================
    
    const TabHelper = {
        init(containerSelector) {
            const container = document.querySelector(containerSelector);
            if (!container) return;
            
            const buttons = container.querySelectorAll('.glass-tab-btn');
            const contents = container.parentElement.querySelectorAll('.glass-tab-content');
            
            buttons.forEach(btn => {
                btn.addEventListener('click', () => {
                    const tabId = btn.dataset.tab;
                    
                    // Deactivate all
                    buttons.forEach(b => b.classList.remove('active'));
                    contents.forEach(c => c.classList.remove('active'));
                    
                    // Activate selected
                    btn.classList.add('active');
                    const content = document.getElementById(tabId);
                    if (content) content.classList.add('active');
                });
            });
        }
    };

    // ===================================
    // EXPORT TO GLOBAL SCOPE
    // ===================================
    
    window.AppNotify = NotifySystem;
    window.AppApi = ApiHelper;
    window.AppLoading = LoadingHelper;
    window.AppModal = ModalHelper;
    window.AppForm = FormHelper;
    window.AppDate = DateHelper;
    window.AppTab = TabHelper;

})(window);
