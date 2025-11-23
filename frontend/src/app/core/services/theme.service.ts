import { Injectable, signal, effect } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class ThemeService {
    isDark = signal<boolean>(true);

    constructor() {
        // Load from local storage or default to dark
        const savedTheme = localStorage.getItem('theme');
        if (savedTheme) {
            this.isDark.set(savedTheme === 'dark');
        } else {
            // Default to dark for this app
            this.isDark.set(true);
        }

        // Effect to apply class to html element
        effect(() => {
            const isDark = this.isDark();
            const html = document.documentElement;

            if (isDark) {
                html.classList.add('dark');
                localStorage.setItem('theme', 'dark');
            } else {
                html.classList.remove('dark');
                localStorage.setItem('theme', 'light');
            }
        });
    }

    toggleTheme() {
        this.isDark.update(d => !d);
    }
}
