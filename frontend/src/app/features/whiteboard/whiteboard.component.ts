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

        window.addEventListener('resize', () => this.resizeCanvas());
    }

    ngOnDestroy() {
        this.whiteboardService.stopConnection();
        window.removeEventListener('resize', () => this.resizeCanvas());
    }

    private initCanvas() {
        const canvas = this.canvasRef.nativeElement;
        this.ctx = canvas.getContext('2d')!;
        this.resizeCanvas();

        // Set initial styles
        this.ctx.lineCap = 'round';
        this.ctx.lineJoin = 'round';
    }

    private resizeCanvas() {
        const canvas = this.canvasRef.nativeElement;
        const parent = canvas.parentElement;

        if (parent) {
            // Save current content
            const tempCanvas = document.createElement('canvas');
            tempCanvas.width = canvas.width;
            tempCanvas.height = canvas.height;
            const tempCtx = tempCanvas.getContext('2d')!;
            tempCtx.drawImage(canvas, 0, 0);

            // Resize
            canvas.width = parent.clientWidth;
            canvas.height = parent.clientHeight;

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
        this.lastX = pos.x;
        this.lastY = pos.y;
    }

    draw(event: MouseEvent | TouchEvent) {
        if (!this.isDrawing) return;
        event.preventDefault(); // Prevent scrolling on touch

        const pos = this.getPos(event);

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

    private getPos(event: MouseEvent | TouchEvent) {
        const canvas = this.canvasRef.nativeElement;
        const rect = canvas.getBoundingClientRect();

        let clientX, clientY;

        if (event instanceof MouseEvent) {
            clientX = event.clientX;
            clientY = event.clientY;
        } else {
            clientX = event.touches[0].clientX;
            clientY = event.touches[0].clientY;
        }

        return {
            x: clientX - rect.left,
            y: clientY - rect.top
        };
    }

    private drawStroke(prevX: number, prevY: number, currX: number, currY: number, color: string, width: number) {
        this.ctx.beginPath();
        this.ctx.moveTo(prevX, prevY);
        this.ctx.lineTo(currX, currY);
        this.ctx.strokeStyle = color;
        this.ctx.lineWidth = width;
        this.ctx.stroke();
        this.ctx.closePath();
    }

    private drawRemote(event: DrawEvent) {
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
        const canvas = this.canvasRef.nativeElement;
        this.ctx.clearRect(0, 0, canvas.width, canvas.height);
    }
}
