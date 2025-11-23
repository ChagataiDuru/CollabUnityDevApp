import { Component, ElementRef, ViewChild, AfterViewInit, OnDestroy, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { WhiteboardService, DrawEvent } from '../../core/services/whiteboard.service';
import { ButtonModule } from 'primeng/button';
import { ColorPickerModule } from 'primeng/colorpicker';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-whiteboard',
    standalone: true,
    imports: [CommonModule, ButtonModule, ColorPickerModule, FormsModule],
    templateUrl: './whiteboard.component.html',
    styleUrls: ['./whiteboard.component.css']
})
export class WhiteboardComponent implements AfterViewInit, OnDestroy {
    @ViewChild('canvas') canvasRef!: ElementRef<HTMLCanvasElement>;

    private whiteboardService = inject(WhiteboardService);
    private route = inject(ActivatedRoute);

    projectId: string = '';
    private ctx!: CanvasRenderingContext2D;
    private isDrawing = false;
    private lastX = 0;
    private lastY = 0;
    private resizeObserver: ResizeObserver | null = null;

    // Tools
    selectedColor: string = '#000000';
    lineWidth: number = 2;
    isEraser: boolean = false;

    constructor() {
        // Listen for remote draw events
        effect(() => {
            const event = this.whiteboardService.drawEvent();
            if (event) {
                this.drawRemote(event);
            }
        });

        // Listen for clear events
        effect(() => {
            if (this.whiteboardService.clearEvent()) {
                this.clearCanvasLocal();
            }
        });
    }

    ngAfterViewInit() {
        this.projectId = this.route.snapshot.paramMap.get('id') || '';
        this.initCanvas();

        if (this.projectId) {
            this.whiteboardService.startConnection(this.projectId);
        }

        // Use ResizeObserver for robust size handling
        const parent = this.canvasRef.nativeElement.parentElement;
        if (parent) {
            this.resizeObserver = new ResizeObserver(() => {
                this.resizeCanvas();
            });
            this.resizeObserver.observe(parent);
        }
    }

    ngOnDestroy() {
        this.whiteboardService.stopConnection();
        if (this.resizeObserver) {
            this.resizeObserver.disconnect();
        }
    }

    private initCanvas() {
        const canvas = this.canvasRef.nativeElement;
        this.ctx = canvas.getContext('2d')!;

        // Initial resize attempt
        this.resizeCanvas();

        // Set initial styles
        this.ctx.lineCap = 'round';
        this.ctx.lineJoin = 'round';
    }

    private resizeCanvas() {
        const canvas = this.canvasRef.nativeElement;
        const parent = canvas.parentElement;

        if (parent) {
            const width = parent.clientWidth;
            const height = parent.clientHeight;

            // Prevent resizing to 0
            if (width === 0 || height === 0) return;

            // Optional: Skip if size hasn't changed
            if (canvas.width === width && canvas.height === height) return;

            // Save current content
            const tempCanvas = document.createElement('canvas');
            tempCanvas.width = canvas.width;
            tempCanvas.height = canvas.height;
            const tempCtx = tempCanvas.getContext('2d');

            // Only draw if we have a context (should always be true)
            if (tempCtx) {
                tempCtx.drawImage(canvas, 0, 0);
            }

            // Resize
            canvas.width = width;
            canvas.height = height;

            // Fill white background
            this.ctx.fillStyle = '#ffffff';
            this.ctx.fillRect(0, 0, canvas.width, canvas.height);

            // Restore content
            this.ctx.drawImage(tempCanvas, 0, 0);

            // Restore context properties lost on resize
            this.ctx.lineCap = 'round';
            this.ctx.lineJoin = 'round';
        }
    }

    // Drawing Logic
    startDrawing(event: MouseEvent | TouchEvent) {
        this.isDrawing = true;
        const pos = this.getPos(event);
        if (pos) {
            this.lastX = pos.x;
            this.lastY = pos.y;
        }
    }

    draw(event: MouseEvent | TouchEvent) {
        if (!this.isDrawing) return;
        // Check if context is available
        if (!this.ctx) return;

        event.preventDefault(); // Prevent scrolling on touch

        const pos = this.getPos(event);
        if (!pos) return;

        this.drawStroke(this.lastX, this.lastY, pos.x, pos.y, this.isEraser ? '#ffffff' : this.selectedColor, this.isEraser ? 20 : this.lineWidth);

        // Send to server
        this.whiteboardService.sendDraw(this.projectId, {
            prevX: this.lastX,
            prevY: this.lastY,
            currX: pos.x,
            currY: pos.y,
            color: this.isEraser ? '#ffffff' : this.selectedColor,
            lineWidth: this.isEraser ? 20 : this.lineWidth,
            type: 'draw'
        });

        this.lastX = pos.x;
        this.lastY = pos.y;
    }

    stopDrawing() {
        this.isDrawing = false;
    }

    private getPos(event: MouseEvent | TouchEvent): { x: number, y: number } | null {
        const canvas = this.canvasRef.nativeElement;
        const rect = canvas.getBoundingClientRect();

        let clientX: number, clientY: number;

        if ('touches' in event) {
            const touchEvent = event as TouchEvent;
            if (touchEvent.touches && touchEvent.touches.length > 0) {
                clientX = touchEvent.touches[0].clientX;
                clientY = touchEvent.touches[0].clientY;
            } else {
                return null;
            }
        } else {
            const mouseEvent = event as MouseEvent;
            clientX = mouseEvent.clientX;
            clientY = mouseEvent.clientY;
        }

        return {
            x: clientX - rect.left,
            y: clientY - rect.top
        };
    }

    private drawStroke(prevX: number, prevY: number, currX: number, currY: number, color: string, width: number) {
        if (!this.ctx) return;
        this.ctx.beginPath();
        this.ctx.moveTo(prevX, prevY);
        this.ctx.lineTo(currX, currY);
        this.ctx.strokeStyle = color;
        this.ctx.lineWidth = width;
        this.ctx.stroke();
        this.ctx.closePath();
    }

    private drawRemote(event: DrawEvent) {
        if (!this.ctx) return;
        this.drawStroke(event.prevX, event.prevY, event.currX, event.currY, event.color, event.lineWidth);
    }

    // Tools
    setTool(tool: 'pen' | 'eraser') {
        this.isEraser = tool === 'eraser';
    }

    clearBoard() {
        this.clearCanvasLocal();
        this.whiteboardService.clearBoard(this.projectId);
    }

    private clearCanvasLocal() {
        if (!this.ctx) return;
        const canvas = this.canvasRef.nativeElement;
        this.ctx.clearRect(0, 0, canvas.width, canvas.height);
        // Re-fill white background after clear because clearRect makes it transparent
        this.ctx.fillStyle = '#ffffff';
        this.ctx.fillRect(0, 0, canvas.width, canvas.height);
    }
}
