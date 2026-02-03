// ============================================
// LOGIN PAGE ANIMATIONS & INTERACTIONS
// Optimized for speed and smooth experience
// ============================================

(function() {
    'use strict';

    // Wait for DOM to be ready
    document.addEventListener('DOMContentLoaded', function() {
        // Hiển thị ngay form để tránh loading lâu
        showElementsImmediately();
        initializeInteractions();
        // Animation nhẹ chạy sau
        requestAnimationFrame(initializeAnimations);
    });

    // ============================================
    // SHOW ELEMENTS IMMEDIATELY - NO WAITING
    // ============================================
    function showElementsImmediately() {
        const elements = document.querySelectorAll('[data-animate]');
        elements.forEach(el => {
            el.style.opacity = '1';
            el.style.transform = 'none';
        });
    }

    // ============================================
    // INITIALIZE ANIMATIONS - SIMPLIFIED
    // ============================================
    function initializeAnimations() {
        if (typeof anime === 'undefined') {
            return; // Không cần fallback vì đã show rồi
        }

        // Animation nhẹ cho form wrapper
        anime({
            targets: '.login-form-wrapper',
            opacity: [0.9, 1],
            translateY: [10, 0],
            duration: 400,
            easing: 'easeOutQuad'
        });

        // Animation nhẹ cho left panel
        anime({
            targets: '.login-left-panel .login-content-wrapper',
            opacity: [0.9, 1],
            duration: 500,
            easing: 'easeOutQuad'
        });
    }

    // ============================================
    // INITIALIZE INTERACTIONS - SIMPLIFIED
    // ============================================
    function initializeInteractions() {
        // Password toggle
        const passwordToggle = document.getElementById('passwordToggle');
        const passwordInput = document.getElementById('passwordInput');
        
        if (passwordToggle && passwordInput) {
            passwordToggle.addEventListener('click', function() {
                const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
                passwordInput.setAttribute('type', type);
                
                const eyeIcon = this.querySelector('.eye-icon');
                const eyeOffIcon = this.querySelector('.eye-off-icon');
                
                if (type === 'text') {
                    eyeIcon.style.display = 'none';
                    eyeOffIcon.style.display = 'block';
                } else {
                    eyeIcon.style.display = 'block';
                    eyeOffIcon.style.display = 'none';
                }
            });
        }

        // Login form submission - optimized
        const loginForm = document.getElementById('loginForm');
        if (loginForm) {
            loginForm.addEventListener('submit', function(e) {
                const btnLogin = this.querySelector('.btn-login');
                const btnText = btnLogin.querySelector('.btn-text');
                const btnLoader = btnLogin.querySelector('.btn-loader');
                
                // Show loading state immediately
                btnLogin.classList.add('loading');
                btnLogin.disabled = true;
                
                if (btnText) btnText.style.opacity = '0';
                if (btnLoader) {
                    btnLoader.style.display = 'inline-flex';
                    btnLoader.style.opacity = '1';
                }
            });
        }

        // Keyboard navigation
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Enter' && e.target.classList.contains('form-control')) {
                const form = e.target.closest('form');
                if (form) {
                    const submitBtn = form.querySelector('.btn-login');
                    if (submitBtn && !submitBtn.disabled) {
                        e.preventDefault();
                        form.submit();
                    }
                }
            }
        });
    }
})();

