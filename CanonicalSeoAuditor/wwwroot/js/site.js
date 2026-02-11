document.addEventListener("DOMContentLoaded", function () {
    var auditForm = document.getElementById('auditForm');
    var loadingOverlay = document.getElementById('loadingOverlay');

    if (auditForm && loadingOverlay) {
        auditForm.addEventListener('submit', function () {
            loadingOverlay.style.display = 'flex';
        });
    }

    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

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
            card.style.transitionDelay = (i % 4) * 60 + 'ms';
            observer.observe(card);
        });
    } else {
        cards.forEach(function (card) {
            card.classList.add('visible');
        });
    }

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