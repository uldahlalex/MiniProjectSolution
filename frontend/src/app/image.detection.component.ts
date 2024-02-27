import {AfterViewInit, Component, ElementRef, inject, Inject, OnInit, ViewChild} from "@angular/core";
import {WebSocketClientService} from "../ws.client.service";
import {FormControl} from "@angular/forms";
import {Object, AnalysisResult} from "../models/imageDetectionModels";
import {ClientWantsToDetectImageObjects} from "../models/clientWantsToDetectImageObjects";

@Component({
  selector: 'app-image-detection',
  template: `
    <h1>Enter an image url to analyze</h1>
    <input [formControl]="imgUrl">
    <button (click)="analyzeImage()">Analyze it!</button>

    <img [src]="imgUrl.value">
    <canvas #canvas></canvas>

  `
})
export class ImageDetectionComponent implements OnInit {

  ngOnInit() {
    this.ws.onImageAnalysisReceived.subscribe((imageObjects: AnalysisResult) => {
      this.drawObjects(imageObjects.objects!);
    });
  }


  public ws: WebSocketClientService = inject(WebSocketClientService);

  //@ts-ignore
  @ViewChild('canvas') canvas: ElementRef<HTMLCanvasElement>;
  //@ts-ignore
  private ctx: CanvasRenderingContext2D;

  imgUrl = new FormControl(
    "https://i.ibb.co/R9CQysZ/image-2.png");

  drawObjects(imageObjects: Object[]): void {
    this.ctx = this.canvas.nativeElement.getContext('2d')!;
    const img = new Image();
    img.src = this.imgUrl.value!;
    img.onload = () => {
      this.canvas.nativeElement.width = img.width;
      this.canvas.nativeElement.height = img.height;
      this.ctx.drawImage(img, 0, 0, img.width, img.height);
      imageObjects.forEach(obj => {
        this.ctx.beginPath();
        this.ctx.rect(obj.rectangle.x, obj.rectangle.y, obj.rectangle.w, obj.rectangle.h);
        this.ctx.lineWidth = 3;
        this.ctx.strokeStyle = 'red';
        this.ctx.fillStyle = 'red';
        this.ctx.stroke();
        this.ctx.fillText(obj.object, obj.rectangle.x, obj.rectangle.y > 20 ? obj.rectangle.y - 5 : 15);
      });
    };

  }

  analyzeImage() {
    this.ws.socketConnection.sendDto(new ClientWantsToDetectImageObjects({url: this.imgUrl.value!}));
  }
}
