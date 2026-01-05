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
})();
