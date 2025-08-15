/**
 * ============================================================================
 * CYBERPUNK DASHBOARD - Main JavaScript Module
 * ============================================================================ */

/**
 * NOTIFICATION MANAGER - Standalone notification system
 */
class NotificationManager {
    constructor() {
        this.notificationTimeout = null;
        this.queue = [];
        this.isShowing = false;
    }

    show(message, isSuccess = true, duration = 3000) {
        const notification = {
            message,
            isSuccess,
            duration,
            timestamp: Date.now()
        };

        this.queue.push(notification);

        if (!this.isShowing) {
            this.displayNext();
        }
    }

    displayNext() {
        if (this.queue.length === 0) {
            this.isShowing = false;
            return;
        }

        this.isShowing = true;
        const notification = this.queue.shift();

        const element = document.getElementById('notification');
        if (!element) {
            console.warn('Notification element not found');
            this.isShowing = false;
            return;
        }

        if (this.notificationTimeout) {
            clearTimeout(this.notificationTimeout);
        }

        element.classList.remove('show', 'error');
        element.style.opacity = '0';
        element.style.transform = 'translateX(400px)';

        const icon = notification.isSuccess ? 'check-circle' : 'exclamation-triangle';
        element.innerHTML = `<i class="fas fa-${icon}"></i> ${notification.message}`;
        element.className = `notification ${notification.isSuccess ? '' : 'error'}`;

        setTimeout(() => {
            element.classList.add('show');
            element.style.opacity = '1';
            element.style.transform = 'translateX(0)';
        }, 50);

        this.notificationTimeout = setTimeout(() => {
            this.hide(element);
            setTimeout(() => {
                this.displayNext();
            }, 500);
        }, notification.duration);
    }

    hide(element) {
        if (element) {
            element.classList.remove('show');
            element.style.opacity = '0';
            element.style.transform = 'translateX(400px)';
        }
    }

    clearAll() {
        this.queue = [];
        if (this.notificationTimeout) {
            clearTimeout(this.notificationTimeout);
        }
        this.isShowing = false;
    }

    destroy() {
        this.clearAll();
    }
}

/**
 * MAIN DASHBOARD CLASS
 */
class Dashboard {
    constructor() {
        this.API_BASE = '/api/FacebookWebhook';
        this.notifications = new NotificationManager();
        this.refreshInterval = null;
        this.refreshCounter = 0;
        this.isInitialized = false;

        this.init();
    }

    init() {
        if (this.isInitialized) return;

        console.log('%c🚀 CYBERPUNK DASHBOARD INITIALIZED 🚀', 'color: #00ffff; font-size: 16px; font-weight: bold;');
        console.log('%cKeyboard Shortcuts:', 'color: #ff00ff; font-size: 14px;');
        console.log('%cCtrl+1: Generate Lead', 'color: #39ff14;');
        console.log('%cCtrl+2: Simulate Facebook', 'color: #39ff14;');
        console.log('%cCtrl+R: Refresh Data', 'color: #39ff14;');

        this.setupKeyboardShortcuts();
        this.setupErrorHandling();
        this.addWelcomeAnimation();

        this.refreshLeads();
        this.startAutoRefresh();

        setTimeout(() => this.typeWriterEffect(), 1000);

        this.isInitialized = true;
    }

    setupKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            if (e.ctrlKey || e.metaKey) {
                switch (e.key) {
                    case '1':
                        e.preventDefault();
                        this.triggerDemo();
                        break;
                    case '2':
                        e.preventDefault();
                        this.simulateFacebook();
                        break;
                    case 'r':
                        e.preventDefault();
                        this.refreshLeads();
                        break;
                }
            }
        });
    }

    setupErrorHandling() {
        window.addEventListener('error', (e) => {
            console.error('System error detected:', e.error);
            this.notifications.show('System error detected - check console', false);
        });
    }

    addWelcomeAnimation() {
        document.body.style.opacity = '0';
        setTimeout(() => {
            document.body.style.transition = 'opacity 1s ease';
            document.body.style.opacity = '1';
        }, 100);
    }

    async triggerDemo() {
        // Find the button - either from event or by selector
        const btn = (window.event && window.event.target) || document.querySelector('.demo-btn:first-child');
        if (!btn) return;

        const originalHTML = btn.innerHTML;

        try {
            btn.innerHTML = '<i class="fas fa-cog fa-spin"></i> Initializing...';
            btn.disabled = true;
            btn.classList.add('loading');

            const randomNum = Math.floor(Math.random() * 900) + 100;
            const leadData = {
                name: `CyberUser_${randomNum}`,
                email: `cyber${randomNum}@nexus.net`,
                phone: `+1-${Math.floor(Math.random() * 900 + 100)}-${Math.floor(Math.random() * 9000 + 1000)}`
            };

            const response = await this.makeAPICall('/trigger-demo', 'POST', leadData);

            if (response.success !== false) {
                btn.innerHTML = '<i class="fas fa-check"></i> Success!';
                btn.classList.add('success');
                this.notifications.show('Lead successfully integrated into database!');

                this.confettiEffect();
                setTimeout(() => this.refreshLeads(), 500);
            } else {
                throw new Error(response.message || 'System error detected');
            }
        } catch (error) {
            btn.innerHTML = '<i class="fas fa-times"></i> Error';
            this.notifications.show('System error: Lead generation failed', false);
            console.error('Demo trigger error:', error);
        } finally {
            setTimeout(() => {
                btn.innerHTML = originalHTML;
                btn.classList.remove('success', 'loading');
                btn.disabled = false;
            }, 2500);
        }
    }

    async simulateFacebook() {
        const btn = (window.event && window.event.target) || document.querySelector('.demo-btn:nth-child(2)');
        if (!btn) return;

        const originalHTML = btn.innerHTML;

        try {
            btn.innerHTML = '<i class="fas fa-satellite-dish fa-pulse"></i> Connecting...';
            btn.disabled = true;
            btn.classList.add('loading');

            const response = await this.makeAPICall('/simulate-facebook', 'POST');

            if (response.success !== false) {
                btn.innerHTML = '<i class="fas fa-check"></i> Connected!';
                btn.classList.add('success');
                this.notifications.show('Facebook API simulation successful!');

                setTimeout(() => this.refreshLeads(), 500);
            } else {
                throw new Error(response.message || 'Connection failed');
            }
        } catch (error) {
            btn.innerHTML = '<i class="fas fa-times"></i> Failed';
            this.notifications.show('Facebook simulation failed', false);
            console.error('Facebook simulation error:', error);
        } finally {
            setTimeout(() => {
                btn.innerHTML = originalHTML;
                btn.classList.remove('success', 'loading');
                btn.disabled = false;
            }, 2500);
        }
    }

    async refreshLeads() {
        try {
            if (window.event) {
                const btn = window.event.target;
                const icon = btn.querySelector('i');
                if (icon) {
                    icon.classList.add('fa-spin');
                    setTimeout(() => icon.classList.remove('fa-spin'), 1000);
                }
            }

            const data = await this.makeAPICall('/leads', 'GET');

            if (data) {
                this.updateStats(data);
                this.displayLeads(data.leads || []);
                this.updateLastRefreshTime();
            }
        } catch (error) {
            console.error('Error fetching leads:', error);
            this.notifications.show('Data sync failed', false);
        }
    }

    async clearLeads() {
        const confirmed = confirm('⚠️ DANGER ZONE ⚠️\n\nThis will permanently delete all lead records.\nAre you sure you want to proceed?');

        if (!confirmed) return;

        const btn = (window.event && window.event.target) || document.querySelector('.demo-btn.clear');
        if (!btn) return;

        const originalHTML = btn.innerHTML;

        try {
            btn.innerHTML = '<i class="fas fa-skull-crossbones fa-pulse"></i> Purging...';
            btn.disabled = true;

            const response = await this.makeAPICall('/clear', 'DELETE');

            if (response.success !== false) {
                btn.innerHTML = '<i class="fas fa-check"></i> Purged!';
                this.notifications.show('All records successfully purged from database!');
                setTimeout(() => this.refreshLeads(), 500);
            } else {
                throw new Error(response.message || 'Purge failed');
            }
        } catch (error) {
            this.notifications.show('Purge operation failed', false);
            console.error('Clear leads error:', error);
        } finally {
            setTimeout(() => {
                btn.innerHTML = originalHTML;
                btn.disabled = false;
            }, 2500);
        }
    }

    async makeAPICall(endpoint, method = 'GET', data = null) {
        const url = `${this.API_BASE}${endpoint}`;
        const options = {
            method: method,
            headers: {
                'Content-Type': 'application/json',
            },
        };

        if (data && (method === 'POST' || method === 'PUT')) {
            options.body = JSON.stringify(data);
        }

        try {
            const response = await fetch(url, options);

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            } else {
                return { success: true, data: await response.text() };
            }
        } catch (error) {
            console.error(`API call failed: ${method} ${url}`, error);
            throw error;
        }
    }

    updateStats(data) {
        const totalElement = document.getElementById('totalLeads');
        const countElement = document.getElementById('leadCount');
        const updateElement = document.getElementById('lastUpdate');

        if (totalElement) {
            const currentValue = parseInt(totalElement.textContent) || 0;
            const newValue = data.totalCount || 0;
            this.animateNumber(totalElement, currentValue, newValue);
        }

        if (countElement) {
            countElement.textContent = `${data.totalCount || 0} records`;
        }

        if (updateElement) {
            const now = new Date();
            const hours = now.getHours().toString().padStart(2, '0');
            const minutes = now.getMinutes().toString().padStart(2, '0');
            updateElement.textContent = `${hours}:${minutes}`;
        }
    }

    animateNumber(element, start, end) {
        if (start === end) return;

        const duration = 1000;
        const increment = end > start ? 1 : -1;
        const stepTime = Math.abs(Math.floor(duration / (end - start))) || 50;

        let current = start;
        const timer = setInterval(() => {
            current += increment;
            element.textContent = current;

            if (current === end) {
                clearInterval(timer);
                element.style.textShadow = '0 0 20px var(--neon-cyan)';
                setTimeout(() => {
                    element.style.textShadow = '';
                }, 1000);
            }
        }, stepTime);
    }

    displayLeads(leads) {
        const container = document.getElementById('leadsContainer');
        if (!container) return;

        if (!leads || leads.length === 0) {
            container.innerHTML = `
                <div class="no-leads">
                    <i class="fas fa-rocket" style="font-size: 2em; margin-bottom: 20px; color: var(--neon-cyan);"></i><br>
                    Initialize system by generating your first lead
                </div>
            `;
            return;
        }

        const leadsHTML = leads.map((lead, index) => `
            <div class="lead-card" style="animation-delay: ${index * 100}ms">
                <div class="lead-header">
                    <div class="lead-name">
                        <i class="fas fa-user-astronaut"></i> ${this.escapeHtml(lead.name)}
                    </div>
                    <div class="lead-time">${this.formatDateTime(lead.timestamp)}</div>
                </div>
                <div class="lead-details">
                    <div class="lead-field">
                        <i class="fas fa-envelope" style="color: var(--neon-cyan); margin-right: 8px;"></i>
                        ${this.escapeHtml(lead.email)}
                    </div>
                    <div class="lead-field">
                        <i class="fas fa-phone" style="color: var(--neon-green); margin-right: 8px;"></i>
                        ${this.escapeHtml(lead.phone)}
                    </div>
                    <div class="lead-field">
                        <i class="fas fa-satellite" style="color: var(--neon-purple); margin-right: 8px;"></i>
                        ${this.escapeHtml(lead.source)}
                    </div>
                    <div class="lead-field">
                        <i class="fas fa-tag" style="color: var(--neon-pink); margin-right: 8px;"></i>
                        ${this.escapeHtml(lead.status)}
                    </div>
                </div>
            </div>
        `).join('');

        container.innerHTML = leadsHTML;
    }

    formatDateTime(dateString) {
        try {
            const date = new Date(dateString);
            return date.toLocaleString('en-US', {
                year: 'numeric',
                month: '2-digit',
                day: '2-digit',
                hour: '2-digit',
                minute: '2-digit',
                second: '2-digit',
                hour12: false
            });
        } catch (error) {
            return 'Invalid Date';
        }
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    startAutoRefresh() {
        if (this.refreshInterval) {
            clearInterval(this.refreshInterval);
        }

        this.refreshInterval = setInterval(() => {
            this.refreshCounter++;
            this.refreshLeads();

            const refreshBtn = document.querySelector('.demo-btn:nth-child(3)');
            if (refreshBtn) {
                refreshBtn.style.borderColor = 'var(--neon-green)';
                setTimeout(() => {
                    refreshBtn.style.borderColor = '';
                }, 300);
            }
        }, 5000);
    }

    stopAutoRefresh() {
        if (this.refreshInterval) {
            clearInterval(this.refreshInterval);
            this.refreshInterval = null;
        }
    }

    updateLastRefreshTime() {
        const indicator = document.querySelector('.last-refresh-indicator');
        if (indicator) {
            const now = new Date();
            indicator.textContent = `Last updated: ${now.toLocaleTimeString()}`;
        }
    }

    confettiEffect() {
        const colors = ['var(--neon-cyan)', 'var(--neon-pink)', 'var(--neon-green)', 'var(--neon-purple)'];

        for (let i = 0; i < 30; i++) {
            const confetti = document.createElement('div');
            confetti.style.cssText = `
                position: fixed;
                width: 8px;
                height: 8px;
                background: ${colors[Math.floor(Math.random() * colors.length)]};
                top: 20%;
                left: ${Math.random() * 100}%;
                z-index: 9999;
                pointer-events: none;
                border-radius: 50%;
                animation: confettiFall ${2 + Math.random() * 3}s ease-out forwards;
            `;

            document.body.appendChild(confetti);

            setTimeout(() => {
                if (confetti.parentNode) {
                    confetti.parentNode.removeChild(confetti);
                }
            }, 5000);
        }
    }

    typeWriterEffect() {
        const webhookElement = document.querySelector('.webhook-url');
        if (!webhookElement) return;

        const text = webhookElement.innerHTML;
        webhookElement.innerHTML = '';

        let i = 0;
        const timer = setInterval(() => {
            webhookElement.innerHTML = text.slice(0, i) +
                (i < text.length ? '<span style="animation: cursorBlink 1s infinite;">|</span>' : '');
            i++;

            if (i > text.length) {
                clearInterval(timer);
                webhookElement.innerHTML = text;
            }
        }, 50);
    }

    destroy() {
        this.stopAutoRefresh();

        if (this.notifications) {
            this.notifications.destroy();
        }

        console.log('🔥 Dashboard destroyed');
    }
}

