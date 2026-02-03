// ============================================
// QUẢN LÝ RỦI RO TÍN DỤNG - SCRIPTS
// ============================================

document.addEventListener('DOMContentLoaded', function() {
    // Set active menu item based on current URL
    const currentPath = window.location.pathname.toLowerCase();
    const navItems = document.querySelectorAll('.sidebar-nav .nav-item');
    
    // Remove active from all items first
    navItems.forEach(nav => nav.classList.remove('active'));
    
    // Find and activate the matching menu item
    let found = false;
    navItems.forEach(item => {
        const href = item.getAttribute('href');
        if (href) {
            const hrefPath = href.toLowerCase();
            // Check if current path matches the href
            if (currentPath === hrefPath || 
                currentPath.includes(hrefPath.replace('/quanlyruiro/', '').replace('/quanlyruiro', '')) ||
                (currentPath.includes('thamdinh') && hrefPath.includes('thamdinh')) ||
                (currentPath.includes('phanloai') && hrefPath.includes('phanloai')) ||
                (currentPath.includes('xaydung') && hrefPath.includes('xaydung')) ||
                (currentPath.includes('theodoi') && hrefPath.includes('theodoi')) ||
                (currentPath.includes('phathien') && hrefPath.includes('phathien')) ||
                (currentPath.includes('dexuat') && hrefPath.includes('dexuat'))) {
                item.classList.add('active');
                found = true;
            }
        }
    });
    
    // If no match found and we're on index page, activate first item
    if (!found && (currentPath.endsWith('/quanlyruiro') || currentPath.endsWith('/quanlyruiro/') || currentPath.endsWith('/quanlyruiro/index'))) {
        const firstItem = document.querySelector('.sidebar-nav .nav-item:first-child');
        if (firstItem) {
            firstItem.classList.add('active');
        }
    }
    
    // Tab functionality for PhanLoaiMucDoRuiRo page
    const tabButtons = document.querySelectorAll('.tab-btn');
    if (tabButtons.length > 0) {
        tabButtons.forEach(btn => {
            btn.addEventListener('click', function() {
                tabButtons.forEach(b => b.classList.remove('active'));
                this.classList.add('active');
                
                const tab = this.getAttribute('data-tab');
                // Add filter logic here if needed
                filterByTab(tab);
            });
        });
    }
    
    // Form validation
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            if (!form.checkValidity()) {
                e.preventDefault();
                e.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });
    
    // Initialize tooltips if Bootstrap is available
    if (typeof bootstrap !== 'undefined') {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
});

// Filter by tab function
function filterByTab(tab) {
    const rows = document.querySelectorAll('.data-table tbody tr');
    rows.forEach(row => {
        if (tab === 'all') {
            row.style.display = '';
        } else {
            const riskLevel = row.querySelector('.risk-badge');
            if (riskLevel) {
                const riskText = riskLevel.textContent.trim().toLowerCase();
                const shouldShow = 
                    (tab === 'low' && riskText.includes('thấp')) ||
                    (tab === 'medium' && riskText.includes('trung bình')) ||
                    (tab === 'high' && riskText.includes('cao'));
                row.style.display = shouldShow ? '' : 'none';
            }
        }
    });
}

// Search functionality
function performSearch() {
    const searchInput = document.querySelector('.search-input');
    if (searchInput) {
        searchInput.addEventListener('input', function(e) {
            const searchTerm = e.target.value.toLowerCase();
            const rows = document.querySelectorAll('.data-table tbody tr');
            
            rows.forEach(row => {
                const text = row.textContent.toLowerCase();
                row.style.display = text.includes(searchTerm) ? '' : 'none';
            });
        });
    }
}

// Initialize search on page load
document.addEventListener('DOMContentLoaded', performSearch);

