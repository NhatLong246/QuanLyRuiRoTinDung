// ============================================
// LANDING PAGE ANIMATIONS
// ============================================

(function() {
    'use strict';

    document.addEventListener('DOMContentLoaded', function() {
        initializeSplashScreen();
    });

    // ============================================
    // SPLASH SCREEN ANIMATION
    // ============================================
    function initializeSplashScreen() {
        if (typeof anime === 'undefined') {
            console.warn('anime.js is not loaded. Skipping animations.');
            showMainContent();
            return;
        }

        const splashScreen = document.getElementById('splashScreen');
        const splashTitle = document.querySelector('.splash-title');
        const mainContent = document.getElementById('mainContent');
        const headerBrandName = document.getElementById('headerBrandName');

        // Show main content but make it invisible to calculate positions
        mainContent.style.display = 'block';
        mainContent.style.visibility = 'hidden';
        mainContent.style.opacity = '0';

        // Wait for layout to calculate positions
        requestAnimationFrame(() => {
            requestAnimationFrame(() => {
                // Calculate positions
                const headerRect = headerBrandName ? headerBrandName.getBoundingClientRect() : null;
                
                // Get viewport center
                const viewportCenterX = window.innerWidth / 2;
                const viewportCenterY = window.innerHeight / 2;
                
                // Calculate target position and scale
                let targetX = 0;
                let targetY = 0;
                // Calculate scale: header font-size (16px) / splash font-size (120px) = 0.133
                const targetScale = 16 / 120; // Approximately 0.133
                
                if (headerBrandName && headerRect) {
                    // Calculate offset from viewport center to header brand name center
                    targetX = (headerRect.left + headerRect.width / 2) - viewportCenterX;
                    targetY = (headerRect.top + headerRect.height / 2) - viewportCenterY;
                }

                // Animate title appearance
                anime({
                    targets: splashTitle,
                    opacity: [0, 1],
                    scale: [0.5, 1],
                    duration: 1000,
                    easing: 'easeOutExpo',
                    complete: function() {
                        // Hold for 1 second, then shrink and move simultaneously
                        setTimeout(() => {
                            // Prepare for movement - set position fixed
                            splashTitle.style.position = 'fixed';
                            splashTitle.style.top = '50%';
                            splashTitle.style.left = '50%';
                            splashTitle.style.transformOrigin = 'center center';
                            
                            // Reset transform to start position
                            splashTitle.style.transform = 'translate(-50%, -50%) scale(1)';
                            
                            // Animate: shrink and move at the same time
                            // Use manual animation to control both scale and translate simultaneously
                            let fadeOutStarted = false;
                            
                            // Start position: center of viewport (0, 0 when using -50%, -50%)
                            const startX = 0;
                            const startY = 0;
                            const startScale = 1;
                            
                            // Make main content visible immediately (but transparent) to start fade in early
                            mainContent.style.visibility = 'visible';
                            mainContent.style.opacity = '0';
                            
                            // Use anime.js to animate a dummy property, then manually control transform
                            anime({
                                targets: { progress: 0 },
                                progress: 1,
                                duration: 1500,
                                easing: 'easeInOutExpo',
                                update: function(anim) {
                                    // Get current progress (0 to 1)
                                    const progress = anim.progress / 100;
                                    
                                    // Calculate current scale and position using same easing
                                    const currentScale = startScale - (startScale - targetScale) * progress;
                                    const currentX = startX + (targetX - startX) * progress;
                                    const currentY = startY + (targetY - startY) * progress;
                                    
                                    // Apply transform - use calc to combine -50% with pixel offset
                                    splashTitle.style.transform = `translate(calc(-50% + ${currentX}px), calc(-50% + ${currentY}px)) scale(${currentScale})`;
                                    
                                    // Fade out splash screen background and fade in main content simultaneously
                                    // Start from 40% progress, complete at 95% progress
                                    if (progress >= 0.4) {
                                        // Map progress from 0.4-0.95 to opacity 0-1 for content
                                        const fadeProgress = (progress - 0.4) / 0.55; // 0.4 to 0.95 = 0 to 1
                                        const contentOpacity = Math.min(fadeProgress, 1);
                                        
                                        // Fade in main content
                                        mainContent.style.opacity = contentOpacity.toString();
                                        
                                        // Fade in header brand name gradually
                                        if (headerBrandName) {
                                            headerBrandName.style.opacity = contentOpacity.toString();
                                        }
                                        
                                        // Fade out splash screen background (inverse of content opacity)
                                        const splashOpacity = 1 - contentOpacity;
                                        splashScreen.style.opacity = splashOpacity.toString();
                                    }
                                    
                                    // When scale is close to target (95% progress), start fading out splash title
                                    // At this point, main content should already be fully visible and splash background should be gone
                                    if (progress >= 0.95 && !fadeOutStarted) {
                                        fadeOutStarted = true;
                                        anime({
                                            targets: splashTitle,
                                            opacity: [1, 0],
                                            duration: 150,
                                            easing: 'easeOutQuad'
                                        });
                                    }
                                },
                                complete: function() {
                                    // Ensure final position
                                    splashTitle.style.transform = `translate(calc(-50% + ${targetX}px), calc(-50% + ${targetY}px)) scale(${targetScale})`;
                                    
                                    // Hide splash title completely
                                    splashTitle.style.display = 'none';
                                    
                                    // Ensure header brand name is fully visible
                                    if (headerBrandName) {
                                        headerBrandName.style.opacity = '1';
                                    }
                                    
                                    // Ensure main content is fully visible
                                    mainContent.style.opacity = '1';
                                    mainContent.style.visibility = 'visible';
                                    
                                    // Ensure splash screen is completely transparent
                                    splashScreen.style.opacity = '0';
                                    
                                    // Hide splash screen after a brief moment
                                    setTimeout(function() {
                                        splashScreen.style.display = 'none';
                                        showMainContent();
                                    }, 100);
                                }
                            });
                        }, 1000);
                    }
                });
            });
        });
    }

    // ============================================
    // MAIN CONTENT ANIMATIONS
    // ============================================
    function showMainContent() {
        const mainContent = document.getElementById('mainContent');
        // Ensure main content is visible
        mainContent.style.display = 'block';
        mainContent.style.visibility = 'visible';
        mainContent.style.opacity = '1';

        if (typeof anime === 'undefined') {
            return;
        }

        // Animate hero content
        anime({
            targets: '.hero-content',
            opacity: [0, 1],
            translateY: [30, 0],
            duration: 1000,
            easing: 'easeOutExpo',
            delay: 300
        });

        // Animate section title and description
        anime.timeline({
            easing: 'easeOutExpo'
        })
        .add({
            targets: '.section-title',
            opacity: [0, 1],
            translateY: [20, 0],
            duration: 800,
            delay: 500
        })
        .add({
            targets: '.section-description',
            opacity: [0, 1],
            translateY: [20, 0],
            duration: 800
        }, '-=400');

        // Animate feature cards with stagger
        anime({
            targets: '.feature-card',
            opacity: [0, 1],
            translateY: [30, 0],
            duration: 800,
            delay: anime.stagger(200, {start: 800}),
            easing: 'easeOutExpo'
        });

        // Animate statistics with stagger
        anime({
            targets: '.stat-item',
            opacity: [0, 1],
            translateY: [20, 0],
            scale: [0.9, 1],
            duration: 600,
            delay: anime.stagger(100, {start: 1200}),
            easing: 'easeOutBack'
        });

        // Add hover animations to feature cards
        const featureCards = document.querySelectorAll('.feature-card');
        featureCards.forEach(card => {
            card.addEventListener('mouseenter', function() {
                if (typeof anime !== 'undefined') {
                    anime({
                        targets: this,
                        scale: [1, 1.02],
                        duration: 300,
                        easing: 'easeOutQuad'
                    });
                }
            });

            card.addEventListener('mouseleave', function() {
                if (typeof anime !== 'undefined') {
                    anime({
                        targets: this,
                        scale: [1.02, 1],
                        duration: 300,
                        easing: 'easeOutQuad'
                    });
                }
            });
        });

        // Animate numbers in statistics
        const statNumbers = document.querySelectorAll('.stat-number');
        statNumbers.forEach(stat => {
            const originalText = stat.textContent.trim();
            
            // Handle percentage (e.g., "99%")
            if (originalText.includes('%')) {
                const value = parseFloat(originalText);
                if (!isNaN(value)) {
                    anime({
                        targets: { count: 0 },
                        count: value,
                        duration: 2000,
                        delay: 1500,
                        easing: 'easeOutExpo',
                        round: 1,
                        update: function(anim) {
                            stat.textContent = Math.floor(anim.animatables[0].target.count) + '%';
                        }
                    });
                }
            }
            // Handle time format (e.g., "24/7")
            else if (originalText.includes('/')) {
                const parts = originalText.split('/');
                const value = parseFloat(parts[0]);
                const suffix = '/' + parts[1];
                if (!isNaN(value)) {
                    anime({
                        targets: { count: 0 },
                        count: value,
                        duration: 2000,
                        delay: 1500,
                        easing: 'easeOutExpo',
                        round: 1,
                        update: function(anim) {
                            stat.textContent = Math.floor(anim.animatables[0].target.count) + suffix;
                        }
                    });
                }
            }
        });
    }

    // ============================================
    // SCROLL ANIMATIONS
    // ============================================
    let scrollAnimationsInitialized = false;

    function initializeScrollAnimations() {
        if (scrollAnimationsInitialized || typeof anime === 'undefined') {
            return;
        }

        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver(function(entries) {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const element = entry.target;
                    if (element.classList.contains('feature-card') && element.style.opacity === '0') {
                        anime({
                            targets: element,
                            opacity: [0, 1],
                            translateY: [30, 0],
                            duration: 800,
                            easing: 'easeOutExpo'
                        });
                    }
                }
            });
        }, observerOptions);

        document.querySelectorAll('.feature-card, .stat-item').forEach(el => {
            observer.observe(el);
        });

        scrollAnimationsInitialized = true;
    }

    // Initialize scroll animations after main content is shown
    setTimeout(() => {
        initializeScrollAnimations();
    }, 2000);

    // ============================================
    // PARALLAX EFFECT FOR HERO
    // ============================================
    window.addEventListener('scroll', function() {
        const scrolled = window.pageYOffset;
        const heroBackground = document.querySelector('.hero-background');
        if (heroBackground) {
            heroBackground.style.transform = `translateY(${scrolled * 0.5}px)`;
        }
    });
})();