// ============================================================================
// GLOBAL INITIALIZATION AND FUNCTIONS
// ============================================================================

// Global dashboard instance
window.dashboard = null;

// Global functions for onclick handlers
function triggerDemo() {
    if (window.dashboard) {
        window.dashboard.triggerDemo();
    }
}

function simulateFacebook() {
    if (window.dashboard) {
        window.dashboard.simulateFacebook();
    }
}

function refreshLeads() {
    if (window.dashboard) {
        window.dashboard.refreshLeads();
    }
}

function clearLeads() {
    if (window.dashboard) {
        window.dashboard.clearLeads();
    }
}

// Initialize dashboard
function initializeDashboard() {
    if (window.dashboard) {
        window.dashboard.destroy();
    }
    window.dashboard = new Dashboard();
}

// Page lifecycle management
document.addEventListener('DOMContentLoaded', initializeDashboard);

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeDashboard);
} else {
    initializeDashboard();
}

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    if (window.dashboard) {
        window.dashboard.destroy();
    }
});

// Handle page visibility changes
document.addEventListener('visibilitychange', () => {
    if (window.dashboard) {
        if (document.hidden) {
            window.dashboard.stopAutoRefresh();
        } else {
            window.dashboard.startAutoRefresh();
            window.dashboard.refreshLeads();
        }
    }
});