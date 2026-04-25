import { Component, HostListener, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/services';

const REVIEWS = [
  { name: 'Marcus T.',  role: 'Home Barista',          text: 'Finally stopped wasting beans every time I switch coffees. Grind Advisor got me dialed in on the first try.' },
  { name: 'Priya N.',   role: 'Coffee Roaster',         text: 'I recommend this to every customer who buys our single origins. It speaks their grinder\'s language.' },
  { name: 'James C.',   role: 'Café Owner',             text: 'We run three different grinders and this keeps settings consistent across all of them. Indispensable.' },
  { name: 'Simone B.',  role: 'Pour Over Enthusiast',   text: 'The NGI concept finally made sense to me after using this app. My V60 has never tasted better.' },
  { name: 'David K.',   role: 'Specialty Importer',     text: 'I\'ve tried every grind app out there. This is the only one that actually learns from your history.' },
  { name: 'Elena R.',   role: 'Home Barista',           text: 'Switched from a Baratza to a Comandante and had zero wasted shots. The cross-grinder estimates are magic.' },
  { name: 'Tom W.',     role: 'Competition Barista',    text: 'When every point counts, you need data. Grind Atlas gives me that data in a way I can actually act on.' },
  { name: 'Aisha M.',   role: 'Subscription Roaster',   text: 'We share this with every subscriber. It turns guessing into a skill they keep building on.' },
  { name: 'Baratza S.',        role: 'Grinder',             text: 'Whirrr... grrrk grrrk grrrk CLUNK whirrr. Grk. Grk grk grk grk WHIRRRRRR clunk. ★★★★★' },
];

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink],
  template: `
    <!-- NAV -->
    <nav class="lp-nav">
      <span class="lp-brand">Grind Atlas</span>
      <div class="lp-nav-actions">
        <a class="btn" routerLink="/login">Log In</a>
        <a class="btn btn-inv" routerLink="/register">Sign Up</a>
      </div>
    </nav>

    <!-- HERO -->
    <section class="lp-hero">
      <div class="hero-accent"></div>
      <div class="hero-content">
        <h1 class="hero-headline">Track every grind.<br>Dial in faster.</h1>
        <p class="hero-sub">The grind tracking app that learns your beans, bridges your grinders, and gives you a starting point that actually makes sense.</p>
        <div class="hero-ctas">
          <a class="btn btn-inv btn-lg" routerLink="/register">Get Started</a>
          <a class="btn btn-lg" routerLink="/login">Log In</a>
        </div>
      </div>
    </section>

    <!-- FEATURES -->
    <section class="lp-features">
      <h2 class="section-heading">Everything you need to dial in.</h2>
      <div class="features-grid">
        <div class="panel feature-card">
          <h3>Grind Advisor</h3>
          <p>4-layer inference engine recommends a starting grind setting based on your coffee, grinder, and brew method — bridged via the Normalized Grind Index.</p>
        </div>
        <div class="panel feature-card">
          <h3>Grind Logs</h3>
          <p>Record every session: coffee, grinder, setting, brew method, and result. Build the dataset that makes future estimates more accurate.</p>
        </div>
        <div class="panel feature-card">
          <h3>Brew Recipes</h3>
          <p>Save and reuse your best recipes. Step-by-step brew timers built in — from espresso to cold brew, every method covered.</p>
        </div>
        <div class="panel feature-card feature-card--coming">
          <div class="coming-header">
            <h3>Collection</h3>
            <span class="status-pill s-hold">Coming Soon</span>
          </div>
          <p>Track every coffee you've tried. Rate them, tag them, and let the Grind Advisor cross-reference your entire history when suggesting settings.</p>
        </div>
      </div>
    </section>

    <!-- REVIEWS -->
    <section class="lp-reviews">
      <h2 class="section-heading">What grinders are saying.</h2>
      <div class="carousel">
        <div class="carousel-track"
             [style.transform]="carouselTransform()">
          @for (r of reviews; track r.name; let i = $index) {
            <div class="review-slide" [class.review-dim]="i !== currentSlide()">
              <div class="panel review-card">
                <p class="review-text">"{{ r.text }}"</p>
                <div class="review-attribution">
                  <strong>{{ r.name }}</strong>
                  <span class="review-role">{{ r.role }}</span>
                </div>
              </div>
            </div>
          }
        </div>
      </div>
      <div class="carousel-controls">
        <button class="btn" (click)="prevSlide()">&#8592; Prev</button>
        <span class="slide-counter">{{ currentSlide() + 1 }} / {{ totalSlides }}</span>
        <button class="btn" (click)="nextSlide()">Next &#8594;</button>
      </div>
    </section>

    <!-- FOOTER -->
    <footer class="lp-footer">
      <span class="footer-brand">Grind Atlas</span>
      <span class="footer-tagline">Track every grind. Dial in faster.</span>
    </footer>
  `,
  styles: [`
    :host { display: block; font-family: 'Courier New', monospace; }

    /* NAV */
    .lp-nav {
      position: sticky; top: 0; z-index: 100;
      display: flex; align-items: center; justify-content: space-between;
      padding: 12px 40px;
      background: var(--ink); color: var(--paper);
    }
    .lp-brand { font-size: 1.1rem; font-weight: bold; letter-spacing: 0.05em; }
    .lp-nav-actions { display: flex; gap: 12px; }

    /* HERO */
    .lp-hero {
      display: flex; align-items: stretch; min-height: 65vh;
      background: var(--paper); color: var(--ink);
    }
    .hero-accent {
      width: 8px; flex-shrink: 0;
      background: var(--ink);
    }
    .hero-content {
      flex: 1; display: flex; flex-direction: column;
      justify-content: center; padding: 72px 60px;
    }
    .hero-headline {
      font-size: 52px; font-weight: bold; line-height: 1.1;
      margin: 0 0 20px; letter-spacing: -0.01em;
    }
    .hero-sub {
      font-size: 1.1rem; max-width: 560px; margin: 0 0 40px;
      line-height: 1.7; color: #555;
    }
    .hero-ctas { display: flex; gap: 16px; flex-wrap: wrap; }
    .btn-lg { padding: 12px 28px; font-size: 1rem; }

    /* FEATURES */
    .lp-features {
      padding: 80px 40px;
      background: var(--paper); color: var(--ink);
      border-top: var(--b-thick) solid var(--ink);
    }
    .section-heading {
      font-size: 1.5rem; font-weight: bold; margin: 0 0 40px;
      letter-spacing: 0.02em;
    }
    .features-grid {
      display: grid; grid-template-columns: repeat(2, 1fr); gap: 24px;
    }
    .feature-card { padding: 24px; }
    .feature-card h3 { margin: 0 0 12px; font-size: 1.05rem; }
    .feature-card p { margin: 0; color: #444; line-height: 1.7; font-size: 0.9rem; }
    .feature-card--coming { opacity: 0.6; background: var(--paper); }
    .coming-header { display: flex; align-items: center; gap: 12px; margin-bottom: 12px; }
    .coming-header h3 { margin: 0; font-size: 1.05rem; }

    /* REVIEWS */
    .lp-reviews {
      padding: 80px 40px;
      background: var(--paper); color: var(--ink);
      border-top: var(--b-thick) solid var(--ink);
    }
    .carousel { overflow: hidden; }
    .carousel-track {
      display: flex;
      transition: transform 0.35s ease;
    }
    .review-slide {
      min-width: calc((100vw - 80px) / 3);
      box-sizing: border-box;
      padding: 0 12px;
      transition: opacity 0.35s ease;
    }
    .review-dim { opacity: 0.25; }
    .review-card {
      padding: 32px;
      display: flex; flex-direction: column; gap: 20px;
    }
    .review-text { font-size: 1rem; line-height: 1.75; margin: 0; font-style: italic; }
    .review-attribution { display: flex; flex-direction: column; gap: 4px; }
    .review-attribution strong { font-size: 0.9rem; }
    .review-role { font-size: 0.8rem; color: #777; }
    .carousel-controls {
      display: flex; align-items: center; gap: 20px;
      margin-top: 28px; justify-content: center;
    }
    .slide-counter { font-size: 0.85rem; color: #777; min-width: 60px; text-align: center; }

    /* FOOTER */
    .lp-footer {
      display: flex; justify-content: space-between; align-items: center;
      padding: 24px 40px;
      background: var(--ink); color: var(--paper);
      border-top: var(--b-thick) solid var(--ink);
      font-size: 0.875rem;
    }
    .footer-brand { font-weight: bold; letter-spacing: 0.05em; }
    .footer-tagline { color: rgba(255,255,255,0.65); }

    @media (max-width: 700px) {
      .features-grid { grid-template-columns: 1fr; }
      .hero-headline { font-size: 36px; }
      .hero-content { padding: 52px 24px; }
      .lp-features, .lp-reviews { padding: 60px 24px; }
      .lp-nav { padding: 12px 24px; }
      .lp-footer { padding: 20px 24px; flex-direction: column; gap: 8px; text-align: center; }
      .review-slide { min-width: calc(100vw - 48px) !important; }
    }
  `],
})
export class LandingComponent implements OnInit {
  private auth = inject(AuthService);
  private router = inject(Router);

  readonly reviews = REVIEWS;
  readonly totalSlides = REVIEWS.length;
  currentSlide = signal(0);
  isMobile = signal(window.innerWidth <= 700);

  @HostListener('window:resize')
  onResize() { this.isMobile.set(window.innerWidth <= 700); }

  carouselTransform() {
    const i = this.currentSlide();
    if (this.isMobile()) {
      // Single-card: shift left by i full-card widths (section has 24px padding each side)
      return `translateX(calc(${-i} * (100vw - 48px)))`;
    }
    // 3-card: center the active card in the middle slot
    return `translateX(calc(${1 - i} * (100vw - 80px) / 3))`;
  }

  ngOnInit(): void {
    if (this.auth.isLoggedIn()) {
      this.router.navigate(['/home']);
    }
  }

  prevSlide(): void {
    this.currentSlide.update(s => (s - 1 + this.totalSlides) % this.totalSlides);
  }

  nextSlide(): void {
    this.currentSlide.update(s => (s + 1) % this.totalSlides);
  }
}
