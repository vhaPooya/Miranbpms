/**
 * Form Builder Engine V2.1 (Fixed Layout & Drag)
 * - Implicit Layout (Auto Split)
 * - Vertical/Horizontal Flex Support
 * - Full Control Rendering
 */

class FormBuilderEngine {
    constructor(config) {
        this.config = config;

        // State
        this.elements = [];
        this.modals = []; // Array of { id, label, elements: [] }
        this.currentView = 'main'; // 'main' or 'modal_ID'
        
        this.selectedElement = null;
        this.draggedType = null;
        this.draggedElement = null; // If reordering

        // Cache DOM
        // Cache DOM
        this.canvas = document.getElementById("form-canvas");
        this.trayContainer = null; // Lazy load
        this.trayWrapper = document.getElementById("component-tray-wrapper");
        this.subHeader = document.getElementById("canvas-sub-header");
        
        // Removed eager check, will check in renderTray
        
        this.modalSettings = new bootstrap.Modal(document.getElementById("modal-form-settings"));
        this.modalProperties = new bootstrap.Modal(document.getElementById("modal-properties"));
        this.modalScript = new bootstrap.Modal(document.getElementById("modal-global-script"));
        this.modalPrint = new bootstrap.Modal(document.getElementById("modal-print-template"));
        this.modalDatabase = new bootstrap.Modal(document.getElementById("modal-database-connection"));
        this.modalActionButtons = new bootstrap.Modal(document.getElementById("modal-action-buttons"));

        // Action buttons state
        this.actionButtonTypes = [];
        this.actionButtonPresets = [];
        this.activeFormButtons = [];
        this.selectedActiveButtonIndex = -1;

        // Debounce timers storage
        this.debounceTimers = {};
        
        // Database connection state
        this.dbConnectionState = {
            selectedTable: null,
            selectedColumns: [],
            currentElement: null
        };
        
        this.init();
    }

    /**
     * Debounce function to delay execution
     */
    debounce(key, func, delay = 800) {
        if (this.debounceTimers[key]) {
            clearTimeout(this.debounceTimers[key]);
        }
        this.debounceTimers[key] = setTimeout(() => {
            func();
            delete this.debounceTimers[key];
        }, delay);
    }

    init() {
        // 1. Initial Flow
        if (this.config.isNew) {
            this.modalSettings.show();
        } else {
            this.loadForm(this.config.initialData);
        }

        // 2. Setup Listeners
        this.setupDragDrop();
        this.setupToolbar();
        this.setupPropertyModalListeners();
        this.setupSettingsModalListeners();
        
        // Load Settings into Modal if exists
        if(this.config.formNameFa) {
            const nameFaInput = document.getElementById("setting-name-fa");
            if(nameFaInput) nameFaInput.value = this.config.formNameFa;
        }
        if(this.config.formNameEn) {
            const nameEnInput = document.getElementById("setting-name-en");
            if(nameEnInput) nameEnInput.value = this.config.formNameEn;
        }
        if(this.config.formCode) {
            const codeInput = document.getElementById("setting-code");
            if(codeInput) codeInput.value = this.config.formCode;
        }
        if(this.config.formType) {
            const typeSelect = document.getElementById("setting-form-type");
            if(typeSelect) typeSelect.value = this.config.formType;
        }
        if(this.config.categoryId) {
            const categorySelect = document.getElementById("setting-category");
            if(categorySelect) categorySelect.value = this.config.categoryId;
        }
        if(this.config.numberingRule) {
            const numberingRuleInput = document.getElementById("setting-numbering-rule");
            if(numberingRuleInput) numberingRuleInput.value = this.config.numberingRule;
        }
        if(this.config.numberingPrefix) {
            const numberingPrefixInput = document.getElementById("setting-numbering-prefix");
            if(numberingPrefixInput) numberingPrefixInput.value = this.config.numberingPrefix;
        }
        
        // Load canvas size
        if(this.config.canvasSize) {
            try {
                const size = JSON.parse(this.config.canvasSize);
                const canvasSizeSelect = document.getElementById("setting-canvas-size");
                if(canvasSizeSelect) {
                    if(size.width === "210mm" && size.height === "297mm") {
                        canvasSizeSelect.value = "A4";
                    } else if(size.width === "148mm" && size.height === "210mm") {
                        canvasSizeSelect.value = "A5";
                    } else if(size.width === "8.5in" && size.height === "11in") {
                        canvasSizeSelect.value = "Letter";
                    } else if(size.width === "100%" && size.height === "auto") {
                        canvasSizeSelect.value = "auto";
                    } else {
                        // Custom size
                        canvasSizeSelect.value = "Custom";
                        // Parse and populate custom inputs
                        const widthMatch = size.width?.match(/^(\d+(?:\.\d+)?)(px|mm|cm|in|%)$/);
                        const heightMatch = size.height?.match(/^(\d+(?:\.\d+)?)(px|mm|cm|in|%|auto)$/);
                        
                        if (widthMatch) {
                            const widthInput = document.getElementById("setting-custom-width");
                            const widthUnitSelect = document.getElementById("setting-custom-width-unit");
                            if (widthInput) widthInput.value = widthMatch[1];
                            if (widthUnitSelect) widthUnitSelect.value = widthMatch[2];
                        }
                        
                        if (heightMatch && heightMatch[2] !== "auto") {
                            const heightInput = document.getElementById("setting-custom-height");
                            const heightUnitSelect = document.getElementById("setting-custom-height-unit");
                            if (heightInput) heightInput.value = heightMatch[1];
                            if (heightUnitSelect) heightUnitSelect.value = heightMatch[2];
                        }
                        
                        // Show custom inputs
                        const customSizeInputs = document.getElementById("custom-size-inputs");
                        const customSizeInputsHeight = document.getElementById("custom-size-inputs-height");
                        if (customSizeInputs) customSizeInputs.style.display = "block";
                        if (customSizeInputsHeight) customSizeInputsHeight.style.display = "block";
                    }
                }
            } catch(e) {
                console.error("Error parsing canvas size:", e);
            }
        }
        
        // Load background settings
        if(this.config.backgroundSettings) {
            try {
                const bg = JSON.parse(this.config.backgroundSettings);
                const bgColorInput = document.getElementById("setting-bg-color");
                const bgImageInput = document.getElementById("setting-bg-image");
                if(bgColorInput && bg.color) bgColorInput.value = bg.color;
                if(bgImageInput && bg.image) bgImageInput.value = bg.image;
            } catch(e) {
                console.error("Error parsing background settings:", e);
            }
        }
        
        // Apply canvas settings immediately
        this.applyCanvasSettings();
        
        // Load Custom Scripts
        if(this.config.customScripts) {
             const scriptEditor = document.getElementById("global-script-editor");
             if(scriptEditor) scriptEditor.value = this.config.customScripts;
        }

        // Initialize CodeMirror for global script editor
        this.initializeCodeMirror();
    
    }

    /**
     * Initialize CodeMirror editor
     */
    initializeCodeMirror() {
        const scriptEditor = document.getElementById("global-script-editor");
        if (!scriptEditor) return;
        
        // Check if CodeMirror is available
        if (typeof CodeMirror !== 'undefined') {
            try {
                this.codeMirrorInstance = CodeMirror.fromTextArea(scriptEditor, {
                    mode: 'javascript',
                    theme: 'monokai',
                    lineNumbers: true,
                    indentUnit: 4,
                    indentWithTabs: false,
                    lineWrapping: true,
                    matchBrackets: true,
                    autoCloseBrackets: true,
                    foldGutter: true,
                    gutters: ['CodeMirror-linenumbers', 'CodeMirror-foldgutter']
                });
                
                // Set initial value
                if (this.config.customScripts) {
                    this.codeMirrorInstance.setValue(this.config.customScripts);
                }
            } catch (error) {
                console.error('Error initializing CodeMirror:', error);
            }
        }
    }

    loadForm(data) {
        if (data && Array.isArray(data)) {
            this.elements = data;
        } else {
            this.elements = [];
        }
        this.render();
    }

    // ==========================================
    //           CORE DRAG & DROP ENGINE
    // ==========================================

    setupDragDrop() {
        // Sidebar Draggables
        document.querySelectorAll(".fb-component-item").forEach((item) => {
            item.addEventListener("dragstart", (e) => {
                this.draggedType = {
                    type: item.dataset.type,
                    id: item.dataset.id,
                    isContainer: item.dataset.container === "true",
                    label: item.querySelector("span").innerText,
                    icon: item.querySelector("i").className,
                };
                e.dataTransfer.effectAllowed = "copy";
                e.dataTransfer.setData("text/plain", JSON.stringify(this.draggedType)); // Firefox fix
            });
            item.addEventListener("dragend", () => {
                this.draggedType = null;
                this.clearDropVisuals();
            });
        });

        // Canvas Events (Delegated)
        this.canvas.addEventListener("dragover", (e) => this.handleDragOver(e));
        this.canvas.addEventListener("dragleave", (e) => this.handleDragLeave(e));
        this.canvas.addEventListener("drop", (e) => this.handleDrop(e));

        // Selection
        this.canvas.addEventListener("click", (e) => {
            const wrapper = e.target.closest(".fb-control-wrapper");
            if (wrapper) {
                e.stopPropagation();
                this.selectElement(wrapper.dataset.id);
            } else {
                // Clicked on canvas background
                this.deselectAll();
            }
        });
    }

    /**
     * Determines where the drop is happening relative to the target:
     * - 'left', 'right': Split (Implicit Layout) -> Creates a Row
     * - 'top', 'bottom': Insert Sibling
     */
    getDropPosition(e, targetRect, isContainer = false) {
        const x = e.clientX - targetRect.left;
        const y = e.clientY - targetRect.top;
        const w = targetRect.width;
        const h = targetRect.height;

        // If it's a container and we are in the middle 50%, treat as "inside"
        if (isContainer) {
             const margin = 15; // px from edge
             if (x > margin && x < w - margin && y > margin && y < h - margin) {
                 return "inside";
             }
        }

        // Define zones (25% edge sensitivity)
        const edgeH = 0.25 * w;
        const edgeV = 0.25 * h;

        // Priority to Horizontal Split for Side drops
        if (x < edgeH) return "left";
        if (x > w - edgeH) return "right";
        if (y < edgeV) return "top";
        return "bottom";
    }

    handleDragOver(e) {
        e.preventDefault();

        const target = e.target.closest(".fb-control-wrapper");
        const emptyZone = e.target.closest(".fb-drop-zone-empty");

        // Clear previous visuals
        this.clearDropVisuals();

        if (emptyZone) {
            emptyZone.classList.add("fb-drop-target-active");
            return;
        }

        if (target) {
            // Glowing Border on Hover
            target.classList.add("fb-drop-target-active");

            const isContainer = target.classList.contains("fb-layout-container") || ["Row", "Container", "Flex", "Card", "Panel", "Fieldset"].includes(target.dataset.type);
            const rect = target.getBoundingClientRect();
            const pos = this.getDropPosition(e, rect, isContainer);

            // Visual Feedback
            if (pos === "inside") {
                 target.style.border = "2px solid #0d6efd"; // Blue border for inside
                 target.classList.add("fb-drop-target-inside");
            } 
            else if (pos === "left" || pos === "right") {
                target.classList.add("fb-shrink-preview"); // Shrink effect to show split

                // Show indicator line on side
                const indicator = document.createElement("div");
                indicator.className = `fb-drop-indicator-side ${pos}`;
                target.appendChild(indicator);
            } else {
                // Top/Bottom - Show horizontal line
                const line = document.createElement("div");
                line.className = "fb-drop-placeholder";
                
                // We need to inject the placeholder into the DOM near the target
                if (pos === "top") {
                    target.parentNode.insertBefore(line, target);
                } else {
                    target.parentNode.insertBefore(line, target.nextSibling);
                }
            }
        } else {
            // Create placeholder at the end of canvas (Append mode)
            const line = document.createElement("div");
            line.className = "fb-drop-placeholder";
            this.canvas.appendChild(line);
        }
    }

    handleDragLeave(e) {
        // e.target.classList.remove('fb-drop-target-active');
    }

    handleDrop(e) {
        e.preventDefault();
        this.clearDropVisuals();

        const target = e.target.closest(".fb-control-wrapper");
        const emptyZone = e.target.closest(".fb-drop-zone-empty");

        if (emptyZone) {
            this.addElement(null, "append");
            emptyZone.style.display = "none";
            return;
        }

        if (target) {
            const isContainer = target.classList.contains("fb-layout-container") || ["Row", "Container", "Flex", "Card", "Panel", "Fieldset"].includes(target.dataset.type);
            const rect = target.getBoundingClientRect();
            const pos = this.getDropPosition(e, rect, isContainer);
            const targetId = target.dataset.id;

            if (this.draggedType && this.draggedType.source === 'canvas') {
                // Reordering Logic - Move to top, bottom, left, or right
                if (targetId && targetId !== this.draggedType.id) {
                    // Check if we should move to left/right (create row) or top/bottom (insert sibling)
                    if (pos === "left" || pos === "right") {
                        this.moveElementToSide(this.draggedType.id, targetId, pos);
                    } else {
                        this.moveElementRelative(this.draggedType.id, targetId, pos);
                    }
                }
            } else {
                // New Component Logic
                if (pos === "inside") {
                    this.addElement(targetId, "inside");
                } else if (pos === "left" || pos === "right") {
                    this.splitElement(targetId, pos);
                } else {
                    this.addElement(targetId, pos);
                }
            }
        } else {
            // Dropped on background -> Append
            this.addElement(null, "append");
        }
    }

    clearDropVisuals() {
        document.querySelectorAll(".fb-drop-placeholder").forEach((el) => el.remove());
        document.querySelectorAll(".fb-drop-indicator-side").forEach((el) => el.remove());
        document.querySelectorAll(".fb-shrink-preview").forEach((el) => el.classList.remove("fb-shrink-preview"));
        document.querySelectorAll(".fb-drop-target-active").forEach((el) => el.classList.remove("fb-drop-target-active"));
        document.querySelectorAll(".fb-drop-target-inside").forEach((el) => {
             el.classList.remove("fb-drop-target-inside");
             el.style.border = ""; // Reset inline border
        });
    }

    // ==========================================
    //           ELEMENT MANAGEMENT
    // ==========================================

    generateId() {
        return "el_" + Math.random().toString(36).substr(2, 9);
    }

    addElement(targetId, position) {
        if (!this.draggedType) return;

        // Special Handling for "Modal" Type
        if (this.draggedType.type === "Modal") {
            this.addModal();
            return;
        }

        const newEl = {
            id: this.generateId(),
            type: this.draggedType.type,
            label: this.draggedType.label,
            icon: this.draggedType.icon,
            props: {
                label: this.draggedType.label,
                placeholder: "",
                required: false,
            },
            style: {
                width: "100%",
                marginTop: "0px",
                marginBottom: "15px"
            },
            events: [],
            data: { 
                dbColumn: '', 
                sqlType: '', 
                maxLength: null, 
                isNullable: true, 
                hasIndex: false 
            },
            children: [], // For containers
        };

        // Specific defaults
        if(newEl.type === 'Row' || newEl.type === 'Container') {
             newEl.style.flexDirection = 'row'; // Default to Row
             newEl.style.display = 'flex';
             newEl.style.gap = '10px';
             newEl.style.padding = '10px';
             newEl.style.border = '1px dashed #ccc';
             newEl.style.minHeight = "60px"; // Ensure droppable area
        }
        
        // Slider Defaults
        if(newEl.type === 'Slider') {
            newEl.data.slides = []; // Array of image URLs
        }

        const targetList = this.getCurrentList(); // Based on View

        if (position === "inside" && targetId) {
             const parentEl = this.findElementById(targetId, targetList);
             if(parentEl) {
                 if(!parentEl.children) parentEl.children = [];
                 parentEl.children.push(newEl);
                 this.render();
                 this.selectElement(newEl.id);
                 return;
             }
        }

        if (targetId && position !== "inside") {
            this.insertElementRelative(newEl, targetId, position, targetList);
        } else {
            targetList.push(newEl);
        }

        this.render();
        this.selectElement(newEl.id);
    } 

    addModal() {
        const modalId = this.generateId();
        const newModal = {
            id: modalId,
            type: 'Modal',
            label: 'مدال جدید ' + (this.modals.length + 1),
            elements: []
        };
        this.modals.push(newModal);
        this.renderTray();
    }

    renderTray() {
        // Robust fetch
        const tray = document.getElementById("component-tray");
        if (!tray) {
            console.warn("CRITICAL: Tray container #component-tray not found in DOM during renderTray!");
            return;
        }
        this.trayContainer = tray; // Update cache

        if(this.modals.length === 0) {
            this.trayContainer.innerHTML = '<div class="text-muted small w-100 text-center align-self-center">هنوز کامپوننتی اضافه نشده است</div>';
            return;
        }
        
        this.trayContainer.innerHTML = '';
        this.modals.forEach(m => {
             const item = document.createElement("div");
             item.className = "d-flex align-items-center p-2 border rounded bg-light pointer";
             item.style.cursor = "pointer";
             item.innerHTML = `<i class="bi bi-window me-2"></i> <span>${m.label}</span>`;
             item.onclick = () => this.switchToModal(m.id);
             this.trayContainer.appendChild(item);
        });
    }

    switchToModal(id) {
        const m = this.modals.find(x => x.id === id);
        if(!m) return;
        
        this.currentView = id;
        this.subHeader.classList.remove('d-none');
        document.getElementById("current-view-name").innerText = m.label;
        document.getElementById("btn-return-main").onclick = () => this.switchToMain();
        
        // Keep it.
        
        this.render();
    }

    switchToMain() {
        this.currentView = 'main';
        this.subHeader.classList.add('d-none');
        this.render();
    }

    getCurrentList() {
        if(this.currentView === 'main') return this.elements;
        const m = this.modals.find(x => x.id === this.currentView);
        return m ? m.elements : this.elements;
    }

    /**
     * IMPLICIT LAYOUT LOGIC
     * Wraps the target element and the new element in a Flex Row
     */
    splitElement(targetId, position, sourceId = null) {
        const targetEl = this.findElementById(targetId);
        if (!targetEl) return;

        let sourceEl;
        if (sourceId) {
            // Use existing element
            sourceEl = this.findElementById(sourceId);
            if (!sourceEl) return;
            
            // Remove source from its current position
            const sourceInfo = this.findParentAndIndex(sourceId, this.getCurrentList());
            if (sourceInfo) {
                sourceEl = sourceInfo.parentList.splice(sourceInfo.index, 1)[0];
            }
        } else {
            // Create new element
            sourceEl = {
                id: this.generateId(),
                type: this.draggedType.type,
                label: this.draggedType.label,
                icon: this.draggedType.icon,
                props: { label: this.draggedType.label },
                style: { width: "50%", marginBottom: "0px" },
                events: [],
                data: { dbColumn: '', sqlType: '', maxLength: null, isNullable: true, hasIndex: false },
            };
        }
        
        // Adjust target width to share space
        targetEl.style.width = "50%";
        targetEl.style.marginBottom = "0px";
        if (sourceEl.style) {
            sourceEl.style.width = "50%";
            sourceEl.style.marginBottom = "0px";
        }

        // Create Container (Row)
        const rowContainer = {
            id: this.generateId(),
            type: "Row",
            isImplicit: true,
            label: "بخش بندی (Row)",
            props: { label: "بخش بندی" },
            style: {
                display: "flex",
                flexDirection: "row",
                gap: "10px",
                width: "100%",
                marginBottom: "15px"
            },
            data: { dbColumn: '', sqlType: '', maxLength: null, isNullable: true, hasIndex: false },
            children: [],
        };

        // Arrange children based on pos
        if (position === "left") {
            rowContainer.children = [sourceEl, targetEl];
        } else {
            rowContainer.children = [targetEl, sourceEl];
        }

        // Replace target in the tree with the new row
        this.replaceElementInTree(targetId, rowContainer);

        this.render();
        this.selectElement(sourceId || sourceEl.id);
    }


    // ==========================================
    //           ADVANCED MANIPULATION
    // ==========================================

    findParentAndIndex(id, list = this.elements, parent = null) {
         for (let i = 0; i < list.length; i++) {
            if (list[i].id === id) {
                return { parentList: list, index: i, parentObj: parent };
            }
            if (list[i].children) {
                const res = this.findParentAndIndex(id, list[i].children, list[i]);
                if (res) return res;
            }
        }
        return null;
    }

    deleteElement(id) {
        if(!confirm("آیا از حذف این آیتم مطمئن هستید؟")) return;

        const info = this.findParentAndIndex(id, this.getCurrentList());
        if (!info) return;

        // Remove
        info.parentList.splice(info.index, 1);

        // Space Redistribution (if parent is Row/Flex)
        if (info.parentObj && info.parentObj.style && (info.parentObj.type === 'Row' || info.parentObj.style.display === 'flex')) {
             this.distributeSpace(info.parentList);
        }

        this.selectedElement = null;
        this.render();
    }

    distributeSpace(list) {
        if (!list || list.length === 0) return;
        const newWidth = (100 / list.length).toFixed(2) + "%";
        list.forEach(el => {
            if(el.style) el.style.width = newWidth;
        });
    }

    swapElements(sourceId, targetId) {
        const sourceInfo = this.findParentAndIndex(sourceId, this.getCurrentList());
        const targetInfo = this.findParentAndIndex(targetId, this.getCurrentList());

        if (!sourceInfo || !targetInfo) return;

        // Swap Logic
        const sourceEl = sourceInfo.parentList[sourceInfo.index];
        const targetEl = targetInfo.parentList[targetInfo.index];

        // Replace at indices
        // Using temporary placeholder to avoid overwriting if in same list
        sourceInfo.parentList[sourceInfo.index] = targetEl;
        targetInfo.parentList[targetInfo.index] = sourceEl;

        this.render();
        this.selectElement(sourceId);
    }

    /**
     * Move element relative to target (top or bottom)
     */
    moveElementRelative(sourceId, targetId, position) {
        const sourceInfo = this.findParentAndIndex(sourceId, this.getCurrentList());
        const targetInfo = this.findParentAndIndex(targetId, this.getCurrentList());

        if (!sourceInfo || !targetInfo) return;
        if (sourceInfo.parentList === targetInfo.parentList && sourceInfo.index === targetInfo.index) return;

        // Remove source from its current position
        const sourceEl = sourceInfo.parentList.splice(sourceInfo.index, 1)[0];

        // Adjust target index if source was before target in same list
        let insertIndex = targetInfo.index;
        if (sourceInfo.parentList === targetInfo.parentList && sourceInfo.index < targetInfo.index) {
            insertIndex = targetInfo.index - 1;
        }

        // Insert at new position
        if (position === "top") {
            targetInfo.parentList.splice(insertIndex, 0, sourceEl);
        } else {
            targetInfo.parentList.splice(insertIndex + 1, 0, sourceEl);
        }

        this.render();
        this.selectElement(sourceId);
    }

    /**
     * Move element to left or right of target (creates row if needed)
     */
    moveElementToSide(sourceId, targetId, position) {
        const sourceInfo = this.findParentAndIndex(sourceId, this.getCurrentList());
        const targetInfo = this.findParentAndIndex(targetId, this.getCurrentList());

        if (!sourceInfo || !targetInfo) return;

        // Check if target is already in a row
        const targetEl = targetInfo.parentList[targetInfo.index];
        const targetParent = this.findElementParent(targetId, this.getCurrentList());
        
        // If target is in a row, add source to same row
        if (targetParent && targetParent.type === 'Row' && targetParent.style.flexDirection === 'row') {
            // Remove source from its current position
            const sourceEl = sourceInfo.parentList.splice(sourceInfo.index, 1)[0];
            
            // Find target index in parent's children
            const targetChildIndex = targetParent.children.findIndex(c => c.id === targetId);
            
            // Insert source in row
            if (position === "left") {
                targetParent.children.splice(targetChildIndex, 0, sourceEl);
            } else {
                targetParent.children.splice(targetChildIndex + 1, 0, sourceEl);
            }
        } else {
            // Create a new row and put both elements in it
            this.splitElement(targetId, position, sourceId);
        }

        this.render();
        this.selectElement(sourceId);
    }

    /**
     * Find parent element of a given element
     */
    findElementParent(elementId, list, parent = null) {
        for (let i = 0; i < list.length; i++) {
            if (list[i].id === elementId) {
                return parent;
            }
            if (list[i].children && list[i].children.length > 0) {
                const found = this.findElementParent(elementId, list[i].children, list[i]);
                if (found !== null) return found;
            }
        }
        return null;
    }

    // ==========================================
    //           RESIZING LOGIC
    // ==========================================

    initResize(e, leftEl, rightEl) {
        e.preventDefault();
        e.stopPropagation();

        this.resizingState = {
            startX: e.clientX,
            leftEl: leftEl,
            rightEl: rightEl,
            leftStartWidth: parseFloat(leftEl.style.width) || 50,
            rightStartWidth: parseFloat(rightEl.style.width) || 50,
            parentWidth: e.target.parentElement.getBoundingClientRect().width
        };

        // Bind global listeners
        this.resizeMoveHandler = (ev) => this.handleResizeMove(ev);
        this.resizeUpHandler = () => this.handleResizeUp();

        document.addEventListener('mousemove', this.resizeMoveHandler);
        document.addEventListener('mouseup', this.resizeUpHandler);
    }

    handleResizeMove(e) {
        if (!this.resizingState) return;

        const deltaPixels = e.clientX - this.resizingState.startX;
        const deltaPercent = (deltaPixels / this.resizingState.parentWidth) * 100;

        let newLeft = this.resizingState.leftStartWidth + deltaPercent;
        let newRight = this.resizingState.rightStartWidth - deltaPercent;

        // Min width constraint (e.g., 5%)
        if (newLeft < 5 || newRight < 5) return;

        this.resizingState.leftEl.style.width = newLeft + "%";
        this.resizingState.rightEl.style.width = newRight + "%";

        this.render(); // Re-render to show update (might be heavy, consider optimizing DOM directly)
    }

    handleResizeUp() {
        document.removeEventListener('mousemove', this.resizeMoveHandler);
        document.removeEventListener('mouseup', this.resizeUpHandler);
        this.resizingState = null;
    }


    render() {
        this.canvas.innerHTML = "";
        
        if (this.elements.length === 0) {
            document.getElementById("start-drop-zone").style.display = "block";
            this.canvas.appendChild(document.getElementById("start-drop-zone"));
            return;
        }

        const currentList = this.getCurrentList();

        if (currentList.length === 0) {
            document.getElementById("start-drop-zone").style.display = "block";
            this.canvas.appendChild(document.getElementById("start-drop-zone"));
            return;
        }

        currentList.forEach((el) => {
            this.canvas.appendChild(this.renderElement(el));
        });

        // Re-apply selection visual
        if (this.selectedElement) {
            const wrapper = document.querySelector(`[data-id="${this.selectedElement.id}"]`);
            if (wrapper) wrapper.classList.add("selected");
        }

        // Initialize controls after rendering (with small delay to ensure DOM is ready)
        setTimeout(() => {
            this.initializeControls();
        }, 50);
    }

