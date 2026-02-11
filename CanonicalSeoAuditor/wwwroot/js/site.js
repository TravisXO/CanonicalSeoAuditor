// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", function () {

    // =========================================================
    // 1. SEO Audit Loading State Logic  (original — untouched)
    // =========================================================
    var auditForm = document.getElementById('auditForm');
    var loadingOverlay = document.getElementById('loadingOverlay');

    if (auditForm && loadingOverlay) {
        auditForm.addEventListener('submit', function () {
            loadingOverlay.style.display = 'flex';
        });
    }

    // =========================================================
    // 2. Initialize Bootstrap Tooltips  (original — untouched)
    // =========================================================
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // =========================================================
    // 3. Scroll-triggered card reveal (Intersection Observer)
    // =========================================================
    var cards = document.querySelectorAll('.card-meta');

    if (cards.length > 0 && 'IntersectionObserver' in window) {
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('visible');
                    observer.unobserve(entry.target);
                }
            });
        }, {
            threshold: 0.1,
            rootMargin: '0px 0px -30px 0px'
        });

        cards.forEach(function (card, i) {
            // stagger each card slightly
            card.style.transitionDelay = (i % 4) * 60 + 'ms';
            observer.observe(card);
        });
    } else {
        // Fallback: just make all cards visible immediately
        cards.forEach(function (card) {
            card.classList.add('visible');
        });
    }

    // =========================================================
    // 4. Animated score counter on Results page
    //    Targets the large display-3 number inside .score-circle
    // =========================================================
    var scoreEl = document.querySelector('.score-circle .display-3');

    if (scoreEl) {
        var target = parseInt(scoreEl.textContent, 10);
        if (!isNaN(target)) {
            var start = 0;
            var duration = 900;
            var startTime = null;

            function animateCount(timestamp) {
                if (!startTime) startTime = timestamp;
                var progress = Math.min((timestamp - startTime) / duration, 1);
                // ease-out cubic
                var ease = 1 - Math.pow(1 - progress, 3);
                scoreEl.textContent = Math.round(start + (target - start) * ease);
                if (progress < 1) {
                    requestAnimationFrame(animateCount);
                } else {
                    scoreEl.textContent = target;
                }
            }

            requestAnimationFrame(animateCount);
        }
    }

    // =========================================================
    // 5. Animated loading messages cycle
    //    Cycles descriptive messages while the overlay is open
    // =========================================================
    if (loadingOverlay) {
        var msgEl = loadingOverlay.querySelector('p');
        var messages = [
            'Crawling metadata, structure, and technical SEO metrics.',
            'Checking canonical tags and hreflang signals...',
            'Analyzing image alt text and media quality...',
            'Inspecting Open Graph and Twitter card data...',
            'Evaluating accessibility and schema markup...',
            'Calculating your final SEO score...'
        ];
        var msgIndex = 0;
        var msgInterval = null;

        auditForm && auditForm.addEventListener('submit', function () {
            if (msgEl) {
                msgInterval = setInterval(function () {
                    msgIndex = (msgIndex + 1) % messages.length;
                    msgEl.style.opacity = '0';
                    setTimeout(function () {
                        msgEl.textContent = messages[msgIndex];
                        msgEl.style.opacity = '1';
                    }, 220);
                }, 1800);
            }
        });
    }

});