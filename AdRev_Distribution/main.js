// AdRev Science - Premium Interactivity System
document.addEventListener('DOMContentLoaded', () => {

    // 1. Scroll-reveal Observer for all animated elements
    const revealOptions = {
        threshold: 0.15,
        rootMargin: "0px 0px -100px 0px"
    };

    const revealObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('revealed');
            }
        });
    }, revealOptions);

    // Initial state setup for reveal elements
    document.querySelectorAll('.animate-on-scroll, .glass-card, .price-card, .process-item, .hero-text, .hero-visual').forEach(el => {
        el.classList.add('reveal-init');
        revealObserver.observe(el);
    });

    // 2. Sophisticated Header Scroll Effect
    const header = document.querySelector('.glass-nav');
    window.addEventListener('scroll', () => {
        if (window.scrollY > 50) {
            header.style.top = '1rem';
            header.style.width = '95%';
            header.style.background = 'rgba(2, 6, 23, 0.9)';
            header.style.boxShadow = '0 20px 40px rgba(0,0,0,0.3)';
        } else {
            header.style.top = '2rem';
            header.style.width = '85%';
            header.style.background = 'rgba(15, 23, 42, 0.6)';
            header.style.boxShadow = 'none';
        }
    });

    // 3. Smooth Anchor Scrolling with offset
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const targetId = this.getAttribute('href');
            if (targetId === '#') return;

            e.preventDefault();
            const target = document.querySelector(targetId);
            if (target) {
                const navHeight = 100;
                window.scrollTo({
                    top: target.offsetTop - navHeight,
                    behavior: 'smooth'
                });
            }
        });
    });

    // 4. Subtle Parallax for Hero
    window.addEventListener('scroll', () => {
        const scrolled = window.scrollY;
        const heroVisual = document.querySelector('.hero-visual');
        if (heroVisual && scrolled < 1000) {
            heroVisual.style.transform = `translateY(${scrolled * 0.1}px)`;
        }
    });

    // 5. Newsletter Mockup Interaction
    const newsletterForm = document.querySelector('.newsletter-form');
    if (newsletterForm) {
        newsletterForm.addEventListener('submit', (e) => {
            e.preventDefault();
            const input = newsletterForm.querySelector('input');
            const button = newsletterForm.querySelector('button');
            if (input.value) {
                const originalText = button.textContent;
                button.textContent = '✓';
                button.style.background = 'var(--secondary)';
                input.value = '';
                setTimeout(() => {
                    button.textContent = originalText;
                    button.style.background = 'var(--primary)';
                }, 3000);
            }
        });
    }

    console.log('AdRev Premium Site Core Restructured 🚀');
});