    /**
     * Initialize controls that need special setup (RichTextEditor, Signature, DatePicker, etc.)
     */
    initializeControls() {
        const currentList = this.getCurrentList();
        
        // Initialize all elements recursively
        const initializeRecursive = (elements) => {
            elements.forEach((el) => {
                this.initializeControl(el);
                // If element has children, initialize them too
                if (el.children && el.children.length > 0) {
                    initializeRecursive(el.children);
                }
            });
        };
        
        initializeRecursive(currentList);
    }

    /**
     * Initialize a single control
     */
    initializeControl(el) {
        switch (el.type) {
            case "RichTextEditor":
            case "Editor":
                this.initializeRichTextEditor(el);
                break;
            case "Signature":
                this.initializeSignature(el);
                break;
            case "PersianDatePicker":
                this.initializePersianDatePicker(el);
                break;
            case "ImageUpload":
                this.initializeImageUpload(el);
                break;
            case "Rating":
                this.initializeRating(el);
                break;
            case "RangeSlider":
                this.initializeRangeSlider(el);
                break;
        }
    }

    /**
     * Initialize Rich Text Editor (Quill)
     */
    initializeRichTextEditor(el) {
        const editorContainer = document.getElementById(`editor_${el.id}`);
        const hiddenInput = document.getElementById(`editor_input_${el.id}`);
        
        if (!editorContainer || editorContainer.dataset.initialized === 'true') return;
        
        // Check if Quill is available
        if (typeof Quill === 'undefined') {
            console.warn('Quill library not loaded');
            return;
        }

        try {
            const quill = new Quill(editorContainer, {
                theme: 'snow',
                modules: {
                    toolbar: [
                        [{ 'header': [1, 2, 3, false] }],
                        ['bold', 'italic', 'underline', 'strike'],
                        [{ 'list': 'ordered'}, { 'list': 'bullet' }],
                        [{ 'color': [] }, { 'background': [] }],
                        ['link', 'image'],
                        ['clean']
                    ]
                },
                placeholder: 'متن خود را اینجا بنویسید...'
            });

            // Load existing content if any
            if (el.props.value) {
                quill.root.innerHTML = el.props.value;
            }

            // Update hidden input on change
            quill.on('text-change', () => {
                if (hiddenInput) {
                    hiddenInput.value = quill.root.innerHTML;
                }
                if (el.props) {
                    el.props.value = quill.root.innerHTML;
                }
            });

            // Store reference
            editorContainer.dataset.initialized = 'true';
            editorContainer.dataset.quillId = el.id;
            if (!this.quillEditors) this.quillEditors = {};
            this.quillEditors[el.id] = quill;
        } catch (error) {
            console.error('Error initializing RichTextEditor:', error);
        }
    }

    /**
     * Initialize Signature Pad
     */
    initializeSignature(el) {
        const canvas = document.getElementById(`sig_canvas_${el.id}`);
        const hiddenInput = document.getElementById(`sig_input_${el.id}`);
        
        if (!canvas || canvas.dataset.initialized === 'true') return;
        
        // Check if SignaturePad is available
        if (typeof SignaturePad === 'undefined') {
            console.warn('SignaturePad library not loaded');
            return;
        }

        try {
            const signaturePad = new SignaturePad(canvas, {
                backgroundColor: 'rgb(255, 255, 255)',
                penColor: 'rgb(0, 0, 0)'
            });

            // Update hidden input on end
            signaturePad.addEventListener('endStroke', () => {
                if (hiddenInput) {
                    hiddenInput.value = signaturePad.toDataURL();
                }
            });

            // Store reference
            canvas.dataset.initialized = 'true';
            if (!this.signaturePads) this.signaturePads = {};
            this.signaturePads[el.id] = signaturePad;
        } catch (error) {
            console.error('Error initializing Signature:', error);
        }
    }

    /**
     * Clear signature
     */
    clearSignature(elementId) {
        if (this.signaturePads && this.signaturePads[elementId]) {
            this.signaturePads[elementId].clear();
            const hiddenInput = document.getElementById(`sig_input_${elementId}`);
            if (hiddenInput) hiddenInput.value = '';
        }
    }

    /**
     * Initialize Persian Date Picker
     */
    initializePersianDatePicker(el) {
        const input = document.getElementById(`pdate_${el.id}`);
        
        if (!input || input.dataset.initialized === 'true') return;
        
        // Check if jalali-date-picker is available
        if (typeof jalaliDatepicker === 'undefined') {
            console.warn('Jalali Date Picker library not loaded');
            return;
        }

        try {
            // Ensure input is not readonly for date picker to work
            input.removeAttribute('readonly');
            input.style.cursor = 'pointer';
            
            // The library automatically detects inputs with data-jdp attribute
            // startWatch should be called once globally, but we ensure it's initialized
            if (!window.jalaliDatepickerInitialized) {
                jalaliDatepicker.startWatch({
                    minDate: 'attr',
                    maxDate: 'attr',
                    time: false
                });
                window.jalaliDatepickerInitialized = true;
            }

            // Store reference
            input.dataset.initialized = 'true';
        } catch (error) {
            console.error('Error initializing PersianDatePicker:', error);
        }
    }

    /**
     * Initialize Image Upload
     */
    initializeImageUpload(el) {
        const container = document.getElementById(`img_upload_${el.id}`);
        const fileInput = document.getElementById(`img_file_${el.id}`);
        const placeholder = container?.querySelector('.image-upload-placeholder');
        const previewContainer = container?.querySelector('.image-preview-container');
        const previewImg = previewContainer?.querySelector('img');
        
        if (!container || !fileInput || container.dataset.initialized === 'true') return;

        // Click handler for placeholder
        if (placeholder) {
            placeholder.addEventListener('click', () => {
                fileInput.click();
            });
        }

        // File change handler
        fileInput.addEventListener('change', (e) => {
            const file = e.target.files[0];
            if (file && file.type.startsWith('image/')) {
                const reader = new FileReader();
                reader.onload = (event) => {
                    if (previewImg) {
                        previewImg.src = event.target.result;
                    }
                    if (placeholder) placeholder.style.display = 'none';
                    if (previewContainer) previewContainer.style.display = 'block';
                };
                reader.readAsDataURL(file);
            }
        });

        // Store reference
        container.dataset.initialized = 'true';
    }

    /**
     * Initialize Rating control
     */
    initializeRating(el) {
        const container = document.getElementById(`rating_${el.id}`);
        const hiddenInput = document.getElementById(`rating_input_${el.id}`);
        const stars = container?.querySelectorAll('.rating-star');
        
        if (!container || !stars || container.dataset.initialized === 'true') return;

        stars.forEach((star, index) => {
            star.addEventListener('mouseenter', () => {
                const value = parseInt(star.dataset.value);
                stars.forEach((s, i) => {
                    if (i < value) {
                        s.classList.remove('bi-star');
                        s.classList.add('bi-star-fill');
                        s.style.color = '#ffc107';
                    } else {
                        s.classList.remove('bi-star-fill');
                        s.classList.add('bi-star');
                        s.style.color = '#dee2e6';
                    }
                });
            });

            star.addEventListener('click', () => {
                const value = parseInt(star.dataset.value);
                if (hiddenInput) hiddenInput.value = value;
                if (el.props) el.props.value = value;
                
                // Update visual state
                stars.forEach((s, i) => {
                    if (i < value) {
                        s.classList.remove('bi-star');
                        s.classList.add('bi-star-fill');
                        s.style.color = '#ffc107';
                    } else {
                        s.classList.remove('bi-star-fill');
                        s.classList.add('bi-star');
                        s.style.color = '#dee2e6';
                    }
                });
            });
        });

        container.addEventListener('mouseleave', () => {
            const currentValue = parseInt(hiddenInput?.value || el.props.value || 0);
            stars.forEach((s, i) => {
                if (i < currentValue) {
                    s.classList.remove('bi-star');
                    s.classList.add('bi-star-fill');
                    s.style.color = '#ffc107';
                } else {
                    s.classList.remove('bi-star-fill');
                    s.classList.add('bi-star');
                    s.style.color = '#dee2e6';
                }
            });
        });

        container.dataset.initialized = 'true';
    }

    /**
     * Initialize Range Slider
     */
    initializeRangeSlider(el) {
        const slider = document.getElementById(`range_${el.id}`);
        const valueDisplay = document.getElementById(`range_value_${el.id}`);
        
        if (!slider || slider.dataset.initialized === 'true') return;

        slider.addEventListener('input', (e) => {
            if (valueDisplay) valueDisplay.textContent = e.target.value;
            if (el.props) el.props.value = e.target.value;
        });

        slider.dataset.initialized = 'true';
    }

    renderElement(el) {
        const wrapper = document.createElement("div");
        wrapper.className = "fb-control-wrapper";
        wrapper.dataset.id = el.id;
        wrapper.setAttribute("draggable", "true"); // Enable reordering

        // Apply Styles
        Object.assign(wrapper.style, el.style);
        
        // If it's a container/row, ensure flex behavior is respected
        if (["Row", "Container", "Flex", "Card", "Panel", "Fieldset"].includes(el.type) || (el.children && el.children.length > 0)) {
             wrapper.classList.add("fb-layout-container");
        }

        // Content
        if (el.children && el.children.length > 0) {
            // Render Children
            el.children.forEach((child, index) => {
                const childWrapper = this.renderElement(child);
                wrapper.appendChild(childWrapper);
                
                // Add Resizer if inside a Row (and not the last one)
                if (el.style.display === 'flex' && el.style.flexDirection === 'row' && index < el.children.length - 1) {
                    const resizer = document.createElement('div');
                    resizer.className = 'fb-resizer';
                    resizer.onmousedown = (e) => this.initResize(e, child, el.children[index+1]);
                    wrapper.appendChild(resizer);
                }
            });
        } else {
            // Leaf Control
            wrapper.innerHTML = this.getControlHtml(el);
        }

        // --- Controls Overlay (Delete / Drag Handle) ---
        const controls = document.createElement("div");
        controls.className = "fb-elem-controls";
        controls.innerHTML = `
            <div class="btn-group btn-group-sm">
                <button class="btn btn-light text-danger btn-delete" title="حذف"><i class="bi bi-trash"></i></button>
            </div>
        `;
        wrapper.appendChild(controls);

        // Bind Delete
        controls.querySelector(".btn-delete").addEventListener("click", (e) => {
            e.stopPropagation();
            this.deleteElement(el.id);
        });

        // Bind Drag Start (Reordering)
        wrapper.addEventListener("dragstart", (e) => {
            e.stopPropagation(); // Prevent bubbling to parent containers
            this.draggedElement = el; // Store reference to actual data object
            this.draggedType = { type: el.type, id: el.id, label: el.props.label, source: 'canvas' };
            e.dataTransfer.effectAllowed = "move";
            e.dataTransfer.setData("text/plain", JSON.stringify(this.draggedType));
            wrapper.classList.add("fb-dragging");
        });
        
        wrapper.addEventListener("dragend", (e) => {
             e.stopPropagation();
             this.draggedElement = null;
             this.draggedType = null;
             wrapper.classList.remove("fb-dragging");
             this.clearDropVisuals();
        });

        return wrapper;
    }

    getControlHtml(el) {
        const lbl = el.props.label || "بدون عنوان";
        const placeholder = el.props.placeholder || "";
        const req = el.props.required ? '<span style="color:#ef4444;">*</span>' : "";
        
        // Build inline style for controls that need text styling
        const buildTextStyle = () => {
            let style = '';
            if(el.style.fontFamily && el.style.fontFamily !== 'inherit') style += `font-family:${el.style.fontFamily};`;
            if(el.style.fontSize) style += `font-size:${el.style.fontSize};`;
            if(el.style.fontWeight) style += `font-weight:${el.style.fontWeight};`;
            if(el.style.color) style += `color:${el.style.color};`;
            else style += `color:#ffffff;`; // Default white for dark theme
            if(el.style.textAlign) style += `text-align:${el.style.textAlign};`;
            if(el.style.fontStyle) style += `font-style:${el.style.fontStyle};`;
            if(el.style.textDecoration) style += `text-decoration:${el.style.textDecoration};`;
            return style;
        };
        
        const buildInputStyle = () => {
            let style = buildTextStyle();
            // Default dark theme styles
            if(!el.style.backgroundColor || el.style.backgroundColor === '#ffffff') {
                style += `background-color:rgba(0,0,0,0.2);`;
            } else {
                style += `background-color:${el.style.backgroundColor};`;
            }
            style += `border:1px solid rgba(255,255,255,0.1);`;
            style += `color:#ffffff;`;
            if(el.style.borderRadius) style += `border-radius:${el.style.borderRadius};`;
            if(el.style.borderColor) style += `border-color:${el.style.borderColor};`;
            return style;
        };
        
        const textStyle = buildTextStyle();
        const inputStyle = buildInputStyle();
        
        let controlHtml = "";

        switch (el.type) {
            case "TextInput":
                // Check if inputType is specified in props (from Specific tab)
                const inputType = el.props.inputType || 'text';
                controlHtml = `<input type="${inputType}" class="form-control" style="${inputStyle}" placeholder="${placeholder}" ${el.props.readonly ? 'readonly' : ''} ${el.props.disabled ? 'disabled' : ''} ${this.buildValidationAttributes(el)}>`;
                break;
            case "Email":
                controlHtml = `<input type="email" class="form-control" style="${inputStyle}" placeholder="${placeholder}" ${el.props.readonly ? 'readonly' : ''} ${el.props.disabled ? 'disabled' : ''} ${this.buildValidationAttributes(el)}>`;
                break;
            case "Url":
                controlHtml = `<input type="url" class="form-control" style="${inputStyle}" placeholder="${placeholder}" ${el.props.readonly ? 'readonly' : ''} ${el.props.disabled ? 'disabled' : ''} ${this.buildValidationAttributes(el)}>`;
                break;
            case "Password":
                controlHtml = `<input type="password" class="form-control" style="${inputStyle}" placeholder="******" ${el.props.readonly ? 'readonly' : ''} ${el.props.disabled ? 'disabled' : ''} ${this.buildValidationAttributes(el)}>`;
                break;
            case "Number":
            case "Currency":
                controlHtml = `<input type="number" class="form-control" style="${inputStyle}" placeholder="${placeholder}" ${el.props.readonly ? 'readonly' : ''} ${el.props.disabled ? 'disabled' : ''} ${this.buildValidationAttributes(el)}>`;
                break;
            case "Phone":
                controlHtml = `<input type="tel" class="form-control" style="${inputStyle}" placeholder="${placeholder || '09xxxxxxxxx'}" ${el.props.readonly ? 'readonly' : ''} ${this.buildValidationAttributes(el)}>`;
                break;
            case "TextArea":
            case "Textarea":
                const textareaRows = el.props.rows || 3;
                controlHtml = `<textarea class="form-control" id="textarea_${el.id}" name="${el.props.nameEn || el.id}" rows="${textareaRows}" style="${inputStyle}" placeholder="${placeholder}" ${el.props.readonly ? 'readonly' : ''} ${el.props.disabled ? 'disabled' : ''} ${this.buildValidationAttributes(el)}>${el.props.value || ''}</textarea>`;
                break;
            //case "RichTextEditor":
            //    controlHtml = `<div class="border rounded p-2 bg-light" style="min-height:100px;${inputStyle}"><i class="bi bi-file-text me-1"></i> ویرایشگر متن پیشرفته</div>`;
            //    break;

            case "RichTextEditor":
            case "Editor":
                controlHtml = `
                    <div id="editor_${el.id}" class="rich-text-editor-container" style="min-height:200px;${inputStyle}">
                        <div class="ql-editor-placeholder" style="display:none;">متن خود را اینجا بنویسید...</div>
                    </div>
                    <input type="hidden" id="editor_input_${el.id}" name="${el.props.nameEn || el.id}">`;
                break;

            case "Checkbox":
                controlHtml = `
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" id="chk_${el.id}" ${el.props.disabled ? 'disabled' : ''}>
                        <label class="form-check-label" style="${textStyle}" for="chk_${el.id}">${lbl}</label>
                    </div>`;
                return controlHtml; 
            case "Switch":
                controlHtml = `
                    <div class="form-check form-switch">
                        <input class="form-check-input" type="checkbox" id="sw_${el.id}" ${el.props.disabled ? 'disabled' : ''}>
                        <label class="form-check-label" style="${textStyle}" for="sw_${el.id}">${lbl}</label>
                    </div>`;
                return controlHtml;
            case "Radio":
            case "RadioButton":
                let radioOptions = el.props.options || [{ value: '1', label: 'گزینه ۱' }, { value: '2', label: 'گزینه ۲' }];
                
                // If database mode, filter and sort
                if (el.data?.sourceType === 'database') {
                    radioOptions = [...radioOptions].sort((a, b) => {
                        const orderA = a.displayOrder || 999;
                        const orderB = b.displayOrder || 999;
                        return orderA - orderB;
                    }).filter(opt => !opt.hide);
                }
                
                controlHtml = radioOptions.map((opt, i) => `
                    <div class="form-check">
                        <input class="form-check-input" type="radio" name="radio_${el.id}" id="radio_${el.id}_${i}" value="${opt.value}" ${el.props.disabled ? 'disabled' : ''}>
                        <label class="form-check-label" style="${textStyle}" for="radio_${el.id}_${i}">${opt.label || opt.value}</label>
                    </div>
                `).join('');
                break;
            case "CheckboxGroup":
                let cbOptions = el.props.options || [{ value: '1', label: 'گزینه ۱' }, { value: '2', label: 'گزینه ۲' }];
                
                // If database mode, filter and sort
                if (el.data?.sourceType === 'database') {
                    cbOptions = [...cbOptions].sort((a, b) => {
                        const orderA = a.displayOrder || 999;
                        const orderB = b.displayOrder || 999;
                        return orderA - orderB;
                    }).filter(opt => !opt.hide);
                }
                
                controlHtml = cbOptions.map((opt, i) => `
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" id="cbg_${el.id}_${i}" value="${opt.value}" ${el.props.disabled ? 'disabled' : ''}>
                        <label class="form-check-label" style="${textStyle}" for="cbg_${el.id}_${i}">${opt.label || opt.value}</label>
                    </div>
                `).join('');
                break;
            case "Select":
            case "Dropdown":
                let selectOptions = el.props.options || [{ value: '', label: 'انتخاب کنید...' }];
                
                // If database mode, filter and sort options
                if (el.data?.sourceType === 'database') {
                    // Sort by display order
                    selectOptions = [...selectOptions].sort((a, b) => {
                        const orderA = a.displayOrder || 999;
                        const orderB = b.displayOrder || 999;
                        return orderA - orderB;
                    });
                    
                    // Filter out hidden options
                    selectOptions = selectOptions.filter(opt => !opt.hide);
                    
                    // Build display text from visible fields (right to left)
                    selectOptions = selectOptions.map(opt => {
                        // For database mode, label should show Persian names
                        return {
                            ...opt,
                            displayText: opt.label || opt.value
                        };
                    });
                }
                
                controlHtml = `
                    <select class="form-select" id="select_${el.id}" name="${el.props.nameEn || el.id}" style="${inputStyle}" ${el.props.disabled ? 'disabled' : ''} ${this.buildValidationAttributes(el)}>
                        ${selectOptions.map(opt => `<option value="${opt.value}" ${el.props.value === opt.value ? 'selected' : ''}>${opt.displayText || opt.label || opt.value}</option>`).join('')}
                    </select>`;
                break;
            case "MultiSelect":
                const multiOptions = el.props.options || [{ value: '1', label: 'گزینه ۱' }, { value: '2', label: 'گزینه ۲' }, { value: '3', label: 'گزینه ۳' }];
                controlHtml = `
                    <select class="form-select" id="multiselect_${el.id}" name="${el.props.nameEn || el.id}" style="${inputStyle}" multiple ${el.props.disabled ? 'disabled' : ''} ${this.buildValidationAttributes(el)}>
                        ${multiOptions.map(opt => `<option value="${opt.value}" ${el.props.selectedValues?.includes(opt.value) ? 'selected' : ''}>${opt.label}</option>`).join('')}
                    </select>`;
                break;
            case "DatePicker":
                controlHtml = `
                    <div class="input-group">
                        <span class="input-group-text"><i class="bi bi-calendar"></i></span>
                        <input type="date" class="form-control" id="date_${el.id}" name="${el.props.nameEn || el.id}" style="${inputStyle}" ${el.props.readonly ? 'readonly' : ''} ${el.props.disabled ? 'disabled' : ''} ${this.buildValidationAttributes(el)}>
                    </div>`;
                break;
            case "PersianDatePicker":
                // Remove readonly for date picker to allow clicking
                const isReadonly = el.props.readonly && !el.props.disabled ? '' : '';
                controlHtml = `
                    <div class="input-group">
                        <span class="input-group-text"><i class="bi bi-calendar-event"></i></span>
                        <input type="text" class="form-control persian-datepicker" id="pdate_${el.id}" name="${el.props.nameEn || el.id}" data-jdp style="${inputStyle};cursor:pointer;" placeholder="1403/xx/xx" ${el.props.disabled ? 'disabled' : ''} ${this.buildValidationAttributes(el)}>
                    </div>`;
                break;
            case "TimePicker":
                controlHtml = `
                    <div class="input-group">
                        <span class="input-group-text"><i class="bi bi-clock"></i></span>
                        <input type="time" class="form-control" id="time_${el.id}" name="${el.props.nameEn || el.id}" style="${inputStyle}" ${el.props.readonly ? 'readonly' : ''} ${el.props.disabled ? 'disabled' : ''} ${this.buildValidationAttributes(el)}>
                    </div>`;
                break;
            case "DateTimePicker":
                controlHtml = `
                    <div class="input-group">
                        <span class="input-group-text"><i class="bi bi-calendar-event"></i></span>
                        <input type="datetime-local" class="form-control" id="datetime_${el.id}" name="${el.props.nameEn || el.id}" style="${inputStyle}" ${el.props.readonly ? 'readonly' : ''} ${el.props.disabled ? 'disabled' : ''} ${this.buildValidationAttributes(el)}>
                    </div>`;
                break;
            case "FileUpload":
                controlHtml = `<input type="file" class="form-control" style="${inputStyle}" ${el.props.disabled ? 'disabled' : ''}>`;
                break;
            case "ImageUpload":
                controlHtml = `
                    <div class="image-upload-container" id="img_upload_${el.id}" style="${inputStyle}">
                        <input type="file" class="d-none" id="img_file_${el.id}" accept="image/*" ${el.props.multiple ? 'multiple' : ''}>
                        <div class="border rounded p-3 text-center bg-light image-upload-placeholder" style="cursor:pointer;min-height:150px;display:flex;align-items:center;justify-content:center;flex-direction:column;">
                            <i class="bi bi-cloud-upload fs-1 text-muted mb-2"></i>
                            <small class="text-muted">برای آپلود تصویر کلیک کنید</small>
                        </div>
                        <div class="image-preview-container mt-2" style="display:none;">
                            <img src="" alt="Preview" class="img-thumbnail" style="max-width:200px;max-height:200px;">
                            <button type="button" class="btn btn-sm btn-danger mt-2" onclick="this.closest('.image-upload-container').querySelector('.image-upload-placeholder').style.display='flex';this.closest('.image-preview-container').style.display='none';this.closest('.image-upload-container').querySelector('input[type=file]').value='';">حذف</button>
                        </div>
                    </div>`;
                break;
            case "Signature":
                controlHtml = `
                    <div class="signature-container" id="sig_container_${el.id}" style="${inputStyle}">
                        <canvas id="sig_canvas_${el.id}" class="border rounded" style="cursor:crosshair;background:#fff;" width="400" height="200"></canvas>
                        <div class="mt-2">
                            <button type="button" class="btn btn-sm btn-secondary" onclick="window.fbEngine?.clearSignature('${el.id}')">پاک کردن</button>
                            <input type="hidden" id="sig_input_${el.id}" name="${el.props.nameEn || el.id}">
                        </div>
                    </div>`;
                break;
            case "Rating":
                const maxStars = el.props.maxStars || 5;
                const ratingValue = el.props.value || 0;
                controlHtml = `
                    <div class="rating-container" id="rating_${el.id}" style="${textStyle}">
                        ${Array.from({length: maxStars}, (_, i) => {
                            const starValue = i + 1;
                            return `<i class="bi bi-star${starValue <= ratingValue ? '-fill' : ''} rating-star" data-value="${starValue}" style="cursor:pointer;font-size:1.5rem;color:${starValue <= ratingValue ? '#ffc107' : '#dee2e6'};"></i>`;
                        }).join(' ')}
                        <input type="hidden" id="rating_input_${el.id}" name="${el.props.nameEn || el.id}" value="${ratingValue}">
                    </div>`;
                break;
            case "ColorPicker":
                const defaultColor = el.props.defaultColor || el.props.value || "#4f46e5";
                controlHtml = `<input type="color" class="form-control form-control-color" id="color_${el.id}" name="${el.props.nameEn || el.id}" style="${inputStyle}" value="${defaultColor}">`;
                break;
            case "RangeSlider":
                const minVal = el.props.min || 0;
                const maxVal = el.props.max || 100;
                const stepVal = el.props.step || 1;
                const sliderValue = el.props.value || (minVal + maxVal) / 2;
                controlHtml = `
                    <div class="range-slider-container">
                        <input type="range" class="form-range" id="range_${el.id}" name="${el.props.nameEn || el.id}" style="${inputStyle}" min="${minVal}" max="${maxVal}" step="${stepVal}" value="${sliderValue}">
                        <div class="d-flex justify-content-between">
                            <small>${minVal}</small>
                            <small id="range_value_${el.id}">${sliderValue}</small>
                            <small>${maxVal}</small>
                        </div>
                    </div>`;
                break;
            case "Button":
            case "SubmitButton":
            case "ResetButton":
                const btnClass = el.props.outline ? `btn-outline-${el.props.variant || 'primary'}` : `btn-${el.props.variant || 'primary'}`;
                const btnBlock = el.props.block ? 'w-100' : '';
                return `<button class="btn ${btnClass} ${btnBlock}" style="${textStyle}" type="${el.props.buttonType || 'button'}">${lbl}</button>`;
            case "Header":
            case "Heading":
                return `<h3 class="border-bottom pb-2" style="${textStyle}">${lbl}</h3>`;
            case "Paragraph":
            case "Text":
                return `<p style="${textStyle}">${lbl}</p>`;
            case "Alert":
                return `<div class="alert alert-info" style="${textStyle}">${lbl}</div>`;
            case "Divider":
            case "Separator":
                return `<hr style="border-top:${el.style.borderWidth || '1px'} ${el.style.borderStyle || 'solid'} ${el.style.borderColor || '#dee2e6'}; width:100%; opacity:1;">`;
            case "Spacer":
                return `<div style="height:${el.props.height || '20px'}"></div>`;
            case "Table":
                const headers = (el.props.headers || 'ردیف,عنوان,مقدار').split(',');
                const rowCount = el.props.rowCount || 3;
                const tableClasses = `table ${el.props.striped ? 'table-striped' : ''} ${el.props.bordered !== false ? 'table-bordered' : ''} ${el.props.hover !== false ? 'table-hover' : ''}`;
                return `
                    <table class="${tableClasses}" style="${textStyle}">
                        <thead class="table-light">
                            <tr>${headers.map(h => `<th>${h.trim()}</th>`).join('')}</tr>
                        </thead>
                        <tbody>
                            ${Array(rowCount).fill('').map((_, i) => `<tr>${headers.map(() => '<td>-</td>').join('')}</tr>`).join('')}
                        </tbody>
                    </table>`;
            case "Slider":
            case "Carousel":
                const slides = el.data.slides || [];
                const animClass = el.props.animation || 'fade';
                return `
                    <div class="fb-slider border rounded p-2" style="${inputStyle}" id="${el.id}">
                        <div class="d-flex align-items-center justify-content-center bg-light text-muted" style="height:150px;">
                            <div class="text-center">
                                <i class="bi bi-images fs-1"></i><br>
                                <small>اسلایدر ${slides.length > 0 ? `(${slides.length} تصویر)` : '(بدون تصویر)'}</small><br>
                                <small class="text-primary">سرعت: ${el.props.speed || 3}ث | انیمیشن: ${animClass}</small>
                            </div>
                        </div>
                    </div>`;
            case "Row":
            case "Container": 
            case "Flex":
            case "FlexContainer":
            case "Card":
            case "Panel":
                return ``; 
            case "Fieldset":
                return `<fieldset class="border rounded p-3" style="border-color:var(--border-light) !important; background:rgba(0,0,0,0.2);"><legend class="float-none w-auto px-2" style="${textStyle}">${lbl}</legend></fieldset>`;
            default:
                controlHtml = `<div class="p-2 border rounded" style="border-color:var(--border-light) !important; background:rgba(0,0,0,0.2); ${textStyle}"><i class="${el.icon} me-1"></i> ${lbl}</div>`;
                break;
        }

        // Standard Wrapper with Label
        const labelStyle = textStyle ? `style="${textStyle}"` : `style="color:#ffffff;"`;
        return `
            <label class="form-label" ${labelStyle}>${lbl} ${req}</label>
            ${controlHtml}
        `;
    }


