// ============================================
// DASHBOARD INTERACTIONS - ENHANCED WITH GSAP, ANIME.JS, VELOCITY.JS
// ============================================

(function() {
    'use strict';

    let tl, cardsAnimation, statusAnimation, alertsAnimation, tableAnimation;

    document.addEventListener('DOMContentLoaded', function() {
        initializeDashboard();
        setupScrollAnimations();
        setupHoverEffects();
        setupNumberCounters();
    });

    function initializeDashboard() {
        // Hide elements initially for animation
        gsap.set('.summary-card', { opacity: 0, y: 50, scale: 0.9 });
        gsap.set('.status-item', { opacity: 0, x: -30 });
        gsap.set('.alert-item', { opacity: 0, x: 30 });
        gsap.set('.data-table tbody tr', { opacity: 0, y: 20 });
        gsap.set('.section-title', { opacity: 0, y: -20 });
        gsap.set('.action-btn', { opacity: 0, scale: 0.8 });

        // Create master timeline
        tl = gsap.timeline({ defaults: { ease: "power3.out" } });

        // Animate section title
        tl.to('.section-title', {
            opacity: 1,
            y: 0,
            duration: 0.8
        });

        // Animate welcome message
        tl.to('.welcome-message', {
            opacity: 1,
            duration: 0.6
        }, "-=0.4");

        // Animate action buttons with stagger
        tl.to('.action-btn', {
            opacity: 1,
            scale: 1,
            duration: 0.6,
            stagger: 0.1,
            ease: "back.out(1.7)"
        }, "-=0.3");

        // Animate summary cards with GSAP
        cardsAnimation = gsap.to('.summary-card', {
            opacity: 1,
            y: 0,
            scale: 1,
            duration: 0.8,
            stagger: {
                amount: 0.6,
                from: "start"
            },
            ease: "power3.out"
        });

        // Animate status items with anime.js
        if (typeof anime !== 'undefined') {
            statusAnimation = anime({
                targets: '.status-item',
                opacity: [0, 1],
                translateX: [-30, 0],
                delay: anime.stagger(150),
                duration: 800,
                easing: 'easeOutExpo',
                complete: function() {
                    // Animate progress bars
                    animateProgressBars();
                }
            });
        }

        // Animate alerts with Velocity.js
        if (typeof Velocity !== 'undefined') {
            alertsAnimation = Velocity(document.querySelectorAll('.alert-item'), {
                opacity: [0, 1],
                translateX: [30, 0]
            }, {
                duration: 600,
                stagger: 150,
                easing: "easeOutCubic"
            });
        }

        // Animate table rows with GSAP
        tableAnimation = gsap.to('.data-table tbody tr', {
            opacity: 1,
            y: 0,
            duration: 0.5,
            stagger: 0.05,
            ease: "power2.out"
        });
    }

    function animateProgressBars() {
        const progressBars = document.querySelectorAll('.status-progress');
        progressBars.forEach((bar, index) => {
            const width = bar.style.width;
            bar.style.width = '0%';
            
            if (typeof anime !== 'undefined') {
                anime({
                    targets: bar,
                    width: width,
                    duration: 1500,
                    delay: index * 200,
                    easing: 'easeOutExpo'
                });
            } else {
                gsap.to(bar, {
                    width: width,
                    duration: 1.5,
                    delay: index * 0.2,
                    ease: "power2.out"
                });
            }
        });
    }

    function setupScrollAnimations() {
        if (typeof ScrollTrigger !== 'undefined') {
            gsap.registerPlugin(ScrollTrigger);

            // Parallax effect for panels
            gsap.utils.toArray('.status-panel, .alerts-panel').forEach(panel => {
                gsap.to(panel, {
                    y: -30,
                    scrollTrigger: {
                        trigger: panel,
                        start: "top bottom",
                        end: "bottom top",
                        scrub: 1
                    }
                });
            });
        }
    }

    function setupHoverEffects() {
        // Enhanced card hover effects with GSAP
        const cards = document.querySelectorAll('.summary-card');
        cards.forEach(card => {
            const cardIcon = card.querySelector('.card-icon');
            const cardNumber = card.querySelector('.card-number');

            card.addEventListener('mouseenter', function() {
                gsap.to(card, {
                    scale: 1.03,
                    y: -8,
                    duration: 0.4,
                    ease: "power2.out"
                });

                if (cardIcon) {
                    gsap.to(cardIcon, {
                        rotation: 10,
                        scale: 1.15,
                        duration: 0.4,
                        ease: "back.out(1.7)"
                    });
                }

                // Pulse animation for numbers
                if (cardNumber && typeof anime !== 'undefined') {
                    anime({
                        targets: cardNumber,
                        scale: [1, 1.1, 1],
                        duration: 600,
                        easing: 'easeInOutQuad'
                    });
                }
            });

            card.addEventListener('mouseleave', function() {
                gsap.to(card, {
                    scale: 1,
                    y: 0,
                    duration: 0.4,
                    ease: "power2.out"
                });

                if (cardIcon) {
                    gsap.to(cardIcon, {
                        rotation: 0,
                        scale: 1,
                        duration: 0.4,
                        ease: "power2.out"
                    });
                }
            });
        });

        // Action buttons click effect with GSAP (more reliable)
        const actionButtons = document.querySelectorAll('.action-btn');
        actionButtons.forEach(btn => {
            btn.addEventListener('click', function(e) {
                createButtonBurstEffect(btn);
            });
        });

        function createButtonBurstEffect(button) {
            // Create burst effect with GSAP
            const rect = button.getBoundingClientRect();
            const centerX = rect.left + rect.width / 2;
            const centerY = rect.top + rect.height / 2;
            
            for (let i = 0; i < 8; i++) {
                const particle = document.createElement('div');
                particle.style.position = 'fixed';
                particle.style.width = '10px';
                particle.style.height = '10px';
                particle.style.borderRadius = '50%';
                particle.style.pointerEvents = 'none';
                particle.style.zIndex = '9999';
                
                const colors = ['#3b82f6', '#8b5cf6', '#10b981'];
                particle.style.background = colors[i % colors.length];
                particle.style.left = centerX + 'px';
                particle.style.top = centerY + 'px';
                
                document.body.appendChild(particle);
                
                const angle = (i / 8) * Math.PI * 2;
                const distance = 50;
                
                gsap.to(particle, {
                    x: Math.cos(angle) * distance,
                    y: Math.sin(angle) * distance,
                    opacity: 0,
                    scale: 0,
                    duration: 0.8,
                    ease: "power2.out",
                    onComplete: () => particle.remove()
                });
            }
        }

        // Table row hover effects - không dùng scale để tránh scrollbar
        const tableRows = document.querySelectorAll('.data-table tbody tr');
        tableRows.forEach(row => {
            row.addEventListener('mouseenter', function() {
                // Chỉ thay đổi background, không scale để tránh scrollbar
                gsap.to(row, {
                    backgroundColor: 'rgba(59, 130, 246, 0.05)',
                    duration: 0.2,
                    ease: "power2.out"
                });
            });

            row.addEventListener('mouseleave', function() {
                gsap.to(row, {
                    backgroundColor: 'transparent',
                    duration: 0.2,
                    ease: "power2.out"
                });
            });
        });
    }

    function setupNumberCounters() {
        // Animate numbers with counting effect
        const numberElements = document.querySelectorAll('.card-number');
        
        numberElements.forEach(el => {
            const finalValue = parseInt(el.textContent.replace(/,/g, ''));
            if (isNaN(finalValue)) return;

            if (typeof anime !== 'undefined') {
                const obj = { count: 0 };
                anime({
                    targets: obj,
                    count: finalValue,
                    duration: 2000,
                    delay: 500,
                    easing: 'easeOutExpo',
                    update: function() {
                        el.textContent = Math.floor(obj.count).toLocaleString('vi-VN');
                    }
                });
            } else {
                // Fallback with GSAP
                gsap.fromTo({ value: 0 }, {
                    value: finalValue,
                    duration: 2,
                    delay: 0.5,
                    ease: "power2.out",
                    onUpdate: function() {
                        el.textContent = Math.floor(this.targets()[0].value).toLocaleString('vi-VN');
                    }
                });
            }
        });
    }

    // Add ripple effect to buttons
    document.querySelectorAll('.action-btn, .alert-action-btn, .action-btn-small').forEach(btn => {
        btn.addEventListener('click', function(e) {
            const ripple = document.createElement('span');
            const rect = btn.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
            const y = e.clientY - rect.top - size / 2;

            ripple.style.width = ripple.style.height = size + 'px';
            ripple.style.left = x + 'px';
            ripple.style.top = y + 'px';
            ripple.classList.add('ripple');

            btn.appendChild(ripple);

            setTimeout(() => ripple.remove(), 600);
        });
    });

    // Add CSS for ripple effect
    const style = document.createElement('style');
    style.textContent = `
        .action-btn, .alert-action-btn, .action-btn-small {
            position: relative;
            overflow: hidden;
        }
        .ripple {
            position: absolute;
            border-radius: 50%;
            background: rgba(255, 255, 255, 0.6);
            transform: scale(0);
            animation: ripple-animation 0.6s ease-out;
            pointer-events: none;
        }
        @keyframes ripple-animation {
            to {
                transform: scale(4);
                opacity: 0;
            }
        }
    `;
    document.head.appendChild(style);

    // ============================================
    // SEARCH FUNCTIONALITY
    // ============================================
    const searchInput = document.getElementById('globalSearchInput');
    const searchResults = document.getElementById('searchResults');
    const searchForm = document.getElementById('globalSearchForm');
    let searchTimeout;

    if (searchInput && searchResults) {
        // Form submit - allow to go to search page
        // No preventDefault here, let form submit normally to show search results page

        // Search on input for dropdown preview
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            const keyword = this.value.trim();

            if (keyword.length < 2) {
                searchResults.style.display = 'none';
                return;
            }

            searchTimeout = setTimeout(() => {
                performSearch(keyword);
            }, 300);
        });

        // Close search results on click outside
        document.addEventListener('click', function(e) {
            if (!searchInput.contains(e.target) && !searchResults.contains(e.target)) {
                searchResults.style.display = 'none';
            }
        });

        // Show results on focus if has value
        searchInput.addEventListener('focus', function() {
            if (this.value.trim().length >= 2) {
                performSearch(this.value.trim());
            }
        });
    }

    function performSearch(keyword) {
        fetch(`/Dashboard/SearchApi?keyword=${encodeURIComponent(keyword)}`)
            .then(response => response.json())
            .then(data => {
                renderSearchResults(data);
            })
            .catch(error => {
                console.error('Search error:', error);
            });
    }

    function renderSearchResults(data) {
        const { customers, loans } = data;
        let html = '';

        if (customers.length === 0 && loans.length === 0) {
            html = '<div class="search-no-results">Không tìm thấy kết quả</div>';
        } else {
            // Customers section
            if (customers.length > 0) {
                html += `
                    <div class="search-results-section">
                        <div class="search-results-title">Khách hàng</div>
                        ${customers.map(c => `
                            <a href="/Customer/Details${c.loai === 'Cá nhân' ? 'CaNhan' : 'DoanhNghiep'}/${c.maKhachHang}" class="search-result-item">
                                <div class="search-result-icon customer">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                        <path d="M20 21V19C20 17.9391 19.5786 16.9217 18.8284 16.1716C18.0783 15.4214 17.0609 15 16 15H8C6.93913 15 5.92172 15.4214 5.17157 16.1716C4.42143 16.9217 4 17.9391 4 19V21"/>
                                        <circle cx="12" cy="7" r="4"/>
                                    </svg>
                                </div>
                                <div class="search-result-info">
                                    <div class="search-result-name">${c.ten}</div>
                                    <div class="search-result-meta">${c.loai} • ${c.soDinhDanh || ''} • ${c.soDienThoai || ''}</div>
                                </div>
                            </a>
                        `).join('')}
                    </div>
                `;
            }

            // Loans section
            if (loans.length > 0) {
                html += `
                    <div class="search-results-section">
                        <div class="search-results-title">Hồ sơ vay</div>
                        ${loans.map(l => `
                            <a href="/Loan/Details/${l.maKhoanVay}" class="search-result-item">
                                <div class="search-result-icon loan">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                        <path d="M14 2H6C5.46957 2 4.96086 2.21071 4.58579 2.58579C4.21071 2.96086 4 3.46957 4 4V20C4 20.5304 4.21071 21.0391 4.58579 21.4142C4.96086 21.7893 5.46957 22 6 22H18C18.5304 22 19.0391 21.7893 19.4142 21.4142C19.7893 21.0391 20 20.5304 20 20V8L14 2Z"/>
                                        <path d="M14 2V8H20"/>
                                    </svg>
                                </div>
                                <div class="search-result-info">
                                    <div class="search-result-name">#${l.maKhoanVayCode}</div>
                                    <div class="search-result-meta">${formatMoney(l.soTienVay)} VND • ${l.trangThai}</div>
                                </div>
                            </a>
                        `).join('')}
                    </div>
                `;
            }
        }

        searchResults.innerHTML = html;
        searchResults.style.display = 'block';
    }

    function formatMoney(amount) {
        return new Intl.NumberFormat('vi-VN').format(amount);
    }

    // ============================================
    // NOTIFICATION FUNCTIONALITY
    // ============================================
    const notificationBtn = document.getElementById('notificationBtn');
    const notificationDropdown = document.getElementById('notificationDropdown');
    const notificationList = document.getElementById('notificationList');
    const notificationCount = document.getElementById('notificationCount');
    const markAllReadBtn = document.getElementById('markAllReadBtn');

    if (notificationBtn && notificationDropdown) {
        // Toggle notification dropdown
        notificationBtn.addEventListener('click', function(e) {
            e.stopPropagation();
            const isVisible = notificationDropdown.style.display === 'block';
            notificationDropdown.style.display = isVisible ? 'none' : 'block';
            
            if (!isVisible) {
                loadNotifications();
            }
        });

        // Close on click outside
        document.addEventListener('click', function(e) {
            if (!notificationBtn.contains(e.target) && !notificationDropdown.contains(e.target)) {
                notificationDropdown.style.display = 'none';
            }
        });

        // Mark all as read
        if (markAllReadBtn) {
            markAllReadBtn.addEventListener('click', function() {
                notificationCount.style.display = 'none';
                notificationCount.textContent = '0';
            });
        }

        // Load notifications on page load
        loadNotifications();
    }

    function loadNotifications() {
        fetch('/Dashboard/GetNotifications')
            .then(response => response.json())
            .then(data => {
                renderNotifications(data);
            })
            .catch(error => {
                console.error('Notification error:', error);
            });
    }

    function renderNotifications(data) {
        const { notifications, count } = data;

        // Update badge
        if (count > 0) {
            notificationCount.textContent = count > 9 ? '9+' : count;
            notificationCount.style.display = 'flex';
        } else {
            notificationCount.style.display = 'none';
        }

        // Render list
        if (notifications.length === 0) {
            notificationList.innerHTML = '<div class="notification-empty">Không có thông báo mới</div>';
            return;
        }

        notificationList.innerHTML = notifications.map(n => `
            <a href="/Loan/Details/${n.id}" class="notification-item">
                <div class="notification-icon-wrap ${n.type}">
                    ${getNotificationIcon(n.icon)}
                </div>
                <div class="notification-content">
                    <div class="notification-title">${n.title}</div>
                    <div class="notification-message">${n.message}</div>
                    <div class="notification-time">${formatTimeAgo(n.date)}</div>
                </div>
            </a>
        `).join('');
    }

    function getNotificationIcon(iconType) {
        const icons = {
            'check-circle': `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M22 11.08V12C21.9988 14.1564 21.3005 16.2547 20.0093 17.9818C18.7182 19.709 16.9033 20.9725 14.8354 21.5839C12.7674 22.1953 10.5573 22.1219 8.53447 21.3746C6.51168 20.6273 4.78465 19.2461 3.61096 17.4371C2.43727 15.628 1.87979 13.4881 2.02168 11.3363C2.16356 9.18455 2.99721 7.13631 4.39828 5.49706C5.79935 3.85782 7.69279 2.71537 9.79619 2.24013C11.8996 1.7649 14.1003 1.98232 16.07 2.85999"/>
                <path d="M22 4L12 14.01L9 11.01"/>
            </svg>`,
            'file-text': `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M14 2H6C5.46957 2 4.96086 2.21071 4.58579 2.58579C4.21071 2.96086 4 3.46957 4 4V20C4 20.5304 4.21071 21.0391 4.58579 21.4142C4.96086 21.7893 5.46957 22 6 22H18C18.5304 22 19.0391 21.7893 19.4142 21.4142C19.7893 21.0391 20 20.5304 20 20V8L14 2Z"/>
                <path d="M14 2V8H20"/>
                <path d="M16 13H8"/>
                <path d="M16 17H8"/>
                <path d="M10 9H9H8"/>
            </svg>`,
            'alert-circle': `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <circle cx="12" cy="12" r="10"/>
                <path d="M12 8V12"/>
                <path d="M12 16H12.01"/>
            </svg>`
        };
        return icons[iconType] || icons['alert-circle'];
    }

    function formatTimeAgo(dateStr) {
        const date = new Date(dateStr);
        const now = new Date();
        const diff = Math.floor((now - date) / 1000);

        if (diff < 60) return 'Vừa xong';
        if (diff < 3600) return `${Math.floor(diff / 60)} phút trước`;
        if (diff < 86400) return `${Math.floor(diff / 3600)} giờ trước`;
        if (diff < 604800) return `${Math.floor(diff / 86400)} ngày trước`;
        return date.toLocaleDateString('vi-VN');
    }
})();
