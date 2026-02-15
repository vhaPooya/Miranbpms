// Form Action Buttons Runtime Handler
(function() {
    'use strict';

    const FormActionButtons = {
        formId: null,
        documentId: null,

        init() {
            const container = document.querySelector('.form-action-buttons');
            if (!container) return;

            this.formId = container.dataset.formId;
            this.documentId = container.dataset.documentId;

            // Setup click handlers for all action buttons
            document.querySelectorAll('.btn-action-button').forEach(btn => {
                btn.addEventListener('click', (e) => this.handleButtonClick(e.target.closest('.btn-action-button')));
            });
        },

        async handleButtonClick(button) {
            const action = button.dataset.action;
            const isProcessService = button.dataset.isProcess === 'true';
            const confirmationMsg = button.dataset.confirmation;

            // Confirmation dialog
            if (confirmationMsg && !confirm(confirmationMsg)) {
                return;
            }

            // Process service button
            if (isProcessService) {
                await this.executeProcessService(button);
                return;
            }

            // Execute predefined action
            await this.executeAction(action, button);
        },

        async executeAction(action, button) {
            const modalId = button.dataset.modal;

            switch (action) {
                case 'save':
                    await this.saveForm(false);
                    break;
                case 'save_changes':
                    await this.saveForm(true);
                    break;
                case 'submit_and_refer':
                case 'refer':
                    this.openModal(modalId || 'modal-refer');
                    break;
                case 'approve_and_refer':
                    await this.approveAndRefer();
                    break;
                case 'print':
                    this.openModal(modalId || 'modal-print');
                    break;
                case 'attach':
                    this.openModal(modalId || 'modal-attach');
                    break;
                case 'reference':
                    this.openModal(modalId || 'modal-reference');
                    break;
                case 'related':
                    this.openModal(modalId || 'modal-related');
                    break;
                case 'follow_up':
                    this.openModal(modalId || 'modal-follow-up');
                    break;
                case 'draft':
                    await this.saveDraft();
                    break;
                case 'personal_archive':
                    await this.addToPersonalArchive();
                    break;
                case 'delete':
                    await this.deleteDocument();
                    break;
                case 'sign':
                    await this.signDocument();
                    break;
                case 'view_signatures':
                    this.viewSignatures();
                    break;
                case 'save_as_new':
                    await this.saveAsNew();
                    break;
                case 'close':
                    this.closeWindow();
                    break;
                case 'register_outgoing':
                    await this.registerOutgoing();
                    break;
                case 'view_copies':
                    this.viewCopies();
                    break;
                default:
                    console.warn('Unknown action:', action);
            }
        },

        async executeProcessService(button) {
            // Workflow removed for re-design; keep placeholder until new engine is implemented.
            await Promise.resolve(button);
            this.showError('فرآیندساز فعلا غیرفعال است و بعدا دوباره طراحی می‌شود.');
        },

        async executeInheritedAction(buttonTypeId) {
            // Find the inherited button type action
            // This would need the button type data - simplified for now
            console.log('Executing inherited action from button type:', buttonTypeId);
        },

        async saveForm(isUpdate) {
            const formData = this.collectFormData();
            const url = isUpdate && this.documentId
                ? `/api/forms/${this.formId}/data/${this.documentId}`
                : `/api/forms/${this.formId}/data`;
            const method = isUpdate ? 'PUT' : 'POST';

            try {
                const response = await fetch(url, {
                    method: method,
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(formData)
                });

                const data = await response.json();
                if (data.success) {
                    this.showSuccess('فرم با موفقیت ذخیره شد');
                    if (data.id && !this.documentId) {
                        this.documentId = data.id;
                        window.location.href = window.location.pathname + '?id=' + data.id;
                    }
                } else {
                    this.showError(data.error || 'خطا در ذخیره فرم');
                }
            } catch (error) {
                console.error('Error saving form:', error);
                this.showError('خطا در ارتباط با سرور');
            }
        },

        async approveAndRefer() {
            await this.saveForm(true);
            this.openModal('modal-refer');
        },

        async saveDraft() {
            const formData = this.collectFormData();
            formData.IsDraft = true;
            await this.saveFormWithData(formData);
        },

        async saveAsNew() {
            const formData = this.collectFormData();
            this.documentId = null; // Force new document
            await this.saveFormWithData(formData);
        },

        async addToPersonalArchive() {
            if (!this.documentId) {
                this.showError('ابتدا فرم را ذخیره کنید');
                return;
            }

            try {
                const response = await fetch(`/api/documents/${this.documentId}/archive/personal`, {
                    method: 'POST'
                });
                const data = await response.json();
                if (data.success) {
                    this.showSuccess('به بایگانی شخصی اضافه شد');
                } else {
                    this.showError(data.error || 'خطا در بایگانی');
                }
            } catch (error) {
                this.showError('خطا در ارتباط با سرور');
            }
        },

        async deleteDocument() {
            if (!this.documentId) {
                this.showError('سندی برای حذف وجود ندارد');
                return;
            }

            if (!confirm('آیا از حذف این مدرک اطمینان دارید؟')) return;

            try {
                const response = await fetch(`/api/documents/${this.documentId}`, {
                    method: 'DELETE'
                });
                const data = await response.json();
                if (data.success) {
                    this.showSuccess('مدرک حذف شد');
                    setTimeout(() => window.close(), 1500);
                } else {
                    this.showError(data.error || 'خطا در حذف مدرک');
                }
            } catch (error) {
                this.showError('خطا در ارتباط با سرور');
            }
        },

        async signDocument() {
            if (!this.documentId) {
                this.showError('ابتدا فرم را ذخیره کنید');
                return;
            }

            try {
                const response = await fetch(`/api/documents/${this.documentId}/sign`, {
                    method: 'POST'
                });
                const data = await response.json();
                if (data.success) {
                    this.showSuccess('مدرک با موفقیت امضا شد');
                    location.reload();
                } else {
                    this.showError(data.error || 'خطا در امضای مدرک');
                }
            } catch (error) {
                this.showError('خطا در ارتباط با سرور');
            }
        },

        async registerOutgoing() {
            if (!this.documentId) {
                this.showError('ابتدا فرم را ذخیره کنید');
                return;
            }

            try {
                const response = await fetch(`/api/documents/${this.documentId}/register-outgoing`, {
                    method: 'POST'
                });
                const data = await response.json();
                if (data.success) {
                    this.showSuccess('صادره ثبت شد. شماره: ' + data.outgoingNumber);
                } else {
                    this.showError(data.error || 'خطا در ثبت صادره');
                }
            } catch (error) {
                this.showError('خطا در ارتباط با سرور');
            }
        },

        viewSignatures() {
            if (!this.documentId) {
                this.showError('سندی وجود ندارد');
                return;
            }
            window.open(`/documents/${this.documentId}/signatures`, '_blank');
        },

        viewCopies() {
            if (!this.documentId) {
                this.showError('سندی وجود ندارد');
                return;
            }
            window.open(`/documents/${this.documentId}/copies`, '_blank');
        },

        openModal(modalId) {
            const modal = document.getElementById(modalId);
            if (modal) {
                const bsModal = new bootstrap.Modal(modal);
                bsModal.show();
            } else {
                this.showError('مودال مورد نظر یافت نشد: ' + modalId);
            }
        },

        closeWindow() {
            if (confirm('آیا می‌خواهید این صفحه را ببندید؟')) {
                window.close();
            }
        },

        async saveFormWithData(formData) {
            const url = `/api/forms/${this.formId}/data${this.documentId ? '/' + this.documentId : ''}`;
            const method = this.documentId ? 'PUT' : 'POST';

            try {
                const response = await fetch(url, {
                    method: method,
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(formData)
                });

                const data = await response.json();
                if (data.success) {
                    this.showSuccess('فرم با موفقیت ذخیره شد');
                    if (data.id && !this.documentId) {
                        this.documentId = data.id;
                    }
                } else {
                    this.showError(data.error || 'خطا در ذخیره فرم');
                }
            } catch (error) {
                this.showError('خطا در ارتباط با سرور');
            }
        },

        collectFormData() {
            const formData = {};
            document.querySelectorAll('input, select, textarea').forEach(input => {
                const name = input.name;
                if (name) {
                    if (input.type === 'checkbox') {
                        formData[name] = input.checked;
                    } else if (input.type === 'radio') {
                        if (input.checked) {
                            formData[name] = input.value;
                        }
                    } else {
                        formData[name] = input.value;
                    }
                }
            });
            return formData;
        },

        showSuccess(message) {
            if (typeof toastr !== 'undefined') {
                toastr.success(message);
            } else {
                alert(message);
            }
        },

        showError(message) {
            if (typeof toastr !== 'undefined') {
                toastr.error(message);
            } else {
                alert(message);
            }
        }
    };

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => FormActionButtons.init());
    } else {
        FormActionButtons.init();
    }

    // Expose to global scope if needed
    window.FormActionButtons = FormActionButtons;
})();