    // ==========================================
    //           SELECTION & MODAL
    // ==========================================

    selectElement(id) {
        this.deselectAll();

        const el = this.findElementById(id);
        if (!el) return;

        this.selectedElement = el;

        // Visual
        const wrapper = document.querySelector(`[data-id="${id}"]`);
        if (wrapper) {
            wrapper.classList.add("selected");
            
            // Highlight Parent Row/Container if exists
            // We look for closest parent wrapper that is NOT the element itself
            const parentWrapper = wrapper.parentElement.closest('.fb-control-wrapper');
            if(parentWrapper) {
                parentWrapper.classList.add("child-selected");
            }
        }

        // Open Modal
        this.openPropertyModal(el);
    }
    
    deselectAll() {
        document.querySelectorAll(".selected").forEach((e) => e.classList.remove("selected"));
        document.querySelectorAll(".child-selected").forEach((e) => e.classList.remove("child-selected"));
        this.selectedElement = null;
    }

    openPropertyModal(el) {
        // Set Control Name in Header
        const controlNameSpan = document.getElementById('prop-control-name');
        if(controlNameSpan) controlNameSpan.textContent = el.props.label || el.type;

        // Initialize data objects if missing
        if(!el.data) el.data = {};
        if(!el.db) el.db = {};

        // ==================== TAB 1: GENERAL ====================
        // Section 1: Basic Info
        document.getElementById('prop_label_fa').value = el.props.label || '';
        document.getElementById('prop_label_en').value = el.props.nameEn || el.id;
        document.getElementById('prop_readonly').checked = el.props.readonly || false;
        document.getElementById('prop_disabled').checked = el.props.disabled || false;

        // Section 2: Font Styles
        document.getElementById('prop_font_family').value = el.style.fontFamily || 'inherit';
        document.getElementById('prop_font_size').value = parseInt(el.style.fontSize) || 14;
        document.getElementById('prop_font_weight').value = el.style.fontWeight || '400';
        document.getElementById('prop_font_color').value = el.style.color || '#333333';
        document.getElementById('prop_font_color_text').value = el.style.color || '#333333';
        
        // Text Align
        const alignValue = el.style.textAlign || 'right';
        document.querySelectorAll('input[name="prop_text_align"]').forEach(r => r.checked = r.value === alignValue);
        
        // Text Styles
        document.getElementById('prop_underline').checked = el.style.textDecoration === 'underline';
        document.getElementById('prop_italic').checked = el.style.fontStyle === 'italic';
        document.getElementById('prop_bold').checked = parseInt(el.style.fontWeight) >= 600;

        // Section 3: Layout & Border
        document.getElementById('prop_margin_top').value = parseInt(el.style.marginTop) || 0;
        document.getElementById('prop_margin_bottom').value = parseInt(el.style.marginBottom) || 15;
        document.getElementById('prop_margin_left').value = parseInt(el.style.marginLeft) || 0;
        document.getElementById('prop_margin_right').value = parseInt(el.style.marginRight) || 0;
        
        document.getElementById('prop_padding_top').value = parseInt(el.style.paddingTop) || 8;
        document.getElementById('prop_padding_bottom').value = parseInt(el.style.paddingBottom) || 8;
        document.getElementById('prop_padding_left').value = parseInt(el.style.paddingLeft) || 8;
        document.getElementById('prop_padding_right').value = parseInt(el.style.paddingRight) || 8;
        
        document.getElementById('prop_border_width').value = parseInt(el.style.borderWidth) || 0;
        document.getElementById('prop_border_style').value = el.style.borderStyle || 'solid';
        document.getElementById('prop_border_color').value = el.style.borderColor || '#cccccc';
        document.getElementById('prop_border_radius').value = parseInt(el.style.borderRadius) || 4;
        
        document.getElementById('prop_bg_color').value = el.style.backgroundColor || '#ffffff';
        document.getElementById('prop_bg_color_text').value = el.style.backgroundColor || '#ffffff';
        document.getElementById('prop_z_index').value = el.style.zIndex || '';
        document.getElementById('prop_display').value = el.style.display || 'block';

        // Section 4: Tooltip
        document.getElementById('prop_tooltip_enabled').checked = el.props.tooltipEnabled || false;
        document.getElementById('prop_tooltip_text').value = el.props.tooltipText || '';
        document.getElementById('prop_tooltip_text').disabled = !el.props.tooltipEnabled;

        // ==================== TAB 2: SPECIFIC ====================
        this.renderSpecificPropertiesNewModal(el);

        // ==================== TAB 3: VALIDATIONS ====================
        this.renderValidationList(el);

        // ==================== TAB 4: DATABASE ====================
        // Auto-detect DataType based on control type
        const detectedType = this.detectDataType(el.type);
        
        // Set database field name from English name or fallback to element ID
        const dbFieldName = el.props.nameEn || el.data?.databaseColumnName || el.db?.columnName || el.id;
        document.getElementById('db_field_name').value = dbFieldName;
        
        // Store in both structures for consistency
        if(!el.data) el.data = {};
        if(!el.db) el.db = {};
        el.data.databaseColumnName = dbFieldName;
        el.db.columnName = dbFieldName;
        
        // Set and store database column type
        document.getElementById('db_data_type').value = detectedType.sqlType;
        el.data.databaseColumnType = detectedType.baseType;
        el.db.columnType = detectedType.baseType;
        
        // Show dynamic parameters
        this.renderDatabaseDynamicParams(el, detectedType);
        
        // Initialize database properties
        if(!el.db) el.db = {};
        document.getElementById('db_nullable').checked = el.db.nullable !== false;
        document.getElementById('db_has_index').checked = el.db.hasIndex || false;
        document.getElementById('db_is_unique').checked = el.db.isUnique || false;
        
        // Default value handling
        const defaultValueInput = document.getElementById('db_default_value');
        if(el.db.defaultFunctionId) {
            // Function is selected, disable textbox
            defaultValueInput.value = '';
            defaultValueInput.disabled = true;
        } else {
            defaultValueInput.value = el.db.defaultValue || '';
            defaultValueInput.disabled = false;
        }
        document.getElementById('db_data_source_type').value = el.data.sourceType || 'static';
        
        // Function selection display
        if(el.db.defaultFunctionName) {
            document.getElementById('selected-function-display').classList.remove('d-none');
            document.getElementById('selected-function-name').textContent = el.db.defaultFunctionName;
        } else {
            document.getElementById('selected-function-display').classList.add('d-none');
        }

        // ==================== TAB 5: CUSTOM CODE ====================
        const customCssEl = document.getElementById('prop_custom_css');
        const customJsEl = document.getElementById('prop_custom_js');
        if (customCssEl) {
            // Set default class name in placeholder or value if empty
            const defaultClass = `.fb-field-${el.id}`;
            customCssEl.value = el.style.customCss || '';
            if (!el.style.customCss && customCssEl.placeholder) {
                customCssEl.placeholder = `/* Custom styles for this field */\n${defaultClass} {\n    /* Your styles here */\n}`;
            }
        }
        if (customJsEl) {
            customJsEl.value = el.props.customJs || '';
        }

        // ==================== BIND ALL EVENTS ====================
        this.bindPropertyModalEvents(el);

        this.modalProperties.show();
    }

    // New method for binding events to property modal inputs
    bindPropertyModalEvents(el) {
        // Tab 1: General - Basic Info
        document.getElementById('prop_label_fa').oninput = (e) => { el.props.label = e.target.value; this.refreshControl(); };
        document.getElementById('prop_label_en').oninput = (e) => { 
            const englishName = e.target.value.trim();
            el.props.nameEn = englishName;
            
            // Update database field name immediately
            const dbFieldNameInput = document.getElementById('db_field_name');
            if(dbFieldNameInput) {
                dbFieldNameInput.value = englishName;
            }
            
            // Update in data structures
            if(!el.data) el.data = {};
            if(!el.db) el.db = {};
            el.data.dbColumn = englishName;
            el.db.columnName = englishName;
            el.data.databaseColumnName = englishName;
        };
        document.getElementById('prop_readonly').onchange = (e) => { el.props.readonly = e.target.checked; this.refreshControl(); };
        document.getElementById('prop_disabled').onchange = (e) => { el.props.disabled = e.target.checked; this.refreshControl(); };

        // Font Styles
        document.getElementById('prop_font_family').onchange = (e) => { el.style.fontFamily = e.target.value; this.refreshControl(); };
        document.getElementById('prop_font_size').oninput = (e) => { el.style.fontSize = e.target.value + 'px'; this.refreshControl(); };
        document.getElementById('prop_font_weight').onchange = (e) => { el.style.fontWeight = e.target.value; this.refreshControl(); };
        document.getElementById('prop_font_color').oninput = (e) => { 
            el.style.color = e.target.value; 
            document.getElementById('prop_font_color_text').value = e.target.value;
            this.refreshControl(); 
        };
        
        // Text Align
        document.querySelectorAll('input[name="prop_text_align"]').forEach(r => {
            r.onchange = (e) => { el.style.textAlign = e.target.value; this.refreshControl(); };
        });
        
        // Text Styles
        document.getElementById('prop_underline').onchange = (e) => { 
            el.style.textDecoration = e.target.checked ? 'underline' : 'none'; 
            this.refreshControl(); 
        };
        document.getElementById('prop_italic').onchange = (e) => { 
            el.style.fontStyle = e.target.checked ? 'italic' : 'normal'; 
            this.refreshControl(); 
        };

        // Margins
        ['prop_margin_top', 'prop_margin_bottom', 'prop_margin_left', 'prop_margin_right'].forEach(id => {
            const prop = id.replace('prop_', '').replace('_', '');
            document.getElementById(id).oninput = (e) => { 
                el.style['margin' + prop.charAt(6).toUpperCase() + prop.slice(7)] = e.target.value + 'px'; 
            };
        });
        document.getElementById('prop_margin_top').oninput = (e) => { el.style.marginTop = e.target.value + 'px'; };
        document.getElementById('prop_margin_bottom').oninput = (e) => { el.style.marginBottom = e.target.value + 'px'; };
        document.getElementById('prop_margin_left').oninput = (e) => { el.style.marginLeft = e.target.value + 'px'; };
        document.getElementById('prop_margin_right').oninput = (e) => { el.style.marginRight = e.target.value + 'px'; };

        // Paddings
        document.getElementById('prop_padding_top').oninput = (e) => { el.style.paddingTop = e.target.value + 'px'; };
        document.getElementById('prop_padding_bottom').oninput = (e) => { el.style.paddingBottom = e.target.value + 'px'; };
        document.getElementById('prop_padding_left').oninput = (e) => { el.style.paddingLeft = e.target.value + 'px'; };
        document.getElementById('prop_padding_right').oninput = (e) => { el.style.paddingRight = e.target.value + 'px'; };

        // Border
        document.getElementById('prop_border_width').oninput = (e) => { el.style.borderWidth = e.target.value + 'px'; this.refreshControl(); };
        document.getElementById('prop_border_style').onchange = (e) => { el.style.borderStyle = e.target.value; this.refreshControl(); };
        document.getElementById('prop_border_color').oninput = (e) => { el.style.borderColor = e.target.value; this.refreshControl(); };
        document.getElementById('prop_border_radius').oninput = (e) => { el.style.borderRadius = e.target.value + 'px'; this.refreshControl(); };

        // Background
        document.getElementById('prop_bg_color').oninput = (e) => { 
            el.style.backgroundColor = e.target.value; 
            document.getElementById('prop_bg_color_text').value = e.target.value;
            this.refreshControl(); 
        };
        document.getElementById('prop_z_index').oninput = (e) => { el.style.zIndex = e.target.value; };
        document.getElementById('prop_display').onchange = (e) => { el.style.display = e.target.value; this.refreshControl(); };

        // Tooltip
        document.getElementById('prop_tooltip_enabled').onchange = (e) => { 
            el.props.tooltipEnabled = e.target.checked;
            document.getElementById('prop_tooltip_text').disabled = !e.target.checked;
        };
        document.getElementById('prop_tooltip_text').oninput = (e) => { el.props.tooltipText = e.target.value; };

        // Tab 4: Database
        // Initialize database properties structure
        if(!el.db) el.db = {};
        if(!el.data) el.data = {};
        
        // Sync between el.db and el.data for backward compatibility
        el.data.isNullable = el.db.nullable !== false;
        el.data.hasIndex = el.db.hasIndex || false;
        el.data.isUnique = el.db.isUnique || false;
        el.data.defaultValue = el.db.defaultValue || '';
        el.data.maxLength = el.db.maxLength;
        
        document.getElementById('db_nullable').checked = el.data.isNullable;
        document.getElementById('db_nullable').onchange = (e) => { 
            el.data.isNullable = e.target.checked;
            el.db.nullable = e.target.checked;
        };
        
        document.getElementById('db_has_index').checked = el.data.hasIndex;
        document.getElementById('db_has_index').onchange = (e) => { 
            el.data.hasIndex = e.target.checked;
            el.db.hasIndex = e.target.checked;
            // If unique is checked, index should also be checked
            if(!e.target.checked && document.getElementById('db_is_unique').checked) {
                document.getElementById('db_is_unique').checked = false;
                el.data.isUnique = false;
                el.db.isUnique = false;
            }
        };
        
        const dbIsUnique = document.getElementById('db_is_unique');
        if(dbIsUnique) {
            dbIsUnique.checked = el.data.isUnique || false;
            dbIsUnique.onchange = (e) => { 
                el.data.isUnique = e.target.checked;
                el.db.isUnique = e.target.checked;
                // Unique requires index
                if(e.target.checked) {
                    document.getElementById('db_has_index').checked = true;
                    el.data.hasIndex = true;
                    el.db.hasIndex = true;
                }
            };
        }
        
        // Default value handling
        const defaultValueInput = document.getElementById('db_default_value');
        if(el.db.defaultFunctionId) {
            defaultValueInput.value = '';
            defaultValueInput.disabled = true;
        } else {
            defaultValueInput.value = el.data.defaultValue || '';
            defaultValueInput.disabled = false;
        }
        defaultValueInput.oninput = (e) => { 
            el.data.defaultValue = e.target.value;
            el.db.defaultValue = e.target.value;
        };
        
        if(document.getElementById('db_data_source_type'))
             document.getElementById('db_data_source_type').onchange = (e) => { el.data.sourceType = e.target.value; };

        // Function Selection Button
        document.getElementById('btn-select-function').onclick = () => this.openFunctionSelectionModal(el);
        document.getElementById('btn-clear-function').onclick = () => {
            if(!el.db) el.db = {};
            el.db.defaultFunctionId = null;
            el.db.defaultFunctionName = null;
            document.getElementById('selected-function-display').classList.add('d-none');
            // Re-enable default value textbox
            const defaultValueInput = document.getElementById('db_default_value');
            if(defaultValueInput) {
                defaultValueInput.disabled = false;
            }
        };

        // Tab 5: Custom Code
        const customCssEl = document.getElementById('prop_custom_css');
        const customJsEl = document.getElementById('prop_custom_js');
        
        if (customCssEl) {
            customCssEl.oninput = (e) => { 
                el.style.customCss = e.target.value; 
                this.debounce(`custom_css_${el.id}`, () => {
                    this.refreshControl(true);
                }, 500);
            };
        }
        
        if (customJsEl) {
            // Initialize CodeMirror for custom JS if available
            if (typeof CodeMirror !== 'undefined' && !customJsEl.dataset.cmInitialized) {
                try {
                    const cmInstance = CodeMirror.fromTextArea(customJsEl, {
                        mode: 'javascript',
                        theme: 'monokai',
                        lineNumbers: true,
                        indentUnit: 4,
                        lineWrapping: true,
                        matchBrackets: true
                    });
                    
                    cmInstance.on('change', (instance) => {
                        el.props.customJs = instance.getValue();
                        this.debounce(`custom_js_${el.id}`, () => {
                            this.refreshControl(true);
                        }, 500);
                    });
                    
                    customJsEl.dataset.cmInitialized = 'true';
                } catch (error) {
                    console.error('Error initializing CodeMirror for custom JS:', error);
                    // Fallback to regular textarea
                    customJsEl.oninput = (e) => { 
                        el.props.customJs = e.target.value; 
                        this.debounce(`custom_js_${el.id}`, () => {
                            this.refreshControl(true);
                        }, 500);
                    };
                }
            } else {
                customJsEl.oninput = (e) => { 
                    el.props.customJs = e.target.value; 
                    this.debounce(`custom_js_${el.id}`, () => {
                        this.refreshControl(true);
                    }, 500);
                };
            }
        }

        // Save button
        document.getElementById('btn-save-properties').onclick = () => {
            this.modalProperties.hide();
            this.refreshControl();
        };
    }

