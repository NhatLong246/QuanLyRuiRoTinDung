// ============================================
// LOGIN PAGE ANIMATIONS & INTERACTIONS
// ============================================

(function() {
    'use strict';

    // Wait for DOM and anime.js to be ready
    document.addEventListener('DOMContentLoaded', function() {
        initializeAnimations();
        initializeInteractions();
    });

    // ============================================
    // INITIALIZE ANIMATIONS
    // ============================================
    function initializeAnimations() {
        if (typeof anime === 'undefined') {
            console.warn('anime.js is not loaded. Falling back to CSS animations.');
            fallbackAnimations();
            return;
        }

        // Logo - no animation, just display normally

        // Animate both left and right panels simultaneously
        // Right panel - form elements
        anime.timeline({
            easing: 'easeOutExpo',
            duration: 1000
        })
        .add({
            targets: '[data-animate="form-header"]',
            opacity: [0, 1],
            translateY: [-40, 0],
            scale: [0.9, 1],
            rotateX: [-15, 0],
            delay: 300
        })
        .add({
            targets: '[data-animate="input-1"]',
            opacity: [0, 1],
            translateX: [-80, 0],
            rotateY: [-20, 0],
            scale: [0.95, 1],
            delay: 200
        }, '-=700')
        .add({
            targets: '[data-animate="input-2"]',
            opacity: [0, 1],
            translateX: [-80, 0],
            rotateY: [-20, 0],
            scale: [0.95, 1],
            delay: 200
        }, '-=500')
        .add({
            targets: '[data-animate="options"]',
            opacity: [0, 1],
            translateX: [-50, 0],
            delay: 200
        }, '-=400')
        .add({
            targets: '[data-animate="button"]',
            opacity: [0, 1],
            translateY: [40, 0],
            scale: [0.8, 1],
            rotateZ: [-5, 0],
            delay: 200
        }, '-=300')
        .add({
            targets: '[data-animate="form-footer"]',
            opacity: [0, 1],
            translateY: [30, 0],
            delay: 200
        }, '-=200');
    }

    // ============================================
    // FALLBACK CSS ANIMATIONS
    // ============================================
    function fallbackAnimations() {
        const elements = document.querySelectorAll('[data-animate]');
        elements.forEach((el, index) => {
            el.style.opacity = '0';
            setTimeout(() => {
                el.style.transition = 'all 0.8s ease-out';
                el.style.opacity = '1';
                if (el.dataset.animate.includes('input') || el.dataset.animate.includes('form')) {
                    el.style.transform = 'translateX(0)';
                } else {
                    el.style.transform = 'translateY(0)';
                }
            }, index * 100);
        });
    }

    // ============================================
    // INITIALIZE INTERACTIONS
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
                    
                    // Animate icon change
                    if (typeof anime !== 'undefined') {
                        anime({
                            targets: eyeOffIcon,
                            scale: [0.8, 1],
                            opacity: [0, 1],
                            duration: 300
                        });
                    }
                } else {
                    eyeIcon.style.display = 'block';
                    eyeOffIcon.style.display = 'none';
                    
                    if (typeof anime !== 'undefined') {
                        anime({
                            targets: eyeIcon,
                            scale: [0.8, 1],
                            opacity: [0, 1],
                            duration: 300
                        });
                    }
                }
            });
        }

        // Form input focus animations
        const inputs = document.querySelectorAll('.form-control');
        inputs.forEach(input => {
            input.addEventListener('focus', function() {
                if (typeof anime !== 'undefined') {
                    anime({
                        targets: this,
                        scale: [1, 1.02],
                        duration: 200,
                        easing: 'easeOutQuad'
                    });
                }
            });

            input.addEventListener('blur', function() {
                if (typeof anime !== 'undefined') {
                    anime({
                        targets: this,
                        scale: [1.02, 1],
                        duration: 200,
                        easing: 'easeOutQuad'
                    });
                }
            });
        });

        // Login form submission
        const loginForm = document.getElementById('loginForm');
        if (loginForm) {
            loginForm.addEventListener('submit', function(e) {
                const btnLogin = this.querySelector('.btn-login');
                const btnText = btnLogin.querySelector('.btn-text');
                const btnLoader = btnLogin.querySelector('.btn-loader');
                
                // Show loading state
                btnLogin.classList.add('loading');
                btnLogin.disabled = true;
                
                // Animate button
                if (typeof anime !== 'undefined') {
                    anime({
                        targets: btnLogin,
                        scale: [1, 0.98],
                        duration: 200
                    });
                    
                    anime({
                        targets: btnText,
                        opacity: [1, 0],
                        translateY: [0, -10],
                        duration: 300
                    });
                    
                    anime({
                        targets: btnLoader,
                        opacity: [0, 1],
                        scale: [0.8, 1],
                        duration: 300,
                        delay: 150
                    });
                }
            });
        }

        // Add ripple effect to login button
        const btnLogin = document.querySelector('.btn-login');
        if (btnLogin) {
            btnLogin.addEventListener('click', function(e) {
                if (typeof anime !== 'undefined') {
                    const ripple = document.createElement('span');
                    ripple.style.position = 'absolute';
                    ripple.style.borderRadius = '50%';
                    ripple.style.background = 'rgba(255, 255, 255, 0.5)';
                    ripple.style.width = '0px';
                    ripple.style.height = '0px';
                    ripple.style.left = e.offsetX + 'px';
                    ripple.style.top = e.offsetY + 'px';
                    ripple.style.transform = 'translate(-50%, -50%)';
                    ripple.style.pointerEvents = 'none';
                    
                    this.appendChild(ripple);
                    
                    anime({
                        targets: ripple,
                        width: '300px',
                        height: '300px',
                        opacity: [0.5, 0],
                        duration: 600,
                        easing: 'easeOutQuad',
                        complete: function() {
                            ripple.remove();
                        }
                    });
                }
            });
        }

        // Removed parallax effect - too distracting

        // Add typing effect to description (optional)
        const description = document.querySelector('.login-description');
        if (description && typeof anime !== 'undefined') {
            // This is just a visual enhancement, not actual typing
            anime({
                targets: description,
                opacity: [0, 1],
                duration: 1500,
                delay: 1000,
                easing: 'easeInOutQuad'
            });
        }

        // Add stagger animation to form inputs on focus
        inputs.forEach((input, index) => {
            input.addEventListener('focus', function() {
                if (typeof anime !== 'undefined') {
                    anime({
                        targets: this.closest('.form-group'),
                        scale: [1, 1.01],
                        duration: 300,
                        easing: 'easeOutQuad'
                    });
                }
            });

            input.addEventListener('blur', function() {
                if (typeof anime !== 'undefined') {
                    anime({
                        targets: this.closest('.form-group'),
                        scale: [1.01, 1],
                        duration: 300,
                        easing: 'easeOutQuad'
                    });
                }
            });
        });

        // Add success animation on successful validation
        const formGroups = document.querySelectorAll('.form-group');
        formGroups.forEach(group => {
            const input = group.querySelector('.form-control');
            if (input) {
                input.addEventListener('input', function() {
                    if (this.value.length > 0 && this.checkValidity()) {
                        if (typeof anime !== 'undefined') {
                            anime({
                                targets: this,
                                borderColor: ['#e5e7eb', '#10b981', '#e5e7eb'],
                                duration: 600,
                                easing: 'easeInOutQuad'
                            });
                        }
                    }
                });
            }
        });
    }

    // ============================================
    // ADDITIONAL ENHANCEMENTS
    // ============================================
    
    // Add entrance animation for page load
    window.addEventListener('load', function() {
        if (typeof anime !== 'undefined') {
            // Animate the entire container
            anime({
                targets: '.login-container',
                opacity: [0, 1],
                duration: 500,
                easing: 'easeOutQuad'
            });
        }
    });

    // Add keyboard navigation enhancements
    document.addEventListener('keydown', function(e) {
        // Enter key on form inputs
        if (e.key === 'Enter' && e.target.classList.contains('form-control')) {
            const form = e.target.closest('form');
            if (form) {
                const submitBtn = form.querySelector('.btn-login');
                if (submitBtn && !submitBtn.disabled) {
                    e.preventDefault();
                    submitBtn.click();
                }
            }
        }
    });

    // ============================================
    // UTILITY FUNCTIONS
    // ============================================
    
    // Add smooth scroll behavior
    if (document.querySelector('.login-right-panel')) {
        document.querySelector('.login-right-panel').style.scrollBehavior = 'smooth';
    }

    // Handle window resize
    let resizeTimer;
    window.addEventListener('resize', function() {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function() {
            // Recalculate animations if needed
            if (typeof anime !== 'undefined') {
                // Reset any transform values that might be affected
                const elements = document.querySelectorAll('[data-animate]');
                elements.forEach(el => {
                    if (window.innerWidth <= 768) {
                        // Mobile adjustments
                        el.style.transform = '';
                    }
                });
            }
        }, 250);
    });
})();