    // Data Type auto-detection based on control type
    detectDataType(controlType) {
        // Map control types to SQL Server data types
        // Returns: { sqlType: string, baseType: string, hasMaxLength: boolean, hasPrecision: boolean }
        const typeMap = {
            // String-based controls
            'TextInput': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: 255 },
            'Password': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: 100 },
            'Email': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: 255 },
            'Phone': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: 20 },
            'Tel': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: 20 },
            'Url': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: 500 },
            'Search': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: 255 },
            'Textarea': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: null }, // MAX
            'RichTextEditor': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: null }, // MAX (XML/JSON)
            
            // Numeric controls
            'Number': { sqlType: 'int', baseType: 'int', hasMaxLength: false, hasPrecision: false },
            'RangeSlider': { sqlType: 'int', baseType: 'int', hasMaxLength: false, hasPrecision: false },
            'Rating': { sqlType: 'int', baseType: 'int', hasMaxLength: false, hasPrecision: false },
            
            // Boolean controls
            'Checkbox': { sqlType: 'bit', baseType: 'bit', hasMaxLength: false, hasPrecision: false },
            'Switch': { sqlType: 'bit', baseType: 'bit', hasMaxLength: false, hasPrecision: false },
            
            // Date/Time controls
            'DatePicker': { sqlType: 'date', baseType: 'date', hasMaxLength: false, hasPrecision: false },
            'PersianDatePicker': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: 10 },
            'TimePicker': { sqlType: 'time', baseType: 'time', hasMaxLength: false, hasPrecision: false },
            'DateTimePicker': { sqlType: 'datetime2', baseType: 'datetime2', hasMaxLength: false, hasPrecision: false },
            
            // Selection controls
            'Select': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: 100 },
            'MultiSelect': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: null }, // MAX
            'RadioButton': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: 100 },
            'ComboBox': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: 100 },
            'Dropdown': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: 100 },
            
            // File controls
            'FileUpload': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: 500 },
            'ImageUpload': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: 500 },
            
            // Special controls
            'ColorPicker': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: 20 },
            'Signature': { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: null } // MAX
        };
        
        const detected = typeMap[controlType] || { sqlType: 'nvarchar', baseType: 'nvarchar', hasMaxLength: true, defaultMaxLength: 255 };
        
        // Format sqlType for display
        let displayType = detected.sqlType;
        if (detected.hasMaxLength && detected.defaultMaxLength !== null) {
            displayType = `${detected.sqlType}(${detected.defaultMaxLength})`;
        } else if (detected.hasMaxLength && detected.defaultMaxLength === null) {
            displayType = `${detected.sqlType}(MAX)`;
        }
        
        return {
            sqlType: displayType,
            baseType: detected.baseType,
            hasMaxLength: detected.hasMaxLength,
            hasPrecision: detected.hasPrecision || false,
            defaultMaxLength: detected.defaultMaxLength
        };
    }

    // Render dynamic parameters in Database tab based on data type
    renderDatabaseDynamicParams(el, detectedType) {
        const container = document.getElementById('db-dynamic-params');
        if(!container) return;
        
        // Initialize el.db if not exists
        if(!el.db) el.db = {};
        
        const baseType = detectedType.baseType;
        
        // Only show parameters relevant to the data type
        if(detectedType.hasMaxLength) {
            // String-based types: Show MaxLength parameter
            const currentMaxLength = el.db.maxLength || detectedType.defaultMaxLength;
            const isMax = currentMaxLength === null || currentMaxLength === 'MAX' || currentMaxLength === '';
            
            container.innerHTML = `
                <div class="col-md-12">
                    <label class="form-label small">حداکثر طول (MaxLength) <span class="text-danger">*</span></label>
                    <div class="input-group input-group-sm">
                        <input type="number" 
                               class="form-control" 
                               id="db_max_length" 
                               value="${isMax ? '' : currentMaxLength}" 
                               placeholder="${isMax ? 'MAX' : currentMaxLength}"
                               min="1"
                               ${isMax ? 'disabled' : ''}>
                        <button type="button" 
                                class="btn btn-outline-secondary" 
                                id="db_max_btn"
                                ${isMax ? 'active' : ''}>
                            ${isMax ? 'MAX فعال' : 'استفاده از MAX'}
                        </button>
                    </div>
                    <small class="text-muted">برای متن‌های طولانی از MAX استفاده کنید</small>
                </div>
            `;
            
            const maxLengthInput = document.getElementById('db_max_length');
            const maxBtn = document.getElementById('db_max_btn');
            
            maxLengthInput.oninput = (e) => {
                const value = parseInt(e.target.value);
                if(value && value > 0) {
                    el.db.maxLength = value;
                    maxBtn.textContent = 'استفاده از MAX';
                    maxBtn.classList.remove('active');
                }
            };
            
            maxBtn.onclick = () => {
                el.db.maxLength = null; // or 'MAX'
                maxLengthInput.value = '';
                maxLengthInput.disabled = true;
                maxBtn.textContent = 'MAX فعال';
                maxBtn.classList.add('active');
            };
            
            // Initialize state
            if(isMax) {
                maxLengthInput.disabled = true;
                maxBtn.classList.add('active');
            }
        } else if(detectedType.hasPrecision) {
            // Decimal types: Show Precision and Scale
            container.innerHTML = `
                <div class="col-md-6">
                    <label class="form-label small">Precision <span class="text-danger">*</span></label>
                    <input type="number" 
                           class="form-control form-control-sm" 
                           id="db_precision" 
                           value="${el.db.precision || 18}" 
                           min="1" 
                           max="38"
                           required>
                    <small class="text-muted">تعداد کل ارقام (1-38)</small>
                </div>
                <div class="col-md-6">
                    <label class="form-label small">Scale <span class="text-danger">*</span></label>
                    <input type="number" 
                           class="form-control form-control-sm" 
                           id="db_scale" 
                           value="${el.db.scale || 2}" 
                           min="0" 
                           max="38"
                           required>
                    <small class="text-muted">تعداد ارقام اعشار (0-38)</small>
                </div>
            `;
            
            document.getElementById('db_precision').oninput = (e) => {
                el.db.precision = parseInt(e.target.value) || 18;
            };
            document.getElementById('db_scale').oninput = (e) => {
                el.db.scale = parseInt(e.target.value) || 2;
            };
        } else {
            // Numeric (int), Boolean (bit), Date/Time types: No additional parameters
            container.innerHTML = `
                <div class="col-12">
                    <div class="alert alert-info py-2 mb-0">
                        <i class="bi bi-info-circle me-1"></i>
                        <small>این نوع داده (${detectedType.baseType.toUpperCase()}) پارامتر اضافی ندارد.</small>
                    </div>
                </div>
            `;
        }
    }

    // Function selection modal
    async openFunctionSelectionModal(el) {
        const modal = document.getElementById('modal-select-function');
        const functionList = document.getElementById('function-list');
        const fieldTypeDisplay = document.getElementById('function-field-type-display');
        const functionPreview = document.getElementById('function-preview');
        const functionError = document.getElementById('function-error');
        const validationStatus = document.getElementById('function-validation-status');
        
        // Get current field data type
        const detectedType = this.detectDataType(el.type);
        const fieldBaseType = detectedType.baseType.toLowerCase();
        
        // Display field type in modal
        if(fieldTypeDisplay) {
            fieldTypeDisplay.textContent = detectedType.sqlType;
        }
        
        // Clear previous state
        functionList.innerHTML = '';
        functionPreview.innerHTML = '<p class="text-muted small text-center mt-4">یک تابع انتخاب کنید</p>';
        if(functionError) functionError.classList.add('d-none');
        if(validationStatus) validationStatus.innerHTML = '';
        
        // Load functions from API
        let functions = [];
        try {
            const response = await fetch('/api/formbuilder/scalarfunctions');
            if(response.ok) {
                functions = await response.json();
            } else {
                functions = this.getSampleFunctions();
            }
        } catch(error) {
            console.warn('Failed to load functions from API, using samples:', error);
            functions = this.getSampleFunctions();
        }
        
        // Map SQL data types for validation
        const typeCompatibilityMap = {
            'nvarchar': ['nvarchar', 'varchar', 'char', 'nchar', 'text', 'ntext'],
            'int': ['int', 'bigint', 'smallint', 'tinyint'],
            'bit': ['bit', 'boolean'],
            'datetime': ['datetime', 'datetime2', 'smalldatetime'],
            'datetime2': ['datetime', 'datetime2', 'smalldatetime'],
            'date': ['date', 'datetime', 'datetime2'],
            'time': ['time'],
            'decimal': ['decimal', 'numeric', 'money', 'smallmoney'],
            'float': ['float', 'real'],
            'uniqueidentifier': ['uniqueidentifier']
        };
        
        // Populate function list with validation
        functions.forEach(f => {
            const opt = document.createElement('option');
            opt.value = f.id || f.Id;
            opt.textContent = f.displayName || f.DisplayName || f.name || f.Name;
            opt.dataset.name = f.name || f.Name;
            opt.dataset.returnType = (f.returnType || f.ReturnType || '').toLowerCase();
            opt.dataset.displayName = f.displayName || f.DisplayName;
            opt.dataset.description = f.description || f.Description || '';
            
            // Check compatibility
            const functionReturnType = opt.dataset.returnType;
            const compatibleTypes = typeCompatibilityMap[fieldBaseType] || [fieldBaseType];
            const isCompatible = compatibleTypes.includes(functionReturnType) || 
                                 functionReturnType === fieldBaseType ||
                                 (fieldBaseType === 'nvarchar' && functionReturnType.includes('varchar'));
            
            if(isCompatible) {
                opt.style.backgroundColor = '#d4edda';
            } else {
                opt.style.color = '#dc3545';
            }
            
            functionList.appendChild(opt);
        });

        // Handle function selection
        functionList.onchange = () => {
            const selected = functionList.selectedOptions[0];
            if(selected) {
                const functionReturnType = selected.dataset.returnType;
                const compatibleTypes = typeCompatibilityMap[fieldBaseType] || [fieldBaseType];
                const isCompatible = compatibleTypes.includes(functionReturnType) || 
                                     functionReturnType === fieldBaseType ||
                                     (fieldBaseType === 'nvarchar' && functionReturnType.includes('varchar'));
                
                functionPreview.innerHTML = `
                    <div class="small">
                        <strong>${selected.textContent}</strong><br>
                        <code>${selected.dataset.name}()</code><br>
                        <span class="text-muted d-block mt-2">نوع خروجی: <code>${selected.dataset.returnType}</code></span>
                        ${selected.dataset.description ? `<p class="text-muted mt-2 small">${selected.dataset.description}</p>` : ''}
                    </div>
                `;
                
                // Show validation status
                if(validationStatus) {
                    if(isCompatible) {
                        validationStatus.innerHTML = `
                            <div class="alert alert-success py-2 mb-0">
                                <i class="bi bi-check-circle me-1"></i>
                                <small>نوع خروجی تابع با نوع داده فیلد مطابقت دارد</small>
                            </div>
                        `;
                        if(functionError) functionError.classList.add('d-none');
                    } else {
                        validationStatus.innerHTML = `
                            <div class="alert alert-danger py-2 mb-0">
                                <i class="bi bi-exclamation-triangle me-1"></i>
                                <small>نوع خروجی تابع (<code>${selected.dataset.returnType}</code>) با نوع داده فیلد (<code>${fieldBaseType}</code>) مطابقت ندارد!</small>
                            </div>
                        `;
                        if(functionError) {
                            functionError.classList.remove('d-none');
                            functionError.innerHTML = `
                                <i class="bi bi-exclamation-triangle me-1"></i>
                                <strong>خطا:</strong> نوع خروجی تابع باید <code>${fieldBaseType}</code> باشد، اما تابع انتخاب شده <code>${selected.dataset.returnType}</code> برمی‌گرداند.
                            `;
                        }
                    }
                }
            }
        };

        // Confirm button handler
        const confirmBtn = document.getElementById('btn-confirm-function');
        if(confirmBtn) {
            confirmBtn.onclick = () => {
                const selected = functionList.selectedOptions[0];
                if(!selected) {
                    alert('لطفا یک تابع انتخاب کنید');
                    return;
                }
                
                const functionReturnType = selected.dataset.returnType;
                const compatibleTypes = typeCompatibilityMap[fieldBaseType] || [fieldBaseType];
                const isCompatible = compatibleTypes.includes(functionReturnType) || 
                                     functionReturnType === fieldBaseType ||
                                     (fieldBaseType === 'nvarchar' && functionReturnType.includes('varchar'));
                
                if(!isCompatible) {
                    if(!confirm('نوع خروجی تابع با نوع داده فیلد مطابقت ندارد. آیا مطمئن هستید که می‌خواهید ادامه دهید؟')) {
                        return;
                    }
                }
                
                // Save function selection
                if(!el.db) el.db = {};
                el.db.defaultFunctionId = selected.value;
                el.db.defaultFunctionName = selected.dataset.name;
                el.db.defaultFunctionDisplayName = selected.textContent;
                
                // Update UI
                document.getElementById('selected-function-display').classList.remove('d-none');
                document.getElementById('selected-function-name').textContent = selected.textContent;
                
                // Disable default value textbox
                const defaultValueInput = document.getElementById('db_default_value');
                if(defaultValueInput) {
                    defaultValueInput.value = '';
                    defaultValueInput.disabled = true;
                }
                
                // Hide modal
                bootstrap.Modal.getInstance(modal).hide();
            };
        }
        
        // Show modal
        new bootstrap.Modal(modal).show();
    }
    
    // Sample functions for fallback
    getSampleFunctions() {
        return [
            { id: '1', name: 'GETDATE', displayName: 'تاریخ و زمان جاری', returnType: 'datetime2', description: 'برگرداندن تاریخ و زمان فعلی سیستم' },
            { id: '2', name: 'NEWID', displayName: 'شناسه یکتا (GUID)', returnType: 'uniqueidentifier', description: 'تولید شناسه یکتای جهانی' },
            { id: '3', name: 'CURRENT_USER', displayName: 'کاربر جاری', returnType: 'nvarchar', description: 'نام کاربری فعلی' },
            { id: '4', name: 'HOST_NAME', displayName: 'نام سیستم کلاینت', returnType: 'nvarchar', description: 'نام کامپیوتر کلاینت' },
            { id: '5', name: 'DEFAULT_FALSE', displayName: 'مقدار خیر', returnType: 'bit', description: 'مقدار پیش‌فرض false' },
            { id: '6', name: 'DEFAULT_TRUE', displayName: 'مقدار بله', returnType: 'bit', description: 'مقدار پیش‌فرض true' },
            { id: '7', name: 'ZERO_INT', displayName: 'عدد صفر', returnType: 'int', description: 'مقدار پیش‌فرض صفر' },
            { id: '8', name: 'EMPTY_STRING', displayName: 'رشته خالی', returnType: 'nvarchar', description: 'رشته خالی به عنوان پیش‌فرض' },
        ];
    }

    // New Specific Properties for new modal structure
    renderSpecificPropertiesNewModal(el) {
        const container = document.getElementById('prop-container-specific');
        if(!container) return;
        
        // Clear and generate based on control type
        container.innerHTML = '';
        
        const type = el.type;
        
        if(['TextInput', 'Password', 'Email', 'Phone', 'Number'].includes(type)) {
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات ورودی</legend>
                    <div class="row g-2">
                        <div class="col-md-6">
                            <label class="form-label small">متن راهنما (Placeholder)</label>
                            <input type="text" class="form-control form-control-sm" id="spec_placeholder" value="${el.props.placeholder || ''}">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">نوع ورودی</label>
                            <select class="form-select form-select-sm" id="spec_input_type">
                                <option value="text" ${(type === 'TextInput' && !el.props.inputType) || el.props.inputType === 'text' ? 'selected' : ''}>متن عادی</option>
                                <option value="password" ${type === 'Password' || el.props.inputType === 'password' ? 'selected' : ''}>رمز عبور</option>
                                <option value="email" ${type === 'Email' || el.props.inputType === 'email' ? 'selected' : ''}>ایمیل</option>
                                <option value="tel" ${type === 'Phone' || el.props.inputType === 'tel' ? 'selected' : ''}>تلفن</option>
                                <option value="number" ${type === 'Number' || el.props.inputType === 'number' ? 'selected' : ''}>عدد</option>
                            </select>
                        </div>
                        <div class="col-md-6">
                            <div class="form-check form-switch mt-2">
                                <input class="form-check-input" type="checkbox" id="spec_required" ${el.props.required ? 'checked' : ''}>
                                <label class="form-check-label small">فیلد اجباری</label>
                            </div>
                        </div>
                    </div>
                </fieldset>
            `;
            document.getElementById('spec_placeholder').oninput = (e) => { el.props.placeholder = e.target.value; this.refreshControl(); };
            document.getElementById('spec_required').onchange = (e) => { el.props.required = e.target.checked; this.refreshControl(); };
            
            // Handle input type change - this is critical for password type
            const inputTypeSelect = document.getElementById('spec_input_type');
            if(inputTypeSelect) {
                inputTypeSelect.onchange = (e) => {
                    el.props.inputType = e.target.value;
                    // Update the control immediately
                    this.refreshControl();
                };
            }
            
        } else if(type === 'Table') {
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات جدول</legend>
                    <div class="row g-2">
                        <div class="col-12">
                            <label class="form-label small">ستون‌های سربرگ (با کاما جدا کنید)</label>
                            <input type="text" class="form-control form-control-sm" id="spec_table_headers" value="${el.props.headers || 'ردیف,عنوان,مقدار'}" placeholder="ردیف,عنوان,مقدار">
                            <small class="text-muted">نام هر ستون را با کاما جدا کنید</small>
                        </div>
                        <div class="col-md-4">
                            <label class="form-label small">تعداد ردیف اولیه</label>
                            <input type="number" class="form-control form-control-sm" id="spec_table_rows" value="${el.props.rowCount || 3}" min="1">
                        </div>
                        <div class="col-md-4">
                            <label class="form-label small">نوع ردیف‌ها</label>
                            <select class="form-select form-select-sm" id="spec_table_row_type">
                                <option value="static" ${el.props.rowType === 'static' ? 'selected' : ''}>ثابت (Static)</option>
                                <option value="dynamic" ${el.props.rowType === 'dynamic' ? 'selected' : ''}>پویا (Dynamic)</option>
                            </select>
                        </div>
                        <div class="col-md-4">
                            <label class="form-label small">منبع داده</label>
                            <select class="form-select form-select-sm" id="spec_table_source">
                                <option value="static" ${el.props.dataSource === 'static' ? 'selected' : ''}>دستی</option>
                                <option value="api" ${el.props.dataSource === 'api' ? 'selected' : ''}>API</option>
                                <option value="query" ${el.props.dataSource === 'query' ? 'selected' : ''}>کوئری SQL</option>
                                <option value="form" ${el.props.dataSource === 'form' ? 'selected' : ''}>فرم دیگر</option>
                                <option value="table" ${el.props.dataSource === 'table' ? 'selected' : ''}>جدول دیتابیس</option>
                            </select>
                        </div>
                        <div class="col-12" id="spec_table_binding_container" style="display:${el.props.dataSource && ['form', 'table'].includes(el.props.dataSource) ? 'block' : 'none'}">
                            <label class="form-label small">انتخاب ${el.props.dataSource === 'form' ? 'فرم' : 'جدول'}</label>
                            <select class="form-select form-select-sm" id="spec_table_binding">
                                <option value="">انتخاب کنید...</option>
                            </select>
                        </div>
                        <div class="col-12" id="spec_table_api_container" style="display:${el.props.dataSource === 'api' ? 'block' : 'none'}">
                            <label class="form-label small">آدرس API</label>
                            <input type="text" class="form-control form-control-sm" id="spec_table_api_url" value="${el.props.apiUrl || ''}" dir="ltr" placeholder="https://api.example.com/data">
                        </div>
                        <div class="col-12">
                            <label class="form-label small">استایل جدول</label>
                            <div class="form-check form-check-inline">
                                <input class="form-check-input" type="checkbox" id="spec_table_striped" ${el.props.striped ? 'checked' : ''}>
                                <label class="form-check-label small">راه‌راه</label>
                            </div>
                            <div class="form-check form-check-inline">
                                <input class="form-check-input" type="checkbox" id="spec_table_bordered" ${el.props.bordered !== false ? 'checked' : ''}>
                                <label class="form-check-label small">حاشیه‌دار</label>
                            </div>
                            <div class="form-check form-check-inline">
                                <input class="form-check-input" type="checkbox" id="spec_table_hover" ${el.props.hover !== false ? 'checked' : ''}>
                                <label class="form-check-label small">Hover</label>
                            </div>
                            <div class="form-check form-check-inline">
                                <input class="form-check-input" type="checkbox" id="spec_table_responsive" ${el.props.responsive !== false ? 'checked' : ''}>
                                <label class="form-check-label small">Responsive</label>
                            </div>
                        </div>
                        <div class="col-12">
                            <div class="form-check form-switch">
                                <input class="form-check-input" type="checkbox" id="spec_table_editable" ${el.props.editable ? 'checked' : ''}>
                                <label class="form-check-label small">قابل ویرایش</label>
                            </div>
                        </div>
                        <div class="col-12">
                            <div class="form-check form-switch">
                                <input class="form-check-input" type="checkbox" id="spec_table_sortable" ${el.props.sortable ? 'checked' : ''}>
                                <label class="form-check-label small">قابل مرتب‌سازی</label>
                            </div>
                        </div>
                    </div>
                </fieldset>
            `;
            document.getElementById('spec_table_headers').oninput = (e) => { el.props.headers = e.target.value; this.refreshControl(); };
            document.getElementById('spec_table_rows').oninput = (e) => { el.props.rowCount = parseInt(e.target.value); this.refreshControl(); };
            document.getElementById('spec_table_row_type').onchange = (e) => { el.props.rowType = e.target.value; };
            document.getElementById('spec_table_source').onchange = (e) => {
                el.props.dataSource = e.target.value;
                const bindingContainer = document.getElementById('spec_table_binding_container');
                const apiContainer = document.getElementById('spec_table_api_container');
                if(['form', 'table'].includes(e.target.value)) {
                    bindingContainer.style.display = 'block';
                    apiContainer.style.display = 'none';
                } else if(e.target.value === 'api') {
                    bindingContainer.style.display = 'none';
                    apiContainer.style.display = 'block';
                } else {
                    bindingContainer.style.display = 'none';
                    apiContainer.style.display = 'none';
                }
            };
            document.getElementById('spec_table_api_url').oninput = (e) => { el.props.apiUrl = e.target.value; };
            document.getElementById('spec_table_binding').onchange = (e) => { el.props.bindingId = e.target.value; };
            document.getElementById('spec_table_striped').onchange = (e) => { el.props.striped = e.target.checked; this.refreshControl(); };
            document.getElementById('spec_table_bordered').onchange = (e) => { el.props.bordered = e.target.checked; this.refreshControl(); };
            document.getElementById('spec_table_hover').onchange = (e) => { el.props.hover = e.target.checked; this.refreshControl(); };
            document.getElementById('spec_table_responsive').onchange = (e) => { el.props.responsive = e.target.checked; this.refreshControl(); };
            document.getElementById('spec_table_editable').onchange = (e) => { el.props.editable = e.target.checked; };
            document.getElementById('spec_table_sortable').onchange = (e) => { el.props.sortable = e.target.checked; };
            
        } else if(type === 'Slider' || type === 'Carousel') {
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات اسلایدر</legend>
                    <div class="row g-2">
                        <div class="col-md-6">
                            <label class="form-label small">سرعت تغییر (ثانیه)</label>
                            <input type="number" class="form-control form-control-sm" id="spec_slider_speed" value="${el.props.speed || 3}" min="1" max="10">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">نوع انیمیشن</label>
                            <select class="form-select form-select-sm" id="spec_slider_animation">
                                <option value="fade" ${el.props.animation === 'fade' ? 'selected' : ''}>Fade In</option>
                                <option value="slide" ${el.props.animation === 'slide' ? 'selected' : ''}>Slide Left</option>
                                <option value="zoom" ${el.props.animation === 'zoom' ? 'selected' : ''}>Zoom In</option>
                            </select>
                        </div>
                        <div class="col-12">
                            <label class="form-label small">تصاویر اسلایدر</label>
                            <div class="input-group input-group-sm">
                                <input type="file" class="form-control" id="spec_slider_images" multiple accept="image/*">
                                <button type="button" class="btn btn-outline-primary" id="btn_upload_slider">
                                    <i class="bi bi-upload"></i>
                                </button>
                            </div>
                            <div id="slider_images_preview" class="d-flex gap-2 mt-2 flex-wrap">
                                ${(el.data.slides || []).map((s, i) => `<div class="position-relative"><img src="${s}" class="rounded" style="width:60px;height:40px;object-fit:cover"><button class="btn btn-danger btn-sm position-absolute top-0 end-0" style="padding:0 4px;font-size:10px" onclick="this.parentElement.remove()">×</button></div>`).join('')}
                            </div>
                        </div>
                        <div class="col-12">
                            <div class="form-check form-switch">
                                <input class="form-check-input" type="checkbox" id="spec_slider_autoplay" ${el.props.autoplay !== false ? 'checked' : ''}>
                                <label class="form-check-label small">پخش خودکار</label>
                            </div>
                        </div>
                        <div class="col-12">
                            <button type="button" class="btn btn-sm btn-outline-success" id="btn_generate_slider_js">
                                <i class="bi bi-code-slash me-1"></i>تولید کد JavaScript
                            </button>
                        </div>
                    </div>
                </fieldset>
            `;
            document.getElementById('spec_slider_speed').oninput = (e) => { el.props.speed = parseInt(e.target.value); };
            document.getElementById('spec_slider_animation').onchange = (e) => { el.props.animation = e.target.value; };
            document.getElementById('spec_slider_autoplay').onchange = (e) => { el.props.autoplay = e.target.checked; };
            document.getElementById('btn_generate_slider_js').onclick = () => { this.generateSliderJS(el); };
            
        } else if(['Select', 'MultiSelect', 'RadioButton', 'CheckboxGroup'].includes(type)) {
            const options = el.props.options || [];
            const isDatabaseMode = el.data?.sourceType === 'database';
            const isRadioOrCheckbox = ['RadioButton', 'CheckboxGroup'].includes(type);
            
            // Determine table columns based on mode
            let tableHeaders = '';
            let tableRows = '';
            
            if (isDatabaseMode) {
                if (isRadioOrCheckbox) {
                    tableHeaders = `
                        <th style="width:50px;" class="text-center">ردیف</th>
                        <th>مقدار قابل ذخیره</th>
                        <th>مقدار نمایش داده شده</th>
                    `;
                    tableRows = options.map((opt, idx) => `
                        <tr data-option-index="${idx}" style="cursor:move;" draggable="true">
                            <td class="text-center">
                                <i class="bi bi-grip-vertical text-muted" style="cursor:grab;"></i>
                            </td>
                            <td><input type="text" class="form-control form-control-sm option-value" value="${opt.value || ''}" readonly></td>
                            <td><input type="text" class="form-control form-control-sm option-label" value="${opt.label || ''}" readonly></td>
                        </tr>
                    `).join('');
                } else {
                    tableHeaders = `
                        <th style="width:50px;" class="text-center">ردیف</th>
                        <th>مقدار قابل ذخیره</th>
                        <th>مقدار نمایش داده شده</th>
                        <th>نوع داده</th>
                        <th style="width:100px;">اولویت نمایش</th>
                        <th style="width:100px;">عدم نمایش</th>
                        <th style="width:120px;">مقدار ذخیره</th>
                    `;
                    tableRows = options.map((opt, idx) => {
                        const displayOrder = opt.displayOrder !== undefined ? opt.displayOrder : idx + 1;
                        const hide = opt.hide || false;
                        const isValue = opt.isValue || false;
                        return `
                            <tr data-option-index="${idx}" style="cursor:move;" draggable="true">
                                <td class="text-center">
                                    <i class="bi bi-grip-vertical text-muted" style="cursor:grab;"></i>
                                </td>
                                <td><input type="text" class="form-control form-control-sm option-value" value="${opt.value || ''}" readonly></td>
                                <td><input type="text" class="form-control form-control-sm option-label" value="${opt.label || ''}"></td>
                                <td><code class="small">${opt.description || opt.dataType || ''}</code></td>
                                <td>
                                    <input type="number" class="form-control form-control-sm option-display-order" value="${displayOrder}" min="1" style="width:80px;">
                                </td>
                                <td class="text-center">
                                    <input type="checkbox" class="form-check-input option-hide" ${hide ? 'checked' : ''}>
                                </td>
                                <td class="text-center">
                                    <input type="radio" name="option_value_${el.id}" class="form-check-input option-is-value" ${isValue ? 'checked' : ''}>
                                </td>
                            </tr>
                        `;
                    }).join('');
                }
            } else {
                tableHeaders = `
                    <th style="width:50px;" class="text-center">ردیف</th>
                    <th>مقدار قابل ذخیره</th>
                    <th>مقدار نمایش داده شده</th>
                    <th>توضیحات</th>
                `;
                tableRows = options.map((opt, idx) => `
                    <tr data-option-index="${idx}" style="cursor:pointer;">
                        <td class="text-center">${idx + 1}</td>
                        <td><input type="text" class="form-control form-control-sm option-value" value="${opt.value || ''}" placeholder="مقدار"></td>
                        <td><input type="text" class="form-control form-control-sm option-label" value="${opt.label || ''}" placeholder="عنوان"></td>
                        <td><input type="text" class="form-control form-control-sm option-desc" value="${opt.description || ''}" placeholder="توضیحات (اختیاری)"></td>
                    </tr>
                `).join('');
            }
            
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات لیست</legend>
                    <div class="row g-2">
                        <div class="col-12">
                            <div class="d-flex justify-content-between align-items-center mb-2">
                                <label class="form-label small mb-0">گزینه‌ها${isDatabaseMode ? ' (از دیتابیس)' : ''}</label>
                                <div>
                                    ${!isDatabaseMode ? `
                                    <button type="button" class="btn btn-sm btn-success rounded-circle p-0" id="btn_add_option_${el.id}" style="width:28px;height:28px;line-height:1;" title="افزودن گزینه جدید">
                                        <i class="bi bi-plus-lg"></i>
                                    </button>
                                    <button type="button" class="btn btn-sm btn-danger rounded-circle p-0 ms-1" id="btn_remove_option_${el.id}" style="width:28px;height:28px;line-height:1;" title="حذف گزینه انتخاب شده" disabled>
                                        <i class="bi bi-dash-lg"></i>
                                    </button>
                                    ` : ''}
                                </div>
                            </div>
                            <div class="table-responsive" style="max-height:300px;overflow-y:auto;">
                                <table class="table table-bordered table-sm table-hover mb-0" id="options_table_${el.id}">
                                    <thead class="table-light sticky-top">
                                        <tr>
                                            ${tableHeaders}
                                        </tr>
                                    </thead>
                                    <tbody id="options_tbody_${el.id}">
                                        ${tableRows}
                                        ${options.length === 0 ? `<tr><td colspan="${isDatabaseMode ? (isRadioOrCheckbox ? 3 : 7) : 4}" class="text-center text-muted">هیچ گزینه‌ای تعریف نشده است</td></tr>` : ''}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">منبع داده</label>
                            <select class="form-select form-select-sm" id="spec_data_source">
                                <option value="static" ${el.data?.sourceType === 'static' || !el.data?.sourceType ? 'selected' : ''}>دستی</option>
                                <option value="api" ${el.data?.sourceType === 'api' ? 'selected' : ''}>API URL</option>
                                <option value="combobox" ${el.data?.sourceType === 'combobox' ? 'selected' : ''}>جدول ComboboxItems</option>
                                <option value="database" ${el.data?.sourceType === 'database' ? 'selected' : ''}>اتصال به دیتابیس</option>
                            </select>
                        </div>
                        <div class="col-md-6" id="spec_api_url_container" style="display:${el.data?.sourceType === 'api' ? 'block' : 'none'}">
                            <label class="form-label small">آدرس API</label>
                            <input type="text" class="form-control form-control-sm" id="spec_api_url" value="${el.data?.apiUrl || ''}" dir="ltr">
                        </div>
                        ${isDatabaseMode ? `
                        <div class="col-12">
                            <div class="alert alert-info py-2">
                                <i class="bi bi-info-circle me-1"></i>
                                <small>منبع داده: <strong>${el.data?.databaseTable || ''}</strong></small>
                                <button type="button" class="btn btn-sm btn-outline-primary float-end" onclick="window.fbEngine?.openDatabaseConnectionModal(window.fbEngine?.selectedElement)">
                                    <i class="bi bi-pencil me-1"></i>ویرایش اتصال
                                </button>
                            </div>
                        </div>
                        ` : ''}
                    </div>
                </fieldset>
            `;
            
            // Initialize options table
            this.initializeOptionsTable(el);
            
            document.getElementById('spec_data_source').onchange = (e) => {
                el.data = el.data || {};
                el.data.sourceType = e.target.value;
                const apiContainer = document.getElementById('spec_api_url_container');
                if (apiContainer) {
                    apiContainer.style.display = e.target.value === 'api' ? 'block' : 'none';
                }
                
                // Open database connection modal if database is selected
                if (e.target.value === 'database') {
                    this.openDatabaseConnectionModal(el);
                } else {
                    // Reset database mode
                    if (el.data.sourceType !== 'database') {
                        delete el.data.databaseTable;
                        delete el.data.selectedColumns;
                    }
                }
            };
            
        } else if(['Button', 'SubmitButton', 'ResetButton'].includes(type)) {
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات دکمه</legend>
                    <div class="row g-2">
                        <div class="col-md-6">
                            <label class="form-label small">استایل دکمه</label>
                            <select class="form-select form-select-sm" id="spec_btn_variant">
                                <option value="primary">Primary (آبی)</option>
                                <option value="secondary">Secondary (خاکستری)</option>
                                <option value="success">Success (سبز)</option>
                                <option value="danger">Danger (قرمز)</option>
                                <option value="warning">Warning (زرد)</option>
                                <option value="info">Info (فیروزه‌ای)</option>
                                <option value="light">Light</option>
                                <option value="dark">Dark</option>
                            </select>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">نوع دکمه</label>
                            <select class="form-select form-select-sm" id="spec_btn_type">
                                <option value="button">معمولی (button)</option>
                                <option value="submit">ارسال فرم (submit)</option>
                                <option value="reset">بازنشانی (reset)</option>
                            </select>
                        </div>
                        <div class="col-12">
                            <div class="form-check form-check-inline">
                                <input class="form-check-input" type="checkbox" id="spec_btn_outline" ${el.props.outline ? 'checked' : ''}>
                                <label class="form-check-label small">Outline</label>
                            </div>
                            <div class="form-check form-check-inline">
                                <input class="form-check-input" type="checkbox" id="spec_btn_block" ${el.props.block ? 'checked' : ''}>
                                <label class="form-check-label small">تمام عرض</label>
                            </div>
                        </div>
                    </div>
                </fieldset>
            `;
            document.getElementById('spec_btn_variant').value = el.props.variant || 'primary';
            document.getElementById('spec_btn_type').value = el.props.buttonType || 'button';
            document.getElementById('spec_btn_variant').onchange = (e) => { el.props.variant = e.target.value; this.refreshControl(); };
            document.getElementById('spec_btn_type').onchange = (e) => { el.props.buttonType = e.target.value; };
            document.getElementById('spec_btn_outline').onchange = (e) => { el.props.outline = e.target.checked; this.refreshControl(); };
            document.getElementById('spec_btn_block').onchange = (e) => { el.props.block = e.target.checked; this.refreshControl(); };
            
        } else if(['Row', 'Container', 'FlexContainer'].includes(type)) {
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات چیدمان</legend>
                    <div class="row g-2">
                        <div class="col-md-6">
                            <label class="form-label small">جهت چیدمان</label>
                            <select class="form-select form-select-sm" id="spec_flex_direction">
                                <option value="row" ${el.style.flexDirection === 'row' ? 'selected' : ''}>افقی (Row)</option>
                                <option value="column" ${el.style.flexDirection === 'column' ? 'selected' : ''}>عمودی (Column)</option>
                            </select>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">فاصله بین آیتم‌ها</label>
                            <input type="text" class="form-control form-control-sm" id="spec_flex_gap" value="${el.style.gap || '10px'}">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">تراز افقی</label>
                            <select class="form-select form-select-sm" id="spec_justify">
                                <option value="flex-start">شروع</option>
                                <option value="center">وسط</option>
                                <option value="flex-end">پایان</option>
                                <option value="space-between">Space Between</option>
                                <option value="space-around">Space Around</option>
                            </select>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">تراز عمودی</label>
                            <select class="form-select form-select-sm" id="spec_align">
                                <option value="stretch">کشیده</option>
                                <option value="flex-start">بالا</option>
                                <option value="center">وسط</option>
                                <option value="flex-end">پایین</option>
                            </select>
                        </div>
                    </div>
                </fieldset>
            `;
            document.getElementById('spec_flex_direction').onchange = (e) => { el.style.flexDirection = e.target.value; this.refreshControl(); };
            document.getElementById('spec_flex_gap').oninput = (e) => { el.style.gap = e.target.value; this.refreshControl(); };
            document.getElementById('spec_justify').value = el.style.justifyContent || 'flex-start';
            document.getElementById('spec_align').value = el.style.alignItems || 'stretch';
            document.getElementById('spec_justify').onchange = (e) => { el.style.justifyContent = e.target.value; this.refreshControl(); };
            document.getElementById('spec_align').onchange = (e) => { el.style.alignItems = e.target.value; this.refreshControl(); };
            
        } else if(['TextArea', 'Textarea'].includes(type)) {
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات متن چندخطی</legend>
                    <div class="row g-2">
                        <div class="col-md-6">
                            <label class="form-label small">تعداد ردیف</label>
                            <input type="number" class="form-control form-control-sm" id="spec_rows" value="${el.props.rows || 3}" min="2" max="20">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">حداکثر کاراکتر</label>
                            <input type="number" class="form-control form-control-sm" id="spec_maxlength" value="${el.props.maxlength || ''}" placeholder="بدون محدودیت">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">متن راهنما</label>
                            <input type="text" class="form-control form-control-sm" id="spec_placeholder" value="${el.props.placeholder || ''}">
                        </div>
                        <div class="col-md-6">
                            <div class="form-check form-switch mt-3">
                                <input class="form-check-input" type="checkbox" id="spec_required" ${el.props.required ? 'checked' : ''}>
                                <label class="form-check-label small">فیلد اجباری</label>
                            </div>
                        </div>
                    </div>
                </fieldset>
            `;
            document.getElementById('spec_rows').oninput = (e) => { el.props.rows = parseInt(e.target.value); this.refreshControl(); };
            document.getElementById('spec_maxlength').oninput = (e) => { el.props.maxlength = e.target.value; };
            document.getElementById('spec_placeholder').oninput = (e) => { el.props.placeholder = e.target.value; this.refreshControl(); };
            document.getElementById('spec_required').onchange = (e) => { el.props.required = e.target.checked; this.refreshControl(); };
            
        } else if(type === 'RichTextEditor') {
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات ویرایشگر متن پیشرفته</legend>
                    <div class="row g-2">
                        <div class="col-md-6">
                            <label class="form-label small">ارتفاع ویرایشگر (px)</label>
                            <input type="number" class="form-control form-control-sm" id="spec_editor_height" value="${el.props.height || 300}" min="200" max="800">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">حداکثر کاراکتر</label>
                            <input type="number" class="form-control form-control-sm" id="spec_editor_maxlength" value="${el.props.maxlength || ''}" placeholder="بدون محدودیت">
                        </div>
                        <div class="col-12">
                            <label class="form-label small">ابزارهای نوار ابزار</label>
                            <div class="row g-2">
                                <div class="col-md-3">
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="spec_editor_bold" ${(el.props.toolbar || {}).bold !== false ? 'checked' : ''}>
                                        <label class="form-check-label small">Bold</label>
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="spec_editor_italic" ${(el.props.toolbar || {}).italic !== false ? 'checked' : ''}>
                                        <label class="form-check-label small">Italic</label>
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="spec_editor_underline" ${(el.props.toolbar || {}).underline !== false ? 'checked' : ''}>
                                        <label class="form-check-label small">Underline</label>
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="spec_editor_list" ${(el.props.toolbar || {}).list !== false ? 'checked' : ''}>
                                        <label class="form-check-label small">List</label>
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="spec_editor_link" ${(el.props.toolbar || {}).link !== false ? 'checked' : ''}>
                                        <label class="form-check-label small">Link</label>
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="spec_editor_image" ${(el.props.toolbar || {}).image !== false ? 'checked' : ''}>
                                        <label class="form-check-label small">Image</label>
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="spec_editor_table" ${(el.props.toolbar || {}).table !== false ? 'checked' : ''}>
                                        <label class="form-check-label small">Table</label>
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="form-check">
                                        <input class="form-check-input" type="checkbox" id="spec_editor_code" ${(el.props.toolbar || {}).code !== false ? 'checked' : ''}>
                                        <label class="form-check-label small">Code</label>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">فرمت ذخیره‌سازی</label>
                            <select class="form-select form-select-sm" id="spec_editor_format">
                                <option value="html" ${el.props.format === 'html' ? 'selected' : ''}>HTML</option>
                                <option value="xml" ${el.props.format === 'xml' ? 'selected' : ''}>XML</option>
                                <option value="json" ${el.props.format === 'json' ? 'selected' : ''}>JSON</option>
                            </select>
                        </div>
                        <div class="col-md-6">
                            <div class="form-check form-switch mt-3">
                                <input class="form-check-input" type="checkbox" id="spec_editor_required" ${el.props.required ? 'checked' : ''}>
                                <label class="form-check-label small">فیلد اجباری</label>
                            </div>
                        </div>
                    </div>
                </fieldset>
            `;
            document.getElementById('spec_editor_height').oninput = (e) => { el.props.height = parseInt(e.target.value); this.refreshControl(); };
            document.getElementById('spec_editor_maxlength').oninput = (e) => { el.props.maxlength = e.target.value; };
            document.getElementById('spec_editor_format').onchange = (e) => { el.props.format = e.target.value; };
            document.getElementById('spec_editor_required').onchange = (e) => { el.props.required = e.target.checked; this.refreshControl(); };
            
            // Toolbar options
            ['bold', 'italic', 'underline', 'list', 'link', 'image', 'table', 'code'].forEach(tool => {
                const checkbox = document.getElementById(`spec_editor_${tool}`);
                if(checkbox) {
                    checkbox.onchange = (e) => {
                        if(!el.props.toolbar) el.props.toolbar = {};
                        el.props.toolbar[tool] = e.target.checked;
                    };
                }
            });
            
        } else if(['Checkbox', 'Switch'].includes(type)) {
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات چک‌باکس</legend>
                    <div class="row g-2">
                        <div class="col-12">
                            <label class="form-label small">متن کنار چک‌باکس</label>
                            <input type="text" class="form-control form-control-sm" id="spec_check_label" value="${el.props.label || ''}">
                        </div>
                        <div class="col-md-6">
                            <div class="form-check form-switch">
                                <input class="form-check-input" type="checkbox" id="spec_checked" ${el.props.checked ? 'checked' : ''}>
                                <label class="form-check-label small">انتخاب شده پیش‌فرض</label>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="form-check form-switch">
                                <input class="form-check-input" type="checkbox" id="spec_required" ${el.props.required ? 'checked' : ''}>
                                <label class="form-check-label small">اجباری</label>
                            </div>
                        </div>
                    </div>
                </fieldset>
            `;
            document.getElementById('spec_check_label').oninput = (e) => { el.props.label = e.target.value; this.refreshControl(); };
            document.getElementById('spec_checked').onchange = (e) => { el.props.checked = e.target.checked; };
            document.getElementById('spec_required').onchange = (e) => { el.props.required = e.target.checked; };
            
        } else if(['Radio', 'Dropdown'].includes(type)) {
            // Use same table UI as Select/RadioButton
            const options = el.props.options || [];
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات گزینه‌ها</legend>
                    <div class="row g-2">
                        <div class="col-12">
                            <div class="d-flex justify-content-between align-items-center mb-2">
                                <label class="form-label small mb-0">گزینه‌ها</label>
                                <div>
                                    <button type="button" class="btn btn-sm btn-success rounded-circle p-0" id="btn_add_option_${el.id}" style="width:28px;height:28px;line-height:1;" title="افزودن گزینه جدید">
                                        <i class="bi bi-plus-lg"></i>
                                    </button>
                                    <button type="button" class="btn btn-sm btn-danger rounded-circle p-0 ms-1" id="btn_remove_option_${el.id}" style="width:28px;height:28px;line-height:1;" title="حذف گزینه انتخاب شده" disabled>
                                        <i class="bi bi-dash-lg"></i>
                                    </button>
                                </div>
                            </div>
                            <div class="table-responsive" style="max-height:300px;overflow-y:auto;">
                                <table class="table table-bordered table-sm table-hover mb-0" id="options_table_${el.id}">
                                    <thead class="table-light sticky-top">
                                        <tr>
                                            <th style="width:50px;" class="text-center">ردیف</th>
                                            <th>مقدار قابل ذخیره</th>
                                            <th>مقدار نمایش داده شده</th>
                                            <th>توضیحات</th>
                                        </tr>
                                    </thead>
                                    <tbody id="options_tbody_${el.id}">
                                        ${options.map((opt, idx) => `
                                            <tr data-option-index="${idx}" style="cursor:pointer;">
                                                <td class="text-center">${idx + 1}</td>
                                                <td><input type="text" class="form-control form-control-sm option-value" value="${opt.value || ''}" placeholder="مقدار"></td>
                                                <td><input type="text" class="form-control form-control-sm option-label" value="${opt.label || ''}" placeholder="عنوان"></td>
                                                <td><input type="text" class="form-control form-control-sm option-desc" value="${opt.description || ''}" placeholder="توضیحات (اختیاری)"></td>
                                            </tr>
                                        `).join('')}
                                        ${options.length === 0 ? '<tr><td colspan="4" class="text-center text-muted">هیچ گزینه‌ای تعریف نشده است</td></tr>' : ''}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="form-check form-switch">
                                <input class="form-check-input" type="checkbox" id="spec_required" ${el.props.required ? 'checked' : ''}>
                                <label class="form-check-label small">اجباری</label>
                            </div>
                        </div>
                    </div>
                </fieldset>
            `;
            
            // Initialize options table
            this.initializeOptionsTable(el);
            
            document.getElementById('spec_required').onchange = (e) => { el.props.required = e.target.checked; };
            
        } else if(['DatePicker', 'PersianDatePicker', 'TimePicker', 'DateTimePicker'].includes(type)) {
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات تاریخ/زمان</legend>
                    <div class="row g-2">
                        <div class="col-md-6">
                            <label class="form-label small">فرمت نمایش</label>
                            <select class="form-select form-select-sm" id="spec_date_format">
                                <option value="YYYY/MM/DD" ${el.props.dateFormat === 'YYYY/MM/DD' ? 'selected' : ''}>1403/01/15</option>
                                <option value="DD/MM/YYYY" ${el.props.dateFormat === 'DD/MM/YYYY' ? 'selected' : ''}>15/01/1403</option>
                                <option value="YYYY-MM-DD" ${el.props.dateFormat === 'YYYY-MM-DD' ? 'selected' : ''}>1403-01-15</option>
                            </select>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">نوع تقویم</label>
                            <select class="form-select form-select-sm" id="spec_calendar_type">
                                <option value="persian" ${el.props.calendarType === 'persian' ? 'selected' : ''}>شمسی</option>
                                <option value="gregorian" ${el.props.calendarType === 'gregorian' ? 'selected' : ''}>میلادی</option>
                            </select>
                        </div>
                        <div class="col-md-6">
                            <div class="form-check form-switch">
                                <input class="form-check-input" type="checkbox" id="spec_required" ${el.props.required ? 'checked' : ''}>
                                <label class="form-check-label small">اجباری</label>
                            </div>
                        </div>
                    </div>
                </fieldset>
            `;
            document.getElementById('spec_date_format').onchange = (e) => { el.props.dateFormat = e.target.value; };
            document.getElementById('spec_calendar_type').onchange = (e) => { el.props.calendarType = e.target.value; };
            document.getElementById('spec_required').onchange = (e) => { el.props.required = e.target.checked; };
            
        } else if(['FileUpload', 'ImageUpload'].includes(type)) {
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات آپلود</legend>
                    <div class="row g-2">
                        <div class="col-md-6">
                            <label class="form-label small">فرمت‌های مجاز</label>
                            <input type="text" class="form-control form-control-sm" id="spec_accept" value="${el.props.accept || (type === 'ImageUpload' ? 'image/*' : '*/*')}" dir="ltr" placeholder=".pdf,.doc,image/*">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">حداکثر حجم (MB)</label>
                            <input type="number" class="form-control form-control-sm" id="spec_max_size" value="${el.props.maxSize || 5}" min="1">
                        </div>
                        <div class="col-md-6">
                            <div class="form-check form-switch">
                                <input class="form-check-input" type="checkbox" id="spec_multiple" ${el.props.multiple ? 'checked' : ''}>
                                <label class="form-check-label small">چند فایل</label>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="form-check form-switch">
                                <input class="form-check-input" type="checkbox" id="spec_required" ${el.props.required ? 'checked' : ''}>
                                <label class="form-check-label small">اجباری</label>
                            </div>
                        </div>
                    </div>
                </fieldset>
            `;
            document.getElementById('spec_accept').oninput = (e) => { el.props.accept = e.target.value; };
            document.getElementById('spec_max_size').oninput = (e) => { el.props.maxSize = parseInt(e.target.value); };
            document.getElementById('spec_multiple').onchange = (e) => { el.props.multiple = e.target.checked; };
            document.getElementById('spec_required').onchange = (e) => { el.props.required = e.target.checked; };
            
        } else if(type === 'Rating') {
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات امتیازدهی</legend>
                    <div class="row g-2">
                        <div class="col-md-6">
                            <label class="form-label small">تعداد ستاره</label>
                            <input type="number" class="form-control form-control-sm" id="spec_star_count" value="${el.props.starCount || 5}" min="3" max="10">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">مقدار پیش‌فرض</label>
                            <input type="number" class="form-control form-control-sm" id="spec_default_rating" value="${el.props.defaultRating || 0}" min="0">
                        </div>
                        <div class="col-md-6">
                            <div class="form-check form-switch">
                                <input class="form-check-input" type="checkbox" id="spec_required" ${el.props.required ? 'checked' : ''}>
                                <label class="form-check-label small">اجباری</label>
                            </div>
                        </div>
                    </div>
                </fieldset>
            `;
            document.getElementById('spec_star_count').oninput = (e) => { el.props.starCount = parseInt(e.target.value); this.refreshControl(); };
            document.getElementById('spec_default_rating').oninput = (e) => { el.props.defaultRating = parseInt(e.target.value); };
            document.getElementById('spec_required').onchange = (e) => { el.props.required = e.target.checked; };
            
        } else if(type === 'Signature') {
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات امضا</legend>
                    <div class="row g-2">
                        <div class="col-md-6">
                            <label class="form-label small">ارتفاع (px)</label>
                            <input type="number" class="form-control form-control-sm" id="spec_sig_height" value="${el.props.height || 150}" min="100" max="400">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">رنگ قلم</label>
                            <input type="color" class="form-control form-control-color form-control-sm" id="spec_sig_color" value="${el.props.penColor || '#000000'}">
                        </div>
                        <div class="col-md-6">
                            <div class="form-check form-switch">
                                <input class="form-check-input" type="checkbox" id="spec_required" ${el.props.required ? 'checked' : ''}>
                                <label class="form-check-label small">اجباری</label>
                            </div>
                        </div>
                    </div>
                </fieldset>
            `;
            document.getElementById('spec_sig_height').oninput = (e) => { el.props.height = e.target.value; this.refreshControl(); };
            document.getElementById('spec_sig_color').oninput = (e) => { el.props.penColor = e.target.value; };
            document.getElementById('spec_required').onchange = (e) => { el.props.required = e.target.checked; };
            
        } else if(['Header', 'Heading', 'Paragraph', 'Text', 'Alert'].includes(type)) {
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات متن</legend>
                    <div class="row g-2">
                        <div class="col-12">
                            <label class="form-label small">متن نمایشی</label>
                            <textarea class="form-control form-control-sm" id="spec_text_content" rows="3">${el.props.label || ''}</textarea>
                        </div>
                        ${type === 'Alert' ? `
                        <div class="col-md-6">
                            <label class="form-label small">نوع پیام</label>
                            <select class="form-select form-select-sm" id="spec_alert_type">
                                <option value="info" ${el.props.alertType === 'info' ? 'selected' : ''}>اطلاعات (آبی)</option>
                                <option value="success" ${el.props.alertType === 'success' ? 'selected' : ''}>موفقیت (سبز)</option>
                                <option value="warning" ${el.props.alertType === 'warning' ? 'selected' : ''}>هشدار (زرد)</option>
                                <option value="danger" ${el.props.alertType === 'danger' ? 'selected' : ''}>خطا (قرمز)</option>
                            </select>
                        </div>` : ''}
                    </div>
                </fieldset>
            `;
            document.getElementById('spec_text_content').oninput = (e) => { el.props.label = e.target.value; this.refreshControl(); };
            if(type === 'Alert' && document.getElementById('spec_alert_type')) {
                document.getElementById('spec_alert_type').onchange = (e) => { el.props.alertType = e.target.value; this.refreshControl(); };
            }
            
        } else if(['ColorPicker', 'RangeSlider'].includes(type)) {
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات ${type === 'ColorPicker' ? 'انتخاب رنگ' : 'اسلایدر'}</legend>
                    <div class="row g-2">
                        ${type === 'RangeSlider' ? `
                        <div class="col-md-4">
                            <label class="form-label small">حداقل</label>
                            <input type="number" class="form-control form-control-sm" id="spec_min" value="${el.props.min || 0}">
                        </div>
                        <div class="col-md-4">
                            <label class="form-label small">حداکثر</label>
                            <input type="number" class="form-control form-control-sm" id="spec_max" value="${el.props.max || 100}">
                        </div>
                        <div class="col-md-4">
                            <label class="form-label small">گام</label>
                            <input type="number" class="form-control form-control-sm" id="spec_step" value="${el.props.step || 1}">
                        </div>` : `
                        <div class="col-md-6">
                            <label class="form-label small">مقدار پیش‌فرض</label>
                            <input type="color" class="form-control form-control-color" id="spec_default_color" value="${el.props.defaultColor || '#4f46e5'}">
                        </div>`}
                        <div class="col-md-6">
                            <div class="form-check form-switch mt-3">
                                <input class="form-check-input" type="checkbox" id="spec_required" ${el.props.required ? 'checked' : ''}>
                                <label class="form-check-label small">اجباری</label>
                            </div>
                        </div>
                    </div>
                </fieldset>
            `;
            if(type === 'RangeSlider') {
                document.getElementById('spec_min').oninput = (e) => { el.props.min = parseInt(e.target.value); };
                document.getElementById('spec_max').oninput = (e) => { el.props.max = parseInt(e.target.value); };
                document.getElementById('spec_step').oninput = (e) => { el.props.step = parseInt(e.target.value); };
            } else {
                document.getElementById('spec_default_color').oninput = (e) => { el.props.defaultColor = e.target.value; };
            }
            document.getElementById('spec_required').onchange = (e) => { el.props.required = e.target.checked; };
            
        } else if (type === 'Spacer') {
            container.innerHTML = `
                <fieldset class="border rounded p-3 mb-3">
                    <legend class="float-none w-auto px-2 fs-6 fw-bold text-primary">تنظیمات فاصله‌گذار</legend>
                    <div class="row g-2">
                        <div class="col-12">
                            <label class="form-label small">ارتفاع (مثال: 50px)</label>
                            <input type="text" class="form-control form-control-sm" id="spec_spacer_height" value="${el.props.height || '20px'}">
                        </div>
                    </div>
                </fieldset>
            `;
            document.getElementById('spec_spacer_height').oninput = (e) => { el.props.height = e.target.value; this.refreshControl(); };
        } else {
            container.innerHTML = `
                <div class="alert alert-light text-center">
                    <i class="bi bi-info-circle me-1"></i>
                    این کنترل (${type}) تنظیمات اختصاصی ندارد
                </div>
            `;
        }
    }

    /**
     * Initialize options table for Select, MultiSelect, RadioButton, CheckboxGroup
     */
    initializeOptionsTable(el) {
        const tableId = `options_table_${el.id}`;
        const tbodyId = `options_tbody_${el.id}`;
        const btnAddId = `btn_add_option_${el.id}`;
        const btnRemoveId = `btn_remove_option_${el.id}`;
        
        const tbody = document.getElementById(tbodyId);
        const btnAdd = document.getElementById(btnAddId);
        const btnRemove = document.getElementById(btnRemoveId);
        const table = document.getElementById(tableId);
        
        if (!tbody || !btnAdd || !btnRemove || !table) return;

        // Check if this is database mode
        const isDatabaseMode = el.data?.sourceType === 'database' && el.data?.selectedColumns;
        
        // If database mode, render advanced table
        if (isDatabaseMode) {
            this.renderAdvancedOptionsTable(el, table, tbody, btnAdd, btnRemove);
            return;
        }

        // Add new option
        btnAdd.addEventListener('click', () => {
            const currentOptions = el.props.options || [];
            const newIndex = currentOptions.length;
            
            const newRow = document.createElement('tr');
            newRow.dataset.optionIndex = newIndex;
            newRow.style.cursor = 'pointer';
            newRow.innerHTML = `
                <td class="text-center">${newIndex + 1}</td>
                <td><input type="text" class="form-control form-control-sm option-value" value="" placeholder="مقدار"></td>
                <td><input type="text" class="form-control form-control-sm option-label" value="" placeholder="عنوان"></td>
                <td><input type="text" class="form-control form-control-sm option-desc" value="" placeholder="توضیحات (اختیاری)"></td>
            `;
            
            // Remove empty row message if exists
            const emptyRow = tbody.querySelector('td[colspan="4"]');
            if (emptyRow) {
                emptyRow.closest('tr').remove();
            }
            
            tbody.appendChild(newRow);
            
            // Update options array
            if (!el.props.options) el.props.options = [];
            el.props.options.push({ value: '', label: '', description: '' });
            
            // Update row numbers
            this.updateOptionRowNumbers(tbody);
            
            // Bind input events
            this.bindOptionRowEvents(newRow, el, newIndex);
            
            // Refresh control
            this.debounce(`options_table_${el.id}`, () => {
                this.refreshControl(true);
            }, 500);
        });

        // Remove selected option
        btnRemove.addEventListener('click', () => {
            const selectedRow = tbody.querySelector('tr.selected');
            if (!selectedRow) return;
            
            const index = parseInt(selectedRow.dataset.optionIndex);
            if (isNaN(index)) return;
            
            // Remove from array
            if (el.props.options && el.props.options.length > index) {
                el.props.options.splice(index, 1);
            }
            
            // Remove from DOM
            selectedRow.remove();
            
            // Update row numbers
            this.updateOptionRowNumbers(tbody);
            
            // Disable remove button if no rows left
            if (tbody.querySelectorAll('tr').length === 0) {
                tbody.innerHTML = '<tr><td colspan="4" class="text-center text-muted">هیچ گزینه‌ای تعریف نشده است</td></tr>';
                btnRemove.disabled = true;
            }
            
            // Refresh control
            this.refreshControl(true);
        });

        // Bind row selection and input events
        tbody.querySelectorAll('tr').forEach((row, idx) => {
            if (row.querySelector('td[colspan]')) return; // Skip empty message row
            
            // Row selection
            row.addEventListener('click', (e) => {
                if (e.target.tagName === 'INPUT') return; // Don't select when clicking input
                
                // Remove previous selection
                tbody.querySelectorAll('tr.selected').forEach(r => r.classList.remove('selected'));
                
                // Add selection
                row.classList.add('selected');
                btnRemove.disabled = false;
            });
            
            // Bind input events
            this.bindOptionRowEvents(row, el, idx);
        });
    }

    /**
     * Bind events for option row inputs
     */
    bindOptionRowEvents(row, el, index) {
        const valueInput = row.querySelector('.option-value');
        const labelInput = row.querySelector('.option-label');
        const descInput = row.querySelector('.option-desc');
        
        if (valueInput) {
            valueInput.addEventListener('input', (e) => {
                if (!el.props.options) el.props.options = [];
                if (!el.props.options[index]) el.props.options[index] = { value: '', label: '', description: '' };
                el.props.options[index].value = e.target.value;
                this.debounce(`options_table_${el.id}`, () => {
                    this.refreshControl(true);
                }, 500);
            });
        }
        
        if (labelInput) {
            labelInput.addEventListener('input', (e) => {
                if (!el.props.options) el.props.options = [];
                if (!el.props.options[index]) el.props.options[index] = { value: '', label: '', description: '' };
                el.props.options[index].label = e.target.value;
                this.debounce(`options_table_${el.id}`, () => {
                    this.refreshControl(true);
                }, 500);
            });
        }
        
        if (descInput) {
            descInput.addEventListener('input', (e) => {
                if (!el.props.options) el.props.options = [];
                if (!el.props.options[index]) el.props.options[index] = { value: '', label: '', description: '' };
                el.props.options[index].description = e.target.value;
            });
        }
    }

    /**
     * Update row numbers in options table
     */
    updateOptionRowNumbers(tbody) {
        tbody.querySelectorAll('tr').forEach((row, idx) => {
            if (row.querySelector('td[colspan]')) return; // Skip empty message row
            const firstCell = row.querySelector('td:first-child');
            if (firstCell) {
                firstCell.textContent = idx + 1;
                row.dataset.optionIndex = idx;
            }
        });
    }

    /**
     * Render advanced options table for database mode
     */
    renderAdvancedOptionsTable(el, table, tbody, btnAdd, btnRemove) {
        const options = el.props.options || [];
        const isRadioOrCheckbox = ['RadioButton', 'CheckboxGroup'].includes(el.type);
        
        // Update table header for database mode
        const thead = table.querySelector('thead tr');
        if (thead) {
            if (isRadioOrCheckbox) {
                thead.innerHTML = `
                    <th style="width:50px;" class="text-center">ردیف</th>
                    <th>مقدار قابل ذخیره</th>
                    <th>مقدار نمایش داده شده</th>
                `;
            } else {
                thead.innerHTML = `
                    <th style="width:50px;" class="text-center">ردیف</th>
                    <th>مقدار قابل ذخیره</th>
                    <th>مقدار نمایش داده شده</th>
                    <th>نوع داده</th>
                    <th style="width:100px;">اولویت نمایش</th>
                    <th style="width:100px;">عدم نمایش</th>
                    <th style="width:120px;">مقدار ذخیره</th>
                `;
            }
        }
        
        // Render rows
        tbody.innerHTML = options.map((opt, idx) => {
            const displayOrder = opt.displayOrder !== undefined ? opt.displayOrder : idx + 1;
            const hide = opt.hide || false;
            const isValue = opt.isValue || false;
            
            if (isRadioOrCheckbox) {
                return `
                    <tr data-option-index="${idx}" style="cursor:move;" draggable="true">
                        <td class="text-center">${idx + 1}</td>
                        <td><input type="text" class="form-control form-control-sm option-value" value="${opt.value || ''}" readonly></td>
                        <td><input type="text" class="form-control form-control-sm option-label" value="${opt.label || ''}" readonly></td>
                    </tr>
                `;
            } else {
                return `
                    <tr data-option-index="${idx}" style="cursor:move;" draggable="true">
                        <td class="text-center">
                            <i class="bi bi-grip-vertical text-muted" style="cursor:grab;"></i>
                        </td>
                        <td><input type="text" class="form-control form-control-sm option-value" value="${opt.value || ''}" readonly></td>
                        <td><input type="text" class="form-control form-control-sm option-label" value="${opt.label || ''}"></td>
                        <td><code class="small">${opt.description || opt.dataType || ''}</code></td>
                        <td>
                            <input type="number" class="form-control form-control-sm option-display-order" value="${displayOrder}" min="1" style="width:80px;">
                        </td>
                        <td class="text-center">
                            <input type="checkbox" class="form-check-input option-hide" ${hide ? 'checked' : ''}>
                        </td>
                        <td class="text-center">
                            <input type="radio" name="option_value_${el.id}" class="form-check-input option-is-value" ${isValue ? 'checked' : ''}>
                        </td>
                    </tr>
                `;
            }
        }).join('');
        
        // Hide add/remove buttons in database mode (or make them work differently)
        if (btnAdd) btnAdd.style.display = 'none';
        if (btnRemove) btnRemove.style.display = 'none';
        
        // Setup drag and drop for reordering
        this.setupOptionsTableDragDrop(tbody, el);
        
        // Bind events
        tbody.querySelectorAll('tr').forEach((row, idx) => {
            if (row.querySelector('td[colspan]')) return;
            
            // Display order input
            const orderInput = row.querySelector('.option-display-order');
            if (orderInput) {
                orderInput.addEventListener('change', (e) => {
                    if (!el.props.options) el.props.options = [];
                    if (el.props.options[idx]) {
                        el.props.options[idx].displayOrder = parseInt(e.target.value) || idx + 1;
                    }
                    this.sortOptionsByDisplayOrder(el);
                    this.refreshControl(true);
                });
            }
            
            // Hide checkbox
            const hideCheckbox = row.querySelector('.option-hide');
            if (hideCheckbox) {
                hideCheckbox.addEventListener('change', (e) => {
                    if (!el.props.options) el.props.options = [];
                    if (el.props.options[idx]) {
                        el.props.options[idx].hide = e.target.checked;
                    }
                    this.debounce(`options_table_${el.id}`, () => {
                        this.refreshControl(true);
                    }, 300);
                });
            }
            
            // Is value radio
            const isValueRadio = row.querySelector('.option-is-value');
            if (isValueRadio) {
                isValueRadio.addEventListener('change', (e) => {
                    if (e.target.checked) {
                        // Uncheck others
                        tbody.querySelectorAll('.option-is-value').forEach(r => {
                            if (r !== e.target) r.checked = false;
                        });
                        
                        // Update state
                        if (!el.props.options) el.props.options = [];
                        el.props.options.forEach((opt, i) => {
                            opt.isValue = (i === idx);
                        });
                        
                        this.debounce(`options_table_${el.id}`, () => {
                            this.refreshControl(true);
                        }, 300);
                    }
                });
            }
            
            // Label input (editable for Select)
            const labelInput = row.querySelector('.option-label');
            if (labelInput && !labelInput.readOnly) {
                labelInput.addEventListener('input', (e) => {
                    if (!el.props.options) el.props.options = [];
                    if (el.props.options[idx]) {
                        el.props.options[idx].label = e.target.value;
                    }
                    this.debounce(`options_table_${el.id}`, () => {
                        this.refreshControl(true);
                    }, 500);
                });
            }
        });
    }

    /**
     * Setup drag and drop for options table
     */
    setupOptionsTableDragDrop(tbody, el) {
        let draggedRow = null;
        
        tbody.querySelectorAll('tr').forEach(row => {
            if (row.querySelector('td[colspan]')) return;
            
            row.addEventListener('dragstart', (e) => {
                draggedRow = row;
                row.style.opacity = '0.5';
                e.dataTransfer.effectAllowed = 'move';
            });
            
            row.addEventListener('dragend', () => {
                if (draggedRow) {
                    draggedRow.style.opacity = '1';
                    draggedRow = null;
                }
            });
            
            row.addEventListener('dragover', (e) => {
                e.preventDefault();
                e.dataTransfer.dropEffect = 'move';
                
                const afterElement = this.getDragAfterElement(tbody, e.clientY);
                if (afterElement == null) {
                    tbody.appendChild(draggedRow);
                } else {
                    tbody.insertBefore(draggedRow, afterElement);
                }
            });
            
            row.addEventListener('drop', (e) => {
                e.preventDefault();
                this.updateOptionsAfterReorder(tbody, el);
            });
        });
    }

    /**
     * Get element after which to insert dragged element
     */
    getDragAfterElement(container, y) {
        const draggableElements = [...container.querySelectorAll('tr:not(.dragging)')];
        
        return draggableElements.reduce((closest, child) => {
            const box = child.getBoundingClientRect();
            const offset = y - box.top - box.height / 2;
            
            if (offset < 0 && offset > closest.offset) {
                return { offset: offset, element: child };
            } else {
                return closest;
            }
        }, { offset: Number.NEGATIVE_INFINITY }).element;
    }

    /**
     * Update options array after reordering
     */
    updateOptionsAfterReorder(tbody, el) {
        const rows = Array.from(tbody.querySelectorAll('tr')).filter(row => !row.querySelector('td[colspan]'));
        const newOptions = [];
        
        rows.forEach((row, idx) => {
            const oldIndex = parseInt(row.dataset.optionIndex);
            if (el.props.options && el.props.options[oldIndex]) {
                const opt = { ...el.props.options[oldIndex] };
                opt.displayOrder = idx + 1;
                newOptions.push(opt);
                row.dataset.optionIndex = idx;
            }
        });
        
        el.props.options = newOptions;
        this.updateOptionRowNumbers(tbody);
        this.debounce(`options_table_${el.id}`, () => {
            this.refreshControl(true);
        }, 300);
    }

    /**
     * Sort options by display order
     */
    sortOptionsByDisplayOrder(el) {
        if (!el.props.options) return;
        
        el.props.options.sort((a, b) => {
            const orderA = a.displayOrder || 999;
            const orderB = b.displayOrder || 999;
            return orderA - orderB;
        });
    }

    // Generate Slider JavaScript
    generateSliderJS(el) {
        const code = `
// Auto-generated Slider JavaScript for ${el.id}
$(document).ready(function() {
    const slider = $('#${el.id}');
    const slides = slider.find('.slide');
    let currentIndex = 0;
    const speed = ${el.props.speed || 3} * 1000;
    const animation = '${el.props.animation || 'fade'}';
    
    function showSlide(index) {
        slides.removeClass('active');
        slides.eq(index).addClass('active');
    }
    
    function nextSlide() {
        currentIndex = (currentIndex + 1) % slides.length;
        showSlide(currentIndex);
    }
    
    ${el.props.autoplay !== false ? 'setInterval(nextSlide, speed);' : ''}
    showSlide(0);
});`;
        
        el.props.customJs = code;
        document.getElementById('prop_custom_js').value = code;
        alert('کد JavaScript اسلایدر تولید و در تب "کد سفارشی" قرار گرفت.');
    }
    
    bindInput(id, callback) {
        const inp = document.getElementById(id);
        if(inp) inp.addEventListener("input", (e) => callback(e.target.value));
    }
    
    bindStyleInput(id, el, prop, suffix = "") {
        const inp = document.getElementById(id);
        if(!inp) return;
        
        // Init value
        if (el.style[prop]) {
             let val = el.style[prop];
             if(suffix && val.endsWith(suffix)) val = val.replace(suffix, "");
             inp.value = val;
        }

        inp.addEventListener("input", (e) => {
            el.style[prop] = e.target.value + suffix;
            this.refreshControl();
        });
    }



    refreshControl(skipModalRefresh = false) {
        if (!this.selectedElement) {
            this.render();
            return;
        }
        
        // Find the wrapper element for the selected control
        const wrapper = document.querySelector(`.fb-control-wrapper[data-id="${this.selectedElement.id}"]`);
        if (!wrapper) {
            this.render();
            return;
        }
        
        // Get the current position in DOM
        const parent = wrapper.parentNode;
        const nextSibling = wrapper.nextSibling;
        
        // Re-render only this control
        const newWrapper = this.renderElement(this.selectedElement);
        
        // Replace the old wrapper with the new one
        parent.replaceChild(newWrapper, wrapper);
        
        // Re-select the element (but don't refresh modal if skipModalRefresh is true)
        if (!skipModalRefresh) {
            this.selectElement(this.selectedElement.id);
        } else {
            // Just update visual selection without opening modal
            this.deselectAll();
            this.selectedElement = this.findElementById(this.selectedElement.id);
            if (newWrapper) {
                newWrapper.classList.add("selected");
            }
        }
        
        // Re-bind events for the new element
        this.bindElementEvents(newWrapper, this.selectedElement);
    }
    
    // Build validation attributes for HTML elements
    buildValidationAttributes(el) {
        if (!el.props) return '';
        
        let attrs = [];
        
        // Required attribute
        if (el.props.required) {
            attrs.push('required');
        }
        
        // Min/Max length
        if (el.props.minLength) {
            attrs.push(`minlength="${el.props.minLength}"`);
        }
        if (el.props.maxLength) {
            attrs.push(`maxlength="${el.props.maxLength}"`);
        }
        
        // Min/Max value for numbers
        if (el.props.min !== undefined && el.props.min !== null) {
            attrs.push(`min="${el.props.min}"`);
        }
        if (el.props.max !== undefined && el.props.max !== null) {
            attrs.push(`max="${el.props.max}"`);
        }
        
        // Pattern for regex validation
        if (el.props.pattern) {
            attrs.push(`pattern="${el.props.pattern}"`);
        }
        
        // Step for number inputs
        if (el.props.step) {
            attrs.push(`step="${el.props.step}"`);
        }
        
        // Custom validation messages
        if (el.props.validationMessage) {
            attrs.push(`title="${el.props.validationMessage}"`);
        }
        
        // Check validation events for additional attributes
        if (el.events && Array.isArray(el.events)) {
            el.events.forEach(ev => {
                if (ev.actionType === 'validate') {
                    // Apply validation rules from events
                    if (ev.trigger === 'onBlur' || ev.trigger === 'onChange') {
                        // These will be handled by JavaScript, but we can add data attributes
                        attrs.push(`data-validation="${ev.trigger}"`);
                    }
                }
            });
        }
        
        return attrs.join(' ');
    }
    
    // Bind events to a rendered element
    bindElementEvents(wrapper, el) {
        // Re-bind delete button
        const deleteBtn = wrapper.querySelector('.btn-delete');
        if (deleteBtn) {
            deleteBtn.onclick = (e) => {
                e.stopPropagation();
                this.deleteElement(el.id);
            };
        }
        
        // Re-bind drag handlers
        wrapper.setAttribute('draggable', 'true');
        wrapper.ondragstart = (e) => {
            this.draggedElement = el;
            e.dataTransfer.effectAllowed = 'move';
        };
        
        // Re-bind click handler
        wrapper.onclick = (e) => {
            if (e.target.closest('.fb-elem-controls')) return;
            this.selectElement(el.id);
        };
    }


    // ==========================================
    //           VALIDATION & EVENTS
    // ==========================================

    renderValidationList(el) {
        const listContainer = document.getElementById("validation-list");
        listContainer.innerHTML = "";

        if (!el.events || el.events.length === 0) {
            listContainer.innerHTML = '<div class="text-muted small text-center">هیچ رویدادی تعریف نشده است</div>';
            return;
        }

        el.events.forEach((ev, index) => {
            const badgeClass = ev.level === "error" ? "bg-danger" : (ev.level === "warning" ? "bg-warning" : "bg-success");
            const item = document.createElement("div");
            item.className = "d-flex align-items-center justify-content-between p-2 border rounded mb-2 bg-white";
            item.innerHTML = `
                <div>
                    <span class="badge bg-secondary me-1">${ev.trigger}</span>
                    <span class="badge ${badgeClass} me-1">${ev.actionType}</span>
                    <small>${ev.msgText || "Script"}</small>
                </div>
                <button class="btn btn-sm btn-link text-danger remove-event" data-index="${index}"><i class="bi bi-trash"></i></button>
            `;
            listContainer.appendChild(item);
        });

        // Remove handlers
        listContainer.querySelectorAll(".remove-event").forEach((btn) => {
            btn.addEventListener("click", (e) => {
                const idx = parseInt(e.currentTarget.dataset.index);
                el.events.splice(idx, 1);
                this.renderValidationList(el);
            });
        });
    }

    addNewEvent() {
        if (!this.selectedElement) return;

        const trigger = document.getElementById("new-event-trigger").value;
        const actionType = document.getElementById("new-event-action-type").value;
        const msgText = document.getElementById("new-event-msg-text").value;
        const msgLevel = document.getElementById("new-event-msg-level").value;
        const component = document.getElementById("new-event-msg-component").value;

        const newEvent = {
            trigger,
            actionType,
            msgText,
            level: msgLevel,
            component
        };

        if (!this.selectedElement.events) this.selectedElement.events = [];
        this.selectedElement.events.push(newEvent);

        this.renderValidationList(this.selectedElement);
        document.getElementById("new-event-msg-text").value = "";
    }
    
    // ==========================================
    //           HELPERS (Tree Utils)
    // ==========================================
    
    findElementById(id, list = null) {
        if(!list) list = this.getCurrentList();
        
        for (let el of list) {
            if (el.id === id) return el;
            if (el.children) {
                const found = this.findElementById(id, el.children);
                if (found) return found;
            }
        }
        return null;
    }

    replaceElementInTree(targetId, newElement, list = this.elements) {
        for (let i = 0; i < list.length; i++) {
            if (list[i].id === targetId) {
                list[i] = newElement;
                return true;
            }
            if (list[i].children) {
                if (this.replaceElementInTree(targetId, newElement, list[i].children)) return true;
            }
        }
        return false;
    }

    insertElementRelative(newEl, targetId, position, list = this.elements) {
        for (let i = 0; i < list.length; i++) {
            if (list[i].id === targetId) {
                if (position === "top") list.splice(i, 0, newEl);
                else list.splice(i + 1, 0, newEl);
                return true;
            }
            if (list[i].children) {
                if (this.insertElementRelative(newEl, targetId, position, list[i].children)) return true;
            }
        }
        return false;
    }


    // ==========================================
    //           SETUP MODAL & TOOLBAR
    // ==========================================
    setupPropertyModalListeners() {
        const btnAddEvent = document.getElementById("btn-add-event");
        if (btnAddEvent) {
            btnAddEvent.addEventListener("click", () => this.addNewEvent());
        }
        
        // Preview Message Listener - Use event delegation to handle dynamically shown buttons
        const validationTab = document.getElementById("prop-tab-validation");
        if (validationTab) {
            validationTab.addEventListener("click", (e) => {
                if (e.target && e.target.id === "btn-preview-message") {
                    e.preventDefault();
                    const text = document.getElementById("new-event-msg-text")?.value || "نمونه پیام تست";
                    const level = document.getElementById("new-event-msg-level")?.value || "info";
                    const comp = document.getElementById("new-event-msg-component")?.value || "toast";
                    
                    this.showPreviewMessage(text, level, comp);
                }
            });
        }
        
        // Also add direct listener for when button is available
        const btnPreviewMessage = document.getElementById("btn-preview-message");
        if (btnPreviewMessage) {
            btnPreviewMessage.addEventListener("click", (e) => {
                e.preventDefault();
                const text = document.getElementById("new-event-msg-text")?.value || "نمونه پیام تست";
                const level = document.getElementById("new-event-msg-level")?.value || "info";
                const comp = document.getElementById("new-event-msg-component")?.value || "toast";
                
                this.showPreviewMessage(text, level, comp);
            });
        }

        // Toggle sections based on action type
        document.getElementById("new-event-action-type").addEventListener("change", (e) => {
            const val = e.target.value;
            const msgSection = document.querySelector(".event-config-message");
            const scriptSection = document.querySelector(".event-config-script");
            
            if(val === 'message') {
                msgSection.classList.remove("d-none");
                scriptSection.classList.add("d-none");
            } else if (val === 'script') {
                 msgSection.classList.add("d-none");
                scriptSection.classList.remove("d-none");
            } else {
                 msgSection.classList.add("d-none");
                scriptSection.classList.add("d-none");
            }
        });
    }

    showPreviewMessage(text, level, component) {
        if (component === 'swal') {
            swal({
                title: level.toUpperCase(),
                text: text,
                icon: level, // success, warning, error, info
                button: "باشه",
            });
        } else if (component === 'toast') {
             // Simple toast simulation
             const toast = document.createElement("div");
             toast.className = `position-fixed bottom-0 end-0 m-3 p-3 text-white bg-${level === 'error' ? 'danger' : level} rounded shadow`;
             toast.style.zIndex = 9999;
             toast.innerText = text;
             document.body.appendChild(toast);
             setTimeout(() => toast.remove(), 3000);
        } else {
            // Banner
            alert(`[Banner] ${level}: ${text}`);
        }
    }


    setupSettingsModalListeners() {
        // Show/hide custom size inputs based on selection
        const canvasSizeSelect = document.getElementById("setting-canvas-size");
        const customSizeInputs = document.getElementById("custom-size-inputs");
        const customSizeInputsHeight = document.getElementById("custom-size-inputs-height");
        
        if (canvasSizeSelect && customSizeInputs && customSizeInputsHeight) {
            const toggleCustomInputs = () => {
                const isCustom = canvasSizeSelect.value === "Custom";
                customSizeInputs.style.display = isCustom ? "block" : "none";
                customSizeInputsHeight.style.display = isCustom ? "block" : "none";
            };
            
            canvasSizeSelect.addEventListener("change", toggleCustomInputs);
            toggleCustomInputs(); // Initial state
        }
        
        document.getElementById("btn-save-settings").addEventListener("click", () => {
            const nameFa = document.getElementById("setting-name-fa").value;
            if (!nameFa) {
                alert("نام فرم الزامی است");
                return;
            }
            
            // Save all settings
            this.config.formNameFa = nameFa;
            this.config.formNameEn = document.getElementById("setting-name-en")?.value || nameFa;
            this.config.formCode = document.getElementById("setting-code")?.value || "";
            this.config.formType = document.getElementById("setting-form-type")?.value || "1";
            this.config.categoryId = document.getElementById("setting-category")?.value || null;
            this.config.numberingRule = document.getElementById("setting-numbering-rule")?.value || "";
            this.config.numberingPrefix = document.getElementById("setting-numbering-prefix")?.value || "";
            
            // Canvas Size
            const canvasSizeSelect = document.getElementById("setting-canvas-size");
            const canvasSizeValue = canvasSizeSelect?.value || "auto";
            let canvasSize = { width: "100%", height: "auto" };
            
            if (canvasSizeValue === "A4") {
                canvasSize = { width: "210mm", height: "297mm" };
            } else if (canvasSizeValue === "A5") {
                canvasSize = { width: "148mm", height: "210mm" };
            } else if (canvasSizeValue === "Letter") {
                canvasSize = { width: "8.5in", height: "11in" };
            } else if (canvasSizeValue === "Custom") {
                // Read custom size values
                const customWidth = document.getElementById("setting-custom-width")?.value;
                const customWidthUnit = document.getElementById("setting-custom-width-unit")?.value || "px";
                const customHeight = document.getElementById("setting-custom-height")?.value;
                const customHeightUnit = document.getElementById("setting-custom-height-unit")?.value || "px";
                
                if (customWidth) {
                    canvasSize.width = `${customWidth}${customWidthUnit}`;
                } else {
                    canvasSize.width = "100%";
                }
                
                if (customHeight) {
                    canvasSize.height = `${customHeight}${customHeightUnit}`;
                } else {
                    canvasSize.height = "auto";
                }
            }
            
            this.config.canvasSize = JSON.stringify(canvasSize);
            
            // Background Settings
            const bgColor = document.getElementById("setting-bg-color")?.value || "#ffffff";
            const bgImage = document.getElementById("setting-bg-image")?.value || "";
            const backgroundSettings = {
                color: bgColor,
                image: bgImage
            };
            this.config.backgroundSettings = JSON.stringify(backgroundSettings);
            
            // Apply settings to canvas immediately
            this.applyCanvasSettings();
            
            // Save settings to form if form exists
            if (this.config.formId) {
                this.saveFormSettings();
            }
            
            this.modalSettings.hide();
        });
    }
    
    applyCanvasSettings() {
        if (!this.canvas) return;
        
        // Get canvas wrapper (the parent element that should be resized)
        const canvasWrapper = this.canvas.closest(".fb-canvas-wrapper");
        
        // Apply canvas size to wrapper (not the canvas content itself)
        if (this.config.canvasSize) {
            try {
                const size = JSON.parse(this.config.canvasSize);
                if (canvasWrapper) {
                    if (size.width) {
                        canvasWrapper.style.width = size.width;
                    }
                    if (size.height && size.height !== "auto") {
                        canvasWrapper.style.minHeight = size.height;
                        canvasWrapper.style.height = size.height;
                    } else {
                        canvasWrapper.style.height = "auto";
                        canvasWrapper.style.minHeight = "297mm"; // Default min height
                    }
                }
            } catch (e) {
                console.error("Error parsing canvas size:", e);
            }
        }
        
        // Apply background to canvas content
        if (this.config.backgroundSettings) {
            try {
                const bg = JSON.parse(this.config.backgroundSettings);
                if (bg.image) {
                    this.canvas.style.backgroundImage = `url(${bg.image})`;
                    this.canvas.style.backgroundSize = "cover";
                    this.canvas.style.backgroundPosition = "center";
                } else if (bg.color) {
                    this.canvas.style.backgroundColor = bg.color;
                    this.canvas.style.backgroundImage = "none";
                }
            } catch (e) {
                console.error("Error parsing background settings:", e);
            }
        }
    }
    
    async saveFormSettings() {
        if (!this.config.formId) return;
        
        try {
            const settingsData = {
                NameFa: this.config.formNameFa,
                NameEn: this.config.formNameEn,
                Code: parseInt(this.config.formCode) || 100,
                FormType: parseInt(this.config.formType) || 1,
                CategoryId: this.config.categoryId || null,
                NumberingRule: this.config.numberingRule || "",
                NumberingPrefix: this.config.numberingPrefix || "",
                CanvasSize: this.config.canvasSize || JSON.stringify({ width: "100%", height: "auto" }),
                BackgroundSettings: this.config.backgroundSettings || JSON.stringify({ color: "#ffffff", image: "" })
            };
            
            const response = await fetch(`/api/formbuilder/forms/${this.config.formId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(settingsData)
            });
            
            if (!response.ok) {
                console.error("Error saving form settings");
            }
        } catch (error) {
            console.error("Error saving form settings:", error);
        }
    }

    setupToolbar() {
        document.getElementById("btn-form-settings").addEventListener("click", () => this.modalSettings.show());
        document.getElementById("btn-global-script").addEventListener("click", () => this.modalScript.show());
        document.getElementById("btn-print-template").addEventListener("click", () => {
            this.modalPrint.show();
            this.loadPrintTemplates();
        });
        
        // Action Buttons
        document.getElementById("btn-action-buttons").addEventListener("click", () => {
            this.openActionButtonsModal();
        });
        this.setupActionButtonsManagement();

        // Print Template Management
        this.setupPrintTemplateManagement();
        
        // Preview
        // Preview
        document.getElementById("btn-preview").addEventListener("click", async () => {
             // 1. Prepare Events Script
             const flatFields = [];
             this.flattenFields(this.elements, null, flatFields);
             
             let eventScript = `
                 // Auto-generated Event Handlers
                 document.addEventListener('DOMContentLoaded', () => {
             `;
             
             flatFields.forEach(f => {
                 if(f.Events) {
                     let events = [];
                     try { events = JSON.parse(f.Events); } catch(e) {}
                     
                     events.forEach(ev => {
                         // Build Handler
                         let handler = "";
                         if(ev.actionType === 'message') {
                             const level = ev.level || 'info';
                             handler = `alert('${ev.msgText}');`;
                             // Using SweetAlert if available
                             // handler = `swal('پیام', '${ev.msgText}', '${level}');`; 
                         } else if (ev.actionType === 'script') {
                             handler = ev.msgText; // In script mode, msgText holds the code
                         }
                         
                         // Attach
                         if(handler) {
                             eventScript += `
                                 const el_${f.FieldKey} = document.getElementById('${f.FieldKey}');
                                 if(el_${f.FieldKey}) {
                                     el_${f.FieldKey}.addEventListener('${ev.trigger}', function(e) {
                                         ${handler}
                                     });
                                 }
                             `;
                         }
                     });
                 }
             });
             
             eventScript += `
                 });
             `;

             // Get canvas size for preview
             let previewWidth = "1000px";
             let previewHeight = "auto";
             if (this.config.canvasSize) {
                 try {
                     const size = JSON.parse(this.config.canvasSize);
                     if (size.width) {
                         previewWidth = size.width;
                     }
                     if (size.height) {
                         previewHeight = size.height;
                     }
                 } catch (e) {
                     console.error("Error parsing canvas size for preview:", e);
                 }
             }
             
             const w = window.open("", "_blank");
             w.document.write(`
                <html>
                <head>
                    <title>پیش‌نمایش فرم</title>
                    <link href="/css/site.css" rel="stylesheet">
                    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
                    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet">
                    <style>
                        body { padding: 2rem; direction: rtl; background-color: #f8f9fa; }
                        .preview-container { 
                            background: white; 
                            padding: 2rem; 
                            border-radius: 8px; 
                            box-shadow: 0 4px 12px rgba(0,0,0,0.1); 
                            width: ${previewWidth};
                            ${previewHeight !== "auto" ? `height: ${previewHeight};` : ""}
                            ${previewHeight !== "auto" ? `min-height: ${previewHeight};` : ""}
                            margin: auto; 
                            box-sizing: border-box;
                        }
                        /* Enable inputs in preview */
                        input, select, textarea { pointer-events: auto !important; cursor: text !important; }
                        /* Hide builder controls */
                        .fb-elem-controls, .fb-resizer { display: none !important; }
                        /* Interactive states */
                        .btn:not(:disabled) { cursor: pointer; }
                        /* Reduce control sizes in preview */
                        .preview-container .form-control,
                        .preview-container .form-select {
                            font-size: 0.75rem !important;
                            padding: 0.375rem 0.5rem !important;
                            line-height: 1.4 !important;
                        }
                        .preview-container .form-label {
                            font-size: 0.75rem !important;
                            margin-bottom: 0.375rem !important;
                        }
                        .preview-container .btn {
                            font-size: 0.75rem !important;
                            padding: 0.375rem 0.75rem !important;
                        }
                        .preview-container .input-group-text {
                            font-size: 0.75rem !important;
                            padding: 0.375rem 0.5rem !important;
                        }
                        /* Ensure icons are visible */
                        .bi {
                            display: inline-block !important;
                            font-family: "bootstrap-icons" !important;
                            font-style: normal;
                            font-weight: normal;
                            font-variant: normal;
                            text-transform: none;
                            line-height: 1;
                            vertical-align: -.125em;
                            -webkit-font-smoothing: antialiased;
                            -moz-osx-font-smoothing: grayscale;
                        }
                    </style>
                </head>
                <body>
                    <div class="preview-container" style="width: ${previewWidth}; ${previewHeight !== "auto" ? `height: ${previewHeight}; min-height: ${previewHeight};` : ""}">
                        <div class="mb-4 text-center">
                            <h2>${this.config.formNameFa || "پیش‌نمایش فرم"}</h2>
                        </div>
                        <form id="preview-form" onsubmit="event.preventDefault(); alert('فرم معتبر است و ارسال شد!');">
                            ${this.canvas.innerHTML}
                        </form>
                        <div id="button-placeholder"></div>
                    </div>
                    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"><\/script>
                    <script>
                        // Enable all inputs
                        document.querySelectorAll("input, select, textarea, button").forEach(el => {
                            el.removeAttribute("disabled");
                            el.removeAttribute("readonly");
                        });

                        ${eventScript}
                    <\/script>
                </body>
                </html>
             `);

             // Inject buttons after page loads
             const buttonsHtml = await this.generatePreviewButtons();
             setTimeout(() => {
                 const placeholder = w.document.getElementById('button-placeholder');
                 if (placeholder) {
                     placeholder.outerHTML = buttonsHtml;
                 }
             }, 100);
        });

        // Save
        // Save
        document.getElementById('btn-save').addEventListener('click', () => {
             this.saveForm();
        });

        // Publish
        document.getElementById('btn-publish').addEventListener('click', async () => {
            await this.publishForm();
        });

         // Reset
        document.getElementById('btn-clear').addEventListener('click', () => {
             if(confirm("آیا مطمئن هستید؟ همه تغییرات از دست می‌رود.")) {
                 this.elements = [];
                 this.modals = [];
                 this.currentView = 'main';
                 this.render();
                 this.renderTray();
             }
        });

    }

    // ==========================================
    //           PERSISTENCE
    // ==========================================
    
    async saveForm() {
        const btn = document.getElementById('btn-save');
        const originalText = btn.innerHTML;
        btn.innerHTML = '<i class="bi bi-hourglass-split me-1"></i> در حال ذخیره...';
        btn.disabled = true;

        try {
            // 1. Prepare Fields
            const flatFields = [];
            this.flattenFields(this.elements, null, flatFields);

            // 2. Prepare Form Data (Properties, Scripts)
            // Save global script from editor if modal exists
            // Get script from CodeMirror instance if available, else from textarea
            if (this.codeMirrorInstance) {
                this.config.customScripts = this.codeMirrorInstance.getValue();
            } else {
                const scriptEditor = document.getElementById("global-script-editor");
                if(scriptEditor) {
                    this.config.customScripts = scriptEditor.value;
                }
            }

            // Check if form name is set
            if (!this.config.formNameFa) {
                const nameFa = document.getElementById("setting-name-fa")?.value;
                if (!nameFa) {
                    alert("لطفا ابتدا نام فرم را در تنظیمات فرم وارد کنید.");
                    this.modalSettings.show();
                    return;
                }
                this.config.formNameFa = nameFa;
            }

            // Get form code from settings - use the exact value user entered
            const formCodeInput = document.getElementById("setting-code")?.value;
            const formCode = formCodeInput ? `FRM-${formCodeInput.padStart(4, '0')}` : null;
            
            const formData = {
                FormCode: formCode, // Use FormCode (string) instead of Code (number)
                NameFa: this.config.formNameFa,
                NameEn: this.config.formNameEn || this.config.formNameFa,
                FormType: parseInt(this.config.formType) || 1,
                CategoryId: this.config.categoryId || null,
                NumberingRule: this.config.numberingRule || "",
                NumberingPrefix: this.config.numberingPrefix || "",
                CanvasSize: this.config.canvasSize || JSON.stringify({ width: "100%", height: "auto" }),
                BackgroundSettings: this.config.backgroundSettings || JSON.stringify({ color: "#ffffff", image: "" }),
                CustomScripts: this.config.customScripts || "",
                CustomStyles: this.config.customStyles || "",
                DesignData: this.elements, // Don't stringify here, let the controller handle it
                CreateTable: false
            };

            let formId = this.config.formId;
            let resForm;

            // 3. Create form if it doesn't exist, otherwise update
            if (!formId) {
                // Create new form
                resForm = await fetch('/api/formbuilder/forms', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(formData)
                });

                if (!resForm.ok) {
                    const errorText = await resForm.text();
                    throw new Error("خطا در ایجاد فرم: " + errorText);
                }

                const createResult = await resForm.json();
                formId = createResult.id;
                this.config.formId = formId;
                
                // Update URL if possible
                if (window.history && window.history.replaceState) {
                    const newUrl = window.location.pathname + '?id=' + formId;
                    window.history.replaceState({}, '', newUrl);
                }
            } else {
                // Update existing form
                const updateData = {
                    NameFa: this.config.formNameFa,
                    NameEn: this.config.formNameEn || this.config.formNameFa,
                    FormType: parseInt(this.config.formType) || 1,
                    CategoryId: this.config.categoryId || null,
                    NumberingRule: this.config.numberingRule || "",
                    NumberingPrefix: this.config.numberingPrefix || "",
                    CanvasSize: this.config.canvasSize || JSON.stringify({ width: "100%", height: "auto" }),
                    BackgroundSettings: this.config.backgroundSettings || JSON.stringify({ color: "#ffffff", image: "" }),
                    CustomScripts: this.config.customScripts || "",
                    CustomStyles: this.config.customStyles || "",
                    DesignData: this.elements,
                    UpdateTable: false
                };
                
                resForm = await fetch(`/api/formbuilder/forms/${formId}`, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(updateData)
                });

                if (!resForm.ok) {
                    const errorText = await resForm.text();
                    throw new Error("خطا در ذخیره تنظیمات فرم: " + errorText);
                }
            }

            // 4. Save fields
            const resFields = await fetch(`/api/formbuilder/forms/${formId}/fields`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(flatFields)
            });

            if (!resFields.ok) {
                const errorText = await resFields.text();
                throw new Error("خطا در ذخیره فیلدها: " + errorText);
            }

            const result = await resFields.json();
            
            if (result.success) {
                if (result.warning) alert(`ذخیره شد ولی هشدار دیتابیس دریافت شد: ${result.warning}`);
                else alert("فرم با موفقیت ذخیره شد.");
            } else {
                throw new Error("خطا در پاسخ سرور");
            }

        } catch (e) {
            console.error("Save Error:", e);
            alert("خطا در ذخیره‌سازی: " + e.message);
        } finally {
            btn.innerHTML = originalText;
            btn.disabled = false;
        }
    }

    async publishForm() {
        if (!this.config.formId) {
            alert("ابتدا فرم را ذخیره کنید");
            return;
        }

        if (!confirm("آیا از انتشار این فرم اطمینان دارید؟ پس از انتشار، فرم برای ایجاد اسناد قابل استفاده خواهد بود.")) {
            return;
        }

        const btn = document.getElementById('btn-publish');
        const originalText = btn.innerHTML;
        btn.innerHTML = '<i class="bi bi-hourglass-split me-1"></i> در حال انتشار...';
        btn.disabled = true;

        try {
            const response = await fetch(`/api/formbuilder/forms/${this.config.formId}/publish`, {
                method: 'POST'
            });

            const data = await response.json();
            if (data.success) {
                alert("فرم با موفقیت منتشر شد و اکنون برای ایجاد اسناد قابل استفاده است!");
                btn.classList.remove('btn-glass-warning');
                btn.classList.add('btn-glass-success');
                btn.innerHTML = '<i class="bi bi-check-circle me-1"></i>منتشر شده';
            } else {
                alert("خطا در انتشار فرم: " + (data.error || "خطای نامشخص"));
            }
        } catch (error) {
            console.error("Publish Error:", error);
            alert("خطا در انتشار فرم: " + error.message);
        } finally {
            if (!btn.classList.contains('btn-glass-success')) {
                btn.innerHTML = originalText;
                btn.disabled = false;
            }
        }
    }

    async generatePreviewButtons() {
        if (!this.config.formId) {
            // Default buttons if form not saved yet
            return `
                <div class="form-action-buttons d-flex gap-2 flex-wrap justify-content-end p-3 border-top bg-light mt-3">
                    <button type="submit" class="btn btn-primary btn-action-button">
                        <i class="bi bi-save me-1"></i>ذخیره
                    </button>
                    <button type="button" class="btn btn-outline-danger btn-action-button" onclick="window.close()">
                        <i class="bi bi-x-lg me-1"></i>بستن
                    </button>
                </div>
            `;
        }

        try {
            const response = await fetch(`/api/formbuilder/forms/${this.config.formId}/buttons`);
            const data = await response.json();

            if (!data.success || !data.data.buttons || data.data.buttons.length === 0) {
                // Default buttons if no config
                return `
                    <div class="form-action-buttons d-flex gap-2 flex-wrap justify-content-end p-3 border-top bg-light mt-3">
                        <button type="submit" class="btn btn-primary btn-action-button">
                            <i class="bi bi-save me-1"></i>ذخیره
                        </button>
                        <button type="button" class="btn btn-outline-danger btn-action-button" onclick="window.close()">
                            <i class="bi bi-x-lg me-1"></i>بستن
                        </button>
                    </div>
                `;
            }

            const buttons = data.data.buttons.filter(b => b.isEnabled).sort((a, b) => a.displayOrder - b.displayOrder);
            const buttonsHtml = buttons.map(btn => {
                const title = btn.customTitleFa || btn.buttonTypeName || 'دکمه';
                const icon = btn.customIcon || btn.buttonTypeIcon || '';
                const displayMode = btn.displayMode || 'icon_and_title';
                const showIcon = displayMode === 'icon_and_title' || displayMode === 'icon_only';
                const showTitle = displayMode === 'icon_and_title' || displayMode === 'title_only';

                let styles = {};
                try {
                    styles = JSON.parse(btn.styleSettings || '{}');
                } catch (e) {}

                const styleAttr = Object.entries(styles).map(([key, value]) => {
                    const kebabKey = key.replace(/([a-z])([A-Z])/g, '$1-$2').toLowerCase();
                    return `${kebabKey}: ${value}`;
                }).join('; ');

                return `
                    <button type="button" class="btn btn-action-button" style="${styleAttr}" disabled>
                        ${showIcon && icon ? `<i class="${icon}${showTitle ? ' me-1' : ''}"></i>` : ''}
                        ${showTitle ? title : ''}
                    </button>
                `;
            }).join('');

            return `
                <div class="form-action-buttons d-flex gap-2 flex-wrap justify-content-end p-3 border-top bg-light mt-3">
                    ${buttonsHtml}
                </div>
            `;
        } catch (error) {
            console.error('Error loading buttons for preview:', error);
            return `
                <div class="form-action-buttons d-flex gap-2 flex-wrap justify-content-end p-3 border-top bg-light mt-3">
                    <button type="submit" class="btn btn-primary btn-action-button">
                        <i class="bi bi-save me-1"></i>ذخیره
                    </button>
                    <button type="button" class="btn btn-outline-danger btn-action-button" onclick="window.close()">
                        <i class="bi bi-x-lg me-1"></i>بستن
                    </button>
                </div>
            `;
        }
    }

    flattenFields(list, parentId, result) {
        let order = 0;
        list.forEach(el => {
            if (!el.guid) el.guid = this.generateGuid(); // Ensure temporary ID

            // Get database information from el.db or el.data
            const dbInfo = el.db || {};
            const dataInfo = el.data || {};
            
            // Get database column name - priority: el.db.columnName > el.data.databaseColumnName > el.data.dbColumn > el.props.nameEn
            const dbColumnName = dbInfo.columnName || dataInfo.databaseColumnName || dataInfo.dbColumn || (el.props.nameEn ? el.props.nameEn : null);
            
            // Get database column type - detect if not set
            let dbColumnType = dbInfo.columnType || dataInfo.databaseColumnType || dataInfo.sqlType;
            if (!dbColumnType) {
                const detectedType = this.detectDataType(el.type);
                dbColumnType = detectedType.baseType;
            }
            
            // Format column type with length if needed
            let formattedType = dbColumnType;
            if (dbInfo.maxLength !== null && dbInfo.maxLength !== undefined) {
                if (dbInfo.maxLength === 'MAX' || dbInfo.maxLength === null) {
                    formattedType = `${dbColumnType}(MAX)`;
                } else {
                    formattedType = `${dbColumnType}(${dbInfo.maxLength})`;
                }
            } else if (dataInfo.maxLength !== null && dataInfo.maxLength !== undefined) {
                if (dataInfo.maxLength === 'MAX' || dataInfo.maxLength === null) {
                    formattedType = `${dbColumnType}(MAX)`;
                } else {
                    formattedType = `${dbColumnType}(${dataInfo.maxLength})`;
                }
            }

            const dto = {
                Id: el.guid,
                FieldTypeId: this.getFieldTypeId(el.type),
                ParentFieldId: parentId,
                FieldKey: el.id,
                Name: el.props.nameEn || el.id, // Use exact English name from user input
                LabelFa: el.props.label || "بدون عنوان",
                LabelEn: el.props.nameEn,
                Placeholder: el.props.placeholder,
                DisplayOrder: order++,
                DatabaseColumnName: dbColumnName,
                DatabaseColumnType: formattedType,
                IsRequired: el.props.required || false,
                Properties: JSON.stringify({
                    ...(el.props || {}),
                    // Include database-specific properties in Properties JSON
                    isUnique: dbInfo.isUnique || dataInfo.isUnique || false,
                    maxLength: dbInfo.maxLength || dataInfo.maxLength || null,
                    precision: dbInfo.precision || dataInfo.precision || null,
                    scale: dbInfo.scale || dataInfo.scale || null,
                    // Store data object for reference
                    data: {
                        ...dataInfo,
                        databaseColumnName: dbColumnName,
                        databaseColumnType: formattedType
                    }
                }),
                Styles: JSON.stringify(el.style || {}),
                Events: JSON.stringify(el.events || []),
                
                // Database mapping properties
                MaxLength: (dbInfo.maxLength !== null && dbInfo.maxLength !== undefined) ? 
                    (dbInfo.maxLength === 'MAX' || dbInfo.maxLength === null ? null : parseInt(dbInfo.maxLength)) :
                    ((dataInfo.maxLength !== null && dataInfo.maxLength !== undefined) ? 
                        (dataInfo.maxLength === 'MAX' || dataInfo.maxLength === null ? null : parseInt(dataInfo.maxLength)) : null),
                IsNullable: (dbInfo.nullable !== undefined) ? dbInfo.nullable : 
                    ((dataInfo.isNullable !== undefined) ? dataInfo.isNullable : true),
                HasIndex: (dbInfo.hasIndex !== undefined) ? dbInfo.hasIndex : 
                    ((dataInfo.hasIndex !== undefined) ? dataInfo.hasIndex : false),
                DefaultValue: dbInfo.defaultValue || dataInfo.defaultValue || el.props.defaultValue || null,
                DefaultFunctionId: dbInfo.defaultFunctionId || null,
                CustomCss: el.style?.customCss || el.props?.customCss || "",
                CustomJs: el.props?.customJs || ""
            };
            
            // Only include fields that should be stored in database
            // Skip layout containers and non-input controls unless they have explicit database mapping
            const isInputControl = !['Row', 'Container', 'FlexContainer', 'Card', 'Panel', 'Fieldset', 
                                     'Header', 'Heading', 'Paragraph', 'Text', 'Alert', 'Spacer'].includes(el.type);
            
            // Include if it's an input control OR has explicit database column name
            if (isInputControl || dbColumnName) {
                result.push(dto);
            }

            if (el.children && el.children.length > 0) {
                this.flattenFields(el.children, el.guid, result);
            }
        });
    }

    getFieldTypeId(typeName) {
        if (!this.config.fieldTypes || !typeName) return 0;
        // Normalize names (e.g. 'Input' vs 'TextInput') if needed. 
        // Assuming data-type matches server Type Name.
        const ft = this.config.fieldTypes.find(t => t.Name.toLowerCase() === typeName.toLowerCase());
        return ft ? ft.Id : 0;
    }

    generateGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
    // ==========================================
    //           PRINT TEMPLATE MANAGEMENT
    // ==========================================
    
    setupPrintTemplateManagement() {
        // Add new template button
        const btnAddNew = document.getElementById("btn-add-new-template");
        if (btnAddNew) {
            btnAddNew.addEventListener("click", () => {
                this.showPrintTemplateForm();
            });
        }

        // Save template button
        const btnSaveTemplate = document.getElementById("btn-save-template");
        if (btnSaveTemplate) {
            btnSaveTemplate.addEventListener("click", () => {
                this.savePrintTemplate();
            });
        }

        // Cancel template form
        const btnCancelTemplate = document.getElementById("btn-cancel-template");
        if (btnCancelTemplate) {
            btnCancelTemplate.addEventListener("click", () => {
                this.hidePrintTemplateForm();
            });
        }

        // Load templates when modal is shown
        if (this.modalPrint && this.modalPrint._element) {
            this.modalPrint._element.addEventListener('shown.bs.modal', () => {
                this.loadPrintTemplates();
            });
        }
    }

    showPrintTemplateForm(templateId = null) {
        const form = document.getElementById("print-template-form");
        const title = document.getElementById("template-form-title");
        
        if (!form || !title) return;
        
        if (templateId) {
            title.textContent = "ویرایش قالب";
            this.currentEditingTemplateId = templateId;
        } else {
            title.textContent = "افزودن قالب جدید";
            this.currentEditingTemplateId = null;
            // Reset form
            document.getElementById("template-name").value = "";
            document.getElementById("template-description").value = "";
            document.getElementById("template-display-order").value = "0";
            document.getElementById("template-file").value = "";
            document.getElementById("template-is-default").checked = false;
        }
        
        form.classList.remove("d-none");
        form.scrollIntoView({ behavior: 'smooth' });
    }

    hidePrintTemplateForm() {
        const form = document.getElementById("print-template-form");
        if (form) {
            form.classList.add("d-none");
        }
        this.currentEditingTemplateId = null;
    }

    async loadPrintTemplates() {
        if (!this.config.formId) return;

        const listContainer = document.getElementById("print-templates-list-manager");
        if (!listContainer) return;

        listContainer.innerHTML = '<div class="text-center text-muted py-4"><i class="bi bi-hourglass-split"></i> در حال بارگذاری...</div>';

        try {
            const response = await fetch(`/api/formbuilder/forms/${this.config.formId}/printtemplates`);
            const templates = await response.json();

            if (!templates || templates.length === 0) {
                listContainer.innerHTML = `
                    <div class="text-center text-muted py-4">
                        <i class="bi bi-inbox"></i>
                        <p class="mt-2">هیچ قالب چاپی تعریف نشده است</p>
                    </div>
                `;
                return;
            }

            listContainer.innerHTML = templates.map(template => `
                <div class="list-group-item d-flex justify-content-between align-items-center">
                    <div class="flex-grow-1">
                        <h6 class="mb-1">
                            ${template.name}
                            ${template.isDefault ? '<span class="badge bg-primary ms-2">پیش‌فرض</span>' : ''}
                        </h6>
                        ${template.description ? `<small class="text-muted">${template.description}</small>` : ''}
                        ${template.filePath ? `<small class="text-success d-block mt-1"><i class="bi bi-file-earmark-word"></i> فایل Word موجود است</small>` : '<small class="text-warning d-block mt-1"><i class="bi bi-exclamation-triangle"></i> فایل Word آپلود نشده</small>'}
                    </div>
                    <div class="btn-group">
                        <button class="btn btn-sm btn-outline-primary btn-edit-template" data-template-id="${template.id}">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-danger btn-delete-template" data-template-id="${template.id}">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </div>
            `).join('');

            // Add event listeners
            listContainer.querySelectorAll('.btn-edit-template').forEach(btn => {
                btn.addEventListener('click', async (e) => {
                    const templateId = e.currentTarget.dataset.templateId;
                    await this.editPrintTemplate(templateId);
                });
            });

            listContainer.querySelectorAll('.btn-delete-template').forEach(btn => {
                btn.addEventListener('click', async (e) => {
                    const templateId = e.currentTarget.dataset.templateId;
                    if (confirm('آیا از حذف این قالب اطمینان دارید؟')) {
                        await this.deletePrintTemplate(templateId);
                    }
                });
            });
        } catch (error) {
            console.error('Error loading print templates:', error);
            listContainer.innerHTML = '<div class="alert alert-danger">خطا در بارگذاری قالب‌های چاپ</div>';
        }
    }

    async editPrintTemplate(templateId) {
        try {
            const response = await fetch(`/api/formbuilder/printtemplates/${templateId}`);
            const template = await response.json();

            document.getElementById("template-name").value = template.name;
            document.getElementById("template-description").value = template.description || "";
            document.getElementById("template-display-order").value = template.displayOrder || 0;
            document.getElementById("template-is-default").checked = template.isDefault;

            this.showPrintTemplateForm(templateId);
        } catch (error) {
            console.error('Error loading template:', error);
            alert('خطا در بارگذاری اطلاعات قالب');
        }
    }

    async savePrintTemplate() {
        const name = document.getElementById("template-name").value;
        if (!name) {
            alert('نام قالب الزامی است');
            return;
        }

        const fileInput = document.getElementById("template-file");
        const file = fileInput.files[0];

        // If editing and no new file, just update metadata
        if (this.currentEditingTemplateId && !file) {
            await this.updatePrintTemplateMetadata();
            return;
        }

        // If new template or file is provided, need to upload
        if (!file) {
            alert('لطفا فایل Word را انتخاب کنید');
            return;
        }

        const formData = new FormData();
        formData.append('file', file);

        try {
            let templateId = this.currentEditingTemplateId;
            
            // If new template, create it first
            if (!templateId) {
                const createResponse = await fetch(`/api/formbuilder/forms/${this.config.formId}/printtemplates`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        name: name,
                        description: document.getElementById("template-description").value,
                        displayOrder: parseInt(document.getElementById("template-display-order").value) || 0,
                        isDefault: document.getElementById("template-is-default").checked
                    })
                });

                const createResult = await createResponse.json();
                if (!createResult.success) {
                    throw new Error('خطا در ایجاد قالب');
                }
                templateId = createResult.id;
            } else {
                // Update metadata first
                await this.updatePrintTemplateMetadata();
            }

            // Upload file
            const uploadResponse = await fetch(`/api/formbuilder/printtemplates/${templateId}/upload`, {
                method: 'POST',
                body: formData
            });

            const uploadResult = await uploadResponse.json();
            if (!uploadResult.success) {
                throw new Error('خطا در آپلود فایل');
            }

            alert('قالب با موفقیت ذخیره شد');
            this.hidePrintTemplateForm();
            this.loadPrintTemplates();
        } catch (error) {
            console.error('Error saving template:', error);
            alert('خطا در ذخیره قالب: ' + error.message);
        }
    }

    async updatePrintTemplateMetadata() {
        const response = await fetch(`/api/formbuilder/forms/${this.config.formId}/printtemplates`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                id: this.currentEditingTemplateId,
                name: document.getElementById("template-name").value,
                description: document.getElementById("template-description").value,
                displayOrder: parseInt(document.getElementById("template-display-order").value) || 0,
                isDefault: document.getElementById("template-is-default").checked
            })
        });

        const result = await response.json();
        if (!result.success) {
            throw new Error('خطا در به‌روزرسانی قالب');
        }
    }

    async deletePrintTemplate(templateId) {
        try {
            const response = await fetch(`/api/formbuilder/printtemplates/${templateId}`, {
                method: 'DELETE'
            });

            const result = await response.json();
            if (result.success) {
                alert('قالب با موفقیت حذف شد');
                this.loadPrintTemplates();
            } else {
                throw new Error('خطا در حذف قالب');
            }
        } catch (error) {
            console.error('Error deleting template:', error);
            alert('خطا در حذف قالب');
        }
    }

    // ==========================================
    //           DATABASE CONNECTION MODAL
    // ==========================================

    /**
     * Open database connection modal
     */
    openDatabaseConnectionModal(el) {
        this.dbConnectionState.currentElement = el;
        this.dbConnectionState.selectedTable = null;
        this.dbConnectionState.selectedColumns = [];
        
        // Reset modal state
        document.getElementById('db-diagram-container').innerHTML = `
            <div class="text-center text-muted py-5">
                <i class="bi bi-diagram-3 display-4 d-block mb-3"></i>
                <p>یک جدول، ویو یا تابع را از سمت چپ انتخاب کنید</p>
            </div>
        `;
        document.getElementById('btn-confirm-db-selection').disabled = true;
        
        // Load tables and functions
        this.loadDatabaseTables();
        this.loadDatabaseFunctions();
        
        // Setup event listeners
        this.setupDatabaseModalListeners();
        
        // Show modal
        this.modalDatabase.show();
    }

    /**
     * Setup event listeners for database modal
     */
    setupDatabaseModalListeners() {
        // Search functionality
        const searchTables = document.getElementById('db-search-tables');
        const searchFunctions = document.getElementById('db-search-functions');
        
        if (searchTables) {
            searchTables.oninput = (e) => {
                const searchTerm = e.target.value.toLowerCase();
                document.querySelectorAll('#db-tables-list .list-group-item').forEach(item => {
                    const text = item.textContent.toLowerCase();
                    item.style.display = text.includes(searchTerm) ? '' : 'none';
                });
            };
        }
        
        if (searchFunctions) {
            searchFunctions.oninput = (e) => {
                const searchTerm = e.target.value.toLowerCase();
                document.querySelectorAll('#db-functions-list .list-group-item').forEach(item => {
                    const text = item.textContent.toLowerCase();
                    item.style.display = text.includes(searchTerm) ? '' : 'none';
                });
            };
        }
        
        // Confirm button
        const btnConfirm = document.getElementById('btn-confirm-db-selection');
        if (btnConfirm) {
            btnConfirm.onclick = () => {
                this.confirmDatabaseSelection();
            };
        }
    }

    /**
     * Load database tables
     */
    async loadDatabaseTables() {
        const container = document.getElementById('db-tables-list');
        if (!container) return;
        
        container.innerHTML = '<div class="text-center text-muted py-4"><i class="bi bi-hourglass-split"></i> در حال بارگذاری...</div>';
        
        try {
            const response = await fetch('/api/formbuilder/database/tables');
            const result = await response.json();
            
            if (result.success && result.tables && result.tables.length > 0) {
                container.innerHTML = result.tables.map(table => `
                    <div class="list-group-item list-group-item-action" 
                         data-table-name="${table.databaseTableName}" 
                         data-table-id="${table.id}"
                         style="cursor: pointer;">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <h6 class="mb-1">${table.nameFa}</h6>
                                <small class="text-muted">${table.databaseTableName} (${table.fieldCount} فیلد)</small>
                            </div>
                            <i class="bi bi-chevron-left"></i>
                        </div>
                    </div>
                `).join('');
                
                // Add click handlers
                container.querySelectorAll('.list-group-item').forEach(item => {
                    item.addEventListener('click', () => {
                        this.selectDatabaseTable(item.dataset.tableName, item.dataset.tableId);
                    });
                });
            } else {
                container.innerHTML = '<div class="text-center text-muted py-4"><i class="bi bi-inbox"></i><p class="mt-2">هیچ جدولی یافت نشد</p></div>';
            }
        } catch (error) {
            console.error('Error loading tables:', error);
            container.innerHTML = '<div class="alert alert-danger">خطا در بارگذاری جداول</div>';
        }
    }

    /**
     * Load database functions
     */
    async loadDatabaseFunctions() {
        const container = document.getElementById('db-functions-list');
        if (!container) return;
        
        container.innerHTML = '<div class="text-center text-muted py-4"><i class="bi bi-hourglass-split"></i> در حال بارگذاری...</div>';
        
        try {
            const response = await fetch('/api/formbuilder/database/functions');
            const result = await response.json();
            
            if (result.success && result.functions && result.functions.length > 0) {
                container.innerHTML = result.functions.map(func => `
                    <div class="list-group-item list-group-item-action" 
                         data-function-id="${func.id}"
                         data-function-name="${func.name}"
                         style="cursor: pointer;">
                        <div class="d-flex justify-content-between align-items-center">
                            <div>
                                <h6 class="mb-1">${func.displayName}</h6>
                                <small class="text-muted">${func.name} (${func.returnType})</small>
                            </div>
                            <i class="bi bi-chevron-left"></i>
                        </div>
                    </div>
                `).join('');
                
                // Add click handlers
                container.querySelectorAll('.list-group-item').forEach(item => {
                    item.addEventListener('click', () => {
                        // TODO: Handle function selection
                        alert('انتخاب تابع در حال توسعه است');
                    });
                });
            } else {
                container.innerHTML = '<div class="text-center text-muted py-4"><i class="bi bi-inbox"></i><p class="mt-2">هیچ تابعی یافت نشد</p></div>';
            }
        } catch (error) {
            console.error('Error loading functions:', error);
            container.innerHTML = '<div class="alert alert-danger">خطا در بارگذاری توابع</div>';
        }
    }

    /**
     * Select database table and show diagram
     */
    async selectDatabaseTable(tableName, tableId) {
        // Update selected state
        document.querySelectorAll('#db-tables-list .list-group-item').forEach(item => {
            item.classList.remove('active');
        });
        const selectedItem = document.querySelector(`#db-tables-list .list-group-item[data-table-name="${tableName}"]`);
        if (selectedItem) {
            selectedItem.classList.add('active');
        }
        
        this.dbConnectionState.selectedTable = { name: tableName, id: tableId };
        
        // Load table columns
        try {
            const response = await fetch(`/api/formbuilder/database/tables/${encodeURIComponent(tableName)}/columns`);
            const result = await response.json();
            
            if (result.success && result.columns) {
                this.renderTableDiagram(result.columns);
                this.dbConnectionState.selectedColumns = result.columns.map(col => ({
                    ...col,
                    selected: false,
                    displayOrder: 0,
                    hide: false,
                    isValue: false
                }));
            }
        } catch (error) {
            console.error('Error loading table columns:', error);
            alert('خطا در بارگذاری ستون‌های جدول');
        }
    }

    /**
     * Render table diagram (like SQL Server diagram)
     */
    renderTableDiagram(columns) {
        const container = document.getElementById('db-diagram-container');
        if (!container) return;
        
        container.innerHTML = `
            <div class="card border-primary">
                <div class="card-header bg-primary text-white">
                    <h6 class="mb-0"><i class="bi bi-table me-2"></i>${this.dbConnectionState.selectedTable.name}</h6>
                </div>
                <div class="card-body p-0">
                    <div class="table-responsive">
                        <table class="table table-bordered table-sm mb-0" id="db-diagram-table">
                            <thead class="table-light">
                                <tr>
                                    <th style="width:50px;" class="text-center">
                                        <input type="checkbox" id="db-select-all-columns" title="انتخاب همه">
                                    </th>
                                    <th>نام انگلیسی</th>
                                    <th>نام فارسی</th>
                                    <th>نوع داده</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${columns.map((col, idx) => `
                                    <tr>
                                        <td class="text-center">
                                            <input type="checkbox" class="db-column-checkbox" data-column-index="${idx}" data-column-name="${col.columnName}">
                                        </td>
                                        <td>${col.columnName}</td>
                                        <td>${col.labelFa || col.labelEn || col.columnName}</td>
                                        <td><code>${col.dataType}${col.maxLength ? `(${col.maxLength})` : ''}</code></td>
                                    </tr>
                                `).join('')}
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        `;
        
        // Setup checkbox handlers
        document.getElementById('db-select-all-columns').addEventListener('change', (e) => {
            const checkboxes = container.querySelectorAll('.db-column-checkbox');
            checkboxes.forEach(cb => cb.checked = e.target.checked);
            this.updateSelectedColumns();
        });
        
        container.querySelectorAll('.db-column-checkbox').forEach(cb => {
            cb.addEventListener('change', () => {
                this.updateSelectedColumns();
            });
        });
        
        // Enable confirm button
        document.getElementById('btn-confirm-db-selection').disabled = false;
    }

    /**
     * Update selected columns state
     */
    updateSelectedColumns() {
        const checkboxes = document.querySelectorAll('.db-column-checkbox:checked');
        const selectedIndices = Array.from(checkboxes).map(cb => parseInt(cb.dataset.columnIndex));
        
        // Update state
        this.dbConnectionState.selectedColumns.forEach((col, idx) => {
            col.selected = selectedIndices.includes(idx);
        });
        
        // Update confirm button state
        const btnConfirm = document.getElementById('btn-confirm-db-selection');
        if (btnConfirm) {
            btnConfirm.disabled = selectedIndices.length === 0;
        }
    }

    /**
     * Confirm database selection and transfer to specific tab
     */
    confirmDatabaseSelection() {
        const selectedCols = this.dbConnectionState.selectedColumns.filter(col => col.selected);
        
        if (selectedCols.length === 0) {
            alert('لطفاً حداقل یک فیلد را انتخاب کنید');
            return;
        }
        
        const el = this.dbConnectionState.currentElement;
        if (!el) return;
        
        // Close modal
        this.modalDatabase.hide();
        
        // Update element data source
        el.data.sourceType = 'database';
        el.data.databaseTable = this.dbConnectionState.selectedTable.name;
        el.data.databaseTableId = this.dbConnectionState.selectedTable.id;
        el.data.selectedColumns = selectedCols;
        
        // Convert to options format based on control type
        if (['Select', 'MultiSelect'].includes(el.type)) {
            // For dropdown, create options from selected columns
            el.props.options = selectedCols.map((col, idx) => ({
                value: col.columnName,
                label: col.labelFa || col.labelEn || col.columnName,
                description: col.dataType,
                displayOrder: idx + 1,
                hide: false,
                isValue: false
            }));
        } else if (['RadioButton', 'CheckboxGroup'].includes(el.type)) {
            // For radio/checkbox, same format
            el.props.options = selectedCols.map((col, idx) => ({
                value: col.columnName,
                label: col.labelFa || col.labelEn || col.columnName,
                description: col.dataType,
                displayOrder: idx + 1,
                hide: false,
                isValue: false
            }));
        }
        
        // Refresh specific properties tab to show new table
        this.renderSpecificPropertiesNewModal(el);
        
        // Switch to specific tab
        document.querySelector('#prop-tab-specific').click();
    }

    // ==========================================
    //           SPECIFIC CONTROL SETTINGS
    // ==========================================

    // Old renderSpecificProperties removed.

    // ==========================================
    //           ACTION BUTTONS MANAGEMENT
    // ==========================================

    async openActionButtonsModal() {
        this.modalActionButtons.show();
        await this.loadActionButtonsData();
        this.renderAvailableButtons();
        this.renderActiveButtons();
        this.populateStylePresets();
    }

    async loadActionButtonsData() {
        try {
            // Load button types
            const typesRes = await fetch('/api/formbuilder/buttontypes');
            const typesData = await typesRes.json();
            if (typesData.success) {
                this.actionButtonTypes = typesData.data;
            }

            // Load style presets
            const presetsRes = await fetch('/api/formbuilder/buttonstylepresets');
            const presetsData = await presetsRes.json();
            if (presetsData.success) {
                this.actionButtonPresets = presetsData.data;
            }

            // Load current form buttons if formId exists
            if (this.config.formId) {
                const buttonsRes = await fetch(`/api/formbuilder/forms/${this.config.formId}/buttons`);
                const buttonsData = await buttonsRes.json();
                if (buttonsData.success) {
                    this.activeFormButtons = buttonsData.data.buttons || [];
                }
            } else {
                this.activeFormButtons = [];
            }
        } catch (error) {
            console.error('Error loading action buttons data:', error);
            alert('خطا در بارگذاری اطلاعات دکمه‌ها: ' + error.message);
        }
    }

    renderAvailableButtons(category = 'all') {
        const container = document.getElementById('available-buttons-list');
        const filtered = category === 'all'
            ? this.actionButtonTypes
            : this.actionButtonTypes.filter(bt => bt.category === category);

        container.innerHTML = filtered.map(btn => `
            <div class="card mb-2 btn-type-item" data-btn-type-id="${btn.id}" style="cursor: pointer;">
                <div class="card-body p-2">
                    <div class="d-flex align-items-center">
                        <i class="${btn.defaultIcon || 'bi bi-circle'} me-2"></i>
                        <div class="flex-grow-1">
                            <div class="fw-bold small">${btn.nameFa}</div>
                            ${btn.description ? `<small class="text-muted">${btn.description}</small>` : ''}
                        </div>
                        <button class="btn btn-sm btn-primary btn-add-button" data-btn-type-id="${btn.id}">
                            <i class="bi bi-plus"></i>
                        </button>
                    </div>
                </div>
            </div>
        `).join('');
    }

    renderActiveButtons() {
        const container = document.getElementById('active-buttons-list');
        const noButtonsMsg = document.getElementById('no-active-buttons');

        if (!this.activeFormButtons || this.activeFormButtons.length === 0) {
            noButtonsMsg?.classList.remove('d-none');
            container.innerHTML = '';
            return;
        }

        noButtonsMsg?.classList.add('d-none');
        container.innerHTML = this.activeFormButtons.map((btn, index) => {
            const title = btn.customTitleFa || btn.buttonTypeName || 'دکمه سفارشی';
            const icon = btn.customIcon || btn.buttonTypeIcon || 'bi bi-circle';
            return `
                <div class="list-group-item list-group-item-action d-flex justify-content-between align-items-center active-btn-item ${this.selectedActiveButtonIndex === index ? 'active' : ''}"
                     data-index="${index}">
                    <div class="d-flex align-items-center flex-grow-1">
                        <i class="${icon} me-2"></i>
                        <span>${title}</span>
                        ${btn.isProcessServiceButton ? '<span class="badge bg-success ms-2">سرویس فرآیندی</span>' : ''}
                        ${!btn.isEnabled ? '<span class="badge bg-secondary ms-2">غیرفعال</span>' : ''}
                    </div>
                    <div class="btn-group btn-group-sm">
                        <button class="btn btn-sm btn-outline-secondary btn-move-up" data-index="${index}" ${index === 0 ? 'disabled' : ''}>
                            <i class="bi bi-arrow-up"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-secondary btn-move-down" data-index="${index}" ${index === this.activeFormButtons.length - 1 ? 'disabled' : ''}>
                            <i class="bi bi-arrow-down"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-danger btn-remove-button" data-index="${index}">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </div>
            `;
        }).join('');
    }

    populateStylePresets() {
        const presetSelect = document.getElementById('btn-style-preset');
        const globalPresetSelect = document.getElementById('global-button-style-preset');

        const options = this.actionButtonPresets.map(p =>
            `<option value="${p.id}">${p.presetName}</option>`
        ).join('');

        if (presetSelect) {
            presetSelect.innerHTML = '<option value="">سفارشی</option>' + options;
        }
        if (globalPresetSelect) {
            globalPresetSelect.innerHTML = '<option value="">پیشفرض</option>' + options;
        }

        // Populate inherit-from dropdown
        const inheritSelect = document.getElementById('btn-inherit-from');
        if (inheritSelect) {
            const inheritOptions = this.actionButtonTypes.map(bt =>
                `<option value="${bt.id}">${bt.nameFa}</option>`
            ).join('');
            inheritSelect.innerHTML = '<option value="">بدون وراثت</option>' + inheritOptions;
        }
    }

    setupActionButtonsManagement() {
        // Category filter tabs
        document.addEventListener('click', (e) => {
            const categoryBtn = e.target.closest('#btntype-category-tabs .nav-link');
            if (categoryBtn) {
                document.querySelectorAll('#btntype-category-tabs .nav-link').forEach(b => b.classList.remove('active'));
                categoryBtn.classList.add('active');
                const category = categoryBtn.dataset.category;
                this.renderAvailableButtons(category);
            }
        });

        // Add button from available list
        document.addEventListener('click', (e) => {
            const addBtn = e.target.closest('.btn-add-button');
            if (addBtn) {
                const btnTypeId = addBtn.dataset.btnTypeId;
                this.addButtonToActive(btnTypeId, false);
            }
        });

        // Add process service button
        document.getElementById('btn-add-process-service')?.addEventListener('click', () => {
            this.addButtonToActive(null, true);
        });

        // Select active button for editing
        document.addEventListener('click', (e) => {
            const activeItem = e.target.closest('.active-btn-item');
            if (activeItem && !e.target.closest('button')) {
                const index = parseInt(activeItem.dataset.index);
                this.selectActiveButton(index);
            }
        });

        // Move up/down
        document.addEventListener('click', (e) => {
            const moveUpBtn = e.target.closest('.btn-move-up');
            const moveDownBtn = e.target.closest('.btn-move-down');

            if (moveUpBtn) {
                const index = parseInt(moveUpBtn.dataset.index);
                this.moveButton(index, -1);
            } else if (moveDownBtn) {
                const index = parseInt(moveDownBtn.dataset.index);
                this.moveButton(index, 1);
            }
        });

        // Remove button
        document.addEventListener('click', (e) => {
            const removeBtn = e.target.closest('.btn-remove-button');
            if (removeBtn) {
                const index = parseInt(removeBtn.dataset.index);
                if (confirm('آیا از حذف این دکمه اطمینان دارید؟')) {
                    this.activeFormButtons.splice(index, 1);
                    this.selectedActiveButtonIndex = -1;
                    this.renderActiveButtons();
                    this.clearButtonSettings();
                }
            }
        });

        // Save action buttons
        document.getElementById('btn-save-action-buttons')?.addEventListener('click', () => {
            this.saveActionButtons();
        });

        // Settings form inputs change
        const settingsInputs = [
            'btn-custom-title-fa', 'btn-custom-title-en', 'btn-display-mode',
            'btn-custom-icon', 'btn-style-preset', 'btn-color', 'btn-bg-color',
            'btn-font-size', 'btn-border-radius', 'btn-padding', 'btn-margin',
            'btn-confirmation-msg', 'btn-is-enabled', 'btn-inherit-from', 'btn-execution-timing'
        ];

        settingsInputs.forEach(id => {
            const input = document.getElementById(id);
            if (input) {
                input.addEventListener('change', () => this.updateSelectedButtonSettings());
                input.addEventListener('input', () => {
                    if (id === 'btn-custom-icon') {
                        const preview = document.querySelector('#btn-icon-preview i');
                        if (preview) {
                            preview.className = input.value || 'bi bi-circle';
                        }
                    }
                });
            }
        });

    }

    addButtonToActive(btnTypeId, isProcessService) {
        const btnType = btnTypeId ? this.actionButtonTypes.find(t => t.id === btnTypeId) : null;

        const newButton = {
            id: null,
            formId: this.config.formId,
            formButtonTypeId: btnTypeId || null,
            isProcessServiceButton: isProcessService,
            customTitleFa: '',
            customTitleEn: '',
            displayOrder: this.activeFormButtons.length,
            displayMode: 'icon_and_title',
            customIcon: '',
            styleSettings: '{}',
            buttonStylePresetId: null,
            inheritFromButtonTypeId: null,
            executionTiming: 'after',
            processServiceConfig: null,
            visibilityCondition: null,
            requiredPermission: null,
            confirmationMessage: null,
            isEnabled: true,
            // Populated from type
            buttonTypeName: btnType?.nameFa || null,
            buttonTypeIcon: btnType?.defaultIcon || null,
            buttonTypeColor: btnType?.defaultColor || null,
            buttonTypeActionHandler: btnType?.actionHandler || null,
            buttonTypeOpensModal: btnType?.opensModal || null,
            buttonTypeModalId: btnType?.modalId || null,
            presetName: null
        };

        this.activeFormButtons.push(newButton);
        this.renderActiveButtons();
        this.selectActiveButton(this.activeFormButtons.length - 1);
    }

    selectActiveButton(index) {
        this.selectedActiveButtonIndex = index;
        this.renderActiveButtons();
        this.loadButtonSettings(index);
    }

    loadButtonSettings(index) {
        const btn = this.activeFormButtons[index];
        if (!btn) return;

        document.getElementById('no-button-selected')?.classList.add('d-none');
        document.getElementById('button-settings-form')?.classList.remove('d-none');

        // Load values
        document.getElementById('btn-custom-title-fa').value = btn.customTitleFa || '';
        document.getElementById('btn-custom-title-en').value = btn.customTitleEn || '';
        document.getElementById('btn-display-mode').value = btn.displayMode || 'icon_and_title';
        document.getElementById('btn-custom-icon').value = btn.customIcon || '';
        document.getElementById('btn-style-preset').value = btn.buttonStylePresetId || '';
        document.getElementById('btn-confirmation-msg').value = btn.confirmationMessage || '';
        document.getElementById('btn-is-enabled').checked = btn.isEnabled !== false;

        // Parse style settings
        let styles = {};
        try {
            styles = JSON.parse(btn.styleSettings || '{}');
        } catch (e) {}

        document.getElementById('btn-color').value = styles.color || '#ffffff';
        document.getElementById('btn-bg-color').value = styles.backgroundColor || '#0d6efd';
        document.getElementById('btn-font-size').value = styles.fontSize || 14;
        document.getElementById('btn-border-radius').value = styles.borderRadius || 6;
        document.getElementById('btn-padding').value = styles.padding || '6px 12px';
        document.getElementById('btn-margin').value = styles.margin || '0 4px';

        // Icon preview
        const iconPreview = document.querySelector('#btn-icon-preview i');
        if (iconPreview) {
            iconPreview.className = btn.customIcon || btn.buttonTypeIcon || 'bi bi-circle';
        }

        // Process service section
        const processSection = document.getElementById('process-service-section');
        if (btn.isProcessServiceButton) {
            processSection?.classList.remove('d-none');
            document.getElementById('btn-inherit-from').value = btn.inheritFromButtonTypeId || '';
            document.getElementById('btn-execution-timing').value = btn.executionTiming || 'after';
        } else {
            processSection?.classList.add('d-none');
        }
    }

    clearButtonSettings() {
        document.getElementById('no-button-selected')?.classList.remove('d-none');
        document.getElementById('button-settings-form')?.classList.add('d-none');
    }

    updateSelectedButtonSettings() {
        if (this.selectedActiveButtonIndex < 0 || this.selectedActiveButtonIndex >= this.activeFormButtons.length) return;

        const btn = this.activeFormButtons[this.selectedActiveButtonIndex];

        btn.customTitleFa = document.getElementById('btn-custom-title-fa').value;
        btn.customTitleEn = document.getElementById('btn-custom-title-en').value;
        btn.displayMode = document.getElementById('btn-display-mode').value;
        btn.customIcon = document.getElementById('btn-custom-icon').value;
        btn.buttonStylePresetId = document.getElementById('btn-style-preset').value || null;
        btn.confirmationMessage = document.getElementById('btn-confirmation-msg').value || null;
        btn.isEnabled = document.getElementById('btn-is-enabled').checked;

        // Style settings
        const styles = {
            color: document.getElementById('btn-color').value,
            backgroundColor: document.getElementById('btn-bg-color').value,
            fontSize: parseInt(document.getElementById('btn-font-size').value) || 14,
            borderRadius: parseInt(document.getElementById('btn-border-radius').value) || 6,
            padding: document.getElementById('btn-padding').value,
            margin: document.getElementById('btn-margin').value
        };
        btn.styleSettings = JSON.stringify(styles);

        // Process service
        if (btn.isProcessServiceButton) {
            btn.inheritFromButtonTypeId = document.getElementById('btn-inherit-from').value || null;
            btn.executionTiming = document.getElementById('btn-execution-timing').value || 'after';
        }

        this.renderActiveButtons();
    }

    moveButton(index, direction) {
        const newIndex = index + direction;
        if (newIndex < 0 || newIndex >= this.activeFormButtons.length) return;

        const temp = this.activeFormButtons[index];
        this.activeFormButtons[index] = this.activeFormButtons[newIndex];
        this.activeFormButtons[newIndex] = temp;

        // Update display order
        this.activeFormButtons.forEach((btn, i) => {
            btn.displayOrder = i;
        });

        if (this.selectedActiveButtonIndex === index) {
            this.selectedActiveButtonIndex = newIndex;
        } else if (this.selectedActiveButtonIndex === newIndex) {
            this.selectedActiveButtonIndex = index;
        }

        this.renderActiveButtons();
    }

    async saveActionButtons() {
        if (!this.config.formId) {
            alert('لطفا ابتدا فرم را ذخیره کنید.');
            return;
        }

        const buttonBarSettings = JSON.stringify({
            layout: 'horizontal',
            alignment: 'end',
            gap: '4px',
            presetId: document.getElementById('global-button-style-preset').value || null
        });

        const config = {
            buttonBarSettings: buttonBarSettings,
            buttons: this.activeFormButtons.map((btn, index) => ({
                ...btn,
                displayOrder: index
            }))
        };

        try {
            const res = await fetch(`/api/formbuilder/forms/${this.config.formId}/buttons`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(config)
            });

            const data = await res.json();
            if (data.success) {
                alert('دکمه‌ها با موفقیت ذخیره شدند');
                this.modalActionButtons.hide();
            } else {
                alert('خطا در ذخیره دکمه‌ها: ' + (data.error || 'خطای نامشخص'));
            }
        } catch (error) {
            console.error('Error saving action buttons:', error);
            alert('خطا در ذخیره دکمه‌ها: ' + error.message);
        }
    }
}



